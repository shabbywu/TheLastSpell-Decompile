using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class FogModifierMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "FogModifier";

	public int IncreaseEveryXDays { get; private set; } = -1;


	public int InitialDensityIndex { get; private set; } = -1;


	public FogModifierMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("IncreaseEveryXDays"));
		if (val2 != null)
		{
			if (!int.TryParse(val2.Value, out var result))
			{
				Debug.LogError((object)("Could not parse " + val2.Value + " to a correct value!"));
				return;
			}
			IncreaseEveryXDays = result;
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("InitialDensityIndex"));
		if (val3 != null)
		{
			if (int.TryParse(val3.Value, out var result2))
			{
				InitialDensityIndex = result2;
			}
			else
			{
				Debug.LogError((object)("Could not parse " + val3.Value + " to a correct value!"));
			}
		}
	}
}
