using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Skill;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class SkillModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "SkillModifier";
	}

	public TheLastStand.Model.Skill.Skill.E_ComputationStat ComputationStat { get; private set; }

	public Node ValueExpression { get; private set; }

	public PerkDataConditionsDefinition PerkDataConditionsDefinition { get; private set; }

	public bool AffectBase { get; private set; }

	public SkillModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("ComputationStat"));
		if (Enum.TryParse<TheLastStand.Model.Skill.Skill.E_ComputationStat>(val2.Value, out var result))
		{
			ComputationStat = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse ComputationStat attribute into an E_ComputationStat : " + val2.Value + "."), (LogType)0, (CLogLevel)2, true, "SkillModifierEffectDefinition", false);
		}
		XAttribute val3 = val.Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val3.Value, ((Definition)this).TokenVariables);
		XAttribute val4 = val.Attribute(XName.op_Implicit("AffectBase"));
		AffectBase = val4 != null && bool.Parse(val4.Value);
		PerkDataConditionsDefinition = new PerkDataConditionsDefinition((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("Conditions")), ((Definition)this).TokenVariables);
	}
}
