using Godot;

namespace MegaCrit.Sts2.SpireDefense.Entities.Monsters;

public partial class SDSlaverMonster : SDMonsterBase
{
    public SDSlaverMonster(MonsterConfig config) : base(config) { }
    protected override Color GetMonsterColor() => new Color(0.7f, 0.5f, 0.3f);
}
