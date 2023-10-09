using System;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class GroundCategoryConditionDefinition : GoalConditionDefinition
{
	public const string Name = "GroundCategory";

	public GroundDefinition.E_GroundCategory GroundCategory { get; private set; }

	public GroundCategoryConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute obj = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Id"));
		if (Enum.TryParse<GroundDefinition.E_GroundCategory>((obj != null) ? obj.Value : null, out var result))
		{
			GroundCategory = result;
		}
		else
		{
			CLoggerManager.Log((object)"Error while parsing GroundCategory", (LogType)0, (CLogLevel)1, true, GetType().Name, false);
		}
	}
}
