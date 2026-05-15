using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaSlotNode : MegaSpineBinding
{
	protected override string SpineClassName => "SpineSlotNode";

	protected override IEnumerable<string> SpineMethods => new global::MegaCrit.Sts2.Core.Collections.ReadOnlySingleElementList<string>("get_normal_material");

	public MegaSlotNode(Variant native)
		: base(native)
	{
	}

	public Material? GetNormalMaterial()
	{
		return CallNullable("get_normal_material")?.As<Material>();
	}
}
