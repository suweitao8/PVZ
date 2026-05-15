using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Writhe : CardModel
{
	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::MegaCrit.Sts2.Core.Collections.ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Innate,
		CardKeyword.Unplayable
	});

	public Writhe()
		: base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}
}
