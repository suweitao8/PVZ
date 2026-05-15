namespace MegaCrit.Sts2.SpireDefense.Core;

/// <summary>
/// 防守单位类型枚举
/// 对应 STS2 中的角色
/// </summary>
public enum SDUnitType
{
    Ironclad,    // 铁卫 - 基于 Ironclad，高血量坦克
    Silent,      // 影弓 - 基于 Silent，远程弓手
    Defect,      // 构造体 - 基于 Defect，法术单位
    Necrobinder, // 死灵师 - 基于 Necrobinder，召唤师
    Regent       // 摄政王 - 基于 Regent，精英单位
}
