using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class CeremonialBeastBoss : EncounterModel
{
	public override RoomType RoomType => RoomType.Boss;

	public override string CustomBgm => "event:/music/act1_boss_ceremonial_beast";

	protected override bool HasCustomBackground => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<CeremonialBeast>());

	public override IEnumerable<string>? ExtraAssetPaths => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<string>(ModelDb.Affliction<Ringing>().OverlayPath);

	public override float GetCameraScaling()
	{
		return 0.9f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 50f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<CeremonialBeast>().ToMutable(), null));
	}
}
