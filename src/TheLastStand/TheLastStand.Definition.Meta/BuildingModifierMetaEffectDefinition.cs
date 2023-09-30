using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class BuildingModifierMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "BuildingModifier";

	public string BuildingId { get; private set; }

	public int GoldCostReduction { get; private set; }

	public int HealthBonus { get; private set; }

	public int PassiveProductionBonus { get; private set; }

	public sbyte MaxCityInstancesBonus { get; private set; }

	public BuildingModifierMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		BuildingId = val.Attribute(XName.op_Implicit("Id")).Value;
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("GoldCostReduction"));
		if (val2 != null)
		{
			if (!int.TryParse(val2.Value, out var result))
			{
				Debug.LogError((object)"GoldCostReduction element has an invalid value!");
				return;
			}
			GoldCostReduction = result;
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("MaxCityInstancesBonus"));
		if (val3 != null)
		{
			if (!sbyte.TryParse(val3.Value, out var result2))
			{
				Debug.LogError((object)"MaxCityInstancesBonus element has an invalid value!");
				return;
			}
			MaxCityInstancesBonus = result2;
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("HealthBonus"));
		if (val4 != null)
		{
			if (!int.TryParse(val4.Value, out var result3))
			{
				Debug.LogError((object)"HealthBonus element has an invalid value!");
				return;
			}
			HealthBonus = result3;
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("PassiveProductionBonus"));
		if (val5 != null)
		{
			if (int.TryParse(val5.Value, out var result4))
			{
				PassiveProductionBonus = result4;
			}
			else
			{
				Debug.LogError((object)"PassiveProductionBonus element has an invalid value!");
			}
		}
	}

	public override string ToString()
	{
		return "BuildingModifier (" + BuildingId + ")";
	}
}
