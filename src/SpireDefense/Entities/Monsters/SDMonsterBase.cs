using System;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.SpireDefense.Core;
using MegaCrit.Sts2.SpireDefense.Entities.Units;

namespace MegaCrit.Sts2.SpireDefense.Entities.Monsters;

/// <summary>
/// 怪物基类
/// 所有进攻怪物的基础实现
/// </summary>
public partial class SDMonsterBase : Control
{
    // 属性
    public int MaxHp { get; protected set; }
    public int CurrentHp { get; protected set; }
    public float MoveSpeed { get; protected set; }
    public int AttackDamage { get; protected set; }
    public float AttackRange { get; protected set; }
    public float AttackInterval { get; protected set; }

    // 状态
    public int Row { get; set; }
    public bool IsDead => CurrentHp <= 0;
    protected bool _isAttacking;

    // 攻击计时
    protected float _attackTimer;
    protected bool _canAttack = true;

    // 目标
    protected SDUnitBase _target;

    // 视觉
    protected Control _body;
    protected Label _hpLabel;
    protected ColorRect _hpBar;

    // 配置
    protected MonsterConfig _config;

    // 事件
    public event Action Died;

    public SDMonsterBase(MonsterConfig config)
    {
        _config = config;
        MaxHp = config.MaxHp;
        CurrentHp = config.MaxHp;
        MoveSpeed = config.MoveSpeed;
        AttackDamage = config.AttackDamage;
        AttackRange = config.AttackRange;
        AttackInterval = config.AttackInterval;
    }

    public override void _Ready()
    {
        CreateVisuals();
    }

    protected virtual void CreateVisuals()
    {
        // 创建怪物主体
        _body = new Control();
        AddChild(_body);

        // 创建简单的矩形表示（后续替换为 Spine）
        var shape = new ColorRect
        {
            CustomMinimumSize = new Vector2(50, 70),
            Position = new Vector2(-25, -35),
            Color = GetMonsterColor()
        };
        _body.AddChild(shape);

        // 血条
        _hpBar = new ColorRect
        {
            Color = Colors.Red,
            Size = new Vector2(40, 4),
            Position = new Vector2(-20, -45)
        };
        AddChild(_hpBar);

        // HP 标签
        _hpLabel = new Label
        {
            Position = new Vector2(-25, -60),
            Size = new Vector2(50, 15),
            HorizontalAlignment = HorizontalAlignment.Center,
            Scale = new Vector2(0.8f, 0.8f)
        };
        _hpLabel.AddThemeColorOverride("font_color", Colors.White);
        AddChild(_hpLabel);

        UpdateHpDisplay();
    }

    protected virtual Color GetMonsterColor() => Colors.Red;

    public void UpdateMonster(float delta)
    {
        if (IsDead) return;

        // 寻找目标
        FindTarget();

        if (_target != null && !_target.IsDead)
        {
            // 在攻击范围内
            float distance = Mathf.Abs(_target.Position.X - Position.X);
            if (distance <= AttackRange)
            {
                // 攻击
                _attackTimer += delta;
                if (_attackTimer >= AttackInterval)
                {
                    Attack();
                    _attackTimer = 0;
                }
            }
            else
            {
                // 移动
                Move(delta);
            }
        }
        else
        {
            // 没有目标，继续移动
            Move(delta);
        }
    }

    protected virtual void FindTarget()
    {
        var grid = SDGame.Instance?.Grid;
        if (grid == null) return;

        // 获取当前行的所有单位
        var units = grid.GetUnitsInRow(Row);
        SDUnitBase closest = null;
        float minDistance = float.MaxValue;

        foreach (var unit in units)
        {
            if (!unit.IsDead)
            {
                float distance = Mathf.Abs(unit.Position.X - Position.X);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = unit;
                }
            }
        }

        _target = closest;
    }

    protected virtual void Move(float delta)
    {
        // 向左移动
        Position = new Vector2(
            Position.X - MoveSpeed * delta,
            Position.Y
        );
    }

    protected virtual void Attack()
    {
        if (_target == null || _target.IsDead)
        {
            _target = null;
            return;
        }

        // 造成伤害
        _target.TakeDamage(AttackDamage);

        // TODO: 播放攻击动画
        // TODO: 播放攻击特效
    }

    public virtual void TakeDamage(int damage)
    {
        CurrentHp -= damage;
        UpdateHpDisplay();

        // TODO: 播放受击动画

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // TODO: 播放死亡动画

        Died?.Invoke();
        Log.Info($"[SDMonsterBase] Monster died");
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
            _hpBar.Size = new Vector2(40 * ratio, 4);
        }
    }
}
