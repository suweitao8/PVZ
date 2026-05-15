using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.RelicPools;

public sealed class DeprecatedRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "colorless";

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<DeprecatedRelic>());
	}
}
