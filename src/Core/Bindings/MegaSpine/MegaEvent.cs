using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaEvent : MegaSpineBinding
{
	protected override string SpineClassName => "SpineEvent";

	protected override IEnumerable<string> SpineMethods => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<string>("get_data");

	public MegaEvent(Variant native)
		: base(native)
	{
	}

	public MegaEventData GetData()
	{
		return new MegaEventData(Call("get_data"));
	}
}
