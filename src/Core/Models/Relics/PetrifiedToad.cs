using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Potions;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PetrifiedToad : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPotion<PotionShapedRock>());

	public override async Task BeforeCombatStartLate()
	{
		Flash();
		await PotionCmd.TryToProcure<PotionShapedRock>(base.Owner);
	}
}
