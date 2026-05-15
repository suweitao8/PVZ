using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.SpireDefense.Entities.Monsters;

namespace MegaCrit.Sts2.SpireDefense.Core;

/// <summary>
/// 波次管理器
/// 管理怪物的生成和波次进度
/// </summary>
public partial class SDWaveManager : Control
{
    // 怪物列表
    private readonly List<SDMonsterBase> _monsters = new List<SDMonsterBase>();

    // 怪物容器
    private Node2D _monstersContainer;

    // 当前波次状态
    private int _currentWave;
    private int _monstersSpawned;
    private int _monstersKilled;
    private int _totalMonstersInWave;

    // 生成计时器
    private float _spawnTimer;
    private float _spawnInterval = 2.0f;
    private bool _isSpawning;

    public int MonsterCount => _monsters.Count;

    // 怪物配置
    private static readonly Dictionary<int, WaveConfig> WaveConfigs = new()
    {
        [1] = new WaveConfig { MonsterTypes = new[] { SDMonsterType.Cultist }, Count = 5 },
        [2] = new WaveConfig { MonsterTypes = new[] { SDMonsterType.Cultist, SDMonsterType.Gremlin }, Count = 7 },
        [3] = new WaveConfig { MonsterTypes = new[] { SDMonsterType.Cultist, SDMonsterType.Gremlin }, Count = 9 },
        [4] = new WaveConfig { MonsterTypes = new[] { SDMonsterType.Gremlin, SDMonsterType.Slaver }, Count = 10 },
        [5] = new WaveConfig { MonsterTypes = new[] { SDMonsterType.Slaver, SDMonsterType.FungiBeast }, Count = 12, IsBossWave = true },
        [6] = new WaveConfig { MonsterTypes = new[] { SDMonsterType.FungiBeast, SDMonsterType.Cultist }, Count = 14 },
        [7] = new WaveConfig { MonsterTypes = new[] { SDMonsterType.Slaver, SDMonsterType.Gremlin }, Count = 16 },
        [8] = new WaveConfig { MonsterTypes = new[] { SDMonsterType.FungiBeast, SDMonsterType.Slaver }, Count = 18 },
        [9] = new WaveConfig { MonsterTypes = new[] { SDMonsterType.Lagavulin, SDMonsterType.Cultist }, Count = 20 },
        [10] = new WaveConfig { MonsterTypes = new[] { SDMonsterType.Hexaghost }, Count = 1, IsBossWave = true }
    };

    public override void _Ready()
    {
        _monstersContainer = GetNode<Node2D>("%MonstersContainer");
        Log.Info("[SDWaveManager] Wave manager initialized");
    }

    public void StartWave(int waveNumber)
    {
        _currentWave = waveNumber;
        _monstersSpawned = 0;
        _monstersKilled = 0;
        _isSpawning = true;

        // 获取波次配置
        if (WaveConfigs.TryGetValue(waveNumber, out var config))
        {
            _totalMonstersInWave = config.Count;
        }
        else
        {
            _totalMonstersInWave = waveNumber * 2 + 3;
        }

        Log.Info($"[SDWaveManager] Starting wave {waveNumber} with {_totalMonstersInWave} monsters");
    }

    public override void _Process(double delta)
    {
        if (!_isSpawning) return;

        _spawnTimer -= (float)delta;

        if (_spawnTimer <= 0 && _monstersSpawned < _totalMonstersInWave)
        {
            SpawnMonster();
            _spawnTimer = _spawnInterval;
        }

        // 更新所有怪物
        UpdateMonsters((float)delta);

        // 检查波次完成
        if (_monstersSpawned >= _totalMonstersInWave && _monsters.Count == 0)
        {
            OnWaveComplete();
        }
    }

    private void SpawnMonster()
    {
        // 获取波次配置
        if (!WaveConfigs.TryGetValue(_currentWave, out var config))
        {
            config = new WaveConfig
            {
                MonsterTypes = new[] { SDMonsterType.Cultist, SDMonsterType.Gremlin },
                Count = _totalMonstersInWave
            };
        }

        // 随机选择怪物类型
        var random = new Random();
        var monsterType = config.MonsterTypes[random.Next(config.MonsterTypes.Length)];

        // 创建怪物
        var monster = SDMonsterFactory.Create(monsterType);
        if (monster != null)
        {
            // 设置生成位置（右侧随机行）
            int row = random.Next(5);
            monster.Row = row;
            monster.Position = new Vector2(900, 150 + row * 100);

            _monstersContainer?.AddChild(monster);
            _monsters.Add(monster);

            // 连接死亡信号
            monster.Died += () => OnMonsterDied(monster);

            _monstersSpawned++;
            Log.Info($"[SDWaveManager] Spawned {monsterType} at row {row}");
        }
    }

    private void UpdateMonsters(float delta)
    {
        var monstersToRemove = new List<SDMonsterBase>();

        foreach (var monster in _monsters)
        {
            monster.UpdateMonster(delta);

            // 检查是否突破防线
            if (monster.Position.X < 50)
            {
                Log.Info("[SDWaveManager] Monster reached the end!");
                SDGame.Instance?.OnDefeat();
                return;
            }

            // 检查死亡
            if (monster.IsDead)
            {
                monstersToRemove.Add(monster);
            }
        }

        // 移除死亡的怪物
        foreach (var monster in monstersToRemove)
        {
            _monsters.Remove(monster);
            monster.QueueFree();
        }
    }

    private void OnMonsterDied(SDMonsterBase monster)
    {
        _monstersKilled++;
        Log.Info($"[SDWaveManager] Monster killed ({_monstersKilled}/{_totalMonstersInWave})");
    }

    private void OnWaveComplete()
    {
        _isSpawning = false;
        SDGame.Instance?.OnWaveComplete();
        Log.Info($"[SDWaveManager] Wave {_currentWave} complete!");
    }

    public SDMonsterBase[] GetAllMonsters() => _monsters.ToArray();

    public SDMonsterBase[] GetMonstersInRow(int row)
    {
        var result = new List<SDMonsterBase>();
        foreach (var monster in _monsters)
        {
            if (monster.Row == row)
            {
                result.Add(monster);
            }
        }
        return result.ToArray();
    }

    public SDMonsterBase GetClosestMonsterInRow(int row, float xPosition)
    {
        SDMonsterBase closest = null;
        float minDistance = float.MaxValue;

        foreach (var monster in _monsters)
        {
            if (monster.Row == row && monster.Position.X > xPosition)
            {
                float distance = monster.Position.X - xPosition;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = monster;
                }
            }
        }

        return closest;
    }
}

/// <summary>
/// 波次配置
/// </summary>
public class WaveConfig
{
    public SDMonsterType[] MonsterTypes { get; set; }
    public int Count { get; set; }
    public bool IsBossWave { get; set; }
}
