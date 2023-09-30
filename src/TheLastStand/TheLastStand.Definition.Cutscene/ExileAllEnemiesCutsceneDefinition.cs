using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class ExileAllEnemiesCutsceneDefinition : Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "ExileAllEnemies";
	}

	public bool OnlyIfVictoryTriggered { get; private set; }

	public ExileAllEnemiesCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("OnlyIfVictoryTriggered"));
		if (!string.IsNullOrEmpty((val != null) ? val.Value : null))
		{
			if (bool.TryParse(val.Value, out var result))
			{
				OnlyIfVictoryTriggered = result;
			}
			else
			{
				CLoggerManager.Log((object)("ExileAllEnemies OnlyIfVictoryTriggered: could not parse " + val.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
	}
}
