using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class GlobeHeadNormal : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<GlobeHead>());

	public override IEnumerable<string>? ExtraAssetPaths => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<string>(ModelDb.Affliction<Galvanized>().OverlayPath);

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<GlobeHead>().ToMutable(), null));
	}
}
