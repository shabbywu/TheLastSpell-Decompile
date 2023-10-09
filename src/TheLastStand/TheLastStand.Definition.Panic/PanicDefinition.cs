using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Definition.Panic;

public class PanicDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<int> GoldValues { get; private set; }

	public List<int> MaterialValues { get; private set; }

	public float PanicAttackMultiplier { get; private set; }

	public PanicLevelDefinition[] PanicLevelDefinitions { get; private set; }

	public float ValueMax { get; private set; }

	public PanicDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = ((XContainer)val).Element(XName.op_Implicit("ValueMax")).Attribute(XName.op_Implicit("Value"));
		if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Debug.LogError((object)("Invalid ValueMax " + val2.Value));
		}
		ValueMax = result;
		XAttribute val3 = ((XContainer)val).Element(XName.op_Implicit("PanicAttackMultiplier")).Attribute(XName.op_Implicit("Value"));
		if (!float.TryParse(val3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			Debug.LogError((object)("Invalid PanicAttackMultiplier " + val3.Value));
		}
		PanicAttackMultiplier = result2;
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("RewardValues"));
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		int i = 1;
		foreach (XElement item in ((XContainer)obj).Elements(XName.op_Implicit("RewardValue")))
		{
			if (!int.TryParse(item.Attribute(XName.op_Implicit("Index")).Value, out var result3))
			{
				CLoggerManager.Log((object)"Could not cast the index into an int !", (Object)(object)TPSingleton<PanicManager>.Instance, (LogType)0, (CLogLevel)2, true, "PanicManager", false);
			}
			if (!int.TryParse(((XContainer)item).Element(XName.op_Implicit("Gold")).Value, out var result4))
			{
				CLoggerManager.Log((object)"Could not cast the gold value into an int !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			if (!int.TryParse(((XContainer)item).Element(XName.op_Implicit("Material")).Value, out var result5))
			{
				CLoggerManager.Log((object)"Could not cast the material value into an int !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			if (i == 1)
			{
				if (result3 != 1)
				{
					CLoggerManager.Log((object)"The RewardValue for the first day (Index=\"1\") is required and must be placed first !", (Object)(object)TPSingleton<PanicManager>.Instance, (LogType)0, (CLogLevel)2, true, "PanicManager", false);
					list.Add(0);
					list2.Add(0);
				}
				else
				{
					list.Add(result4);
					list2.Add(result5);
				}
				i++;
			}
			else if (result3 < i)
			{
				CLoggerManager.Log((object)"The order of the RewardValues isn't respected, it might lead to errors !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				for (; i < result3; i++)
				{
					CLoggerManager.Log((object)"Some indexes are missing in the RewardValues, it might be unintended !", (LogType)2, (CLogLevel)1, true, "StaticLog", false);
					list.Add(list[list.Count - 1]);
					list2.Add(list2[list2.Count - 1]);
				}
				list.Add(result4);
				list2.Add(result5);
				i++;
			}
		}
		GoldValues = list;
		MaterialValues = list2;
		XElement obj2 = ((XContainer)val).Element(XName.op_Implicit("Levels"));
		IEnumerable<XElement> source = ((XContainer)obj2).Elements(XName.op_Implicit("Level"));
		PanicLevelDefinitions = new PanicLevelDefinition[source.Count()];
		int num = 0;
		foreach (XElement item2 in ((XContainer)obj2).Elements(XName.op_Implicit("Level")))
		{
			PanicLevelDefinitions[num++] = new PanicLevelDefinition((XContainer)(object)item2);
		}
	}
}
