using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class RestoreStatEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "RestoreStat";
	}

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public Node ValueExpression { get; private set; }

	public bool HideDisplayEffect { get; private set; }

	public RestoreStatEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Stat"));
		if (Enum.TryParse<UnitStatDefinition.E_Stat>(val.Value, out var result))
		{
			Stat = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse Stat attribute into an E_Stat : " + val.Value + "."), (LogType)0, (CLogLevel)2, true, "RestoreStatEffectDefinition", false);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val2.Value, ((Definition)this).TokenVariables);
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("HideDisplayEffect"));
		if (val3 != null)
		{
			if (bool.TryParse(val3.Value, out var result2))
			{
				HideDisplayEffect = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse HideDisplayEffect attribute into a bool : " + val3.Value + "."), (LogType)0, (CLogLevel)2, true, "ApplyStatusEffectDefinition", false);
			}
		}
	}
}
