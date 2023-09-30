using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public class OptimalTargetingMethodDefinition : TargetingMethodDefinition
{
	public const string Name = "Optimal";

	public Dictionary<DamageableType, int> DamageableTypesWeight { get; private set; }

	public List<(string[] ids, int weight)> DamageableIdsWeight { get; private set; }

	public OptimalTargetingMethodDefinition(XContainer container = null)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("DamageableType")))
		{
			if (DamageableTypesWeight == null)
			{
				Dictionary<DamageableType, int> dictionary2 = (DamageableTypesWeight = new Dictionary<DamageableType, int>());
			}
			XAttribute val2 = item.Attribute(XName.op_Implicit("Weight"));
			int result = 1;
			if (val2 != null && !int.TryParse(val2.Value, out result))
			{
				CLoggerManager.Log((object)("GoalConditionDefinition Optimal UnitType is incorrect: " + val2.Value + " is not a valid Integer"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			if (Enum.TryParse<DamageableType>(item.Value, out var result2))
			{
				DamageableTypesWeight.Add(result2, result);
			}
			else
			{
				CLoggerManager.Log((object)("GoalConditionDefinition Optimal UnitType is incorrect: " + item.Value + " is not a valid DamageableType"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("DamageableId")))
		{
			if (DamageableIdsWeight == null)
			{
				List<(string[], int)> list2 = (DamageableIdsWeight = new List<(string[], int)>());
			}
			XAttribute val3 = item2.Attribute(XName.op_Implicit("Weight"));
			int result3 = 1;
			if (val3 != null && !int.TryParse(val3.Value, out result3))
			{
				CLoggerManager.Log((object)("GoalConditionDefinition Optimal UnitType is incorrect: " + val3.Value + " is not a valid Integer"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			List<string> list3 = new List<string>();
			foreach (XElement item3 in ((XContainer)item2).Elements(XName.op_Implicit("IdList")))
			{
				XAttribute val4 = item3.Attribute(XName.op_Implicit("Value"));
				foreach (string id in GenericDatabase.IdsListDefinitions[val4.Value].Ids)
				{
					if (!list3.Contains(id))
					{
						list3.Add(id);
					}
				}
			}
			foreach (XElement item4 in ((XContainer)item2).Elements(XName.op_Implicit("Id")))
			{
				XAttribute val5 = item4.Attribute(XName.op_Implicit("Value"));
				if (!list3.Contains(val5.Value))
				{
					list3.Add(val5.Value);
				}
			}
			DamageableIdsWeight.Add((list3.ToArray(), result3));
		}
	}
}
