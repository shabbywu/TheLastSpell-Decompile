using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit;

public class StatModifierDefinition : Definition
{
	public static class Constants
	{
		public const string Id = "StatModifier";
	}

	public float FlatModifier { get; set; }

	public float PercentageModifier { get; set; }

	public StatModifierDefinition(float flatModifier, float percentageModifier)
		: base((XContainer)null, (Dictionary<string, string>)null)
	{
		FlatModifier = flatModifier;
		PercentageModifier = percentageModifier;
	}

	public StatModifierDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (container != null)
		{
			XElement obj = ((XContainer)val).Element(XName.op_Implicit("PercentageModifier"));
			string text = ((obj != null) ? obj.Value : null);
			XElement obj2 = ((XContainer)val).Element(XName.op_Implicit("FlatModifier"));
			string text2 = ((obj2 != null) ? obj2.Value : null);
			PercentageModifier = ((text != null) ? ((float)int.Parse(text)) : 100f);
			FlatModifier = ((text2 != null) ? ((float)int.Parse(text2)) : 0f);
		}
	}
}
