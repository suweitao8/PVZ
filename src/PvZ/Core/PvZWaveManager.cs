using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.PvZ.Entities.Zombies;

namespace MegaCrit.Sts2.PvZ.Core;

/// <summary>
/// 波次配置
/// </summary>
public class WaveConfig
{
    public int WaveNumber { get; set; }
    public List<(ZombieType type, int count, int row)> Zombies { get; set; } = new();
}

/// <summary>
/// PvZ 波次管理器
/// 管理僵尸波次的生成
/// </summary>
public partial class PvZWaveManager : Node
{
    #region Configuration

    /// <summary>
    /// 基础僵尸数量（第一波）
    /// </summary>
    [Export]
    public int BaseZombieCount { get; set; } = 3;

    /// <summary>
    /// 每波僵尸数量增量
    /// </summary>
    [Export]
    public int ZombieCountPerWave { get; set; } = 2;

    /// <summary>
    /// 僵尸生成间隔（秒）
    /// </summary>
    [Export]
    public float SpawnInterval { get; set; } = 2.0f;

    #endregion

    #region State

    private readonly List<ZombieBase> _activeZombies = new();
    private readonly Queue<(ZombieType type, int row)> _spawnQueue = new();
    private float _spawnTimer;
    private bool _isSpawning;

    #endregion

    #region References

    private Node2D _zombieContainer = null!;

    #endregion

    public override void _Ready()
    {
        _zombieContainer = GetNode<Node2D>("../ZombieContainer");
        Log.Info("[PvZ] Wave manager initialized");
    }

    public override void _Process(double delta)
    {
        if (!_isSpawning || _spawnQueue.Count == 0)
            return;

        _spawnTimer -= (float)delta;

        if (_spawnTimer <= 0)
        {
            SpawnNextZombie();
            _spawnTimer = SpawnInterval;

            if (_spawnQueue.Count == 0)
            {
                _isSpawning = false;
            }
        }
    }

    #region Wave Spawning

    /// <summary>
    /// 生成指定波次的僵尸
    /// </summary>
    public void SpawnWave(int waveNumber)
    {
        var config = GenerateWaveConfig(waveNumber);
        _spawnQueue.Clear();

        foreach (var (type, count, row) in config.Zombies)
        {
            for (int i = 0; i < count; i++)
            {
                _spawnQueue.Enqueue((type, row >= 0 ? row : GetRandomRow()));
            }
        }

        _spawnTimer = 0f;
        _isSpawning = true;

        Log.Info($"[PvZ] Wave {waveNumber}: {_spawnQueue.Count} zombies queued");
    }

    /// <summary>
    /// 生成波次配置
    /// </summary>
    private WaveConfig GenerateWaveConfig(int waveNumber)
    {
        var config = new WaveConfig { WaveNumber = waveNumber };

        int totalZombies = BaseZombieCount + waveNumber * ZombieCountPerWave;
        int totalWaves = PvZGameManager.Instance.TotalWaves;

        // 根据波次决定僵尸类型分布
        for (int i = 0; i < totalZombies; i++)
        {
            ZombieType type = GetZombieTypeForWave(waveNumber, totalWaves);
            int row = GetRandomRow();
            config.Zombies.Add((type, 1, row));
        }

        // 后期波次添加特殊僵尸
        if (waveNumber >= 5)
        {
            // 添加路障僵尸
            config.Zombies.Add((ZombieType.Conehead, 1, GetRandomRow()));
        }

        if (waveNumber >= 7)
        {
            // 添加铁桶僵尸
            config.Zombies.Add((ZombieType.Buckethead, 1, GetRandomRow()));
        }

        if (waveNumber >= 9)
        {
            // 添加巨人僵尸
            config.Zombies.Add((ZombieType.Gargantuar, 1, 2));
        }

        return config;
    }

    private ZombieType GetZombieTypeForWave(int wave, int totalWaves)
    {
        float progress = (float)wave / totalWaves;

        if (progress >= 0.8f)
        {
            // 20% 几率跑步僵尸
            return new Random().NextDouble() < 0.2 ? ZombieType.Runner : ZombieType.Basic;
        }
        else if (progress >= 0.5f)
        {
            // 15% 几率路障僵尸
            return new Random().NextDouble() < 0.15 ? ZombieType.Conehead : ZombieType.Basic;
        }

        return ZombieType.Basic;
    }

    private int GetRandomRow()
    {
        return new Random().Next(0, 5);
    }

    private void SpawnNextZombie()
    {
        if (_spawnQueue.Count == 0)
            return;

        var (type, row) = _spawnQueue.Dequeue();
        SpawnZombie(type, row);
    }

    #endregion

    #region Zombie Management

    /// <summary>
    /// 在指定行生成僵尸
    /// </summary>
    public void SpawnZombie(ZombieType type, int row)
    {
        var zombie = CreateZombie(type, row);
        if (zombie == null)
            return;

        _activeZombies.Add(zombie);
        _zombieContainer.AddChild(zombie);

        // 僵尸死亡时移除
        zombie.TreeExited += () => _activeZombies.Remove(zombie);

        Log.Info($"[PvZ] Spawned {type} at row {row}");
    }

    private ZombieBase? CreateZombie(ZombieType type, int row)
    {
        var gridManager = PvZGameManager.Instance.GridManager;
        if (gridManager == null)
            return null;

        // 僵尸从右侧出生
        Vector2 spawnPos = new Vector2(
            gridManager.GridOffset.X + gridManager.Columns * gridManager.CellWidth + 50,
            gridManager.GridOffset.Y + row * gridManager.CellHeight + gridManager.CellHeight / 2
        );

        ZombieBase? zombie = type switch
        {
            ZombieType.Basic => new PvZBasicZombie(),
            ZombieType.Conehead => new PvZConeheadZombie(),
            ZombieType.Buckethead => new PvZBucketheadZombie(),
            ZombieType.Runner => new PvZRunnerZombie(),
            ZombieType.Gargantuar => new PvZGargantuarZombie(),
            _ => null
        };

        if (zombie != null)
        {
            zombie.Position = spawnPos;
            zombie.Setup(row);
        }

        return zombie;
    }

    /// <summary>
    /// 获取指定行的所有僵尸
    /// </summary>
    public List<ZombieBase> GetZombiesInRow(int row)
    {
        var zombies = new List<ZombieBase>();
        foreach (var zombie in _activeZombies)
        {
            if (zombie.Row == row && !zombie.IsDead)
            {
                zombies.Add(zombie);
            }
        }
        return zombies;
    }

    /// <summary>
    /// 检查是否所有僵尸都已清除
    /// </summary>
    public bool AreAllZombiesCleared()
    {
        return !_isSpawning && _activeZombies.Count == 0;
    }

    #endregion
}
