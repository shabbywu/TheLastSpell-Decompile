using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class EvolutiveLevelArtSetStageCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "EvolutiveLevelArtSetStage";
	}

	public int StageIndex { get; private set; }

	public EvolutiveLevelArtSetStageCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		if (int.TryParse(val.Value, out var result))
		{
			StageIndex = result;
		}
		else
		{
			CLoggerManager.Log((object)("EvolutiveLevelArtSetStageCutsceneDefinition Could not parse Value attribute into a valid int : " + val.Value), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
	}
}
