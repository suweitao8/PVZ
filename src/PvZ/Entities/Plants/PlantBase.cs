using System;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.PvZ.Core;

namespace MegaCrit.Sts2.PvZ.Entities.Plants;

/// <summary>
/// 植物基类
/// 所有植物继承此类
/// </summary>
public abstract partial class PlantBase : Node2D
{
    #region Properties

    /// <summary>
    /// 植物类型
    /// </summary>
    public abstract PlantType Type { get; }

    /// <summary>
    /// 阳光消耗
    /// </summary>
    public abstract int SunCost { get; }

    /// <summary>
    /// 冷却时间（秒）
    /// </summary>
    public abstract float Cooldown { get; }

    /// <summary>
    /// 所在行
    /// </summary>
    public int Row { get; private set; }

    /// <summary>
    /// 所在列
    /// </summary>
    public int Col { get; private set; }

    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized { get; private set; }

    #endregion

    #region Configuration

    /// <summary>
    /// 植物碰撞区域大小
    /// </summary>
    protected Vector2 HitboxSize { get; set; } = new Vector2(60, 80);

    #endregion

    /// <summary>
    /// 初始化植物位置
    /// </summary>
    public void Setup(int row, int col)
    {
        Row = row;
        Col = col;
        IsInitialized = true;
        OnPlaced();
    }

    public override void _Ready()
    {
        if (!IsInitialized)
        {
            Log.Warn($"[PvZ] Plant {Type} created without Setup() call");
        }
    }

    #region Abstract Methods

    /// <summary>
    /// 植物被放置时调用
    /// </summary>
    public abstract void OnPlaced();

    /// <summary>
    /// 当僵尸进入同一行时调用
    /// </summary>
    public abstract void OnZombieInRow();

    #endregion

    #region Helpers

    /// <summary>
    /// 获取同一行的所有僵尸
    /// </summary>
    protected PvZ.Entities.Zombies.ZombieBase[] GetZombiesInRow()
    {
        var waveManager = PvZGameManager.Instance?.WaveManager;
        return waveManager?.GetZombiesInRow(Row).ToArray() ?? Array.Empty<PvZ.Entities.Zombies.ZombieBase>();
    }

    /// <summary>
    /// 检查是否有僵尸在本植物右侧
    /// </summary>
    protected bool HasZombieToRight()
    {
        foreach (var zombie in GetZombiesInRow())
        {
            if (zombie.Position.X > Position.X)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取本行最左侧的僵尸
    /// </summary>
    protected PvZ.Entities.Zombies.ZombieBase? GetFirstZombieToRight()
    {
        PvZ.Entities.Zombies.ZombieBase? closest = null;
        float closestDist = float.MaxValue;

        foreach (var zombie in GetZombiesInRow())
        {
            if (zombie.Position.X > Position.X)
            {
                float dist = zombie.Position.X - Position.X;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = zombie;
                }
            }
        }

        return closest;
    }

    #endregion
}
