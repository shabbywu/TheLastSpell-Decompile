using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingGaugeEffect;

public class GainMaterialsDefinition : BuildingGaugeEffectDefinition
{
	public const string Name = "GainMaterials";

	public Node MaterialsGain { get; private set; }

	public GainMaterialsDefinition(XContainer container)
		: base("GainMaterials", container)
	{
	}

	public override BuildingGaugeEffectDefinition Clone()
	{
		GainMaterialsDefinition obj = base.Clone() as GainMaterialsDefinition;
		obj.MaterialsGain = MaterialsGain.Clone();
		return obj;
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("Materials"));
		if (val.IsNullOrEmpty())
		{
			Debug.LogError((object)"BuildingGaugeEffectDefinition must have Materials");
			return;
		}
		float result = 1f;
		XAttribute val2 = val.Attribute(XName.op_Implicit("Proba"));
		if (val2 != null && !float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
		{
			Debug.LogError((object)"Materials must have a valid Proba");
		}
		else
		{
			MaterialsGain = Parser.Parse(val.Value);
		}
	}
}
