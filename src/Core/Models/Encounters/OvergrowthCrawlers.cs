using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class OvergrowthCrawlers : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<EncounterTag> Tags => new global::MegaCrit.Sts2.Core.Collections.ReadOnlyArray<EncounterTag>(new EncounterTag[2]
	{
		EncounterTag.Shrinker,
		EncounterTag.Crawler
	});

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::MegaCrit.Sts2.Core.Collections.ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<ShrinkerBeetle>(),
		ModelDb.Monster<FuzzyWurmCrawler>()
	});

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(ModelDb.Monster<ShrinkerBeetle>().ToMutable(), null),
			(ModelDb.Monster<FuzzyWurmCrawler>().ToMutable(), null)
		});
	}
}
