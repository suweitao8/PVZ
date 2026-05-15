using Godot;

namespace MegaCrit.Sts2.SpireDefense.Entities.Monsters;

public partial class SDLagavulinMonster : SDMonsterBase
{
    public SDLagavulinMonster(MonsterConfig config) : base(config) { }
    protected override Color GetMonsterColor() => new Color(0.5f, 0.5f, 0.6f);
}