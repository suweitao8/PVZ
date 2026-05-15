using Godot;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.Entities.Units;

/// <summary>
/// 摄政王单位 - 基于 Regent
/// 精英单位，强化相邻单位
/// </summary>
public partial class SDRegentUnit : SDUnitBase
{
    public SDRegentUnit(UnitConfig config) : base(config)
    {
    }

    protected override Color GetUnitColor() => new Color(0.9f, 0.7f, 0.2f); // 金色
}
