using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Controller.Unit.Pathfinding;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.PositioningMethod;
using TheLastStand.Framework;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.Model.Unit.Pathfinding;
using TheLastStand.Serialization.Unit;
using TheLastStand.View.Sound;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy;

public class EnemyUnitController : UnitController, IBehaviorController
{
	public EnemyUnit EnemyUnit => base.Unit as EnemyUnit;

	public InterpretedTurnConditionContext InterpretedTurnConditionContext { get; private set; }

	public EnemyUnitController(EnemyUnitTemplateDefinition enemyUnitTemplateDefinition, UnitView view, Tile tile, UnitCreationSettings unitCreationSettings)
	{
		base.Unit = new EnemyUnit(enemyUnitTemplateDefinition, this, view, unitCreationSettings)
		{
			State = TheLastStand.Model.Unit.Unit.E_State.Ready
		};
		Init(view, tile, unitCreationSettings.OverrideVariantId);
	}

	public EnemyUnitController(SerializedEnemyUnit serializedUnit, UnitView unitView, UnitCreationSettings unitCreationSettings, int saveVersion = -1)
	{
		base.Unit = new EnemyUnit(EnemyUnitDatabase.EnemyUnitTemplateDefinitions[serializedUnit.Id], serializedUnit, this, unitView, unitCreationSettings, saveVersion)
		{
			State = TheLastStand.Model.Unit.Unit.E_State.Ready
		};
		base.Unit.DeserializeAfterInit((ISerializedData)(object)serializedUnit, saveVersion);
		Init(serializedUnit, unitView, base.Unit.OriginTile, saveVersion);
	}

	protected EnemyUnitController()
	{
	}

	public static float ComputeExperienceGain(float baseExperience)
	{
		float num = SpawnWaveManager.SpawnDefinition.SpawnsCountPerWave.EvalToFloat((object)TPSingleton<SpawnWaveManager>.Instance.SpawnWaveInterpreterObject) * SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.SpawnsCountMultiplier;
		int spawnsCount = SpawnWaveManager.CurrentSpawnWave.SpawnsCount;
		return Mathf.Round(baseExperience * (num / (float)spawnsCount));
	}

	public void ClearCurrentGoal()
	{
		EnemyUnit.Log("Cleared current goal", (CLogLevel)0);
		if (EnemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving)
		{
			EnemyUnit.OccupiedTiles.ForEach(delegate(Tile tile)
			{
				tile.WillBeReachedBy = null;
			});
		}
		EnemyUnit.TargetTile = null;
		EnemyUnit.CurrentGoals = new ComputedGoal[EnemyUnit.NumberOfGoalsToCompute];
	}

	public virtual void ComputeCurrentGoals(Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedTiles = null)
	{
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		ClearCurrentGoal();
		EnemyUnit.Log($"Computing current goals out of {EnemyUnit.EnemyUnitTemplateDefinition.Behavior.GoalDefinitions.Length} possible goals", (CLogLevel)0);
		for (int i = 0; i < EnemyUnit.NumberOfGoalsToCompute; i++)
		{
			int j = 0;
			for (int num = EnemyUnit.EnemyUnitTemplateDefinition.Behavior.GoalDefinitions.Length; j < num; j++)
			{
				Goal goal = EnemyUnit.Goals[j];
				if (goal.GoalDefinition.GoalComputingStep.HasFlag(EnemyUnit.GoalComputingStep))
				{
					goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(base.Unit);
					SkillTargetedTileInfo skillTargetedTileInfo = goal.GoalController.ComputeTarget(alreadyTargetedTiles);
					if (skillTargetedTileInfo != null)
					{
						goal.Skill.SkillAction.SkillActionExecution.SkillSourceTileObject = base.Unit.OccupiedTiles.GetFirstClosestTile(skillTargetedTileInfo.Tile).tile;
						EnemyUnit.Log($"Validated {skillTargetedTileInfo.Tile.Position} Orientation: {skillTargetedTileInfo.Orientation} as best goal, skill source tile origin : {goal.Skill.SkillAction.SkillActionExecution.SkillSourceTileObject.OriginTile.Position}", (CLogLevel)1, forcePrintInUnity: true);
						EnemyUnit.CurrentGoals[i] = new ComputedGoal(goal, skillTargetedTileInfo);
						break;
					}
				}
			}
		}
		if (EnemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving)
		{
			ComputeBestTileToMove();
		}
	}

	public void DecrementGoalsCooldown()
	{
		Goal[] goals = EnemyUnit.Goals;
		for (int i = 0; i < goals.Length; i++)
		{
			goals[i].GoalController.StartTurn();
		}
	}

	public override void EndTurn()
	{
		base.EndTurn();
		EffectManager.DisplayEffects();
	}

	public void ExecuteAllGoals()
	{
		if (EnemyUnit.CurrentGoals == null || EnemyUnit.CurrentGoals.Length == 0)
		{
			return;
		}
		List<ComputedGoal> list = EnemyUnit.CurrentGoals.ToList();
		if (list.Count != 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ComputedGoal goalToExecute = list[i];
				ExecuteGoal(goalToExecute);
			}
		}
	}

	public void ExecuteGoal(ComputedGoal goalToExecute)
	{
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		EnemyUnit.Log($"Executing Goal -> target tile is {goalToExecute.TargetTileInfo.Tile}, orientation is {goalToExecute.TargetTileInfo.Orientation}", (CLogLevel)0);
		bool flag = goalToExecute.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInRange(goalToExecute.TargetTileInfo.Tile);
		bool flag2 = goalToExecute.Goal.Skill.SkillAction.HasEffect("IgnoreLineOfSight") || EnemyUnit.OccupiedTiles.Contains(goalToExecute.TargetTileInfo.Tile) || goalToExecute.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(goalToExecute.TargetTileInfo.Tile);
		if (flag && flag2)
		{
			if (!goalToExecute.Goal.Skill.SkillAction.SkillActionExecution.HasBeenPrepared)
			{
				goalToExecute.Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(EnemyUnit, EnemyUnit.OccupiedTiles.GetFirstClosestTile(goalToExecute.TargetTileInfo.Tile).tile);
			}
			goalToExecute.Goal.Skill.SkillAction.SkillActionExecution.TargetTiles.Add(goalToExecute.TargetTileInfo);
			goalToExecute.Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.ExecuteSkill();
			if (GetModifiedMaxRange(goalToExecute.Goal.Skill) > 1 && EnemyUnitManager.DebugEnemyAttackFeedback)
			{
				EnemyAttackFeedback enemyAttackFeedback = Object.Instantiate<EnemyAttackFeedback>(EnemyUnit.EnemyUnitView.EnemyAttackFeedbackPrefab);
				((Component)enemyAttackFeedback).transform.position = ((Component)EnemyUnit.EnemyUnitView).transform.position;
				((Component)enemyAttackFeedback).transform.localScale = new Vector3(((Component)enemyAttackFeedback).transform.localScale.x * ((Component)EnemyUnit.EnemyUnitView.OrientationRootTransform).transform.localScale.x, ((Component)enemyAttackFeedback).transform.localScale.y * ((Component)EnemyUnit.EnemyUnitView.OrientationRootTransform).transform.localScale.y, 1f);
				enemyAttackFeedback.TargetPosition = TileMapView.GetCellCenterWorldPosition(goalToExecute.TargetTileInfo.Tile);
				enemyAttackFeedback.SpriteRenderer.color = EnemyUnitManager.GetEnemyAttackFeedbackColor(EnemyUnit.EnemyUnitTemplateDefinition);
				enemyAttackFeedback.EnemyUnitView = EnemyUnit.EnemyUnitView;
			}
		}
		else
		{
			EnemyUnit.LogError("A computed goal shouldn't be computed !\n" + $"LineOfSight : {flag2}\n" + $"InRange : {flag}\n" + $"TargetTile : {goalToExecute.TargetTileInfo}\n" + $"TargetType : {goalToExecute.TargetType}", (CLogLevel)1);
			if (goalToExecute.TargetTileInfo.Tile.Damageable != null)
			{
				EnemyUnit.LogError($"Goal was targeting a(n) {goalToExecute.TargetTileInfo.Tile.Damageable.DamageableType} : {(goalToExecute.TargetTileInfo.Tile.Damageable as IEntity)?.UniqueIdentifier}", (CLogLevel)1);
			}
		}
		EnemyUnit.TargetTile = null;
	}

	public void ExecuteDeathRattle()
	{
		if (EnemyUnit.IsDeathRattling)
		{
			EnemyUnit enemyUnit = EnemyUnit;
			if (enemyUnit.TargetTile == null)
			{
				Tile tile = (enemyUnit.TargetTile = EnemyUnit.OriginTile);
			}
			ExecuteAllGoals();
		}
	}

	public void ExecuteSpawnGoals()
	{
		EnemyUnit.GoalComputingStep = IBehaviorModel.E_GoalComputingStep.OnSpawn;
		ComputeCurrentGoals();
		if (EnemyUnit.CurrentGoals != null && EnemyUnit.CurrentGoals.Length != 0 && EnemyUnit.CurrentGoals[0] != null)
		{
			EnemyUnit.TargetTile = EnemyUnit.OriginTile;
			ExecuteAllGoals();
		}
	}

	public override void FilterTilesInRange(TilesInRangeInfos tilesInRangeInfos, List<Tile> skillSourceTiles)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> item in tilesInRangeInfos.Range)
		{
			if (item.Key.HasFog)
			{
				item.Value.HasLineOfSight = false;
				item.Value.TileColor = TileMapView.SkillHiddenRangeTilesColorInvalidOrientation._Color;
			}
		}
	}

	public override bool IsImmuneTo(TheLastStand.Model.Status.Status status)
	{
		EnemyBarrierAffixController.StatusValidity statusValidity = new EnemyBarrierAffixController.StatusValidity(status.StatusType, newValidity: true);
		TriggerAffixes(E_EffectTime.OnStatusImmunityComputation, statusValidity);
		if (statusValidity.validity)
		{
			return base.IsImmuneTo(status);
		}
		return true;
	}

	public override void LoseHealth(float amount, ISkillCaster attacker = null, bool refreshHud = true)
	{
		if (base.Unit.State == TheLastStand.Model.Unit.Unit.E_State.Dead || EnemyUnit.IsDeathRattling)
		{
			return;
		}
		base.LoseHealth(amount, attacker, refreshHud);
		base.Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.Health, amount, includeChildStat: false, refreshHud);
		PlayableUnit playableUnit = attacker as PlayableUnit;
		if (playableUnit != null)
		{
			TrophyManager.AppendValueToTrophiesConditions<EnemiesDamagedTrophyConditionController>(new object[2] { playableUnit.RandomId, EnemyUnit.RandomId });
		}
		else if (attacker is EnemyUnit enemyUnit)
		{
			enemyUnit.EnemyUnitController.EnemyUnit.AlliesDamaged++;
		}
		if (base.Unit.Health <= 0f)
		{
			if (playableUnit != null && EnemyUnit.EnemyUnitTemplateDefinition.Id == "Boomer")
			{
				EnemyUnit.ExplosionResponsible = playableUnit;
			}
			PrepareForDeath(attacker);
		}
		else
		{
			UpdatePoisonFeedbackCondition();
		}
	}

	public override void OnHit(ISkillCaster attacker)
	{
		base.OnHit(attacker);
		TriggerAffixes(E_EffectTime.OnHitTaken, attacker);
	}

	public void PrepareForDeathRattle()
	{
		if (base.Unit.HasBeenExiled)
		{
			return;
		}
		EnemyUnit.GoalComputingStep = IBehaviorModel.E_GoalComputingStep.OnDeath;
		ComputeCurrentGoals();
		if (EnemyUnit.CurrentGoals[0]?.Goal != null && EnemyUnit.CurrentGoals[0].TargetTileInfo != null)
		{
			if (!TPSingleton<EnemyUnitManager>.Instance.EnemiesDeathRattling.Contains(EnemyUnit))
			{
				TPSingleton<EnemyUnitManager>.Instance.EnemiesDeathRattling.Add(EnemyUnit);
			}
			EnemyUnit.TargetTile = EnemyUnit.OriginTile;
			EnemyUnit.IsDeathRattling = true;
		}
	}

	public override void PrepareForDeath(ISkillCaster killer = null)
	{
		if (base.Unit.State == TheLastStand.Model.Unit.Unit.E_State.Dead)
		{
			return;
		}
		if ((base.Unit.StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.AllNegative) != 0)
		{
			TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.AlterationKills, 1.0);
			if (TPSingleton<MetaConditionManager>.Instance.RunContext.GetDouble(MetaConditionSpecificContext.E_ValueCategory.AlterationKills) >= 300.0)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_KILL_300_NEGATED_ENEMIES_RUN);
			}
		}
		base.PrepareForDeath(killer);
		EnemyUnit.EnemyUnitView.RefreshZoneControl();
		if (EnemyUnit.IsBossPhaseActor)
		{
			EnemyUnit.PrepareBossActorDeath();
		}
		PlayableUnit playableUnit = killer as PlayableUnit;
		bool flag = killer is BattleModule;
		int num = 0;
		while (true)
		{
			if (num < TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight.Count)
			{
				KillReportData killReportData = TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight[num];
				if (killReportData.SpecificId == EnemyUnit.SpecificId)
				{
					killReportData.AddKillForEntity(killer as IEntity);
					break;
				}
				num++;
				continue;
			}
			AddNewKillReportEntry(killer);
			break;
		}
		if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits && EnemyUnit.ShouldCausePanic)
		{
			PanicManager.Panic.PanicController.ComputeExpectedValue(updateView: true);
		}
		if (playableUnit != null)
		{
			TrophyManager.AppendValueToTrophiesConditions<EnemiesKilledTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
			TrophyManager.AppendValueToTrophiesConditions<EnemiesKilledSingleAttackTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
			if (playableUnit.IsInWatchtower)
			{
				TrophyManager.AppendValueToTrophiesConditions<EnemiesKilledFromWatchtowerTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
			}
			switch (EnemyUnit.Id)
			{
			case "Bloody":
				TrophyManager.AppendValueToTrophiesConditions<BloodyKilledAfterEatingAlliesTrophyConditionController>(new object[2] { playableUnit.RandomId, EnemyUnit });
				break;
			case "Ghost":
				TrophyManager.AppendValueToTrophiesConditions<GhostKilledWithoutDebuffingTrophyConditionController>(new object[2] { playableUnit, EnemyUnit.RandomId });
				break;
			case "SpeedyClawer":
				TrophyManager.AppendValueToTrophiesConditions<SpeedyKilledWithoutDodgingTrophyConditionController>(new object[1] { EnemyUnit });
				break;
			}
		}
		else if (killer is EnemyUnit enemyUnit)
		{
			if (enemyUnit.Id == "Boomer" && enemyUnit.ExplosionResponsible != null)
			{
				TrophyManager.AppendValueToTrophiesConditions<EnemiesKilledSingleAttackTrophyConditionController>(new object[2]
				{
					enemyUnit.ExplosionResponsible.RandomId,
					1
				});
			}
			enemyUnit.AlliesKilled++;
		}
		if (killer is EnemyUnit || (killer is BattleModule battleModule && !battleModule.BuildingParent.IsHandledDefense))
		{
			TrophyManager.AppendValueToTrophiesConditions<EnemiesKilledWithoutAttackTrophyConditionController>(new object[1] { 1 });
		}
		if (flag || playableUnit != null)
		{
			TrophyManager.AddEnemyKill(Mathf.RoundToInt(EnemyUnit.UnitStatsController.UnitStats.Stats[UnitStatDefinition.E_Stat.DamnedSoulsEarned].FinalClamped));
		}
		if (playableUnit != null)
		{
			TPSingleton<MetaConditionManager>.Instance.IncreaseEnemiesKilled(EnemyUnit);
			TPSingleton<AchievementManager>.Instance.IncreaseAchievementProgression(StatContainer.STAT_ENEMIES_KILLED_AMOUNT, 1);
		}
		if (!base.Unit.HasBeenExiled)
		{
			PrepareForDeathRattle();
		}
		if (EnemyUnit.IsBossPhaseActor && TPSingleton<BossManager>.Instance.ShouldTriggerBossWaveVictory)
		{
			TPSingleton<BossManager>.Instance.IsBossVanquished = true;
		}
	}

	public override Task PrepareForMovement(bool playWalkAnim = true, bool followPathOrientation = true, float moveSpeed = -1f, float delay = 0f, bool isMovementInstant = false)
	{
		EnemyUnit.EnemyUnitView.Hovered = false;
		EnemyUnit.EnemyUnitView.Selected = false;
		Task result = base.PrepareForMovement(playWalkAnim, followPathOrientation, moveSpeed, delay, isMovementInstant);
		EnemyUnit.EnemyUnitView.RefreshZoneControl();
		return result;
	}

	public override void StartTurn()
	{
		base.StartTurn();
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
			{
			case Game.E_NightTurn.EnemyUnits:
			{
				if (EnemyUnit.IsInCitySincePlayerTurnStart && !EnemyUnit.IsDead && !EnemyUnit.IsStunned)
				{
					PanicManager.Panic.PanicController.AddValue(EnemyUnit.EnemyUnitStatsController.EnemyUnitStats.Stats[UnitStatDefinition.E_Stat.Panic].FinalClamped, useAttackMultiplier: false, updateView: false);
				}
				DecrementGoalsCooldown();
				Tile originTile = base.Unit.OriginTile;
				if (originTile != null && originTile.HasFog)
				{
					EnemyUnit.LastHourInFog = TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
				}
				originTile = base.Unit.OriginTile;
				if (originTile != null && originTile.HasAnyFog)
				{
					EnemyUnit.LastHourInAnyFog = TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
				}
				TriggerAffixes(E_EffectTime.OnStartNightTurnEnemy);
				break;
			}
			case Game.E_NightTurn.PlayableUnits:
				EnemyUnit.IsInCitySincePlayerTurnStart = EnemyUnit.IsInCity;
				break;
			}
		}
		EffectManager.DisplayEffects();
	}

	public void TriggerAffixes(E_EffectTime effectTime, object data = null)
	{
		foreach (EnemyAffix affix in EnemyUnit.Affixes)
		{
			affix.EnemyAffixController.Trigger(effectTime, data);
		}
	}

	protected virtual void AddNewKillReportEntry(ISkillCaster killer)
	{
		int num = -1;
		if (EnemyUnitDatabase.EnemyToEliteIds.TryGetValue(EnemyUnit.Id, out var value))
		{
			for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight.Count; i++)
			{
				if (TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight[i].SpecificId == value)
				{
					num = i;
					break;
				}
			}
		}
		if (num != -1)
		{
			TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight.Insert(num, new KillReportData(EnemyUnit, killer as IEntity));
		}
		else
		{
			TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight.Add(new KillReportData(EnemyUnit, killer as IEntity));
		}
	}

	protected virtual void ComputeBestTileToMove()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		ComputedGoal computedGoal = EnemyUnit.CurrentGoals[0];
		Goal goal = computedGoal?.Goal;
		Tile tile = computedGoal?.TargetTileInfo.Tile;
		if (goal == null || tile == null)
		{
			if (EnemyUnit.Path == null)
			{
				EnemyUnit.Path = new List<Tile> { EnemyUnit.OriginTile };
			}
			else
			{
				EnemyUnit.Path.Clear();
				EnemyUnit.Path.Add(EnemyUnit.OriginTile);
			}
			return;
		}
		List<Tile> list = new List<Tile>();
		Vector2Int range = goal.Skill.SkillDefinition.Range;
		int distanceFromTargetMin = ((Vector2Int)(ref range)).x;
		int distanceFromTargetMax = GetModifiedMaxRange(goal.Skill);
		if (computedGoal.DesiredReachedTile != null)
		{
			EnemyUnit.Log($"DesiredReachedTile : {computedGoal.DesiredReachedTile.Position}", (CLogLevel)1);
			list.Add(computedGoal.DesiredReachedTile);
			distanceFromTargetMin = 0;
			distanceFromTargetMax = 0;
		}
		else if (tile.Building != null)
		{
			list.AddRange(tile.Building.OccupiedTiles);
		}
		else
		{
			list.Add(tile);
		}
		PathfindingData pathfindingData = default(PathfindingData);
		pathfindingData.Unit = EnemyUnit;
		pathfindingData.TargetTiles = list.ToArray();
		pathfindingData.MoveRange = (int)EnemyUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).FinalClamped;
		pathfindingData.DistanceFromTargetMin = distanceFromTargetMin;
		pathfindingData.DistanceFromTargetMax = distanceFromTargetMax;
		pathfindingData.IgnoreCanStopOnConstraints = false;
		pathfindingData.PathfindingStyle = EnemyUnit.EnemyUnitTemplateDefinition.Behavior.PathfindingStyle;
		pathfindingData.AIPathfindingData = new AIPathfindingData
		{
			SkillExecution = goal.Skill.SkillAction.SkillActionExecution,
			ThinkingScope = EnemyUnit.EnemyUnitTemplateDefinition.Behavior.ThinkingScope,
			SpreadFactor = UnitDatabase.PathfindingDefinition.EnemyAISpreadFactor,
			InterruptionCondition = goal.GoalDefinition.InterruptionCondition
		};
		PathfindingData pathfindingData2 = pathfindingData;
		if (goal.GoalDefinition.PositioningMethod is ClosestTilePositioningMethod)
		{
			range = goal.Skill.SkillDefinition.Range;
			pathfindingData2.DistanceFromTargetMax = ((Vector2Int)(ref range)).x;
			pathfindingData2.DistanceFromTargetMin = Mathf.Min(pathfindingData2.DistanceFromTargetMin, pathfindingData2.DistanceFromTargetMax);
		}
		else if (goal.GoalDefinition.PositioningMethod is FarthestTilePositioningMethod)
		{
			pathfindingData2.DistanceFromTargetMin = GetModifiedMaxRange(goal.Skill);
			pathfindingData2.DistanceFromTargetMax = Mathf.Max(pathfindingData2.DistanceFromTargetMin, pathfindingData2.DistanceFromTargetMax);
		}
		EnemyUnit.Path = PathfindingController.ComputePath(pathfindingData2);
		EnemyUnit.TargetTile = EnemyUnit.Path[^1];
		EnemyUnit.Log($"Computed path : I will land on {EnemyUnit.TargetTile?.Position} (null? {EnemyUnit.TargetTile == null})", (CLogLevel)0);
		foreach (Tile occupiedTile in EnemyUnit.TargetTile.GetOccupiedTiles(EnemyUnit.EnemyUnitTemplateDefinition))
		{
			if (occupiedTile.WillBeReached)
			{
				EnemyUnit.LogError($"targeted tile will be reached by {EnemyUnit.TargetTile.WillBeReachedBy}, {base.Unit.RandomId} is stuck? should not happen", (CLogLevel)0);
			}
			occupiedTile.WillBeReachedBy = base.Unit.RandomId;
		}
	}

	protected override IEnumerator FinalizeDeathWhenNeeded()
	{
		yield return base.Unit.UnitView.WaitUntilIsDying;
		if (EnemyUnit.EnemyUnitTemplateDefinition.DeathSoundFolderName != "None")
		{
			string text = ((EnemyUnit.EnemyUnitTemplateDefinition.DeathSoundFolderName != string.Empty) ? EnemyUnit.EnemyUnitTemplateDefinition.DeathSoundFolderName : "Clawer");
			AudioClip[] array = ResourcePooler<AudioClip>.LoadAllOnce("Sounds/SFX/Enemy/Deaths/" + text, false);
			if (array != null && array.Length != 0)
			{
				ObjectPooler.GetPooledComponent<OneShotSound>("Enemies Death Spatialized", EnemyUnitManager.EnemyDeathSpatializedSFXPrefab, (Transform)null, false).PlaySpatialized(TPHelpers.RandomElement<AudioClip>(array), EnemyUnit.OriginTile, TPHelpers.RandomFloatInRange(EnemyUnitManager.DelayBetweenDeathAndSound));
			}
			else
			{
				EnemyUnit.LogError(" The death sounds folder (" + text + ") doesn't exist or is empty", (CLogLevel)1);
			}
		}
		ExecuteDeathRattle();
		if (EnemyUnit.Id == "Boomer")
		{
			TrophyManager.SetValueToTrophiesConditions<EnemiesDamagedByBoomerTrophyConditionController>(new object[1] { EnemyUnit });
		}
		yield return base.Unit.UnitView.WaitUntilDeathCanBeFinalized;
		FinalizeDeath();
		base.Unit.FinalizeDeathWhenNeededCoroutine = null;
	}

	protected virtual int GetRandomVisualVariantIndex()
	{
		return RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, EnemyUnit.EnemyUnitTemplateDefinition.VisualVariants.Count);
	}

	protected void Init(UnitView view, Tile tile, int overrideVariantId = -1)
	{
		InitStats();
		TriggerAffixes(E_EffectTime.OnCreation);
		SnapBaseStats();
		SetTile(tile);
		Tile originTile = base.Unit.OriginTile;
		if (originTile != null && originTile.HasFog)
		{
			EnemyUnit.LastHourInFog = TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
		}
		Tile originTile2 = base.Unit.OriginTile;
		if (originTile2 != null && originTile2.HasAnyFog)
		{
			EnemyUnit.LastHourInAnyFog = TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
		}
		if ((Object)(object)view != (Object)null)
		{
			((Object)view).name = base.Unit.UniqueIdentifier;
		}
		if (overrideVariantId < 0 || overrideVariantId >= EnemyUnit.EnemyUnitTemplateDefinition.VisualVariants.Count)
		{
			InitVariantId();
		}
		else
		{
			EnemyUnit.VariantId = EnemyUnit.EnemyUnitTemplateDefinition.VisualVariants[overrideVariantId];
		}
		int num = EnemyUnit.EnemyUnitTemplateDefinition.Behavior.GoalDefinitions.Length;
		EnemyUnit.Goals = new Goal[num];
		for (int i = 0; i < num; i++)
		{
			EnemyUnit.Goals[i] = new GoalController(EnemyUnit.EnemyUnitTemplateDefinition.Behavior.GoalDefinitions[i], EnemyUnit).Goal;
		}
		InterpretedTurnConditionContext = new InterpretedTurnConditionContext(TPSingleton<GameManager>.Instance.Game.CurrentNightHour);
	}

	protected void Init(ASerializedEnemyUnit serializedUnit, UnitView view, Tile tile, int saveVersion = -1)
	{
		SetTile(tile);
		HookLinkedBuilding();
		EnemyUnit.LastHourInFog = serializedUnit.LastHourInFog;
		EnemyUnit.LastHourInAnyFog = serializedUnit.LastHourInAnyFog;
		if ((Object)(object)view != (Object)null)
		{
			((Object)view).name = base.Unit.UniqueIdentifier;
		}
		if (serializedUnit.OverrideVariantId < 0 || serializedUnit.OverrideVariantId >= EnemyUnit.EnemyUnitTemplateDefinition.VisualVariants.Count)
		{
			InitVariantId();
		}
		else
		{
			EnemyUnit.VariantId = EnemyUnit.EnemyUnitTemplateDefinition.VisualVariants[serializedUnit.OverrideVariantId];
		}
		EnemyUnit.DeserializeBehavior(serializedUnit.SerializedBehavior, saveVersion);
		InterpretedTurnConditionContext = new InterpretedTurnConditionContext(TPSingleton<GameManager>.Instance.Game.CurrentNightHour);
	}

	protected virtual void InitStats()
	{
		base.Unit.UnitStatsController = new EnemyUnitStatsController(EnemyUnit);
	}

	protected virtual void InitVariantId()
	{
		if (EnemyUnit.EnemyUnitTemplateDefinition.VisualVariants != null)
		{
			EnemyUnit.CurrentVariantIndex = ((EnemyUnit.EnemyUnitTemplateDefinition.VisualEvolutions == null) ? GetRandomVisualVariantIndex() : 0);
			EnemyUnit.VariantId = EnemyUnit.EnemyUnitTemplateDefinition.VisualVariants[EnemyUnit.CurrentVariantIndex];
		}
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		EnemyUnit.OriginTile.TileController.AddDeadBody(EnemyUnit);
		UnhookLinkedBuilding();
		TPSingleton<EnemyUnitManager>.Instance.EnemiesDying.Remove(EnemyUnit);
		if (TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Contains(EnemyUnit))
		{
			EnemyUnitManager.AddBonePilePercentage(EnemyUnit);
			EnemyUnitManager.DestroyUnit(EnemyUnit);
		}
		TriggerAffixes(E_EffectTime.OnDeath);
	}

	protected void SnapBaseStats()
	{
		base.Unit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.Health, UnitStatDefinition.E_Stat.HealthTotal);
		base.Unit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.Armor, UnitStatDefinition.E_Stat.ArmorTotal);
		base.Unit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.MovePoints, UnitStatDefinition.E_Stat.MovePointsTotal);
	}

	private void UnhookLinkedBuilding()
	{
		if (EnemyUnit.LinkedBuilding?.PassivesModule?.BuildingPassives == null)
		{
			return;
		}
		foreach (BuildingPassive buildingPassife in EnemyUnit.LinkedBuilding.PassivesModule.BuildingPassives)
		{
			foreach (BuildingPassiveEffect passiveEffect in buildingPassife.PassiveEffects)
			{
				if (passiveEffect is GenerateGuardian generateGuardian && generateGuardian.Guardian == EnemyUnit)
				{
					generateGuardian.Guardian = null;
				}
			}
		}
	}

	private void HookLinkedBuilding()
	{
		if (EnemyUnit.LinkedBuilding?.PassivesModule?.BuildingPassives == null)
		{
			return;
		}
		foreach (BuildingPassive buildingPassife in EnemyUnit.LinkedBuilding.PassivesModule.BuildingPassives)
		{
			foreach (BuildingPassiveEffect passiveEffect in buildingPassife.PassiveEffects)
			{
				if (passiveEffect is GenerateGuardian generateGuardian && generateGuardian.Guardian == null)
				{
					generateGuardian.Guardian = EnemyUnit;
				}
			}
		}
	}
}
