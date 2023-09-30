using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class DamageableAroundConditionDefinition : GoalConditionDefinition
{
	public const string Name = "DamageableAround";

	public int MinAmount { get; private set; } = 1;


	public DamageableType DamageableType { get; private set; }

	public int MaxRange { get; private set; } = 1;


	public int MinRange { get; private set; }

	public DamageableAroundConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("MinAmount"));
		if (val != null)
		{
			MinAmount = int.Parse(val.Value);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("DamageableType"));
		if (Enum.TryParse<DamageableType>(val2.Value, out var result))
		{
			DamageableType = result;
		}
		else
		{
			CLoggerManager.Log((object)("GoalConditionDefinition DamageableAround UnitType is incorrect: " + val2.Value + " is not a valid DamageableType"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("MinRange"));
		if (val3 != null)
		{
			MinRange = int.Parse(val3.Value);
		}
		XAttribute val4 = ((XElement)obj).Attribute(XName.op_Implicit("MaxRange"));
		if (val4 != null)
		{
			MaxRange = int.Parse(val4.Value);
		}
	}
}
