using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class AttackDataModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "AttackDataModifier";
	}

	public AttackSkillActionExecutionTileData.E_AttackDataParameter AttackDataParameter { get; private set; }

	public Node ValueExpression { get; private set; }

	public AttackDataModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		if (Enum.TryParse<AttackSkillActionExecutionTileData.E_AttackDataParameter>(((XElement)obj).Attribute(XName.op_Implicit("AttackDataParameter")).Value, out var result))
		{
			AttackDataParameter = result;
		}
		else
		{
			CLoggerManager.Log((object)"Could not parse AttackDataParameter attribute into an E_AttackDataParameter", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val.Value, ((Definition)this).TokenVariables);
	}
}
