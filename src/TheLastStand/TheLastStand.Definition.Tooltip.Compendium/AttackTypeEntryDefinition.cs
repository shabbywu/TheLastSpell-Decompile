using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Skill.SkillAction;
using UnityEngine;

namespace TheLastStand.Definition.Tooltip.Compendium;

public class AttackTypeEntryDefinition : ACompendiumEntryDefinition
{
	public static class Constants
	{
		public const string Name = "AttackTypeEntry";
	}

	public AttackSkillActionDefinition.E_AttackType AttackType { get; private set; }

	public AttackTypeEntryDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("AttackType"));
		AttackSkillActionDefinition.E_AttackType result2;
		if (val != null)
		{
			if (Enum.TryParse<AttackSkillActionDefinition.E_AttackType>(val.Value, out var result))
			{
				AttackType = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse AttackType element into a E_AttackType : " + val.Value), (LogType)0, (CLogLevel)2, true, "AttackTypeEntryDefinition", false);
			}
		}
		else if (Enum.TryParse<AttackSkillActionDefinition.E_AttackType>(base.Id, out result2))
		{
			AttackType = result2;
		}
	}
}
