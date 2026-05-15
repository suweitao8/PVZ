using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class TanxsWhistle : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Whistle>());

	public override async Task AfterObtained()
	{
		CardModel card = base.Owner.RunState.CreateCard<Whistle>(base.Owner);
		CardCmd.PreviewCardPileAdd(new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<CardPileAddResult>(await CardPileCmd.Add(card, PileType.Deck)), 2f);
	}
}
