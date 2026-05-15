using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class TheObscuraNormal : EncounterModel
{
	public const string illusionSlot = "illusion";

	private const string _obscuraSlot = "obscura";

	public override RoomType RoomType => RoomType.Monster;

	public override IReadOnlyList<string> Slots => new global::MegaCrit.Sts2.Core.Collections.ReadOnlyArray<string>(new string[2] { "illusion", "obscura" });

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<TheObscura>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<TheObscura>().ToMutable(), "obscura"));
	}
}
