using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class DealDamageEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "DealDamage";
	}

	public bool IgnoreDefense { get; private set; }

	public PerkTargetingDefinition PerkTargetingDefinition { get; private set; }

	public Node Value { get; private set; }

	public DealDamageEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("IgnoreDefense"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result))
			{
				IgnoreDefense = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse IgnoreDefense attribute into a bool : " + val2.Value + "."), (LogType)0, (CLogLevel)2, true, "DealDamageEffectDefinition", false);
			}
		}
		PerkTargetingDefinition = new PerkTargetingDefinition((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("PerkTargeting")), ((Definition)this).TokenVariables);
		XAttribute val3 = val.Attribute(XName.op_Implicit("Value"));
		string text = StringExtensions.Replace(val3.Value, ((Definition)this).TokenVariables);
		if (!string.IsNullOrEmpty(text))
		{
			Value = Parser.Parse(text, (Dictionary<string, string>)null);
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse Value attribute : " + val3.Value + "."), (LogType)0, (CLogLevel)2, true, "DealDamageEffectDefinition", false);
		}
	}
}
