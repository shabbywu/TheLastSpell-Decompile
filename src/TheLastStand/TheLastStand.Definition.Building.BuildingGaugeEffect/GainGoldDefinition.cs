using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingGaugeEffect;

public class GainGoldDefinition : BuildingGaugeEffectDefinition
{
	public const string Name = "GainGold";

	public Node GoldGain { get; private set; }

	public GainGoldDefinition(XContainer container)
		: base("GainGold", container)
	{
	}

	public override BuildingGaugeEffectDefinition Clone()
	{
		GainGoldDefinition obj = base.Clone() as GainGoldDefinition;
		obj.GoldGain = GoldGain.Clone();
		return obj;
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("Gold"));
		if (val.IsNullOrEmpty())
		{
			Debug.LogError((object)"BuildingGaugeEffectDefinition must have Golds");
		}
		else
		{
			GoldGain = Parser.Parse(val.Value);
		}
	}
}
