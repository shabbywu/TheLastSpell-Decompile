using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class DestroyActorPhaseActionDefinition : ABossPhaseActionDefinition
{
	public string ActorId { get; private set; }

	public bool CameraFocus { get; private set; }

	public bool WaitDeathAnim { get; private set; }

	public DestroyActorPhaseActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		ActorId = val.Value;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("CameraFocus"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result))
			{
				CameraFocus = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val2.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("WaitDeathAnim"));
		if (val3 != null)
		{
			if (bool.TryParse(val3.Value, out var result2))
			{
				WaitDeathAnim = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val3.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
	}
}
