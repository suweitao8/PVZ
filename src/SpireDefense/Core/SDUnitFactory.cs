using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.SpireDefense.Entities.Monsters;
using MegaCrit.Sts2.SpireDefense.Entities.Units;

namespace MegaCrit.Sts2.SpireDefense.Core;

/// <summary>
/// 单位工厂
/// 创建防守单位和进攻怪物
/// </summary>
public static class SDUnitFactory
{
    // 单位配置
    private static readonly Dictionary<SDUnitType, UnitConfig> UnitConfigs = new()
    {
        [SDUnitType.Ironclad] = new UnitConfig
        {
            EnergyCost = 50,
            MaxHp = 150,
            AttackDamage = 20,
            AttackRange = 60f,
            AttackInterval = 1.5f,
            SpinePath = "res://animations/characters/ironclad/ironclad_skel_data.tres"
        },
        [SDUnitType.Silent] = new UnitConfig
        {
            EnergyCost = 100,
            MaxHp = 80,
            AttackDamage = 15,
            AttackRange = 300f,
            AttackInterval = 1.0f,
            SpinePath = "res://animations/characters/silent/silent_skel_data.tres"
        },
        [SDUnitType.Defect] = new UnitConfig
        {
            EnergyCost = 75,
            MaxHp = 100,
            AttackDamage = 25,
            AttackRange = 250f,
            AttackInterval = 2.0f,
            SpinePath = "res://animations/characters/defect/defect_skel_data.tres"
        },
        [SDUnitType.Necrobinder] = new UnitConfig
        {
            EnergyCost = 100,
            MaxHp = 90,
            AttackDamage = 18,
            AttackRange = 200f,
            AttackInterval = 1.8f,
            SpinePath = "res://animations/characters/necrobinder/necrobinder_skel_data.tres"
        },
        [SDUnitType.Regent] = new UnitConfig
        {
            EnergyCost = 150,
            MaxHp = 200,
            AttackDamage = 30,
            AttackRange = 150f,
            AttackInterval = 2.0f,
            SpinePath = "res://animations/characters/regent/regent_skel_data.tres"
        }
    };

    public static SDUnitBase Create(SDUnitType type)
    {
        if (!UnitConfigs.TryGetValue(type, out var config))
        {
            Log.Error($"[SDUnitFactory] Unknown unit type: {type}");
            return null;
        }

        return type switch
        {
            SDUnitType.Ironclad => new SDIroncladUnit(config),
            SDUnitType.Silent => new SDSilentUnit(config),
            SDUnitType.Defect => new SDDefectUnit(config),
            SDUnitType.Necrobinder => new SDNecrobinderUnit(config),
            SDUnitType.Regent => new SDRegentUnit(config),
            _ => new SDUnitBase(config)
        };
    }

    /// <summary>
    /// 获取单位配置
    /// </summary>
    public static UnitConfig GetUnitConfig(SDUnitType type)
    {
        if (!UnitConfigs.TryGetValue(type, out var config))
        {
            Log.Warn($"[SDUnitFactory] Unknown unit type: {type}");
            return null;
        }
        return config;
    }
}

/// <summary>
/// 单位配置
/// </summary>
public class UnitConfig
{
    public int EnergyCost { get; set; }
    public int MaxHp { get; set; }
    public int AttackDamage { get; set; }
    public float AttackRange { get; set; }
    public float AttackInterval { get; set; }
    public string SpinePath { get; set; }
}
