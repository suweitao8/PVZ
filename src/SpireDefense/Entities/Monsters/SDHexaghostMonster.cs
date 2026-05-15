using Godot;

namespace MegaCrit.Sts2.SpireDefense.Entities.Monsters;

public partial class SDHexaghostMonster : SDMonsterBase
{
    public SDHexaghostMonster(MonsterConfig config) : base(config) { }
    protected override Color GetMonsterColor() => new Color(0.8f, 0.2f, 0.2f);
}