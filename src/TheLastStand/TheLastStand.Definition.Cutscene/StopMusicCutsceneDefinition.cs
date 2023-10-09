using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class StopMusicCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "StopMusic";
	}

	public bool OnlyIfVictoryTriggered { get; private set; }

	public StopMusicCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
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
				CLoggerManager.Log((object)("StopMusic OnlyIfVictoryTriggered: could not parse " + val.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
	}
}
