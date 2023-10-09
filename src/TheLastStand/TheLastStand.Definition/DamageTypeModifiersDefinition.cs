using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class DamageTypeModifiersDefinition : TheLastStand.Framework.Serialization.Definition
{
	public float MeleeArmorShreddingBonus { get; private set; }

	public Dictionary<int, float> DodgeMultiplierByDistance { get; private set; }

	public DamageTypeModifiersDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = container.Element(XName.op_Implicit("DamageTypeModifiersDefinition"));
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Melee"));
		if (val2 != null)
		{
			XElement val3 = ((XContainer)val2).Element(XName.op_Implicit("ArmorShredding"));
			if (!float.TryParse(val3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log((object)("Could not parse " + val3.Value + " to a valid float value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			MeleeArmorShreddingBonus = result;
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Range"));
		if (val4 == null)
		{
			return;
		}
		XElement obj = ((XContainer)val4).Element(XName.op_Implicit("DodgeMultipliersByDistance"));
		DodgeMultiplierByDistance = new Dictionary<int, float>();
		int num = -1;
		foreach (XElement item in ((XContainer)obj).Elements(XName.op_Implicit("Multiplier")))
		{
			XAttribute val5 = item.Attribute(XName.op_Implicit("StartingDistance"));
			XAttribute val6 = item.Attribute(XName.op_Implicit("Value"));
			if (!int.TryParse(val5.Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse StartingDistance attribute value " + val5.Value + " to a valid integer value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			if (!float.TryParse(val6.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
			{
				CLoggerManager.Log((object)("Could not parse Value attribute value " + val6.Value + " to a valid integer value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			_ = -1;
			num = result2;
			DodgeMultiplierByDistance.Add(result2, result3);
		}
	}
}
