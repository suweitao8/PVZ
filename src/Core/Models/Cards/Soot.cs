using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Soot : CardModel
{
	public override bool CanBeGeneratedInCombat => false;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

	public Soot()
		: base(-1, CardType.Status, CardRarity.Status, TargetType.None)
	{
	}
}
