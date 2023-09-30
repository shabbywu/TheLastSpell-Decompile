using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Tutorial;

public class DuringNightTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "DuringNight";
	}

	public bool BossNightOnly { get; private set; }

	public DuringNightTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("BossNightOnly"));
		if (val != null)
		{
			if (!bool.TryParse(val.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse DuringNightTutorialConditionDefinition BossNightOnly value " + val.Value + " to a valid bool!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				BossNightOnly = result;
			}
		}
	}
}
