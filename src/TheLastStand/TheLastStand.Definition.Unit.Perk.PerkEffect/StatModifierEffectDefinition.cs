using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class StatModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "StatModifier";
	}

	public bool ChildStatFollows { get; private set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public Node ValueExpression { get; private set; }

	public StatModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
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
			CLoggerManager.Log((object)("Could not parse Stat attribute into an E_Stat : " + val.Value + "."), (LogType)0, (CLogLevel)2, true, "StatModifierEffectDefinition", false);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val2.Value, ((Definition)this).TokenVariables);
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("ChildStatFollows"));
		bool result2;
		if (val3 == null)
		{
			ChildStatFollows = true;
		}
		else if (bool.TryParse(val3.Value, out result2))
		{
			ChildStatFollows = result2;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse ChildStatFollows attribute into a bool : " + val3.Value + "."), (LogType)0, (CLogLevel)2, true, "StatModifierEffectDefinition", false);
		}
	}
}
