using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class AfterXProductionPhasesTriggerDefinition : PassiveTriggerDefinition
{
	public int NumberOfProductionPhases { get; private set; }

	public AfterXProductionPhasesTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnStartProductionTurn;
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("NumberOfProductionPhases"));
		if (val == null || !int.TryParse(val.Value, out var result))
		{
			CLoggerManager.Log((object)"The NumberOfProductionPhases attribute (in AfterXProductionPhasesTrigger) is missing or could not be parsed into an int.", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		else
		{
			NumberOfProductionPhases = result;
		}
	}
}
