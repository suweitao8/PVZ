namespace MegaCrit.Sts2.PvZ.Core;

/// <summary>
/// PvZ 迷你游戏状态枚举
/// </summary>
public enum PvZGameState
{
    /// <summary>
    /// 准备阶段 - 玩家可以放置植物
    /// </summary>
    Preparing,

    /// <summary>
    /// 战斗进行中
    /// </summary>
    Playing,

    /// <summary>
    /// 游戏暂停
    /// </summary>
    Paused,

    /// <summary>
    /// 胜利 - 所有波次完成
    /// </summary>
    Victory,

    /// <summary>
    /// 失败 - 僵尸到达终点
    /// </summary>
    Defeat
}

/// <summary>
/// 植物类型枚举
/// </summary>
public enum PlantType
{
    Sunflower,      // 向日葵
    Peashooter,     // 豌豆射手
    Wallnut,        // 坚果墙
    SnowPea,        // 寒冰射手
    CherryBomb,     // 樱桃炸弹
    Repeater,       // 双发射手
    Chomper         // 大嘴花
}

/// <summary>
/// 僵尸类型枚举
/// </summary>
public enum ZombieType
{
    Basic,          // 普通僵尸
    Conehead,       // 路障僵尸
    Buckethead,     // 铁桶僵尸
    Runner,         // 跑步僵尸
    Gargantuar      // 巨人僵尸
}
