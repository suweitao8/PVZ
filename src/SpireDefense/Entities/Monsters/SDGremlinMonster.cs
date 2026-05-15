using Godot;

namespace MegaCrit.Sts2.SpireDefense.Entities.Monsters;

/// <summary>
/// 哥布林怪物
/// 快速但脆弱，死亡可能分裂
/// </summary>
public partial class SDGremlinMonster : SDMonsterBase
{
    public SDGremlinMonster(MonsterConfig config) : base(config)
    {
    }

    protected override Color GetMonsterColor() => new Color(0.5f, 0.6f, 0.3f); // 黄绿色

    protected override void Die()
    {
        // TODO: 可能分裂成更小的哥布林
        base.Die();
    }
}
