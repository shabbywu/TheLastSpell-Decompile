using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class StatLockerEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "StatLocker";
	}

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public StatLockerEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Stat"));
		if (Enum.TryParse<UnitStatDefinition.E_Stat>(val.Value, out var result))
		{
			Stat = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse Stat attribute into an E_Stat : " + val.Value + "."), (LogType)0, (CLogLevel)2, true, "StatLockerEffectDefinition", false);
		}
	}
}
