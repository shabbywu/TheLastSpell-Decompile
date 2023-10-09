using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkDataCondition;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class SkillModifierEffect : APerkEffect
{
	public bool HasBeenUsed;

	public SkillModifierEffectDefinition SkillModifierEffectDefinition => base.APerkEffectDefinition as SkillModifierEffectDefinition;

	public float Value => SkillModifierEffectDefinition.ValueExpression.EvalToInt(base.APerkModule.Perk);

	public PerkDataConditions PerkDataConditions { get; private set; }

	public SkillModifierEffect(SkillModifierEffectDefinition aPerkEffectDefinition, SkillModifierEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
		PerkDataConditions = new PerkDataConditions(SkillModifierEffectDefinition.PerkDataConditionsDefinition, aPerkModule.Perk);
	}
}
