using Godot;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.Entities.Units;

/// <summary>
/// 铁卫单位 - 基于 Ironclad
/// 高血量坦克单位
/// </summary>
public partial class SDIroncladUnit : SDUnitBase
{
    public SDIroncladUnit(UnitConfig config) : base(config)
    {
        // Ironclad 特殊属性
        // 更高的血量，近战攻击
    }

    protected override Color GetUnitColor() => new Color(0.8f, 0.3f, 0.2f); // 红色

    protected override void Attack()
    {
        // 铁卫的攻击带有额外的击退效果
        base.Attack();

        // TODO: 添加击退效果
    }
}
