using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class GremlinMercNormal : EncounterModel
{
	public const string mercSlot = "merc";

	public const string sneakySlot = "sneaky";

	public const string fatSlot = "fat";

	public override RoomType RoomType => RoomType.Monster;

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::MegaCrit.Sts2.Core.Collections.ReadOnlyArray<MonsterModel>(new MonsterModel[3]
	{
		ModelDb.Monster<GremlinMerc>(),
		ModelDb.Monster<FatGremlin>(),
		ModelDb.Monster<SneakyGremlin>()
	});

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<GremlinMerc>().ToMutable(), "merc"));
	}
}
