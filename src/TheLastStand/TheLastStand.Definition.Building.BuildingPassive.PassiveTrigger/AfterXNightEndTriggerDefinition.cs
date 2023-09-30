using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class AfterXNightEndTriggerDefinition : PassiveTriggerDefinition
{
	public int NumberOfNightEnd { get; private set; }

	public AfterXNightEndTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnNightEnd;
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("NumberOfNightEnd"));
		if (val == null || !int.TryParse(val.Value, out var result))
		{
			CLoggerManager.Log((object)"The NumberOfNightEnd attribute (in AfterXNightEndTrigger) is missing or could not be parsed into an int.", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		else
		{
			NumberOfNightEnd = result;
		}
	}
}
