using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Controller.Status;
using TheLastStand.Controller.Status.Immunity;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Item;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.Controller.Skill.SkillAction;

public abstract class SkillActionController
{
	public TheLastStand.Model.Skill.SkillAction.SkillAction SkillAction { get; protected set; }

	public virtual List<SkillActionResultDatas> ApplyEffect(ISkillCaster caster, List<Tile> affectedTiles, List<Tile> surroundingTiles)
	{
		AttackSkillAction attackSkillAction = SkillAction as AttackSkillAction;
		List<SkillActionResultDatas> list = new List<SkillActionResultDatas>();
		if (attackSkillAction != null)
		{
			attackSkillAction.HeroHitsEnemyCount = 0;
			attackSkillAction.HeroHitsHeroCount = 0;
			attackSkillAction.HeroHitsBuildings.Clear();
		}
		if (SkillAction.SkillActionDefinition.ApplyOnCaster)
		{
			SkillActionResultDatas item = ApplyActionOnTile(caster.OriginTile, caster);
			list.Add(item);
		}
		else
		{
			foreach (Tile affectedTile in affectedTiles)
			{
				if (this is SpawnSkillActionController || (SkillAction.SkillActionExecution.SkillExecutionController.IsTileAffected(affectedTile) && !IsDamageableAlreadyHit(list, affectedTile)))
				{
					list.Add(ApplyActionOnTile(affectedTile, caster));
				}
			}
			foreach (Tile surroundingTile in surroundingTiles)
			{
				if (SkillAction.SkillActionExecution.SkillExecutionController.IsTileAffected(surroundingTile) && !IsDamageableAlreadyHit(list, surroundingTile))
				{
					list.Add(ApplyActionOnSurroundingTile(surroundingTile, caster));
				}
			}
			if (SkillAction.HasEffect("CasterEffect") && (!(caster is TheLastStand.Model.Unit.Unit unit) || !unit.IsDead))
			{
				if (caster is BattleModule battleModule)
				{
					DamageableModule damageableModule = battleModule.BuildingParent.DamageableModule;
					if (damageableModule != null && damageableModule.IsDead)
					{
						goto IL_0177;
					}
				}
				list.Add(ApplyActionOnCaster(caster));
			}
		}
		goto IL_0177;
		IL_0177:
		if (list.Count != 0)
		{
			if (caster is BattleModule battleModule2 && battleModule2.BuildingParent.IsTrap)
			{
				if (TPSingleton<GlyphManager>.Instance.FreeTrapUsageChances == 0 || RandomManager.GetRandomRange(this, 0, 100) > TPSingleton<GlyphManager>.Instance.FreeTrapUsageChances)
				{
					battleModule2.RemainingTrapCharges--;
				}
				TPSingleton<MetaConditionManager>.Instance.IncreaseTrapsUsed(battleModule2.BuildingParent.BuildingDefinition.Id);
			}
			if (caster is PlayableUnit playableUnit)
			{
				if (SkillAction.Skill.SkillContainer is TheLastStand.Model.Item.Item item2 && ItemDefinition.E_Category.Usable.HasFlag(item2.ItemDefinition.Category))
				{
					TrophyManager.AppendValueToTrophiesConditions<UsableUsedTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
				}
				if (SkillAction.Skill.IsPunch)
				{
					TrophyManager.AppendValueToTrophiesConditions<PunchUsedTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
				}
				else if (SkillAction.Skill.SkillDefinition.Id == "JumpOverWall")
				{
					TrophyManager.AppendValueToTrophiesConditions<JumpOverWallUsedTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
				}
				if (SkillAction.HasEffect("Momentum") && playableUnit.MomentumTilesActive > 0)
				{
					TrophyManager.SetMaxValueToTrophiesConditions<TilesMovedBeforeMomentumTrophyConditionController>(new object[2] { playableUnit.RandomId, playableUnit.MomentumTilesActive });
				}
			}
		}
		else if (attackSkillAction == null && caster is PlayableUnit playableUnit2 && SkillAction.Skill.SkillDefinition.Id == "JumpOverWall")
		{
			TrophyManager.AppendValueToTrophiesConditions<JumpOverWallUsedTrophyConditionController>(new object[2] { playableUnit2.RandomId, 1 });
		}
		return list;
	}

	public void EnsurePerkData(Tile targetTile = null, IDamageable targetDamageable = null, AttackSkillActionExecutionTileData attackData = null, bool? isTriggeredByPerk = null, TheLastStand.Model.Status.Status.E_StatusType targetUnitPreviousStatuses = TheLastStand.Model.Status.Status.E_StatusType.None, TheLastStand.Model.Status.Status statusApplied = null)
	{
		TheLastStand.Model.Status.Status.E_StatusType valueOrDefault = ((targetUnitPreviousStatuses != 0) ? new TheLastStand.Model.Status.Status.E_StatusType?(targetUnitPreviousStatuses) : SkillAction.PerkDataContainer?.TargetUnitPreviousStatuses).GetValueOrDefault();
		ResetPerkData(targetTile ?? SkillAction.PerkDataContainer?.TargetTile, targetDamageable ?? SkillAction.PerkDataContainer?.TargetDamageable, attackData ?? SkillAction.PerkDataContainer?.AttackData, isTriggeredByPerk ?? SkillAction.PerkDataContainer?.IsTriggeredByPerk, valueOrDefault, statusApplied ?? SkillAction.PerkDataContainer?.StatusApplied, SkillAction.PerkDataContainer?.AllAttackData);
	}

	public void ResetPerkData(Tile targetTile = null, IDamageable targetDamageable = null, AttackSkillActionExecutionTileData attackData = null, bool? isTriggeredByPerk = null, TheLastStand.Model.Status.Status.E_StatusType targetUnitPreviousStatuses = TheLastStand.Model.Status.Status.E_StatusType.None, TheLastStand.Model.Status.Status statusApplied = null, HashSet<AttackSkillActionExecutionTileData> allAttackData = null)
	{
		SkillAction.PerkDataContainer = new PerkDataContainer
		{
			AttackData = attackData,
			Caster = (SkillAction.SkillActionExecution?.Caster ?? SkillAction.Skill.OwnerOrSelected),
			Skill = SkillAction.Skill,
			TargetDamageable = targetDamageable,
			TargetTile = targetTile,
			IsTriggeredByPerk = isTriggeredByPerk.GetValueOrDefault(),
			StatusApplied = statusApplied,
			TargetUnitPreviousStatuses = targetUnitPreviousStatuses,
			AllAttackData = allAttackData
		};
		if (attackData != null)
		{
			if (SkillAction.PerkDataContainer.AllAttackData == null)
			{
				SkillAction.PerkDataContainer.AllAttackData = new HashSet<AttackSkillActionExecutionTileData>();
			}
			SkillAction.PerkDataContainer.AllAttackData.Add(attackData);
		}
	}

	public abstract bool IsBuildingAffected(Tile targetTile);

	public abstract bool IsUnitAffected(Tile targetTile);

	public virtual void Reset()
	{
		ResetPerkData(SkillAction.PerkDataContainer.TargetTile);
	}

	protected abstract SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster);

	protected abstract SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster);

	protected virtual SkillActionResultDatas ApplyActionOnCaster(ISkillCaster caster)
	{
		SkillActionResultDatas skillActionResultDatas = new SkillActionResultDatas();
		if (!SkillAction.TryGetFirstEffect<CasterEffectDefinition>("CasterEffect", out var effect))
		{
			((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).LogError((object)("No caster effect found on skill " + SkillAction.Skill.SkillDefinition.Id + "!"), (CLogLevel)0, true, true);
			return skillActionResultDatas;
		}
		if (!(caster is TheLastStand.Model.Unit.Unit unit) || !unit.IsDead)
		{
			if (caster is BattleModule battleModule)
			{
				DamageableModule damageableModule = battleModule.BuildingParent.DamageableModule;
				if (damageableModule != null && damageableModule.IsDead)
				{
					goto IL_0082;
				}
			}
			ApplySkillEffectsOnTile(caster.OriginTile, caster, skillActionResultDatas, effect.SkillEffectDefinitions, caster is TheLastStand.Model.Unit.Unit, caster is BattleModule, forceApply: true);
			return skillActionResultDatas;
		}
		goto IL_0082;
		IL_0082:
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).LogError((object)("Tried to apply an action on a dead caster (" + ((IEntity)caster).UniqueIdentifier + "), skipping."), (CLogLevel)0, true, true);
		return skillActionResultDatas;
	}

	protected virtual void ApplySkillEffectsOnTile(Tile targetTile, ISkillCaster caster, SkillActionResultDatas resultDatas, List<SkillEffectDefinition> skillEffectDefinitions, bool hitsUnit, bool hitsBuilding, bool forceApply = false)
	{
		if (hitsUnit)
		{
			resultDatas.PotentiallyAffectedDamageables.Add(targetTile.Unit);
			{
				foreach (SkillEffectDefinition skillEffectDefinition in skillEffectDefinitions)
				{
					ApplySkillEffectToUnit(targetTile.Unit, caster, resultDatas, skillEffectDefinition);
				}
				return;
			}
		}
		if (!hitsBuilding)
		{
			return;
		}
		resultDatas.PotentiallyAffectedDamageables.Add(targetTile.Building.DamageableModule);
		foreach (SkillEffectDefinition skillEffectDefinition2 in skillEffectDefinitions)
		{
			ApplySkillEffectToBuilding(targetTile.Building, caster, resultDatas, skillEffectDefinition2);
		}
	}

	private void ApplySkillEffectToBuilding(TheLastStand.Model.Building.Building targetBuilding, ISkillCaster caster, SkillActionResultDatas resultDatas, SkillEffectDefinition skillEffectDefinition)
	{
		if (skillEffectDefinition is ExtinguishBrazierSkillEffectDefinition extinguishBrazierSkillEffectDefinition && targetBuilding.IsLitBrazier)
		{
			int num = targetBuilding.BrazierModule.BrazierModuleController.LoseBrazierPoints(extinguishBrazierSkillEffectDefinition.BrazierDamage);
			if (num > 0)
			{
				resultDatas.AddAffectedBuilding(targetBuilding);
				targetBuilding.BuildingView.ExtinguishBrazierFeedback.InitBrazierLoss(num, targetBuilding.BrazierModule.BrazierPoints);
				targetBuilding.BlueprintModule.BlueprintModuleController.AddEffectDisplay(targetBuilding.BuildingView.ExtinguishBrazierFeedback);
				EffectManager.Register(targetBuilding.BlueprintModule.BlueprintModuleController);
			}
		}
	}

	protected void ApplySkillEffectToUnit(TheLastStand.Model.Unit.Unit targetUnit, ISkillCaster caster, SkillActionResultDatas resultDatas, SkillEffectDefinition skillEffectDefinition)
	{
		if (targetUnit == null || (skillEffectDefinition is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition && !affectingUnitSkillEffectDefinition.AffectedUnits.ShouldDamageableBeAffected(caster, targetUnit)))
		{
			return;
		}
		PlayableUnit playableUnit = caster as PlayableUnit;
		TheLastStand.Model.Status.Status status = null;
		TheLastStand.Model.Status.Status.E_StatusType statusOwned = targetUnit.StatusOwned;
		if (!(skillEffectDefinition is BuffEffectDefinition buffEffectDefinition))
		{
			if (!(skillEffectDefinition is DebuffEffectDefinition debuffEffectDefinition))
			{
				if (!(skillEffectDefinition is KillSkillEffectDefinition killSkillEffectDefinition))
				{
					if (!(skillEffectDefinition is PoisonEffectDefinition poisonEffectDefinition))
					{
						if (!(skillEffectDefinition is StunEffectDefinition stunEffectDefinition))
						{
							if (!(skillEffectDefinition is ContagionEffectDefinition contagionEffectDefinition))
							{
								if (!(skillEffectDefinition is ChargedEffectDefinition chargedEffectDefinition))
								{
									if (!(skillEffectDefinition is RemoveStatusEffectDefinition removeStatusEffectDefinition))
									{
										if (!(skillEffectDefinition is ImmuneToNegativeStatusEffectDefinition immunityEffectDefinition))
										{
											if (!(skillEffectDefinition is RegenStatSkillEffectDefinition regenStatSkillEffectDefinition))
											{
												if (skillEffectDefinition is DecreaseStatSkillEffectDefinition decreaseStatSkillEffectDefinition)
												{
													if (decreaseStatSkillEffectDefinition.Stat == UnitStatDefinition.E_Stat.Mana)
													{
														targetUnit.UnitStatsController.DecreaseBaseStat(decreaseStatSkillEffectDefinition.Stat, decreaseStatSkillEffectDefinition.LossValue, includeChildStat: false);
														LoseManaDisplay loseManaDisplay = targetUnit.UnitView.SkillEffectDisplays.FirstOrDefault((IDisplayableEffect x) => x is LoseManaDisplay loseManaDisplay2 && !loseManaDisplay2.IsBeingDisplayed) as LoseManaDisplay;
														if ((Object)(object)loseManaDisplay == (Object)null)
														{
															loseManaDisplay = ObjectPooler.GetPooledComponent<LoseManaDisplay>("LoseManaDisplay", ResourcePooler.LoadOnce<LoseManaDisplay>("Prefab/Displayable Effect/UI Effect Displays/LoseManaDisplay", failSilently: false), EffectManager.EffectDisplaysParent, dontSetParent: false);
															loseManaDisplay.Init((int)decreaseStatSkillEffectDefinition.LossValue);
															targetUnit.UnitController.AddEffectDisplay(loseManaDisplay);
														}
														else
														{
															loseManaDisplay.AddManaLoss((int)decreaseStatSkillEffectDefinition.LossValue);
														}
													}
													else
													{
														((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).LogError((object)$"Trying to apply a DecreaseStat skill effect but the stat {decreaseStatSkillEffectDefinition.Stat} is not handled.", (CLogLevel)1, true, true);
													}
												}
											}
											else
											{
												switch (regenStatSkillEffectDefinition.Stat)
												{
												case UnitStatDefinition.E_Stat.Mana:
												case UnitStatDefinition.E_Stat.ActionPoints:
												case UnitStatDefinition.E_Stat.MovePoints:
												{
													targetUnit.UnitStatsController.IncreaseBaseStat(regenStatSkillEffectDefinition.Stat, regenStatSkillEffectDefinition.Bonus, includeChildStat: false);
													RestoreStatDisplay pooledComponent = ObjectPooler.GetPooledComponent<RestoreStatDisplay>("RestoreStatDisplay", ResourcePooler.LoadOnce<RestoreStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/RestoreStatDisplay", failSilently: false), EffectManager.EffectDisplaysParent, dontSetParent: false);
													pooledComponent.Init(regenStatSkillEffectDefinition.Stat, (int)regenStatSkillEffectDefinition.Bonus);
													targetUnit.UnitController.AddEffectDisplay(pooledComponent);
													break;
												}
												case UnitStatDefinition.E_Stat.Health:
												{
													float healAmount = targetUnit.UnitController.GainHealth(regenStatSkillEffectDefinition.Bonus, refreshHud: false);
													HealFeedback healFeedback = targetUnit.DamageableView.HealFeedback;
													healFeedback.AddHealInstance(healAmount, targetUnit.Health);
													targetUnit.UnitController.AddEffectDisplay(healFeedback);
													break;
												}
												default:
													((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).LogError((object)$"Trying to apply a RegenStat skill effect but the stat {regenStatSkillEffectDefinition.Stat} is not handled.", (CLogLevel)1, true, true);
													break;
												}
											}
										}
										else
										{
											status = ApplySkillEffectImmunity(caster, targetUnit, immunityEffectDefinition, resultDatas);
										}
									}
									else
									{
										ApplySkillEffectRemoveStatus(targetUnit, removeStatusEffectDefinition, resultDatas);
									}
								}
								else
								{
									status = ApplySkillEffectCharged(caster, targetUnit, chargedEffectDefinition, resultDatas);
								}
							}
							else
							{
								status = ApplySkillEffectContagion(caster, targetUnit, contagionEffectDefinition, resultDatas);
							}
						}
						else
						{
							status = ApplySkillEffectStun(caster, targetUnit, stunEffectDefinition, resultDatas);
						}
					}
					else
					{
						status = ApplySkillEffectPoison(ComputePoisonDamage(caster, poisonEffectDefinition), caster, targetUnit, poisonEffectDefinition, resultDatas);
					}
				}
				else
				{
					ApplySkillEffectKill(caster, targetUnit, killSkillEffectDefinition, resultDatas);
				}
			}
			else
			{
				status = ApplySkillEffectDebuff(caster, targetUnit, debuffEffectDefinition, resultDatas);
			}
		}
		else
		{
			status = ApplySkillEffectBuff(caster, targetUnit, buffEffectDefinition, resultDatas);
		}
		if (status != null)
		{
			SkillAction.SkillActionController.EnsurePerkData(null, targetUnit, null, SkillAction.Skill.SkillContainer is Perk, statusOwned, status);
			playableUnit?.Events.GetValueOrDefault(E_EffectTime.OnSkillStatusApplied)?.Invoke(SkillAction.PerkDataContainer);
		}
	}

	protected TheLastStand.Model.Status.Status ApplySkillEffectBuff(ISkillCaster caster, TheLastStand.Model.Unit.Unit targetUnit, BuffEffectDefinition buffEffectDefinition, SkillActionResultDatas resultDatas)
	{
		bool flag = RandomManager.GetRandomRange(this, 0f, 1f) < buffEffectDefinition.BaseChance;
		int num = caster.ComputeStatusDuration(buffEffectDefinition.StatusType, buffEffectDefinition.TurnsCount, SkillAction.PerkDataContainer);
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).Log((object)string.Format("Trying to apply buff (stat:{0}, value:{1}, turns count:{2}) on {3}, {4}.", buffEffectDefinition.Stat, buffEffectDefinition.Bonus, num, targetUnit.UniqueIdentifier, flag ? "succeed" : "failed"), (CLogLevel)0, false, false);
		if (!flag)
		{
			return null;
		}
		StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
		statusCreationInfo.Source = caster;
		statusCreationInfo.TurnsCount = num;
		statusCreationInfo.Value = buffEffectDefinition.ModifierValue;
		statusCreationInfo.Stat = buffEffectDefinition.Stat;
		StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
		TheLastStand.Model.Status.Status status = new BuffStatusController(targetUnit, statusCreationInfo2).Status;
		targetUnit.UnitController.AddStatus(status);
		resultDatas.AddAffectedUnit(targetUnit);
		if (SkillAction.Skill.SkillContainer is TheLastStand.Model.Item.Item item && item.ItemDefinition.Category == ItemDefinition.E_Category.BodyArmor && ((targetUnit is PlayableUnit && buffEffectDefinition.AffectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.PlayableUnit)) || (targetUnit == caster && caster is PlayableUnit && buffEffectDefinition.AffectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster))))
		{
			TrophyManager.AppendValueToTrophiesConditions<BodyArmorBuffUsedTrophyConditionController>(new object[2] { targetUnit.RandomId, 1 });
		}
		return status;
	}

	protected TheLastStand.Model.Status.Status ApplySkillEffectCharged(ISkillCaster caster, TheLastStand.Model.Unit.Unit targetUnit, ChargedEffectDefinition chargedEffectDefinition, SkillActionResultDatas resultDatas)
	{
		int num = caster.ComputeStatusDuration(chargedEffectDefinition.StatusType, chargedEffectDefinition.TurnsCount, SkillAction.PerkDataContainer);
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).Log((object)$"Trying to apply charged status (turns count:{num}) on {targetUnit.UniqueIdentifier}, succeed.", (CLogLevel)0, false, false);
		StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
		statusCreationInfo.Source = caster;
		statusCreationInfo.TurnsCount = num;
		StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
		TheLastStand.Model.Status.Status status = new ChargedStatusController(targetUnit, statusCreationInfo2).Status;
		targetUnit.UnitController.AddStatus(status);
		resultDatas.AddAffectedUnit(targetUnit);
		return status;
	}

	protected TheLastStand.Model.Status.Status ApplySkillEffectContagion(ISkillCaster caster, TheLastStand.Model.Unit.Unit targetUnit, ContagionEffectDefinition contagionEffectDefinition, SkillActionResultDatas resultDatas)
	{
		float baseChance = contagionEffectDefinition.BaseChance;
		float randomRange = RandomManager.GetRandomRange(this, 0f, 1f);
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).Log((object)("Trying to apply contagion on " + targetUnit.UniqueIdentifier + ", " + ((randomRange < baseChance) ? "succeed" : "failed") + "."), (CLogLevel)0, false, false);
		if (randomRange < baseChance)
		{
			int turnsCount = caster.ComputeStatusDuration(contagionEffectDefinition.StatusType, contagionEffectDefinition.TurnsCount, SkillAction.PerkDataContainer);
			StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
			statusCreationInfo.Source = caster;
			statusCreationInfo.TurnsCount = turnsCount;
			statusCreationInfo.Value = contagionEffectDefinition.Count;
			StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
			TheLastStand.Model.Status.Status status = new ContagionStatusController(targetUnit, statusCreationInfo2).Status;
			targetUnit.UnitController.AddStatus(status);
			resultDatas.AddAffectedUnit(targetUnit);
			return status;
		}
		return null;
	}

	protected TheLastStand.Model.Status.Status ApplySkillEffectDebuff(ISkillCaster caster, TheLastStand.Model.Unit.Unit targetUnit, DebuffEffectDefinition debuffEffectDefinition, SkillActionResultDatas resultDatas)
	{
		bool flag = RandomManager.GetRandomRange(this, 0f, 1f) < debuffEffectDefinition.BaseChance;
		int num = caster.ComputeStatusDuration(debuffEffectDefinition.StatusType, debuffEffectDefinition.TurnsCount, SkillAction.PerkDataContainer);
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).Log((object)string.Format("Trying to apply debuff (stat:{0}, value:{1}, turns count:{2}) on {3}, {4}.", debuffEffectDefinition.Stat, debuffEffectDefinition.Malus, num, targetUnit.UniqueIdentifier, flag ? "succeed" : "failed"), (CLogLevel)0, false, false);
		if (!flag)
		{
			return null;
		}
		StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
		statusCreationInfo.Source = caster;
		statusCreationInfo.TurnsCount = num;
		statusCreationInfo.Stat = debuffEffectDefinition.Stat;
		statusCreationInfo.Value = debuffEffectDefinition.ModifierValue;
		StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
		TheLastStand.Model.Status.Status status = new DebuffStatusController(targetUnit, statusCreationInfo2).Status;
		targetUnit.UnitController.AddStatus(status);
		resultDatas.AddAffectedUnit(targetUnit);
		if (caster is EnemyUnit enemyUnit && targetUnit is PlayableUnit playableUnit && enemyUnit.EnemyUnitTemplateDefinition.Id == "Ghost" && !playableUnit.PlayableUnitController.DebuffedByGhosts.Contains(enemyUnit.RandomId))
		{
			playableUnit.PlayableUnitController.DebuffedByGhosts.Add(enemyUnit.RandomId);
		}
		if (targetUnit is EnemyUnit && caster is PlayableUnit)
		{
			TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.DebuffsApplied, 1.0);
		}
		return status;
	}

	protected void ApplySkillEffectKill(ISkillCaster caster, TheLastStand.Model.Unit.Unit targetUnit, KillSkillEffectDefinition killSkillEffectDefinition, SkillActionResultDatas resultDatas)
	{
		resultDatas.AddAffectedUnit(targetUnit);
		PlayableUnit playableUnit = caster as PlayableUnit;
		AttackSkillActionExecutionTileData attackSkillActionExecutionTileData = new AttackSkillActionExecutionTileData
		{
			Damageable = targetUnit,
			TotalDamage = targetUnit.Health,
			HealthDamage = targetUnit.Health,
			IsInstantKill = true,
			TargetTile = targetUnit.OriginTile,
			TargetRemainingHealth = 0f,
			TargetHealthTotal = targetUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthTotal).FinalClamped
		};
		if (playableUnit != null)
		{
			playableUnit.LifetimeStats.LifetimeStatsController.IncreaseDamagesInflicted(attackSkillActionExecutionTileData.HealthDamageIncludingOverkill);
			TrophyManager.AppendValueToTrophiesConditions<DamageInflictedTrophyConditionController>(new object[2] { playableUnit.RandomId, attackSkillActionExecutionTileData.HealthDamageIncludingOverkill });
			TrophyManager.SetValueToTrophiesConditions<DamageInflictedSingleAttackTrophyConditionController>(new object[2] { playableUnit.RandomId, attackSkillActionExecutionTileData.HealthDamageIncludingOverkill });
			if (targetUnit.DamageableController.Damageable is EnemyUnit enemyUnit)
			{
				playableUnit.LifetimeStats.LifetimeStatsController.IncreaseDamagesInflictedToEnemyType(enemyUnit, attackSkillActionExecutionTileData.HealthDamageIncludingOverkill);
				if (targetUnit.DamageableController.Damageable is EliteEnemyUnit && SkillAction.Skill.SkillDefinition.Id == "FinishHim")
				{
					TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_FATALITY_ON_ELITE);
				}
			}
		}
		targetUnit.DamageableController.LoseHealth(attackSkillActionExecutionTileData.HealthDamage, caster, refreshHud: false);
		AttackFeedback attackFeedback = targetUnit.DamageableView.AttackFeedback;
		attackFeedback.AddDamageInstance(attackSkillActionExecutionTileData);
		targetUnit.DamageableController.AddEffectDisplay(attackFeedback);
		if (playableUnit != null)
		{
			EnsurePerkData(SkillAction.PerkDataContainer?.TargetTile ?? targetUnit.OriginTile, targetUnit, attackSkillActionExecutionTileData, SkillAction.Skill.SkillContainer is Perk);
			playableUnit.Events.GetValueOrDefault(E_EffectTime.OnTargetKilled)?.Invoke(SkillAction.PerkDataContainer);
		}
	}

	protected TheLastStand.Model.Status.Status ApplySkillEffectImmunity(ISkillCaster caster, TheLastStand.Model.Unit.Unit targetUnit, ImmuneToNegativeStatusEffectDefinition immunityEffectDefinition, SkillActionResultDatas resultDatas)
	{
		if (targetUnit == null || (targetUnit == caster && !immunityEffectDefinition.AffectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster)))
		{
			return null;
		}
		StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
		statusCreationInfo.Source = caster;
		statusCreationInfo.TurnsCount = caster.ComputeStatusDuration(immunityEffectDefinition.StatusType, immunityEffectDefinition.TurnsCount, SkillAction.PerkDataContainer);
		StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
		TheLastStand.Model.Status.Status status = new ImmunityStatusController(targetUnit, statusCreationInfo2, immunityEffectDefinition.StatusImmunity).Status;
		targetUnit.UnitController.AddStatus(status);
		resultDatas.AddAffectedUnit(targetUnit);
		return status;
	}

	protected TheLastStand.Model.Status.Status ApplySkillEffectPoison(float damage, ISkillCaster caster, TheLastStand.Model.Unit.Unit targetUnit, PoisonEffectDefinition poisonEffectDefinition, SkillActionResultDatas resultDatas)
	{
		float randomRange = RandomManager.GetRandomRange(this, 0f, 1f);
		int num = caster.ComputeStatusDuration(poisonEffectDefinition.StatusType, poisonEffectDefinition.TurnsCount, SkillAction.PerkDataContainer);
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).Log((object)string.Format("Trying to apply poison (dmg:{0}, turns count:{1}) on {2}, {3}.", damage, num, targetUnit.UniqueIdentifier, (randomRange < poisonEffectDefinition.BaseChance) ? "succeed" : "failed"), (CLogLevel)0, false, false);
		if (randomRange < poisonEffectDefinition.BaseChance)
		{
			StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
			statusCreationInfo.Source = caster;
			statusCreationInfo.TurnsCount = num;
			statusCreationInfo.Value = Mathf.CeilToInt(damage);
			StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
			TheLastStand.Model.Status.Status status = new PoisonStatusController(targetUnit, statusCreationInfo2).Status;
			targetUnit.UnitController.AddStatus(status);
			resultDatas.AddAffectedUnit(targetUnit);
			if (targetUnit is EnemyUnit)
			{
				TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.PoisonedEnemies, 1.0);
				if (TPSingleton<MetaConditionManager>.Instance.RunContext.GetDouble(MetaConditionSpecificContext.E_ValueCategory.PoisonedEnemies) >= 400.0)
				{
					TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_POISON_400_ENEMIES_RUN);
				}
			}
			return status;
		}
		return null;
	}

	protected bool ApplySkillEffectRemoveStatus(TheLastStand.Model.Unit.Unit targetUnit, RemoveStatusEffectDefinition removeStatusEffectDefinition, SkillActionResultDatas resultDatas)
	{
		float randomRange = RandomManager.GetRandomRange(this, 0f, 1f);
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).Log((object)("Trying to remove " + string.Join(", ", removeStatusEffectDefinition.RemoveStatusDefinition.Status) + " on " + targetUnit.UniqueIdentifier + ", " + ((randomRange < removeStatusEffectDefinition.RemoveStatusDefinition.BaseChance) ? "succeed" : "failed") + "."), (CLogLevel)0, false, false);
		if (randomRange < removeStatusEffectDefinition.RemoveStatusDefinition.BaseChance)
		{
			targetUnit.UnitController.RemoveStatus(removeStatusEffectDefinition.RemoveStatusDefinition.Status, refreshHud: false);
			resultDatas.AddAffectedUnit(targetUnit);
			if (removeStatusEffectDefinition.RemoveStatusDefinition.Status != TheLastStand.Model.Status.Status.E_StatusType.Charged)
			{
				DispelDisplay pooledComponent = ObjectPooler.GetPooledComponent<DispelDisplay>("DispelDisplay", ResourcePooler.LoadOnce<DispelDisplay>("Prefab/Displayable Effect/UI Effect Displays/DispelDisplay", failSilently: false), EffectManager.EffectDisplaysParent, dontSetParent: false);
				pooledComponent.Init(removeStatusEffectDefinition.RemoveStatusDefinition.Status);
				targetUnit.UnitController.AddEffectDisplay(pooledComponent);
			}
			return true;
		}
		return false;
	}

	protected TheLastStand.Model.Status.Status ApplySkillEffectStun(ISkillCaster caster, TheLastStand.Model.Unit.Unit targetUnit, StunEffectDefinition stunEffectDefinition, SkillActionResultDatas resultDatas)
	{
		if (targetUnit.IsStunned)
		{
			return null;
		}
		float stunChance = ((caster is TheLastStand.Model.Unit.Unit unit) ? unit.UnitController.GetModifiedStunChance(stunEffectDefinition.BaseChance) : stunEffectDefinition.BaseChance);
		float randomRange = RandomManager.GetRandomRange(this, 0f, 1f);
		float resistedStunChance = targetUnit.UnitController.GetResistedStunChance(stunChance);
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).Log((object)("Trying to apply stun on " + targetUnit.UniqueIdentifier + ", " + ((randomRange < resistedStunChance) ? "succeed" : "failed") + "."), (CLogLevel)0, false, false);
		if (randomRange < resistedStunChance)
		{
			StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
			statusCreationInfo.Source = caster;
			statusCreationInfo.TurnsCount = caster.ComputeStatusDuration(stunEffectDefinition.StatusType, stunEffectDefinition.TurnsCount, SkillAction.PerkDataContainer);
			StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
			TheLastStand.Model.Status.Status status = new StunStatusController(targetUnit, statusCreationInfo2).Status;
			targetUnit.UnitController.AddStatus(status);
			resultDatas.AddAffectedUnit(targetUnit);
			if (targetUnit is EnemyUnit)
			{
				TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.StunnedEnemies, 1.0);
				(caster as PlayableUnit)?.LifetimeStats.LifetimeStatsController.IncreaseStunnedEnemies();
			}
			return status;
		}
		return null;
	}

	public int ComputeManaCost(ISkillCaster overrideOwner = null, bool refreshPerkData = true)
	{
		ISkillCaster obj = overrideOwner ?? SkillAction.Skill.Owner;
		float num = SkillAction.Skill.BaseManaCost;
		if (refreshPerkData)
		{
			EnsurePerkData();
		}
		if (obj is PlayableUnit playableUnit)
		{
			if (playableUnit.IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.ManaCost))
			{
				return 0;
			}
			float perkModifierForComputationStat = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.ManaCost, SkillAction.PerkDataContainer);
			num -= perkModifierForComputationStat;
		}
		return (int)Mathf.Max(0f, num);
	}

	public int ComputeActionPointsCost(ISkillCaster overrideOwner = null, bool refreshPerkData = true)
	{
		ISkillCaster obj = overrideOwner ?? SkillAction.Skill.Owner;
		float num = SkillAction.Skill.BaseActionPointsCost;
		if (refreshPerkData)
		{
			EnsurePerkData();
		}
		if (obj is PlayableUnit playableUnit)
		{
			float perkModifierForComputationStat = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.ActionPointsCost, SkillAction.PerkDataContainer);
			num -= perkModifierForComputationStat;
		}
		return (int)Mathf.Max(0f, num);
	}

	public int ComputeMovePointsCost(ISkillCaster overrideOwner = null, bool refreshPerkData = true)
	{
		ISkillCaster obj = overrideOwner ?? SkillAction.Skill.Owner;
		float num = SkillAction.Skill.SkillDefinition.MovePointsCost;
		if (refreshPerkData)
		{
			EnsurePerkData();
		}
		if (obj is PlayableUnit playableUnit)
		{
			float perkModifierForComputationStat = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.MovePointsCost, SkillAction.PerkDataContainer);
			num -= perkModifierForComputationStat;
		}
		return (int)Mathf.Max(0f, num);
	}

	public int ComputeBaseHealthCost()
	{
		float num = SkillAction.Skill.SkillDefinition.HealthCost;
		if (SkillAction.Skill.Owner is PlayableUnit playableUnit)
		{
			float perkModifierForComputationStat = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.HealthCost, SkillAction.PerkDataContainer, true);
			num -= perkModifierForComputationStat;
		}
		return (int)Mathf.Max(0f, num);
	}

	public int ComputeHealthCost(ISkillCaster overrideOwner = null, bool refreshPerkData = true)
	{
		ISkillCaster obj = overrideOwner ?? SkillAction.Skill.Owner;
		float num = SkillAction.Skill.BaseHealthCost;
		if (refreshPerkData)
		{
			EnsurePerkData();
		}
		if (obj is PlayableUnit playableUnit)
		{
			float perkModifierForComputationStat = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.HealthCost, SkillAction.PerkDataContainer, false);
			num -= perkModifierForComputationStat;
		}
		return (int)Mathf.Max(0f, num);
	}

	public int ComputeUsesPerTurn(ISkillCaster overrideOwner = null, bool refreshPerkData = true)
	{
		ISkillCaster skillCaster = overrideOwner ?? SkillAction.Skill.Owner;
		int usesPerTurnCount = SkillAction.Skill.SkillDefinition.UsesPerTurnCount;
		if (refreshPerkData)
		{
			EnsurePerkData();
		}
		if (SkillAction.Skill.SkillDefinition.UsesPerTurnCount != -1 && skillCaster is PlayableUnit playableUnit && SkillAction.Skill.SkillContainer is TheLastStand.Model.Item.Item item && item.ItemDefinition.IsHandItem)
		{
			return usesPerTurnCount + (int)playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.BonusSkillUses).FinalClamped;
		}
		return usesPerTurnCount;
	}

	protected float ComputePoisonDamage(ISkillCaster caster, PoisonEffectDefinition poisonEffectDefinition)
	{
		float num = poisonEffectDefinition.DamagePerTurn;
		if (caster is TheLastStand.Model.Unit.Unit unit)
		{
			num = unit.UnitController.GetModifiedPoisonDamage(poisonEffectDefinition.DamagePerTurn);
		}
		if (SkillAction.Skill.SkillContainer is TheLastStand.Model.Item.Item item)
		{
			num *= SkillDatabase.PoisonDamageScaleDefinition.GetMultiplierAtLevel(item.Level);
		}
		if (caster is PlayableUnit playableUnit)
		{
			float num2 = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.PoisonDamageModifier, SkillAction.PerkDataContainer) / 100f;
			num += num * num2;
		}
		return num;
	}

	private bool IsDamageableAlreadyHit(List<SkillActionResultDatas> resultDatas, Tile affectedTile)
	{
		if (affectedTile.Damageable == null)
		{
			return false;
		}
		foreach (SkillActionResultDatas resultData in resultDatas)
		{
			if (affectedTile.Unit != null)
			{
				if (resultData.AffectedUnits.Contains(affectedTile.Unit))
				{
					return true;
				}
			}
			else if (affectedTile.Building != null && resultData.AffectedBuildings.Contains(affectedTile.Building))
			{
				return true;
			}
		}
		return false;
	}
}
