using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Wisp : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<DynamicVar>(new EnergyVar(1));

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<IHoverTip>(base.EnergyHoverTip);

	public Wisp()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Retain);
	}
}
