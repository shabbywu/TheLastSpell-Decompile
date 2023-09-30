using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class AddPerkSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "AddSkill";
	}

	public SkillDefinition SkillDefinition { get; private set; }

	public int UsesPerNight { get; private set; } = -1;


	public int UsesPerTurn { get; private set; } = -1;


	public AddPerkSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("SkillId"));
		if (SkillDatabase.SkillDefinitions.TryGetValue(val.Value, out var value))
		{
			SkillDefinition = value;
		}
		else
		{
			CLoggerManager.Log((object)("Skill " + val.Value + " not found!"), (LogType)0, (CLogLevel)2, true, "AddPerkSkillEffectDefinition", false);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("UsesPerNight"));
		if (val2 != null)
		{
			if (int.TryParse(val2.Value, out var result))
			{
				UsesPerNight = result;
			}
			else
			{
				CLoggerManager.Log((object)"Found a UsesPerNight for AddSkill but the int parsing failed. Assigning -1 by default.", (LogType)2, (CLogLevel)2, true, "AddPerkSkillEffectDefinition", false);
				UsesPerNight = -1;
			}
		}
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("UsesPerTurn"));
		if (val3 != null)
		{
			if (int.TryParse(val3.Value, out var result2))
			{
				UsesPerTurn = result2;
				return;
			}
			CLoggerManager.Log((object)"Found a UsesPerTurn for AddSkill but the int parsing failed. Assigning -1 by default.", (LogType)2, (CLogLevel)2, true, "AddPerkSkillEffectDefinition", false);
			UsesPerTurn = -1;
		}
		else
		{
			CLoggerManager.Log((object)"UsesPerTurn was not specified for AddSkill. Assigning the SkillDefinition value by default.", (LogType)3, (CLogLevel)1, true, "AddPerkSkillEffectDefinition", false);
			UsesPerTurn = SkillDefinition.UsesPerTurnCount;
		}
	}
}
