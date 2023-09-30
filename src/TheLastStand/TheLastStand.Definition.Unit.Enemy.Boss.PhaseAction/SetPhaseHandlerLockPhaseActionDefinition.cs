using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class SetPhaseHandlerLockPhaseActionDefinition : ABossPhaseActionDefinition
{
	public string HandlerId { get; private set; }

	public bool LockValue { get; private set; } = true;


	public SetPhaseHandlerLockPhaseActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("HandlerId"));
		HandlerId = val.Value;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("LockValue"));
		if (bool.TryParse(val2.Value, out var result))
		{
			LockValue = result;
		}
		else
		{
			CLoggerManager.Log((object)("Unable to parse " + val2.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
	}
}
