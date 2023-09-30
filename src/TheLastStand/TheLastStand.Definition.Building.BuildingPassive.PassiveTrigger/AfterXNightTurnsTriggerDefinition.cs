using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class AfterXNightTurnsTriggerDefinition : PassiveTriggerDefinition
{
	public int NumberOfNightTurns { get; private set; }

	public AfterXNightTurnsTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnEndNightTurnPlayable;
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("NumberOfNightTurns"));
		if (val == null || !int.TryParse(val.Value, out var result))
		{
			CLoggerManager.Log((object)"The NumberOfNightTurns attribute (in AfterXNightTurnsTrigger) is missing or could not be parsed into an int.", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		else
		{
			NumberOfNightTurns = result;
		}
	}
}
