using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "EvolutiveLevelArtSetActiveCurrentStage";
	}

	public bool Value { get; private set; }

	public EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		if (bool.TryParse(val.Value, out var result))
		{
			Value = result;
		}
		else
		{
			CLoggerManager.Log((object)("EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition Unable to parse " + val.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
	}
}
