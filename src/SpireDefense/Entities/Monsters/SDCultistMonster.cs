using Godot;

namespace MegaCrit.Sts2.SpireDefense.Entities.Monsters;

/// <summary>
/// 教徒怪物
/// 基础怪物，可远程攻击
/// </summary>
public partial class SDCultistMonster : SDMonsterBase
{
    public SDCultistMonster(MonsterConfig config) : base(config)
    {
    }

    protected override Color GetMonsterColor() => new Color(0.6f, 0.4f, 0.7f); // 紫色
}
