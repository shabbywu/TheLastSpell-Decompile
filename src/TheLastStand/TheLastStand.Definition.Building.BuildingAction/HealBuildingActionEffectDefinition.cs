using System;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class HealBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	private static class Constants
	{
		public const string ActionEstimationIconId = "Health";
	}

	public int Amount { get; private set; }

	public E_BuildingActionTargeting BuildingActionTargeting { get; private set; }

	public override string ActionEstimationIconId => "Health";

	public HealBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val != null)
		{
			XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Amount"));
			if (val2 != null)
			{
				if (!int.TryParse(val2.Value, out var result))
				{
					CLoggerManager.Log((object)("Heal Amount " + HasAnInvalid("int", val2.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				Amount = result;
				XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Target"));
				if (val3 != null)
				{
					if (!Enum.TryParse<E_BuildingActionTargeting>(val3.Value, out var result2))
					{
						CLoggerManager.Log((object)("Heal Target " + HasAnInvalid("E_Target", val3.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
					else
					{
						BuildingActionTargeting = result2;
					}
				}
			}
			else
			{
				CLoggerManager.Log((object)"Heal must have a Bonus", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		else
		{
			CLoggerManager.Log((object)"Heal doesn't have a XElement", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
