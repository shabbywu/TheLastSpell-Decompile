using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition : ABossPhaseActionDefinition
{
	public bool Value { get; private set; }

	public EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		if (bool.TryParse(val.Value, out var result))
		{
			Value = result;
		}
		else
		{
			CLoggerManager.Log((object)("Unable to parse " + val.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
	}
}
