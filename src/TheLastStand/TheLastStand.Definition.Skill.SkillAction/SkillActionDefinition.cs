using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillAction;

public abstract class SkillActionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public bool ApplyOnCaster { get; set; }

	public Dictionary<string, List<SkillEffectDefinition>> SkillEffectDefinitions { get; private set; }

	public SkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public static SkillEffectDefinition DeserializeSkillEffect(XElement skillEffectElement)
	{
		return skillEffectElement.Name.LocalName switch
		{
			"ArmorPiercing" => new ArmorPiercingEffectDefinition((XContainer)(object)skillEffectElement), 
			"ArmorShredding" => new ArmorShreddingEffectDefinition((XContainer)(object)skillEffectElement), 
			"Buff" => new BuffEffectDefinition((XContainer)(object)skillEffectElement), 
			"CasterEffect" => new CasterEffectDefinition((XContainer)(object)skillEffectElement), 
			"Charged" => new ChargedEffectDefinition((XContainer)(object)skillEffectElement), 
			"Contagion" => new ContagionEffectDefinition((XContainer)(object)skillEffectElement), 
			"Debuff" => new DebuffEffectDefinition((XContainer)(object)skillEffectElement), 
			"ExileCaster" => new ExileCasterEffectDefinition((XContainer)(object)skillEffectElement), 
			"Follow" => new FollowSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"IgnoreLineOfSight" => new IgnoreLineOfSightEffectDefinition((XContainer)(object)skillEffectElement), 
			"Inaccurate" => new InaccurateSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"Isolated" => new IsolatedSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"Kill" => new KillSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"Maneuver" => new ManeuverSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"Momentum" => new MomentumEffectDefinition((XContainer)(object)skillEffectElement), 
			"MultiHit" => new MultiHitSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"NoBlock" => new NoBlockEffectDefinition((XContainer)(object)skillEffectElement), 
			"NoDodge" => new NoDodgeEffectDefinition((XContainer)(object)skillEffectElement), 
			"NoMomentum" => new NoMomentumEffectDefinition((XContainer)(object)skillEffectElement), 
			"Opportunistic" => new OpportunisticSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"Poison" => new PoisonEffectDefinition((XContainer)(object)skillEffectElement), 
			"Propagation" => new PropagationSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"RemoveStatus" => new RemoveStatusEffectDefinition((XContainer)(object)skillEffectElement), 
			"RegenStat" => new RegenStatSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"DecreaseStat" => new DecreaseStatSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"Stun" => new StunEffectDefinition((XContainer)(object)skillEffectElement), 
			"SurroundingEffect" => new SurroundingEffectDefinition((XContainer)(object)skillEffectElement), 
			"NegativeStatusImmunityEffect" => new ImmuneToNegativeStatusEffectDefinition((XContainer)(object)skillEffectElement), 
			"ResupplyCharges" => new ResupplyChargesSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"ResupplyOverallUses" => new ResupplyOverallUsesSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"ResupplySkills" => new ResupplySkillsSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			"ExtinguishBrazier" => new ExtinguishBrazierSkillEffectDefinition((XContainer)(object)skillEffectElement), 
			_ => null, 
		};
	}

	public List<SkillEffectDefinition> GetEffects(string effectId)
	{
		if (SkillEffectDefinitions != null && SkillEffectDefinitions.TryGetValue(effectId, out var value))
		{
			if (value.Count <= 0)
			{
				return null;
			}
			return value;
		}
		return null;
	}

	public List<T> GetEffects<T>(string effectId) where T : SkillEffectDefinition
	{
		if (SkillEffectDefinitions == null || !SkillEffectDefinitions.TryGetValue(effectId, out var value))
		{
			return null;
		}
		if (value.Count == 0)
		{
			return null;
		}
		List<T> list = new List<T>();
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i] is T)
			{
				list.Add(value[i] as T);
			}
		}
		if (list.Count != 0)
		{
			return list;
		}
		return null;
	}

	public T GetFirstEffect<T>(string effectId) where T : SkillEffectDefinition
	{
		List<SkillEffectDefinition> effects = GetEffects(effectId);
		if (effects == null || effects.Count <= 0)
		{
			return null;
		}
		return effects[0] as T;
	}

	public bool HasAnyEffect(IEnumerable<string> effectIds)
	{
		foreach (string effectId in effectIds)
		{
			if (HasEffect(effectId))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect(string effectId)
	{
		if (SkillEffectDefinitions != null)
		{
			return SkillEffectDefinitions.ContainsKey(effectId);
		}
		return false;
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ApplyOnCaster"));
		if (!val.IsNullOrEmpty())
		{
			if (bool.TryParse(val.Value, out var result))
			{
				ApplyOnCaster = result;
			}
			else
			{
				CLoggerManager.Log((object)"The skill must have a valid ApplyOnCaster", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		foreach (XElement item in obj.Elements())
		{
			XElement val2 = ((XContainer)item).Element(XName.op_Implicit("SkillEffects"));
			if (val2 == null)
			{
				continue;
			}
			foreach (XElement item2 in ((XContainer)val2).Elements())
			{
				SkillEffectDefinition skillEffectDefinition = DeserializeSkillEffect(item2);
				AddEffect(skillEffectDefinition);
			}
		}
	}

	public bool TryGetAllEffects<T>(string effectId, out List<T> effects) where T : SkillEffectDefinition
	{
		effects = GetEffects<T>(effectId);
		return effects != null;
	}

	public bool TryGetFirstEffect<T>(string effectId, out T effect) where T : SkillEffectDefinition
	{
		effect = GetFirstEffect<T>(effectId);
		return effect != null;
	}

	private void AddEffect(SkillEffectDefinition skillEffectDefinition)
	{
		if (SkillEffectDefinitions == null)
		{
			SkillEffectDefinitions = new Dictionary<string, List<SkillEffectDefinition>>();
		}
		if (!SkillEffectDefinitions.TryGetValue(skillEffectDefinition.Id, out var value))
		{
			value = new List<SkillEffectDefinition>();
			SkillEffectDefinitions.Add(skillEffectDefinition.Id, value);
		}
		value.Add(skillEffectDefinition);
	}
}
