using Godot;

namespace MegaCrit.Sts2.SpireDefense.Entities.Monsters;

public partial class SDFungiBeastMonster : SDMonsterBase
{
    public SDFungiBeastMonster(MonsterConfig config) : base(config) { }
    protected override Color GetMonsterColor() => new Color(0.4f, 0.6f, 0.4f);
}
