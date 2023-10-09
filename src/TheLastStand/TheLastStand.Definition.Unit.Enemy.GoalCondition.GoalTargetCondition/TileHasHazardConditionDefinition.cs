using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Hazard;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class TileHasHazardConditionDefinition : GoalConditionDefinition
{
	public const string Name = "TileHasHazard";

	public HazardDefinition.E_HazardType HazardType { get; private set; }

	public TileHasHazardConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("HazardType"));
		if (Enum.TryParse<HazardDefinition.E_HazardType>((val != null) ? val.Value : null, out var result))
		{
			HazardType = result;
		}
		else
		{
			CLoggerManager.Log((object)("Error while parsing HazardType : " + ((val != null) ? val.Value : null) + " is not a correct HazardType"), (LogType)0, (CLogLevel)1, true, GetType().Name, false);
		}
	}
}
