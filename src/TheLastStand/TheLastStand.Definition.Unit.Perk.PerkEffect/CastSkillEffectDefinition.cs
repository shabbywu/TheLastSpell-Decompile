using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class CastSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "CastSkill";
	}

	public SkillDefinition SkillDefinition { get; private set; }

	public PerkTargetingDefinition PerkTargetingDefinition { get; private set; }

	public CastSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("SkillId"));
		if (SkillDatabase.SkillDefinitions.TryGetValue(val2.Value, out var value))
		{
			SkillDefinition = value;
		}
		else
		{
			CLoggerManager.Log((object)("Skill " + val2.Value + " not found!"), (LogType)0, (CLogLevel)2, true, "CastSkillEffectDefinition", false);
		}
		PerkTargetingDefinition = new PerkTargetingDefinition((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("PerkTargeting")), base.TokenVariables);
	}
}
