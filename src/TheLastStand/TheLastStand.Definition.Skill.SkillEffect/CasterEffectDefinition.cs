using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Skill.SkillEffect.SkillSurroundingEffect;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class CasterEffectDefinition : AffectingUnitSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "CasterEffect";
	}

	public override string Id => "CasterEffect";

	public override bool DisplayCompendiumEntry => true;

	public override bool ShouldBeDisplayed => false;

	public List<SkillEffectDefinition> SkillEffectDefinitions { get; set; } = new List<SkillEffectDefinition>();


	public CasterEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		foreach (XElement item2 in ((container is XElement) ? container : null).Elements())
		{
			SkillEffectDefinition item = item2.Name.LocalName switch
			{
				"Buff" => new BuffEffectDefinition((XContainer)(object)item2), 
				"Charged" => new ChargedEffectDefinition((XContainer)(object)item2), 
				"Contagion" => new ContagionEffectDefinition((XContainer)(object)item2), 
				"Damage" => new DamageSurroundingEffectDefinition((XContainer)(object)item2), 
				"Debuff" => new DebuffEffectDefinition((XContainer)(object)item2), 
				"Poison" => new PoisonEffectDefinition((XContainer)(object)item2), 
				"RemoveStatus" => new RemoveStatusEffectDefinition((XContainer)(object)item2), 
				"Stun" => new StunEffectDefinition((XContainer)(object)item2), 
				"RegenStat" => new RegenStatSkillEffectDefinition((XContainer)(object)item2), 
				"DecreaseStat" => new DecreaseStatSkillEffectDefinition((XContainer)(object)item2), 
				"NegativeStatusImmunityEffect" => new ImmuneToNegativeStatusEffectDefinition((XContainer)(object)item2), 
				_ => null, 
			};
			SkillEffectDefinitions.Add(item);
		}
		SkillEffectDefinitions.ForEach(delegate(SkillEffectDefinition ske)
		{
			if (ske is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition)
			{
				affectingUnitSkillEffectDefinition.AffectedUnits = E_SkillUnitAffect.Caster;
			}
		});
		AffectedUnits = E_SkillUnitAffect.Caster;
	}

	public List<TEffect> GetEffects<TEffect>() where TEffect : SkillEffectDefinition
	{
		List<TEffect> list = new List<TEffect>();
		for (int num = SkillEffectDefinitions.Count - 1; num >= 0; num--)
		{
			if (SkillEffectDefinitions[num] is TEffect)
			{
				list.Add(SkillEffectDefinitions[num] as TEffect);
			}
		}
		return list;
	}
}
