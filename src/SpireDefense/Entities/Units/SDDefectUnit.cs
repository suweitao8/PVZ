using Godot;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.Entities.Units;

/// <summary>
/// 构造体单位 - 基于 Defect
/// 法师类型，可充能后爆发
/// </summary>
public partial class SDDefectUnit : SDUnitBase
{
    public SDDefectUnit(UnitConfig config) : base(config)
    {
        // Defect 特殊属性
        // 电弧攻击，充能机制
    }

    protected override Color GetUnitColor() => new Color(0.3f, 0.5f, 0.8f); // 蓝色

    protected override void Attack()
    {
        // Defect 的电弧攻击
        base.Attack();

        // TODO: 添加电弧特效
    }
}
