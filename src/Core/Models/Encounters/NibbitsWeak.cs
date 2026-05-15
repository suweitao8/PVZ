using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class NibbitsWeak : EncounterModel
{
	public override IEnumerable<EncounterTag> Tags => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<EncounterTag>(EncounterTag.Nibbit);

	public override RoomType RoomType => RoomType.Monster;

	public override bool IsWeak => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<Nibbit>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		Nibbit nibbit = (Nibbit)ModelDb.Monster<Nibbit>().ToMutable();
		nibbit.IsAlone = true;
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<(MonsterModel, string)>((nibbit, null));
	}
}
