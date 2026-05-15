using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters.Mocks;

public sealed class MockTwoMonsterEncounter : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override bool IsDebugEncounter => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<BigDummy>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(ModelDb.Monster<BigDummy>().ToMutable(), null),
			(ModelDb.Monster<BigDummy>().ToMutable(), null)
		});
	}
}
