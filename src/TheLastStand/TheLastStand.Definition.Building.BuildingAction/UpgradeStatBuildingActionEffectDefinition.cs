using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class UpgradeStatBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	public static class Constants
	{
		public const string GainHealthMax = "GainHealthMax";

		public const string GainManaMax = "GainManaMax";
	}

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public int Bonus { get; private set; }

	public E_BuildingActionTargeting BuildingActionTargeting { get; private set; }

	public override string ActionEstimationIconId => Stat switch
	{
		UnitStatDefinition.E_Stat.HealthTotal => "GainHealthMax", 
		UnitStatDefinition.E_Stat.ManaTotal => "GainManaMax", 
		UnitStatDefinition.E_Stat.MovePointsTotal => UnitStatDefinition.E_Stat.MovePoints.ToString(), 
		UnitStatDefinition.E_Stat.ActionPointsTotal => UnitStatDefinition.E_Stat.ActionPoints.ToString(), 
		_ => Stat.ToString(), 
	};

	public UpgradeStatBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val != null)
		{
			XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Stat"));
			if (val2 != null)
			{
				if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val2.Value, out var result))
				{
					CLoggerManager.Log((object)("UpgradeStat Stat " + ((Definition)this).HasAnInvalid("E_Stat", val2.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				Stat = result;
				XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Bonus"));
				if (val3 != null)
				{
					if (!int.TryParse(val3.Value, out var result2))
					{
						CLoggerManager.Log((object)("UpgradeStat Bonus " + ((Definition)this).HasAnInvalid("int", val3.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
						return;
					}
					Bonus = result2;
					XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Target"));
					if (val4 != null)
					{
						if (!Enum.TryParse<E_BuildingActionTargeting>(val4.Value, out var result3))
						{
							CLoggerManager.Log((object)("UpgradeStat Target " + ((Definition)this).HasAnInvalid("E_Target", val4.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
						}
						else
						{
							BuildingActionTargeting = result3;
						}
					}
					else
					{
						CLoggerManager.Log((object)"UpgradeStat must have a Target", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
				}
				else
				{
					CLoggerManager.Log((object)"UpgradeStat must have a Bonus", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
			}
			else
			{
				CLoggerManager.Log((object)"UpgradeStat must have a stat", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		else
		{
			CLoggerManager.Log((object)"UpgradeStat doesn't have a XElement", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
