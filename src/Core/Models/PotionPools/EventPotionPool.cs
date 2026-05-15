using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Potions;

namespace MegaCrit.Sts2.Core.Models.PotionPools;

public sealed class EventPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "colorless";

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlyArray<PotionModel>(new PotionModel[2]
		{
			ModelDb.Potion<FoulPotion>(),
			ModelDb.Potion<GlowwaterPotion>()
		});
	}
}
