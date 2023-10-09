using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UpgradeStatDefinition : TheLastStand.Framework.Serialization.Definition
{
	public UnitStatDefinition.E_Stat Stat { get; set; }

	public int Bonus { get; set; }

	public UpgradeStatDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Stat"));
		if (val2 != null)
		{
			if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val2.Value, out var result))
			{
				CLoggerManager.Log((object)("UpgradeStat Stat " + HasAnInvalid("E_Stat", val2.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			Stat = result;
			XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Bonus"));
			if (val3 != null)
			{
				if (!int.TryParse(val3.Value, out var result2))
				{
					CLoggerManager.Log((object)("UpgradeStat Bonus " + HasAnInvalid("int", val3.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					Bonus = result2;
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
}
