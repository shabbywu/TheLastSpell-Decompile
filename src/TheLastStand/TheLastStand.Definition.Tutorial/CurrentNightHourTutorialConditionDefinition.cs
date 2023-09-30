using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Tutorial;

public class CurrentNightHourTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "CurrentNightHour";
	}

	public int NightHour { get; private set; }

	public CurrentNightHourTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("NightHour"));
		if (!int.TryParse(val.Value, out var result))
		{
			CLoggerManager.Log((object)("Could not parse CurrentNightHourTutorialConditionDefinition value " + val.Value + " to a valid int!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			NightHour = result;
		}
	}
}
