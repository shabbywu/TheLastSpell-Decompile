using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Tutorial;

public class TotalActionPointsSpentTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "TotalActionPointsSpent";
	}

	public int Value { get; private set; }

	public TotalActionPointsSpentTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		if (!int.TryParse(val.Value, out var result))
		{
			CLoggerManager.Log((object)("Could not parse TotalActionPointsSpentTutorialConditionDefinition value " + val.Value + " to a valid int!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			Value = result;
		}
	}
}
