using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.SpireDefense.Entities.Monsters;

/// <summary>
/// 怪物类型枚举
/// </summary>
public enum SDMonsterType
{
    Cultist,      // 教徒
    Gremlin,      // 哥布林
    Slaver,       // 奴役者
    FungiBeast,   // 真菌兽
    Lagavulin,    // 拉加
    Hexaghost     // 六角幽灵 (Boss)
}

/// <summary>
/// 怪物工厂
/// </summary>
public static class SDMonsterFactory
{
    private static readonly Dictionary<SDMonsterType, MonsterConfig> MonsterConfigs = new()
    {
        [SDMonsterType.Cultist] = new MonsterConfig
        {
            MaxHp = 80,
            MoveSpeed = 30f,
            AttackDamage = 10,
            AttackRange = 50f,
            AttackInterval = 1.5f
        },
        [SDMonsterType.Gremlin] = new MonsterConfig
        {
            MaxHp = 40,
            MoveSpeed = 60f,
            AttackDamage = 8,
            AttackRange = 40f,
            AttackInterval = 1.0f
        },
        [SDMonsterType.Slaver] = new MonsterConfig
        {
            MaxHp = 150,
            MoveSpeed = 25f,
            AttackDamage = 15,
            AttackRange = 50f,
            AttackInterval = 2.0f
        },
        [SDMonsterType.FungiBeast] = new MonsterConfig
        {
            MaxHp = 100,
            MoveSpeed = 20f,
            AttackDamage = 12,
            AttackRange = 60f,
            AttackInterval = 1.8f
        },
        [SDMonsterType.Lagavulin] = new MonsterConfig
        {
            MaxHp = 300,
            MoveSpeed = 15f,
            AttackDamage = 25,
            AttackRange = 80f,
            AttackInterval = 3.0f
        },
        [SDMonsterType.Hexaghost] = new MonsterConfig
        {
            MaxHp = 500,
            MoveSpeed = 20f,
            AttackDamage = 20,
            AttackRange = 100f,
            AttackInterval = 2.5f
        }
    };

    public static SDMonsterBase Create(SDMonsterType type)
    {
        if (!MonsterConfigs.TryGetValue(type, out var config))
        {
            Log.Error($"[SDMonsterFactory] Unknown monster type: {type}");
            return null;
        }

        return type switch
        {
            SDMonsterType.Cultist => new SDCultistMonster(config),
            SDMonsterType.Gremlin => new SDGremlinMonster(config),
            SDMonsterType.Slaver => new SDSlaverMonster(config),
            SDMonsterType.FungiBeast => new SDFungiBeastMonster(config),
            SDMonsterType.Lagavulin => new SDLagavulinMonster(config),
            SDMonsterType.Hexaghost => new SDHexaghostMonster(config),
            _ => new SDMonsterBase(config)
        };
    }
}

/// <summary>
/// 怪物配置
/// </summary>
public class MonsterConfig
{
    public int MaxHp { get; set; }
    public float MoveSpeed { get; set; }
    public int AttackDamage { get; set; }
    public float AttackRange { get; set; }
    public float AttackInterval { get; set; }
}
