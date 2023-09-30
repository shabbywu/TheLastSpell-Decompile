using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class PauseWavePhaseActionDefinition : ABossPhaseActionDefinition
{
	public bool PauseState { get; private set; }

	public PauseWavePhaseActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (bool.TryParse(((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("State")).Value, out var result))
		{
			PauseState = result;
		}
		else
		{
			CLoggerManager.Log((object)"Invalid state value for PauseWave PhaseAction", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
	}
}
