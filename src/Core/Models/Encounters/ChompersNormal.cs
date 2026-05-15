using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class ChompersNormal : EncounterModel
{
	public override IEnumerable<EncounterTag> Tags => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<EncounterTag>(EncounterTag.Chomper);

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<Chomper>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		Chomper item = (Chomper)ModelDb.Monster<Chomper>().ToMutable();
		Chomper chomper = (Chomper)ModelDb.Monster<Chomper>().ToMutable();
		chomper.ScreamFirst = true;
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(item, null),
			(chomper, null)
		});
	}
}
