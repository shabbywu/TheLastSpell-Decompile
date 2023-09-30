using System.Collections.Generic;
using System.Linq;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;

namespace TheLastStand.Model.Skill.SkillAction;

public abstract class SkillAction
{
	public abstract string EstimationIconId { get; }

	public PerkDataContainer PerkDataContainer { get; set; }

	public Skill Skill { get; }

	public SkillActionController SkillActionController { get; }

	public SkillActionDefinition SkillActionDefinition { get; }

	public TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecution SkillActionExecution { get; set; }

	public SkillAction(SkillActionDefinition definition, SkillActionController controller, Skill skill)
	{
		SkillActionDefinition = definition;
		SkillActionController = controller;
		Skill = skill;
	}

	public static bool IsSkillEffectUnique(string effectId)
	{
		switch (effectId)
		{
		default:
			return effectId == "Follow";
		case "NoBlock":
		case "NoDodge":
		case "IgnoreLineOfSight":
		case "ArmorPiercing":
			return true;
		}
	}

	public Dictionary<string, List<SkillEffectDefinition>> GetAllEffects()
	{
		Dictionary<string, List<SkillEffectDefinition>> dictionary = new Dictionary<string, List<SkillEffectDefinition>>();
		if (SkillActionDefinition.SkillEffectDefinitions != null)
		{
			foreach (KeyValuePair<string, List<SkillEffectDefinition>> skillEffectDefinition in SkillActionDefinition.SkillEffectDefinitions)
			{
				dictionary.Add(skillEffectDefinition.Key, new List<SkillEffectDefinition>(skillEffectDefinition.Value));
			}
		}
		if (Skill.Owner is PlayableUnit playableUnit)
		{
			foreach (AddSkillEffect perkAddedSkillEffect in playableUnit.PerkAddedSkillEffects)
			{
				if (!perkAddedSkillEffect.PerkDataConditions.IsValid(PerkDataContainer))
				{
					continue;
				}
				foreach (KeyValuePair<string, List<SkillEffectDefinition>> skillEffectDefinition2 in perkAddedSkillEffect.AddSkillEffectDefinition.SkillEffectDefinitions)
				{
					if (dictionary.ContainsKey(skillEffectDefinition2.Key))
					{
						if (dictionary[skillEffectDefinition2.Key] == null || dictionary[skillEffectDefinition2.Key].Count <= 0 || !IsSkillEffectUnique(skillEffectDefinition2.Key))
						{
							if (dictionary[skillEffectDefinition2.Key] == null)
							{
								dictionary[skillEffectDefinition2.Key] = new List<SkillEffectDefinition>();
							}
							dictionary[skillEffectDefinition2.Key].AddRange(skillEffectDefinition2.Value);
						}
					}
					else
					{
						dictionary.Add(skillEffectDefinition2.Key, new List<SkillEffectDefinition>(skillEffectDefinition2.Value));
					}
				}
			}
		}
		return dictionary;
	}

	public List<T> GetEffects<T>(string effectId) where T : SkillEffectDefinition
	{
		List<T> list = new List<T>();
		List<T> effects = SkillActionDefinition.GetEffects<T>(effectId);
		if (effects != null)
		{
			list.AddRange(effects);
		}
		if (Skill.Owner is PlayableUnit playableUnit)
		{
			foreach (AddSkillEffect perkAddedSkillEffect in playableUnit.PerkAddedSkillEffects)
			{
				if (perkAddedSkillEffect.AddSkillEffectDefinition.SkillEffectDefinitions.TryGetValue(effectId, out var value) && value.Count > 0 && perkAddedSkillEffect.PerkDataConditions.IsValid(PerkDataContainer) && (list.Count == 0 || !IsSkillEffectUnique(effectId)))
				{
					list.AddRange(value.Cast<T>());
				}
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list;
	}

	public T GetFirstEffect<T>(string effectId) where T : SkillEffectDefinition
	{
		if (Skill.Owner is PlayableUnit playableUnit)
		{
			foreach (AddSkillEffect perkAddedSkillEffect in playableUnit.PerkAddedSkillEffects)
			{
				if (perkAddedSkillEffect.AddSkillEffectDefinition.SkillEffectDefinitions.TryGetValue(effectId, out var value) && value.Count > 0 && perkAddedSkillEffect.PerkDataConditions.IsValid(PerkDataContainer))
				{
					return value[0] as T;
				}
			}
		}
		return SkillActionDefinition.GetFirstEffect<T>(effectId);
	}

	public bool HasEffect(string effectId)
	{
		if (Skill.Owner is PlayableUnit playableUnit)
		{
			foreach (AddSkillEffect perkAddedSkillEffect in playableUnit.PerkAddedSkillEffects)
			{
				if (perkAddedSkillEffect.AddSkillEffectDefinition.SkillEffectDefinitions.TryGetValue(effectId, out var value) && value.Count > 0 && perkAddedSkillEffect.PerkDataConditions.IsValid(PerkDataContainer))
				{
					return true;
				}
			}
		}
		return SkillActionDefinition.HasEffect(effectId);
	}

	public bool HasSurroundingEffect<T>() where T : SkillEffectDefinition
	{
		if (TryGetEffects("SurroundingEffect", out List<SurroundingEffectDefinition> effects))
		{
			for (int num = effects.Count - 1; num >= 0; num--)
			{
				for (int num2 = effects[num].SkillEffectDefinitions.Count - 1; num2 >= 0; num2--)
				{
					if (effects[num].SkillEffectDefinitions[num2] is T)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool TryGetAllEffects(out Dictionary<string, List<SkillEffectDefinition>> effects)
	{
		effects = GetAllEffects();
		return effects != null;
	}

	public bool TryGetEffects<T>(string effectId, out List<T> effects) where T : SkillEffectDefinition
	{
		effects = GetEffects<T>(effectId);
		return effects != null;
	}

	public bool TryGetFirstEffect<T>(string effectId, out T effect) where T : SkillEffectDefinition
	{
		effect = GetFirstEffect<T>(effectId);
		return effect != null;
	}
}
