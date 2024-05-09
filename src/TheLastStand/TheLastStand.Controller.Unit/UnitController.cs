using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Controller.Status;
using TheLastStand.Controller.Status.Immunity;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.Status;
using TheLastStand.Model.Status.Immunity;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.Controller.Unit;

public abstract class UnitController : ITileObjectController, IDamageableController, IEffectTargetSkillActionController, ISkillCasterController
{
	private Coroutine displayEffectsCoroutine;

	private Coroutine waitDisplayEffectsCoroutine;

	private Coroutine displayEffectCoroutine;

	public IDamageable Damageable => Unit;

	public Task MoveTask { get; private set; }

	public ISkillCaster SkillCaster => Unit;

	public TheLastStand.Model.Unit.Unit Unit { get; protected set; }

	public static Vector2Int ComputeResistanceDamageOffsetRange(Vector2Int damageRange, float resistance)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return Vector2Int.RoundToInt(Vector2Int.op_Implicit(damageRange) * (resistance * 0.01f));
	}

	public static Vector2Int GetVector2IntPercentage(Vector2Int range, float percentage)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Vector2Int.RoundToInt(Vector2Int.op_Implicit(range) * percentage);
	}

	public void AddEffectDisplay(IDisplayableEffect displayableEffect)
	{
		if (displayableEffect != null)
		{
			Unit.UnitView.AddSkillEffectDisplay(displayableEffect);
			EffectManager.Register(this);
		}
	}

	public bool AddStatus(TheLastStand.Model.Status.Status status, bool refreshHud = false)
	{
		if (IsImmuneTo(status))
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < Unit.StatusList.Count; i++)
		{
			if (Unit.StatusList[i].StatusController.TryMergeStatus(status))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Unit.StatusList.Add(status);
		}
		TheLastStand.Model.Status.Status.E_StatusType statusOwned = Unit.StatusOwned;
		Unit.StatusOwned |= status.StatusType;
		Unit.UnitStatsController.OnStatusAdded(status);
		TPSingleton<AchievementManager>.Instance.HandleStatusAdded(Unit, status);
		ApplyStatModifierStatusToChildStat(status as StatModifierStatus, isStatusRemoved: false, refreshHud);
		UpdatePoisonFeedbackCondition();
		PlayableUnit playableUnit = status.Source as PlayableUnit;
		if (!(status is DebuffStatus))
		{
			if (!(status is ImmunityStatus))
			{
				if (!(status is PoisonStatus))
				{
					if (status is StunStatus && status.Unit is EnemyUnit)
					{
						goto IL_01b0;
					}
				}
				else if (status.Unit is EnemyUnit)
				{
					goto IL_01b0;
				}
			}
			else
			{
				for (int num = Unit.StatusList.Count - 1; num >= 0; num--)
				{
					TheLastStand.Model.Status.Status status2 = Unit.StatusList[num];
					if (IsImmuneTo(status2))
					{
						RemoveStatus(status2);
					}
				}
			}
		}
		else if (status.Unit is EnemyUnit enemyUnit && playableUnit != null)
		{
			TrophyManager.AppendValueToTrophiesConditions<EnemiesDebuffedSeveralTimesSingleTurnTrophyConditionController>(new object[3]
			{
				TPSingleton<GameManager>.Instance.Game.CurrentNightHour,
				playableUnit.RandomId,
				enemyUnit.RandomId
			});
		}
		goto IL_01c1;
		IL_01b0:
		PanicManager.Panic.PanicController.ComputeExpectedValue(updateView: true);
		goto IL_01c1;
		IL_01c1:
		if (TheLastStand.Model.Status.Status.E_StatusType.AllNegative.HasFlag(status.StatusType) && status.Unit is EnemyUnit && playableUnit != null)
		{
			TrophyManager.AppendValueToTrophiesConditions<StatusInflictedTrophyConditionController>(new object[3] { playableUnit.RandomId, status.StatusType, 1 });
		}
		if ((Unit.StatusOwned & status.StatusType) != 0 && refreshHud)
		{
			Unit.UnitView.RefreshStatus();
		}
		bool result = !status.HideDisplayEffect && status.StatusController.CreateEffectDisplay(Damageable.DamageableController);
		if (playableUnit != null)
		{
			PerkDataContainer obj = new PerkDataContainer
			{
				TargetDamageable = Unit,
				StatusApplied = status,
				TargetUnitPreviousStatuses = statusOwned,
				IsTriggeredByPerk = status.IsFromPerk
			};
			Dictionary<E_EffectTime, Action<PerkDataContainer>> events = playableUnit.Events;
			if (events == null)
			{
				return result;
			}
			Action<PerkDataContainer> valueOrDefault = events.GetValueOrDefault(E_EffectTime.OnStatusApplied);
			if (valueOrDefault == null)
			{
				return result;
			}
			valueOrDefault(obj);
		}
		return result;
	}

	public void ApplyContagionToAdjacentUnits()
	{
		if ((Unit.StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.Contagion) == 0)
		{
			return;
		}
		for (int num = Unit.StatusList.Count - 1; num >= 0; num--)
		{
			if (Unit.StatusList[num].StatusType == TheLastStand.Model.Status.Status.E_StatusType.Contagion)
			{
				(Unit.StatusList[num].StatusController as ContagionStatusController).ApplyContagion();
			}
		}
	}

	public virtual bool IsImmuneTo(TheLastStand.Model.Status.Status status)
	{
		return status.StatusType switch
		{
			TheLastStand.Model.Status.Status.E_StatusType.Debuff => Unit.StatusOwned.HasFlag(TheLastStand.Model.Status.Status.E_StatusType.DebuffImmunity), 
			TheLastStand.Model.Status.Status.E_StatusType.Poison => Unit.StatusOwned.HasFlag(TheLastStand.Model.Status.Status.E_StatusType.PoisonImmunity), 
			TheLastStand.Model.Status.Status.E_StatusType.Stun => Unit.StatusOwned.HasFlag(TheLastStand.Model.Status.Status.E_StatusType.StunImmunity), 
			TheLastStand.Model.Status.Status.E_StatusType.Contagion => Unit.StatusOwned.HasFlag(TheLastStand.Model.Status.Status.E_StatusType.ContagionImmunity), 
			TheLastStand.Model.Status.Status.E_StatusType.AllNegative => Unit.StatusOwned.HasFlag(TheLastStand.Model.Status.Status.E_StatusType.AllNegativeImmunity), 
			_ => false, 
		};
	}

	public float ComputeInjuryThresholdRatio(InjuryDefinition injuryDefinition)
	{
		float finalClamped = Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthTotal).FinalClamped;
		float num = (finalClamped - injuryDefinition.BaseHealth) * injuryDefinition.RatioMultiplier;
		return Mathf.Min((injuryDefinition.BaseThreshold + num) / finalClamped, injuryDefinition.BaseRatio);
	}

	public void DisplayEffects(float delay = 0f)
	{
		if (displayEffectsCoroutine != null)
		{
			waitDisplayEffectsCoroutine = ((MonoBehaviour)TPSingleton<GameManager>.Instance).StartCoroutine(WaitCoroutineIsNullThenStartItAgain(delay));
		}
		else
		{
			displayEffectsCoroutine = ((MonoBehaviour)TPSingleton<GameManager>.Instance).StartCoroutine(DisplayEffectsCoroutine(delay));
		}
	}

	private IEnumerator WaitCoroutineIsNullThenStartItAgain(float delay)
	{
		while (displayEffectsCoroutine != null)
		{
			yield return null;
		}
		if ((Object)(object)Unit.UnitView != (Object)null)
		{
			displayEffectsCoroutine = ((MonoBehaviour)TPSingleton<GameManager>.Instance).StartCoroutine(DisplayEffectsCoroutine(0f));
		}
	}

	public virtual void EndTurn()
	{
		bool flag = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits;
		bool flag2 = flag == this is PlayableUnitController;
		for (int num = Unit.StatusList.Count - 1; num >= 0; num--)
		{
			TheLastStand.Model.Status.Status status = Unit.StatusList[num];
			if ((status.StatusEffectTime == TheLastStand.Model.Status.Status.E_StatusTime.EndMyTurn && flag2) || (status.StatusEffectTime == TheLastStand.Model.Status.Status.E_StatusTime.EndPlayerTurn && flag) || (status.StatusEffectTime == TheLastStand.Model.Status.Status.E_StatusTime.EndEnemyTurn && !flag))
			{
				AddEffectDisplay(status.StatusController.ApplyStatus());
			}
			if (((status.StatusDestructionTime == TheLastStand.Model.Status.Status.E_StatusTime.EndMyTurn && flag2) || (status.StatusDestructionTime == TheLastStand.Model.Status.Status.E_StatusTime.EndPlayerTurn && flag) || (status.StatusDestructionTime == TheLastStand.Model.Status.Status.E_StatusTime.EndEnemyTurn && !flag)) && status.RemainingTurnsCount != -1 && --status.RemainingTurnsCount <= 0)
			{
				RemoveStatus(status);
			}
		}
		if (!Unit.IsDead && Unit.StatusOwned.HasFlag(TheLastStand.Model.Status.Status.E_StatusType.Poison))
		{
			UpdateInjuryStage();
		}
	}

	public void ExecuteExile()
	{
		if (Unit.FinalizeDeathWhenNeededCoroutine != null)
		{
			((MonoBehaviour)TPSingleton<GameManager>.Instance).StopCoroutine(Unit.FinalizeDeathWhenNeededCoroutine);
			Unit.FinalizeDeathWhenNeededCoroutine = null;
		}
		if (Unit.ExileForcePlayDieAnim)
		{
			Unit.UnitView.PlayDieAnim();
		}
		FinalizeDeath();
	}

	public void FillArmor()
	{
		float finalClamped = Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ArmorTotal).FinalClamped;
		float num = Unit.UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.Armor, finalClamped, includeChildStat: false, refreshHud: false);
		if (num > 0f)
		{
			GainArmorFeedback gainArmorFeedback = Unit.DamageableView.GainArmorFeedback;
			gainArmorFeedback.AddArmorGainInstance(num, Unit.Armor);
			AddEffectDisplay(gainArmorFeedback);
		}
	}

	public virtual void FilterTilesInRange(TilesInRangeInfos tilesInRangeInfos, List<Tile> skillSourceTiles)
	{
	}

	public void FreeOccupiedTiles()
	{
		foreach (Tile occupiedTile in Unit.OccupiedTiles)
		{
			if (occupiedTile.Unit == Unit)
			{
				occupiedTile.TileController.SetUnit(null);
			}
		}
	}

	public virtual float GainHealth(float amount, bool refreshHud = true)
	{
		amount *= Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.HealingReceived) * 0.01f;
		amount = Mathf.Floor(amount);
		float result = Unit.UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.Health, amount, includeChildStat: false, refreshHud);
		UpdateInjuryStage(refreshHud);
		UpdatePoisonFeedbackCondition();
		return result;
	}

	public List<Tile> GetAdjacentTiles()
	{
		HashSet<Tile> tiles = new HashSet<Tile>();
		foreach (Tile occupiedTile in Unit.OccupiedTiles)
		{
			occupiedTile.GetAdjacentTiles().ForEach(delegate(Tile o)
			{
				if (!Unit.OccupiedTiles.Contains(o))
				{
					tiles.Add(o);
				}
			});
		}
		return tiles.ToList();
	}

	public List<Tile> GetAdjacentTilesWithDiagonals()
	{
		HashSet<Tile> tiles = new HashSet<Tile>();
		foreach (Tile occupiedTile in Unit.OccupiedTiles)
		{
			occupiedTile.GetAdjacentTilesWithDiagonals().ForEach(delegate(Tile o)
			{
				if (!Unit.OccupiedTiles.Contains(o))
				{
					tiles.Add(o);
				}
			});
		}
		return tiles.ToList();
	}

	public int GetEffectsCount()
	{
		return Unit.UnitView.SkillEffectDisplays.Count;
	}

	public int GetModifiedMaxRange(TheLastStand.Model.Skill.Skill skill, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (skill.SkillDefinition.RangeModifiable)
		{
			num2 = (int)Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.SkillRangeModifier);
			num2 += (int)(statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.SkillRangeModifier) ?? 0f);
		}
		if (Unit is PlayableUnit playableUnit && skill.SkillContainer is TheLastStand.Model.Item.Item item)
		{
			if (item.ItemDefinition.HasTag("Gauntlet"))
			{
				num3 += (int)playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.GauntletRangeModifier).FinalClamped;
				num3 += (int)(statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.GauntletRangeModifier) ?? 0f);
			}
			if (item.ItemDefinition.Category == ItemDefinition.E_Category.Potion)
			{
				num += (int)playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PotionRangeModifier).FinalClamped;
				num += (int)(statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.PotionRangeModifier) ?? 0f);
			}
		}
		Vector2Int range = skill.SkillDefinition.Range;
		int num4 = ((Vector2Int)(ref range)).y + num + num3 + num2;
		range = skill.SkillDefinition.Range;
		return Mathf.Max(num4, ((Vector2Int)(ref range)).x);
	}

	public int GetModifiedMultiHitsCount(int multiHitsCount, float? statModifier = null)
	{
		return multiHitsCount + (int)Unit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.MultiHitsCountModifier, statModifier);
	}

	public float GetModifiedPoisonDamage(float skillBaseDamage, float? statModifier = null)
	{
		return Mathf.Round(skillBaseDamage * (Unit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.PoisonDamageModifier, statModifier) / 100f));
	}

	public int GetModifiedPropagationsCount(int skillPropagationsCount, float? statModifier = null)
	{
		return skillPropagationsCount + (int)Unit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.PropagationBouncesModifier, statModifier);
	}

	public float GetModifiedStunChance(float skillBaseChance, float? statModifier = null)
	{
		return Mathf.Max(Mathf.Round(skillBaseChance * 100f + Unit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.StunChanceModifier, statModifier)) / 100f, 0f);
	}

	public float GetResistedStunChance(float stunChance, float? statModifier = null)
	{
		return Mathf.Max(Mathf.Round(stunChance * 100f - Unit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.StunResistance, statModifier)) / 100f, 0f);
	}

	public List<Tile> GetTilesInRange(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return Unit.OccupiedTiles.GetTilesInRange(maxRange, minRange, cardinalOnly);
	}

	public Dictionary<Tile, Tile> GetTilesInRangeWithClosestOccupiedTile(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return Unit.OccupiedTiles.GetTilesInRangeWithClosestOccupiedTile(maxRange, minRange, cardinalOnly);
	}

	public void LookAt(Tile targetTile, Tile sourceTile = null)
	{
		if (sourceTile == null)
		{
			sourceTile = Unit.OriginTile;
		}
		LookAtDirection(TileMapController.GetDirectionBetweenTiles(sourceTile, targetTile));
	}

	public void LookAtDirection(GameDefinition.E_Direction direction)
	{
		if (Unit.LookDirection != direction && direction != GameDefinition.E_Direction.None)
		{
			Unit.LookDirection = direction;
			Unit.UnitView.LookAtDirection(Unit.LookDirection);
		}
	}

	public virtual void LoseArmor(float amount, ISkillCaster attacker = null, bool refreshHud = true)
	{
		if (Unit.State != TheLastStand.Model.Unit.Unit.E_State.Dead)
		{
			Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.Armor, amount, includeChildStat: false, refreshHud);
		}
	}

	public virtual void LoseHealth(float amount, ISkillCaster attacker = null, bool refreshHud = true)
	{
		if (Unit.State != TheLastStand.Model.Unit.Unit.E_State.Dead)
		{
			Unit.UnitView.IsTakingDamage = true;
		}
	}

	public virtual void OnHit(ISkillCaster attacker)
	{
	}

	public void OnUnitHUDAnimatedDisplayFinished()
	{
		Unit.UnitView.RefreshHealth();
		Unit.UnitView.RefreshArmor();
		Unit.UnitView.RefreshStatus();
		Unit.UnitView.RefreshInjuryStage();
		Unit.UnitView.UnitHUD.DisplayIconAndTileFeedback(show: true);
		Unit.UnitView.UnitHUD.AnimatedDisplayFinishEvent -= OnUnitHUDAnimatedDisplayFinished;
	}

	public virtual void PaySkillCost(TheLastStand.Model.Skill.Skill skill)
	{
	}

	public void PlaySkillCastAnim(SkillActionExecution skillExecution)
	{
		Unit.UnitView.PlaySkillCastAnim(skillExecution);
	}

	public virtual void PrepareForDeath(ISkillCaster killer = null)
	{
		if (Unit.State != TheLastStand.Model.Unit.Unit.E_State.Dead)
		{
			Unit.Events.GetValueOrDefault(E_EffectTime.OnDeath)?.Invoke(null);
			Unit.State = TheLastStand.Model.Unit.Unit.E_State.Dead;
			TPSingleton<EnemyUnitManager>.Instance.DyingEnemiesWithContagion.Add(Unit);
			FreeOccupiedTiles();
			TileMapView.SetTiles(TileMapView.UnitFeedbackTilemap, Unit.OccupiedTiles);
			(killer as PlayableUnit)?.LifetimeStats.LifetimeStatsController.IncreaseKills(1, Unit.IsIsolated);
			if (killer is BattleModule caster)
			{
				PerkDataContainer data = new PerkDataContainer
				{
					Caster = caster,
					TargetDamageable = Unit
				};
				TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnDefenseTargetKilled, data);
			}
			if (!Unit.HasBeenExiled && !Unit.ExileForcePlayDieAnim)
			{
				Unit.FinalizeDeathWhenNeededCoroutine = ((MonoBehaviour)TPSingleton<GameManager>.Instance).StartCoroutine(FinalizeDeathWhenNeeded());
			}
		}
	}

	public void PrepareForExile(bool forcePlayDieAnim = false)
	{
		Unit.HasBeenExiled = true;
		Unit.ExileForcePlayDieAnim = forcePlayDieAnim;
		Unit.UnitController.PrepareForDeath();
	}

	public int ReduceIncomingDamage(int incomingDamage, TheLastStand.Model.Unit.Unit source, AttackSkillActionDefinition.E_AttackType attackType, bool checkBlock, out int blockedDamage, bool updateLifetimeStats = false)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = ReduceIncomingDamage(new Vector2Int(incomingDamage, incomingDamage), source, attackType, checkBlock, out blockedDamage, updateLifetimeStats);
		return ((Vector2Int)(ref val)).x;
	}

	public Vector2Int ReduceIncomingDamage(Vector2Int incomingDamage, TheLastStand.Model.Unit.Unit source, AttackSkillActionDefinition.E_AttackType attackType, bool checkBlock, out int blockedDamage, bool updateLifetimeStats = false)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		float resistanceReduction = source?.GetClampedStatValue(UnitStatDefinition.E_Stat.ResistanceReduction) ?? 0f;
		float reductionPercentage = source?.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).ClampStatValue(source.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).FinalClamped + ((attackType == AttackSkillActionDefinition.E_AttackType.Magical) ? UnitDatabase.MagicDamagePercentageResistanceReduction : 0f)) ?? 0f;
		float reducedResistance = Unit.GetReducedResistance(resistanceReduction, reductionPercentage);
		Vector2Int val = ComputeResistanceDamageOffsetRange(incomingDamage, reducedResistance);
		incomingDamage -= val;
		blockedDamage = 0;
		if (checkBlock)
		{
			incomingDamage = ReduceIncomingDamageWithBlock(incomingDamage, updateLifetimeStats, out var _, out var blockedDamageValue);
			blockedDamage = blockedDamageValue;
		}
		return incomingDamage;
	}

	public Vector2Int ComputeResistanceReductionRange(Vector2Int incomingDamage, TheLastStand.Model.Unit.Unit source, AttackSkillActionDefinition.E_AttackType attackType, PerkDataContainer perkDataContainer = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int zero = Vector2Int.zero;
		float num = source?.GetClampedStatValue(UnitStatDefinition.E_Stat.ResistanceReduction) ?? 0f;
		float num2 = source?.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).ClampStatValue(source.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).FinalClamped + ((attackType == AttackSkillActionDefinition.E_AttackType.Magical) ? UnitDatabase.MagicDamagePercentageResistanceReduction : 0f)) ?? 0f;
		float num3 = 0f;
		float num4 = 0f;
		if (source is PlayableUnit playableUnit)
		{
			num3 = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.FlatResistanceReduction, perkDataContainer);
			num4 = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.PercentageResistanceReduction, perkDataContainer);
		}
		float reducedResistance = Unit.GetReducedResistance(num + num3, num2 + num4);
		Vector2Int val = ComputeResistanceDamageOffsetRange(incomingDamage, reducedResistance);
		return zero - val;
	}

	public Vector2Int ComputeBlockReductionRange(bool checkBlock, bool updateLifetimeStats = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = Vector2Int.zero;
		if (checkBlock)
		{
			val += ReduceIncomingDamageWithBlock(Vector2Int.zero, updateLifetimeStats, out var _, out var _);
		}
		return val;
	}

	protected virtual Vector2Int ReduceIncomingDamageWithBlock(Vector2Int incomingDamage, bool updateLifetimeStats, out int blockValue, out int blockedDamageValue)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		blockValue = (int)Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Block);
		blockedDamageValue = Mathf.Min(((Vector2Int)(ref incomingDamage)).x, blockValue);
		return incomingDamage - Vector2Int.one * blockValue;
	}

	public virtual void RefreshStats()
	{
		Unit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.Health, Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Health).Base);
		Unit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.Armor, Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Armor).Base);
		Unit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.MovePoints, Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).Base);
	}

	public virtual Task PrepareForMovement(bool playWalkAnim = true, bool followPathOrientation = true, float moveSpeed = -1f, float delay = 0f, bool isMovementInstant = false)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		Unit.UnitController.SetTile(Unit.Path[^1]);
		MoveTask = Unit.UnitView.CreateMoveTask(followPathOrientation, moveSpeed, delay, isMovementInstant);
		Task moveTaskBuffer = MoveTask;
		Task moveTask = MoveTask;
		moveTask.OnCompleteAction = (UnityAction)Delegate.Combine((Delegate?)(object)moveTask.OnCompleteAction, (Delegate?)(UnityAction)delegate
		{
			if (MoveTask == moveTaskBuffer)
			{
				MoveTask = null;
			}
		});
		if (playWalkAnim)
		{
			Task moveTask2 = MoveTask;
			moveTask2.OnStartAction = (UnityAction)Delegate.Combine((Delegate?)(object)moveTask2.OnStartAction, (Delegate?)(UnityAction)delegate
			{
				Unit.UnitView.PlayWalkAnim(doWalk: true);
			});
			Task moveTask3 = MoveTask;
			moveTask3.OnCompleteAction = (UnityAction)Delegate.Combine((Delegate?)(object)moveTask3.OnCompleteAction, (Delegate?)(UnityAction)delegate
			{
				Unit.UnitView.PlayWalkAnim(doWalk: false);
			});
		}
		return MoveTask;
	}

	public void RemoveStatus(TheLastStand.Model.Status.Status status, bool refreshHud = true)
	{
		Unit.StatusList.Remove(status);
		Unit.UnitStatsController.OnStatusRemoved(status);
		ApplyStatModifierStatusToChildStat(status as StatModifierStatus, isStatusRemoved: true, refreshHud);
		UpdatePoisonFeedbackCondition();
		if (!Unit.StatusList.Any((TheLastStand.Model.Status.Status o) => o.StatusType == status.StatusType))
		{
			Unit.StatusOwned &= ~status.StatusType;
			Unit.UnitView.RefreshStatus();
		}
		if (!(status is PoisonStatus))
		{
			if (!(status is StunStatus))
			{
				return;
			}
			if (!(status.Unit is EnemyUnit))
			{
				if (status.RemainingTurnsCount == 0 && status.Unit is PlayableUnit targetUnit)
				{
					SkillManager.AddStatus(targetUnit, TheLastStand.Model.Status.Status.E_StatusType.StunImmunity, new StatusCreationInfo
					{
						TurnsCount = 1
					});
				}
				return;
			}
		}
		else if (!(status.Unit is EnemyUnit))
		{
			return;
		}
		PanicManager.Panic.PanicController.ComputeExpectedValue(updateView: true);
	}

	public void RemoveStatus(TheLastStand.Model.Status.Status.E_StatusType statusType, bool refreshHud = true)
	{
		for (int num = Unit.StatusList.Count - 1; num >= 0; num--)
		{
			TheLastStand.Model.Status.Status status = Unit.StatusList[num];
			if (statusType.HasFlag(status.StatusType))
			{
				RemoveStatus(status, refreshHud);
			}
		}
		if (statusType == TheLastStand.Model.Status.Status.E_StatusType.Charged)
		{
			StyledKeyDisplay pooledComponent = ObjectPooler.GetPooledComponent<StyledKeyDisplay>("StyledKeyDisplay", ResourcePooler.LoadOnce<StyledKeyDisplay>("Prefab/Displayable Effect/UI Effect Displays/StyledKeyDisplay", failSilently: false), EffectManager.EffectDisplaysParent, dontSetParent: false);
			pooledComponent.Init("Discharge", "StatusName_Discharge");
			AddEffectDisplay(pooledComponent);
		}
	}

	public void RemoveAllStatuses()
	{
		for (int num = Unit.StatusList.Count - 1; num >= 0; num--)
		{
			RemoveStatus(Unit.StatusList[num]);
		}
	}

	public virtual void SetTile(Tile tile)
	{
		if (tile != null && tile.Unit == Unit)
		{
			return;
		}
		if (Unit.OriginTile != null && Unit.OriginTile.Unit == Unit)
		{
			Unit.OccupiedTiles.ForEach(delegate(Tile t)
			{
				t.TileController.SetUnit(null);
			});
		}
		Unit.OriginTile = tile;
		if (Unit.OriginTile != null)
		{
			Unit.OccupiedTiles.ForEach(delegate(Tile t)
			{
				t.TileController.SetUnit(Unit);
			});
		}
	}

	public float SpendMovePoints(float value, bool refreshHud = true)
	{
		return Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.MovePoints, value, includeChildStat: false, refreshHud);
	}

	public virtual void StartTurn()
	{
		if (Unit.IsDead)
		{
			return;
		}
		bool flag = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits;
		bool flag2 = flag == this is PlayableUnitController;
		bool flag3 = Unit.StatusOwned.HasFlag(TheLastStand.Model.Status.Status.E_StatusType.Poison);
		for (int num = Unit.StatusList.Count - 1; num >= 0; num--)
		{
			TheLastStand.Model.Status.Status status = Unit.StatusList[num];
			if ((status.StatusEffectTime == TheLastStand.Model.Status.Status.E_StatusTime.StartMyTurn && flag2) || (status.StatusEffectTime == TheLastStand.Model.Status.Status.E_StatusTime.StartPlayerTurn && flag) || (status.StatusEffectTime == TheLastStand.Model.Status.Status.E_StatusTime.StartEnemyTurn && !flag))
			{
				AddEffectDisplay(status.StatusController.ApplyStatus());
			}
			if (((status.StatusDestructionTime == TheLastStand.Model.Status.Status.E_StatusTime.StartMyTurn && flag2) || (status.StatusDestructionTime == TheLastStand.Model.Status.Status.E_StatusTime.StartPlayerTurn && flag) || (status.StatusDestructionTime == TheLastStand.Model.Status.Status.E_StatusTime.StartEnemyTurn && !flag)) && status.RemainingTurnsCount != -1 && --status.RemainingTurnsCount <= 0)
			{
				RemoveStatus(status);
			}
		}
		if (flag2 && !Unit.IsDead)
		{
			if (flag3)
			{
				UpdateInjuryStage();
			}
			FillArmor();
		}
		if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			Unit.LastSkillType = AttackSkillActionDefinition.E_AttackType.None;
		}
	}

	public void UpdateInjuryStage(bool updateHud = true)
	{
		if (Unit?.UnitTemplateDefinition.InjuryDefinitions == null)
		{
			return;
		}
		int injuryStage = Unit.UnitStatsController.UnitStats.InjuryStage;
		Unit.UnitStatsController.ResetInjuries();
		for (int i = 0; i < Unit.UnitTemplateDefinition.InjuryDefinitions.Count; i++)
		{
			InjuryDefinition injuryDefinition = Unit.UnitTemplateDefinition.InjuryDefinitions[i];
			if (ComputeInjuryThresholdRatio(injuryDefinition) >= Unit.Health / Unit.HealthTotal)
			{
				Unit.UnitStatsController.AddInjury(injuryDefinition);
			}
		}
		if (Unit.UnitStatsController.UnitStats.InjuryStage != injuryStage)
		{
			if (this is EnemyUnitController enemyUnitController && enemyUnitController.EnemyUnit.ShouldCausePanic)
			{
				PanicManager.Panic.PanicController.ComputeExpectedValue(updateView: true);
			}
			if (Unit.UnitStatsController.UnitStats.InjuryStage > injuryStage)
			{
				TriggerInjuryStageChangeCallback(injuryStage, Unit.UnitStatsController.UnitStats.InjuryStage);
			}
		}
		if (updateHud)
		{
			Unit.UnitView?.RefreshInjuryStage();
		}
	}

	protected float Distance(ITileObject target)
	{
		return TileMapController.DistanceBetweenTiles(target.OriginTile, Unit.OriginTile);
	}

	protected virtual void OnDeath()
	{
		EffectManager.Unregister(this);
		Unit.UnitView.UnitHUD.Unit = null;
		Unit.UnitView.SetFrontAndBackActive(active: false);
		Unit.UnitView.ClearWaitUntils();
	}

	protected void FinalizeDeath()
	{
		if (Unit.IsDead)
		{
			OnDeath();
		}
	}

	protected virtual IEnumerator FinalizeDeathWhenNeeded()
	{
		yield return Unit.UnitView.WaitUntilDeathCanBeFinalized;
		FinalizeDeath();
		Unit.FinalizeDeathWhenNeededCoroutine = null;
	}

	protected virtual void TriggerInjuryStageChangeCallback(int oldStage, int currentStage)
	{
		for (int i = oldStage; i < currentStage; i++)
		{
			InjuryDefinition injuryDefinition = Unit.UnitTemplateDefinition.InjuryDefinitions[i];
			for (int num = injuryDefinition.RemoveStatusDefinitions.Count - 1; num >= 0; num--)
			{
				float randomRange = RandomManager.GetRandomRange(this, 0f, 1f);
				RemoveStatusDefinition removeStatusDefinition = injuryDefinition.RemoveStatusDefinitions[num];
				if (randomRange < removeStatusDefinition.BaseChance)
				{
					RemoveStatus(removeStatusDefinition.Status, refreshHud: false);
					DispelDisplay pooledComponent = ObjectPooler.GetPooledComponent<DispelDisplay>("DispelDisplay", ResourcePooler.LoadOnce<DispelDisplay>("Prefab/Displayable Effect/UI Effect Displays/DispelDisplay", failSilently: false), EffectManager.EffectDisplaysParent, dontSetParent: false);
					pooledComponent.Init(removeStatusDefinition.Status);
					AddEffectDisplay(pooledComponent);
				}
			}
			for (int num2 = injuryDefinition.Statuses.Count - 1; num2 >= 0; num2--)
			{
				if (injuryDefinition.Statuses[num2] is BuffEffectDefinition buffEffectDefinition)
				{
					StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
					statusCreationInfo.Source = null;
					statusCreationInfo.TurnsCount = buffEffectDefinition.TurnsCount;
					statusCreationInfo.Value = buffEffectDefinition.Bonus;
					statusCreationInfo.Stat = buffEffectDefinition.Stat;
					statusCreationInfo.IsFromInjury = true;
					StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
					AddStatus(new BuffStatusController(Unit, statusCreationInfo2).Status);
				}
				else if (injuryDefinition.Statuses[num2] is DebuffEffectDefinition debuffEffectDefinition)
				{
					StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
					statusCreationInfo.Source = null;
					statusCreationInfo.TurnsCount = debuffEffectDefinition.TurnsCount;
					statusCreationInfo.Value = debuffEffectDefinition.ModifierValue;
					statusCreationInfo.Stat = debuffEffectDefinition.Stat;
					statusCreationInfo.IsFromInjury = true;
					StatusCreationInfo statusCreationInfo3 = statusCreationInfo;
					AddStatus(new DebuffStatusController(Unit, statusCreationInfo3).Status);
				}
				else if (injuryDefinition.Statuses[num2] is StunEffectDefinition stunEffectDefinition)
				{
					StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
					statusCreationInfo.Source = null;
					statusCreationInfo.TurnsCount = stunEffectDefinition.TurnsCount;
					statusCreationInfo.IsFromInjury = true;
					StatusCreationInfo statusCreationInfo4 = statusCreationInfo;
					AddStatus(new StunStatusController(Unit, statusCreationInfo4).Status);
				}
				else if (injuryDefinition.Statuses[num2] is ContagionEffectDefinition contagionEffectDefinition)
				{
					StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
					statusCreationInfo.Source = null;
					statusCreationInfo.TurnsCount = contagionEffectDefinition.TurnsCount;
					statusCreationInfo.Value = contagionEffectDefinition.Count;
					statusCreationInfo.IsFromInjury = true;
					StatusCreationInfo statusCreationInfo5 = statusCreationInfo;
					AddStatus(new ContagionStatusController(Unit, statusCreationInfo5).Status);
				}
				else if (injuryDefinition.Statuses[num2] is PoisonEffectDefinition poisonEffectDefinition)
				{
					StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
					statusCreationInfo.Source = null;
					statusCreationInfo.TurnsCount = poisonEffectDefinition.TurnsCount;
					statusCreationInfo.Value = poisonEffectDefinition.DamagePerTurn;
					statusCreationInfo.IsFromInjury = true;
					StatusCreationInfo statusCreationInfo6 = statusCreationInfo;
					AddStatus(new PoisonStatusController(Unit, statusCreationInfo6).Status);
				}
				else if (injuryDefinition.Statuses[num2] is ChargedEffectDefinition chargedEffectDefinition)
				{
					StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
					statusCreationInfo.Source = null;
					statusCreationInfo.TurnsCount = chargedEffectDefinition.TurnsCount;
					statusCreationInfo.IsFromInjury = true;
					StatusCreationInfo statusCreationInfo7 = statusCreationInfo;
					AddStatus(new ChargedStatusController(Unit, statusCreationInfo7).Status);
				}
				else if (injuryDefinition.Statuses[num2] is ImmuneToNegativeStatusEffectDefinition immuneToNegativeStatusEffectDefinition)
				{
					StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
					statusCreationInfo.Source = null;
					statusCreationInfo.TurnsCount = immuneToNegativeStatusEffectDefinition.TurnsCount;
					statusCreationInfo.IsFromInjury = true;
					StatusCreationInfo statusCreationInfo8 = statusCreationInfo;
					AddStatus(new ImmunityStatusController(Unit, statusCreationInfo8, immuneToNegativeStatusEffectDefinition.StatusImmunity).Status);
				}
				else
				{
					((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)("Unhandled status Id " + injuryDefinition.Statuses[num2].Id + " to apply on unit injury stage change."), (CLogLevel)1, true, true);
				}
			}
		}
	}

	protected void UpdatePoisonFeedbackCondition()
	{
		Unit.WillDieByPoison = Unit.StatusOwned.HasFlag(TheLastStand.Model.Status.Status.E_StatusType.Poison) && Unit.GetNextTurnPoisonDamage() >= Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Health);
	}

	private void ApplyStatModifierStatusToChildStat(StatModifierStatus statModifierStatus, bool isStatusRemoved = false, bool refreshHud = true)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (statModifierStatus == null || UnitDatabase.UnitStatDefinitions[statModifierStatus.Stat].ChildStatId == UnitStatDefinition.E_Stat.Undefined)
		{
			return;
		}
		UnitStatDefinition.E_Stat childStatId = UnitDatabase.UnitStatDefinitions[statModifierStatus.Stat].ChildStatId;
		if (isStatusRemoved)
		{
			Unit.UnitStatsController.DecreaseBaseStat(childStatId, statModifierStatus.ModifierValue, includeChildStat: false, refreshHud);
		}
		else
		{
			Unit.UnitStatsController.IncreaseBaseStat(childStatId, statModifierStatus.ModifierValue, includeChildStat: false, refreshHud);
		}
		Vector2 boundaries = Unit.UnitStatsController.GetStat(childStatId).Boundaries;
		boundaries.y = Unit.GetClampedStatValue(statModifierStatus.Stat);
		if (boundaries.x > boundaries.y)
		{
			boundaries.x = boundaries.y;
		}
		Unit.UnitStatsController.SetBaseStat(childStatId, TPHelpers.Clamp(Unit.UnitStatsController.GetStat(childStatId).Base, boundaries), updateChildStat: false, refreshHud);
		if (refreshHud && (Object)(object)Unit.UnitView != (Object)null)
		{
			if (!Unit.UnitView.UnitHUD.IsAnimating)
			{
				Unit.UnitView.RefreshHud(statModifierStatus.Stat);
			}
			else
			{
				Unit.UnitView.UnitHUD.AnimatedDisplayFinishEvent += OnUnitHUDAnimatedDisplayFinished;
			}
		}
	}

	private IEnumerator DisplayEffectsCoroutine(float delay)
	{
		displayEffectCoroutine = Unit.UnitView.DisplaySkillEffects(delay);
		yield return displayEffectCoroutine;
		EffectManager.Unregister(this);
		displayEffectsCoroutine = null;
	}
}
