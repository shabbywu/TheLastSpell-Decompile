using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class HealManaBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	private static class Constants
	{
		public const string ActionEstimationIconId = "Mana";
	}

	public int Amount { get; private set; }

	public E_BuildingActionTargeting BuildingActionTargeting { get; private set; }

	public override string ActionEstimationIconId => "Mana";

	public HealManaBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
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
					CLoggerManager.Log((object)("HealMana Amount " + ((Definition)this).HasAnInvalid("int", val2.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				Amount = result;
				XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Target"));
				if (val3 != null)
				{
					if (!Enum.TryParse<E_BuildingActionTargeting>(val3.Value, out var result2))
					{
						CLoggerManager.Log((object)("HealMana Target " + ((Definition)this).HasAnInvalid("E_Target", val3.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
					else
					{
						BuildingActionTargeting = result2;
					}
				}
			}
			else
			{
				CLoggerManager.Log((object)"HealMana must have a Bonus", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		else
		{
			CLoggerManager.Log((object)"HealMana doesn't have a XElement", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
