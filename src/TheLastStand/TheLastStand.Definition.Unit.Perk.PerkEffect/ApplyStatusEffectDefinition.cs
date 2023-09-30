using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ApplyStatusEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ApplyStatus";
	}

	public PerkTargetingDefinition PerkTargetingDefinition { get; private set; }

	public Status.E_StatusType StatusType { get; private set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; } = UnitStatDefinition.E_Stat.Undefined;


	public Node ChanceExpression { get; private set; }

	public Node ValueExpression { get; private set; }

	public Node TurnsCountExpression { get; private set; }

	public bool HideDisplayEffect { get; private set; }

	public bool RefreshHUD { get; private set; } = true;


	public ApplyStatusEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		PerkTargetingDefinition = new PerkTargetingDefinition((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("PerkTargeting")), ((Definition)this).TokenVariables);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Status"));
		if (Enum.TryParse<Status.E_StatusType>(val2.Value, out var result))
		{
			StatusType = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse Status attribute into an E_StatusType : " + val2.Value + "."), (LogType)0, (CLogLevel)2, true, "ApplyStatusEffectDefinition", false);
		}
		XAttribute val3 = val.Attribute(XName.op_Implicit("Stat"));
		if ((StatusType & Status.E_StatusType.Buff) != 0 || (StatusType & Status.E_StatusType.Debuff) != 0)
		{
			if (Enum.TryParse<UnitStatDefinition.E_Stat>(val3.Value, out var result2))
			{
				Stat = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse Stat attribute into an E_Stat : " + val3.Value + "."), (LogType)0, (CLogLevel)2, true, "ApplyStatusEffectDefinition", false);
			}
		}
		else if (val3 != null)
		{
			CLoggerManager.Log((object)$"Specified an E_Stat {val3.Value} while {StatusType} does not need one.", (LogType)2, (CLogLevel)2, true, "ApplyStatusEffectDefinition", false);
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("Value"));
		if (val4 != null)
		{
			ValueExpression = Parser.Parse(val4.Value, ((Definition)this).TokenVariables);
		}
		XAttribute val5 = val.Attribute(XName.op_Implicit("Chance"));
		if (val5 != null)
		{
			ChanceExpression = Parser.Parse(val5.Value, ((Definition)this).TokenVariables);
		}
		XAttribute val6 = val.Attribute(XName.op_Implicit("TurnsCount"));
		TurnsCountExpression = Parser.Parse(val6.Value, ((Definition)this).TokenVariables);
		XAttribute val7 = val.Attribute(XName.op_Implicit("HideDisplayEffect"));
		if (val7 != null)
		{
			if (bool.TryParse(val7.Value, out var result3))
			{
				HideDisplayEffect = result3;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse HideDisplayEffect attribute into a bool : " + val7.Value + "."), (LogType)0, (CLogLevel)2, true, "ApplyStatusEffectDefinition", false);
			}
		}
		XAttribute val8 = val.Attribute(XName.op_Implicit("RefreshHUD"));
		if (val8 != null)
		{
			if (bool.TryParse(val8.Value, out var result4))
			{
				RefreshHUD = result4;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse RefreshHUD attribute into a bool : " + val8.Value + "."), (LogType)0, (CLogLevel)2, true, "ApplyStatusEffectDefinition", false);
			}
		}
	}
}
