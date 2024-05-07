using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TPLib;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.View.Skill.SkillAction;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class DealDamageEffectController : APerkEffectController
{
	public DealDamageEffect DealDamageEffect => base.PerkEffect as DealDamageEffect;

	public DealDamageEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new DealDamageEffect(aPerkEffectDefinition as DealDamageEffectDefinition, this, aPerkModule);
	}

	public static void UpdateTargetInjuryStageAndAddStatsDataFromAttack(IDamageable damageableTarget, AttackSkillActionExecutionTileData attackData, PlayableUnit playableUnitCaster)
	{
		if (!(damageableTarget.DamageableController is UnitController unitController))
		{
			return;
		}
		if (playableUnitCaster != null)
		{
			playableUnitCaster.LifetimeStats.LifetimeStatsController.IncreaseDamagesInflicted(attackData.HealthDamageIncludingOverkill);
			if (damageableTarget is EnemyUnit enemyUnit)
			{
				playableUnitCaster.LifetimeStats.LifetimeStatsController.IncreaseDamagesInflictedToEnemyType(enemyUnit, attackData.HealthDamageIncludingOverkill);
			}
			TrophyManager.AppendValueToTrophiesConditions<DamageInflictedTrophyConditionController>(new object[2] { playableUnitCaster.RandomId, attackData.HealthDamageIncludingOverkill });
			TrophyManager.SetValueToTrophiesConditions<DamageInflictedSingleAttackTrophyConditionController>(new object[2] { playableUnitCaster.RandomId, attackData.HealthDamageIncludingOverkill });
		}
		TPSingleton<MetaConditionManager>.Instance.RefreshMaxSingleHitDamageByType(attackData.TotalDamage, AttackSkillActionDefinition.E_AttackType.None);
		unitController.UpdateInjuryStage();
	}

	public override void Trigger(PerkDataContainer data)
	{
		if (data != null && data.IsTriggeredByPerk && !base.PerkEffect.APerkEffectDefinition.CanBeTriggeredByPerk)
		{
			return;
		}
		base.Trigger(data);
		TPSingleton<PlayableUnitManager>.Instance.ShouldClearUndoStack = true;
		HashSet<IDamageable> hashSet = new HashSet<IDamageable>();
		float num = DealDamageEffect.DealDamageEffectDefinition.Value.EvalToFloat(base.PerkEffect.APerkModule.Perk);
		PlayableUnit owner = base.PerkEffect.APerkModule.Perk.Owner;
		HashSet<Tile> targetTiles = DealDamageEffect.PerkTargeting.GetTargetTiles(data, base.PerkEffect.APerkModule.Perk);
		LinqExtensions.AddRange<IDamageable>(hashSet, from t in targetTiles
			select t.Damageable into d
			where d != null
			select d);
		if (hashSet.Count <= 0)
		{
			return;
		}
		PerkDataContainer perkDataContainer = new PerkDataContainer
		{
			Caster = owner,
			AllAttackData = new HashSet<AttackSkillActionExecutionTileData>(),
			IsTriggeredByPerk = true
		};
		foreach (IDamageable item in hashSet)
		{
			int blockedDamage = 0;
			if (item is TheLastStand.Model.Unit.Unit unit && !DealDamageEffect.DealDamageEffectDefinition.IgnoreDefense)
			{
				num = unit.UnitController.ReduceIncomingDamage((int)num, owner, AttackSkillActionDefinition.E_AttackType.None, checkBlock: true, out blockedDamage);
			}
			if (!(num <= 0f))
			{
				float armor = item.Armor;
				float num2 = Mathf.Min(armor, num);
				if (num2 > 0f)
				{
					item.DamageableController.LoseArmor(num2, owner, refreshHud: false);
				}
				float num3 = Mathf.Max(0f, num - armor);
				if (num3 > 0f)
				{
					item.DamageableController.LoseHealth(num3, owner, refreshHud: false);
				}
				AttackSkillActionExecutionTileData attackSkillActionExecutionTileData = new AttackSkillActionExecutionTileData
				{
					Damageable = item,
					TargetTile = base.PerkEffect.APerkModule.Perk.Owner.OriginTile,
					ArmorDamage = num2,
					TargetRemainingArmor = item.Armor,
					TargetArmorTotal = item.ArmorTotal,
					TargetRemainingHealth = item.Health,
					TargetHealthTotal = item.HealthTotal,
					HealthDamage = num3,
					TotalDamage = num,
					BlockedDamage = blockedDamage
				};
				AttackFeedback attackFeedback = item.DamageableView.AttackFeedback;
				attackFeedback.AddDamageInstance(attackSkillActionExecutionTileData);
				item.DamageableController.AddEffectDisplay(attackFeedback);
				EffectManager.Register(item.DamageableController);
				UpdateTargetInjuryStageAndAddStatsDataFromAttack(item, attackSkillActionExecutionTileData, owner);
				perkDataContainer.AllAttackData.Add(attackSkillActionExecutionTileData);
				PerkDataContainer obj = new PerkDataContainer
				{
					Caster = owner,
					TargetDamageable = item,
					AttackData = attackSkillActionExecutionTileData,
					IsTriggeredByPerk = true
				};
				owner?.Events.GetValueOrDefault(E_EffectTime.OnDealDamageTargetHit)?.Invoke(obj);
				if (item.IsDead)
				{
					owner?.Events.GetValueOrDefault(E_EffectTime.OnDealDamageTargetKill)?.Invoke(obj);
				}
			}
		}
		owner?.Events.GetValueOrDefault(E_EffectTime.OnDealDamageExecutionEnd)?.Invoke(perkDataContainer);
	}
}
