using System;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.Entities.Units;

/// <summary>
/// 防守单位基类
/// 所有防守单位的基础实现，支持 Spine 动画
/// </summary>
public partial class SDUnitBase : Node2D
{
    // 属性
    public int MaxHp { get; protected set; }
    public int CurrentHp { get; protected set; }
    public int AttackDamage { get; protected set; }
    public float AttackRange { get; protected set; }
    public float AttackInterval { get; protected set; }
    public int EnergyCost { get; protected set; }
    public SDUnitType UnitType { get; protected set; }

    // 状态
    public Vector2I GridPosition { get; set; }
    public bool IsDead => CurrentHp <= 0;

    // 攻击计时
    protected float _attackTimer;
    protected bool _canAttack = true;

    // 目标
    protected Monsters.SDMonsterBase _target;

    // Spine 动画
    protected Node2D _spineNode;
    protected MegaSprite _spineSprite;
    protected CreatureAnimator _animator;

    // UI
    protected Label _hpLabel;
    protected ColorRect _hpBar;
    protected ColorRect _hpBarBg;

    // 配置
    protected UnitConfig _config;

    public SDUnitBase(UnitConfig config)
    {
        _config = config;
        EnergyCost = config.EnergyCost;
        MaxHp = config.MaxHp;
        CurrentHp = config.MaxHp;
        AttackDamage = config.AttackDamage;
        AttackRange = config.AttackRange;
        AttackInterval = config.AttackInterval;
    }

    public override void _Ready()
    {
        CreateVisuals();
        SetupAnimator();
        Log.Info($"[SDUnitBase] Unit created: {GetType().Name}");
    }

    protected virtual void CreateVisuals()
    {
        // 尝试加载 Spine 动画
        if (!string.IsNullOrEmpty(_config.SpinePath))
        {
            CreateSpineVisuals();
        }
        else
        {
            CreateFallbackVisuals();
        }

        // 创建血条
        CreateHpBar();
    }

    protected virtual void CreateSpineVisuals()
    {
        // Spine 动画需要场景实例化，这里使用回退方案
        // 未来可以改为实例化 scenes/creature_visuals/ 下的场景
        CreateFallbackVisuals();
    }

    protected virtual void CreateFallbackVisuals()
    {
        // 创建简单的圆形表示
        var shape = new ColorRect
        {
            CustomMinimumSize = new Vector2(60, 60),
            Position = new Vector2(-30, -60),
            Color = GetUnitColor()
        };
        AddChild(shape);
    }

    protected virtual void CreateHpBar()
    {
        // 血条背景
        _hpBarBg = new ColorRect
        {
            Color = new Color(0.2f, 0.2f, 0.2f),
            Size = new Vector2(50, 6),
            Position = new Vector2(-25, -90)
        };
        AddChild(_hpBarBg);

        // 血条
        _hpBar = new ColorRect
        {
            Color = Colors.Green,
            Size = new Vector2(50, 6),
            Position = new Vector2(-25, -90)
        };
        AddChild(_hpBar);

        // HP 标签
        _hpLabel = new Label
        {
            Position = new Vector2(-25, -105),
            Size = new Vector2(50, 20),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _hpLabel.AddThemeColorOverride("font_color", Colors.White);
        AddChild(_hpLabel);

        UpdateHpDisplay();
    }

    protected virtual void SetupAnimator()
    {
        if (_spineSprite == null) return;

        // 创建动画状态机
        var idleState = new AnimState("idle_loop", isLooping: true);
        _animator = new CreatureAnimator(idleState, _spineSprite);

        // 添加攻击动画过渡
        var attackState = new AnimState("attack", isLooping: false);
        attackState.NextState = idleState;
        _animator.AddAnyState(CreatureAnimator.attackTrigger, attackState);
    }

    protected virtual Color GetUnitColor() => Colors.Blue;

    public override void _Process(double delta)
    {
        if (IsDead) return;

        // 更新攻击计时
        _attackTimer += (float)delta;
        if (_attackTimer >= AttackInterval)
        {
            _canAttack = true;
        }

        // 寻找目标
        FindTarget();

        // 攻击
        if (_canAttack && _target != null)
        {
            Attack();
            _canAttack = false;
            _attackTimer = 0;
        }
    }

    protected virtual void FindTarget()
    {
        var waveManager = SDGame.Instance?.GetNode<SDWaveManager>("%WaveManager");
        if (waveManager == null) return;

        // 获取当前行的怪物
        var row = GridPosition.Y;
        _target = waveManager.GetClosestMonsterInRow(row, Position.X);
    }

    protected virtual void Attack()
    {
        if (_target == null || _target.IsDead)
        {
            _target = null;
            return;
        }

        // 检查距离
        float distance = Mathf.Abs(_target.Position.X - Position.X);
        if (distance > AttackRange) return;

        // 播放攻击动画
        _animator?.SetTrigger(CreatureAnimator.attackTrigger);

        // 造成伤害
        _target.TakeDamage(AttackDamage);
        Log.Info($"[SDUnitBase] {GetType().Name} attacked for {AttackDamage} damage");
    }

    public virtual void TakeDamage(int damage)
    {
        CurrentHp -= damage;
        UpdateHpDisplay();

        // 播放受击动画
        _animator?.SetTrigger(CreatureAnimator.hitTrigger);

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Log.Info($"[SDUnitBase] {GetType().Name} died");

        // 播放死亡动画
        _animator?.SetTrigger(CreatureAnimator.deathTrigger);

        // 从网格移除
        var grid = SDGame.Instance?.Grid;
        if (grid != null)
        {
            grid.RemoveUnit(GridPosition.Y, GridPosition.X);
        }

        // 延迟删除（等待动画）
        GetTree().CreateTimer(0.5).Timeout += () => QueueFree();
    }

    protected void UpdateHpDisplay()
    {
        if (_hpLabel != null)
        {
            _hpLabel.Text = $"{CurrentHp}/{MaxHp}";
        }

        if (_hpBar != null)
        {
            float ratio = (float)CurrentHp / MaxHp;
            _hpBar.Size = new Vector2(50 * ratio, 6);
            _hpBar.Color = ratio > 0.5f ? Colors.Green : ratio > 0.25f ? Colors.Yellow : Colors.Red;
        }
    }

    /// <summary>
    /// 获取卡牌图标路径
    /// </summary>
    public virtual string GetCardIconPath()
    {
        return UnitType switch
        {
            SDUnitType.Ironclad => "res://images/atlases/ui_atlas.sprites/card/energy_ironclad.tres",
            SDUnitType.Silent => "res://images/atlases/ui_atlas.sprites/card/energy_silent.tres",
            SDUnitType.Defect => "res://images/atlases/ui_atlas.sprites/card/energy_defect.tres",
            _ => null
        };
    }
}
