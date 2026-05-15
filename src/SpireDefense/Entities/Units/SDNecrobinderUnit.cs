using Godot;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.Entities.Units;

/// <summary>
/// 死灵师单位 - 基于 Necrobinder
/// 可召唤骷髅助战
/// </summary>
public partial class SDNecrobinderUnit : SDUnitBase
{
    public SDNecrobinderUnit(UnitConfig config) : base(config)
    {
    }

    protected override Color GetUnitColor() => new Color(0.5f, 0.3f, 0.6f); // 紫色
}
