using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class DynamicStatsModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "DynamicStatsModifier";
	}

	public List<UnitStatDefinition.E_Stat> Stats { get; private set; } = new List<UnitStatDefinition.E_Stat>();


	public Node ValueExpression { get; private set; }

	public DynamicStatsModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Stat")))
		{
			if (Enum.TryParse<UnitStatDefinition.E_Stat>(item.Value, out var result))
			{
				Stats.Add(result);
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse Stat element into an E_Stat : " + item.Value + "."), (LogType)0, (CLogLevel)2, true, "StatModifierEffectDefinition", false);
			}
		}
		XAttribute val2 = val.Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val2.Value, base.TokenVariables);
	}
}
