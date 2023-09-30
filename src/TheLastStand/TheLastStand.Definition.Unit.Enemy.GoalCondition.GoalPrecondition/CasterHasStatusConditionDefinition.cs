using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class CasterHasStatusConditionDefinition : GoalConditionDefinition
{
	public const string Name = "CasterHasStatus";

	public Status.E_StatusType StatusType;

	public CasterHasStatusConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("StatusType"));
		if (val != null)
		{
			if (Enum.TryParse<Status.E_StatusType>(val.Value, out var result))
			{
				StatusType = result;
			}
			else
			{
				CLoggerManager.Log((object)("GoalConditionDefinition CasterHasStatus StatusType is incorrect: " + val.Value + " is not a valid StatusType"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}
}
