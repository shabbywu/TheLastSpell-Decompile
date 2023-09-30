using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class DamageableCountInAoeConditionDefinition : GoalConditionDefinition
{
	public const string Name = "DamageableCountInAoe";

	public List<DamageableType> DamageableTypesToCount = new List<DamageableType>();

	public int Max { get; private set; } = int.MaxValue;


	public int Min { get; private set; }

	public DamageableCountInAoeConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Min"));
		if (val != null)
		{
			Min = int.Parse(val.Value);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Max"));
		if (val2 != null)
		{
			Max = int.Parse(val2.Value);
		}
		foreach (XElement item in obj.Elements(XName.op_Implicit("DamageableTypeToCount")))
		{
			if (Enum.TryParse<DamageableType>(item.Value, out var result))
			{
				DamageableTypesToCount.Add(result);
			}
			else
			{
				CLoggerManager.Log((object)("GoalConditionDefinition DamageableCountInAoe UnitType is incorrect: " + item.Value + " is not a valid DamageableType"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}
}
