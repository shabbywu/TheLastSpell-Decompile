using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class PermanentTriggerDefinition : PassiveTriggerDefinition
{
	public bool TriggerOnLoad { get; private set; }

	public PermanentTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.Permanent;
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("TriggerOnLoad"));
		if (bool.TryParse(val.Value, out var result))
		{
			TriggerOnLoad = result;
		}
		else
		{
			CLoggerManager.Log((object)("PermanentTriggerDefinition : Unable to parse TriggerOnLoad value " + ((val != null) ? val.Value : null) + " into a bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
	}
}
