using System;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.PvZ.Core;
using MegaCrit.Sts2.PvZ.Entities.Plants;

namespace MegaCrit.Sts2.PvZ.Entities.Zombies;

/// <summary>
/// 僵尸基类
/// 所有僵尸继承此类
/// </summary>
public abstract partial class ZombieBase : Node2D
{
    #region Properties

    /// <summary>
    /// 僵尸类型
    /// </summary>
    public abstract ZombieType Type { get; }

    /// <summary>
    /// 最大生命值
    /// </summary>
    public abstract int MaxHealth { get; }

    /// <summary>
    /// 移动速度（像素/秒）
    /// </summary>
    public abstract float MoveSpeed { get; }

    /// <summary>
    /// 所在行
    /// </summary>
    public int Row { get; private set; }

    /// <summary>
    /// 当前生命值
    /// </summary>
    public int CurrentHealth { get; protected set; }

    /// <summary>
    /// 是否已死亡
    /// </summary>
    public bool IsDead { get; private set; }

    /// <summary>
    /// 是否被减速
    /// </summary>
    public bool IsSlowed { get; private set; }

    /// <summary>
    /// 减速结束时间
    /// </summary>
    private float _slowTimer;

    #endregion

    #region Configuration

    /// <summary>
    /// 攻击距离（像素）
    /// </summary>
    protected float AttackRange { get; set; } = 10f;

    /// <summary>
    /// 攻击间隔（秒）
    /// </summary>
    protected float AttackInterval { get; set; } = 1.0f;

    /// <summary>
    /// 攻击伤害
    /// </summary>
    protected int AttackDamage { get; set; } = 100;

    #endregion

    #region State

    private float _attackTimer;
    private bool _isAttacking;
    private Plants.PlantBase? _targetPlant;

    #endregion

    /// <summary>
    /// 初始化僵尸
    /// </summary>
    public void Setup(int row)
    {
        Row = row;
        CurrentHealth = MaxHealth;
        _attackTimer = AttackInterval;
    }

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }

    public override void _Process(double delta)
    {
        if (IsDead) return;

        // 更新减速状态
        if (IsSlowed)
        {
            _slowTimer -= (float)delta;
            if (_slowTimer <= 0)
            {
                IsSlowed = false;
            }
        }

        // 检查是否到达终点
        CheckReachEnd();

        // 检查是否有植物阻挡
        _targetPlant = GetPlantInFront();

        if (_targetPlant != null)
        {
            // 攻击植物
            AttackPlant((float)delta);
        }
        else
        {
            // 继续移动
            Move((float)delta);
        }
    }

    #region Movement

    protected virtual void Move(float delta)
    {
        float speed = IsSlowed ? MoveSpeed * 0.5f : MoveSpeed;
        Position = new Vector2(Position.X - speed * delta, Position.Y);
    }

    #endregion

    #region Combat

    /// <summary>
    /// 受到伤害
    /// </summary>
    public virtual void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 应用减速效果
    /// </summary>
    public virtual void ApplySlow(float duration = 5.0f)
    {
        IsSlowed = true;
        _slowTimer = duration;
    }

    protected virtual void Die()
    {
        IsDead = true;
        QueueFree();
    }

    private void CheckReachEnd()
    {
        var gridManager = PvZGameManager.Instance?.GridManager;
        if (gridManager == null) return;

        float endX = gridManager.GridOffset.X - 50;

        if (Position.X <= endX)
        {
            PvZGameManager.Instance?.TriggerDefeat();
            Die();
        }
    }

    protected Plants.PlantBase? GetPlantInFront()
    {
        var gridManager = PvZGameManager.Instance?.GridManager;
        if (gridManager == null) return null;

        // 检查当前位置对应的格子
        var gridPos = gridManager.WorldToGrid(Position);
        if (gridPos == null) return null;

        int col = gridPos.Value.col;
        int row = gridPos.Value.row;

        // 检查当前格子和前一个格子
        for (int c = col; c >= 0 && c >= col - 1; c--)
        {
            var cell = gridManager.GetCell(row, c);
            if (cell?.HasPlant == true)
            {
                float plantX = cell.Plant.Position.X;
                if (Position.X - plantX < AttackRange + 40)
                {
                    return cell.Plant;
                }
            }
        }

        return null;
    }

    private void AttackPlant(float delta)
    {
        if (_targetPlant == null) return;

        _attackTimer -= delta;

        if (_attackTimer <= 0)
        {
            // 造成伤害
            if (_targetPlant is PvZWallnut wallnut)
            {
                wallnut.TakeDamage(AttackDamage);
            }
            else
            {
                // 其他植物直接销毁
                _targetPlant.QueueFree();
                PvZGameManager.Instance?.GridManager.RemovePlant(_targetPlant.Row, _targetPlant.Col);
            }

            _attackTimer = AttackInterval;
        }
    }

    #endregion

    #region Drawing

    public override void _Draw()
    {
        // 基础僵尸绘制（子类可以重写）
        Color bodyColor = IsSlowed ? new Color(0.5f, 0.7f, 1.0f) : new Color(0.4f, 0.6f, 0.3f);

        // 身体
        DrawRect(new Rect2(-15, -30, 30, 50), bodyColor);
        // 头部
        DrawCircle(new Vector2(0, -40), 15, new Color(0.6f, 0.8f, 0.5f));
        // 眼睛
        DrawCircle(new Vector2(-5, -42), 4, new Color(1, 0, 0));
        DrawCircle(new Vector2(5, -42), 4, new Color(1, 0, 0));

        // 生命条
        float healthPercent = (float)CurrentHealth / MaxHealth;
        DrawRect(new Rect2(-20, -60, 40, 5), new Color(0.3f, 0.3f, 0.3f));
        DrawRect(new Rect2(-20, -60, 40 * healthPercent, 5), new Color(1, 0, 0));
    }

    #endregion
}
