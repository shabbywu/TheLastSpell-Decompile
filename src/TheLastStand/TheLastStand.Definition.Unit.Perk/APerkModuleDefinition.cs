using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit.Perk.PerkCondition;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public abstract class APerkModuleDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<APerkConditionDefinition> PerkConditionDefinitions { get; private set; } = new List<APerkConditionDefinition>();


	public List<PerkEventDefinition> PerkEventDefinitions { get; private set; } = new List<PerkEventDefinition>();


	public List<APerkEffectDefinition> PerkEffectDefinitions { get; private set; } = new List<APerkEffectDefinition>();


	public APerkModuleDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("Conditions"));
		if (val != null)
		{
			DeserializeConditions(val);
		}
		XElement val2 = obj.Element(XName.op_Implicit("Events"));
		if (val2 != null)
		{
			DeserializeEvents(val2);
		}
		XElement val3 = obj.Element(XName.op_Implicit("Effects"));
		if (val3 != null)
		{
			DeserializeEffects(val3);
		}
	}

	protected void DeserializeConditions(XElement xConditions)
	{
		foreach (XElement item in ((XContainer)xConditions).Elements())
		{
			switch (item.Name.LocalName)
			{
			case "IsTrue":
				PerkConditionDefinitions.Add(new IsTrueConditionDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "IsFalse":
				PerkConditionDefinitions.Add(new IsFalseConditionDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			default:
				CLoggerManager.Log((object)("Tried to Deserialize an unimplemented PerkCondition: " + item.Name.LocalName), (LogType)0, (CLogLevel)2, true, "APerkModuleDefinition", false);
				break;
			}
		}
	}

	protected void DeserializeEvents(XElement xEvents)
	{
		foreach (XElement item in ((XContainer)xEvents).Elements(XName.op_Implicit("Event")))
		{
			PerkEventDefinitions.Add(new PerkEventDefinition((XContainer)(object)item, base.TokenVariables));
		}
	}

	protected void DeserializeEffects(XElement xEffects)
	{
		foreach (XElement item in ((XContainer)xEffects).Elements())
		{
			switch (item.Name.LocalName)
			{
			case "StatModifier":
				PerkEffectDefinitions.Add(new StatModifierEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "PermanentBaseStatModifier":
				PerkEffectDefinitions.Add(new PermanentBaseStatModifierEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "StatLocker":
				PerkEffectDefinitions.Add(new StatLockerEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "DynamicStatsModifier":
				PerkEffectDefinitions.Add(new DynamicStatsModifierEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "ComputationStatLocker":
				PerkEffectDefinitions.Add(new ComputationStatLockerEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "SkillModifier":
				PerkEffectDefinitions.Add(new SkillModifierEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "AddSkillEffect":
				PerkEffectDefinitions.Add(new AddSkillEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "AddSkill":
				PerkEffectDefinitions.Add(new AddPerkSkillEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "LockSkill":
				PerkEffectDefinitions.Add(new LockSkillEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "AllowDiagonalPropagation":
				PerkEffectDefinitions.Add(new AllowDiagonalPropagationEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "UnlockContextualSkill":
				PerkEffectDefinitions.Add(new UnlockContextualSkillEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "SwapContextualSkill":
				PerkEffectDefinitions.Add(new SwapContextualSkillEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "ResetBuffer":
				PerkEffectDefinitions.Add(new ResetBufferEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "RestoreStat":
				PerkEffectDefinitions.Add(new RestoreStatEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "ApplyStatus":
				PerkEffectDefinitions.Add(new ApplyStatusEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "CastSkill":
				PerkEffectDefinitions.Add(new CastSkillEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "DealDamage":
				PerkEffectDefinitions.Add(new DealDamageEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "ModifyDefensesDamage":
				PerkEffectDefinitions.Add(new ModifyDefensesDamageEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "GetAdditionalExperience":
				PerkEffectDefinitions.Add(new GetAdditionalExperienceEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "EquipmentSlotModifier":
				PerkEffectDefinitions.Add(new EquipmentSlotModifierEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "RestoreUses":
				PerkEffectDefinitions.Add(new RestoreUsesEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "ReplacePerk":
				PerkEffectDefinitions.Add(new ReplacePerkEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "AttackDataModifier":
				PerkEffectDefinitions.Add(new AttackDataModifierEffectDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			default:
				CLoggerManager.Log((object)("Tried to Deserialize an unimplemented PerkEffect: " + item.Name.LocalName), (LogType)0, (CLogLevel)2, true, "APerkModuleDefinition", false);
				break;
			}
		}
	}
}
