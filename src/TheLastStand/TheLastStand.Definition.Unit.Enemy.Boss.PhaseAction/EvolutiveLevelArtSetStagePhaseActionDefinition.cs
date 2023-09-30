using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class EvolutiveLevelArtSetStagePhaseActionDefinition : ABossPhaseActionDefinition
{
	public int StageIndex { get; private set; }

	public EvolutiveLevelArtSetStagePhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		if (int.TryParse(val.Value, out var result))
		{
			StageIndex = result;
		}
		else
		{
			CLoggerManager.Log((object)("EvolutiveLevelArtSetStagePhaseActionDefinition Could not parse Value attribute into a valid int : " + val.Value), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
	}
}
