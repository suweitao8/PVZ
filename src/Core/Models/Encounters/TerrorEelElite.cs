using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class TerrorEelElite : EncounterModel
{
	public override RoomType RoomType => RoomType.Elite;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<TerrorEel>());

	public override float GetCameraScaling()
	{
		return 0.9f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<TerrorEel>().ToMutable(), null));
	}
}
