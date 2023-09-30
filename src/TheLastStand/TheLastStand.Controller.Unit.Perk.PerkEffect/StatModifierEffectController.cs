using System.Collections.Generic;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class StatModifierEffectController : APerkEffectController
{
	public StatModifierEffect StatModifierEffect => base.PerkEffect as StatModifierEffect;

	public StatModifierEffectController(StatModifierEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new StatModifierEffect(aPerkEffectDefinition as StatModifierEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		List<StatModifierEffect> perkStatModifierEffects = base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(StatModifierEffect.StatModifierEffectDefinition.Stat).PerkStatModifierEffects;
		if (!perkStatModifierEffects.Contains(StatModifierEffect))
		{
			perkStatModifierEffects.Add(StatModifierEffect);
		}
		if (!StatModifierEffect.StatModifierEffectDefinition.ChildStatFollows)
		{
			return;
		}
		UnitStatDefinition.E_Stat childStatIfExists = UnitDatabase.UnitStatDefinitions[StatModifierEffect.StatModifierEffectDefinition.Stat].GetChildStatIfExists();
		if (childStatIfExists != UnitStatDefinition.E_Stat.Undefined)
		{
			perkStatModifierEffects = StatModifierEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(childStatIfExists).PerkStatModifierEffects;
			if (!perkStatModifierEffects.Contains(StatModifierEffect))
			{
				perkStatModifierEffects.Add(StatModifierEffect);
			}
		}
	}

	public override void Lock(bool onLoad)
	{
		List<StatModifierEffect> perkStatModifierEffects = base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(StatModifierEffect.StatModifierEffectDefinition.Stat).PerkStatModifierEffects;
		if (perkStatModifierEffects.Contains(StatModifierEffect))
		{
			perkStatModifierEffects.Remove(StatModifierEffect);
		}
		if (!StatModifierEffect.StatModifierEffectDefinition.ChildStatFollows)
		{
			return;
		}
		UnitStatDefinition.E_Stat childStatIfExists = UnitDatabase.UnitStatDefinitions[StatModifierEffect.StatModifierEffectDefinition.Stat].GetChildStatIfExists();
		if (childStatIfExists != UnitStatDefinition.E_Stat.Undefined)
		{
			perkStatModifierEffects = StatModifierEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(childStatIfExists).PerkStatModifierEffects;
			if (perkStatModifierEffects.Contains(StatModifierEffect))
			{
				perkStatModifierEffects.Remove(StatModifierEffect);
			}
		}
	}
}
