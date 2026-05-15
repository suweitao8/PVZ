using Godot;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.Entities.Units;

/// <summary>
/// 影弓单位 - 基于 Silent
/// 远程攻击，可叠加中毒
/// </summary>
public partial class SDSilentUnit : SDUnitBase
{
    public SDSilentUnit(UnitConfig config) : base(config)
    {
        // Silent 特殊属性
        // 远程攻击，中毒效果
    }

    protected override Color GetUnitColor() => new Color(0.3f, 0.6f, 0.4f); // 绿色

    protected override void Attack()
    {
        // Silent 的攻击附加中毒
        base.Attack();

        // TODO: 添加中毒效果
    }
}
