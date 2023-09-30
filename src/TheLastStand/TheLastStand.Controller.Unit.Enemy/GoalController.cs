using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.GoalCondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPostcondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;
using TheLastStand.Definition.Unit.Enemy.TargetingMethod;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy;

public class GoalController
{
	private List<TileObjectSelectionManager.E_Orientation> orientationsToCheck;

	public bool IsOrientationImportant
	{
		get
		{
			if (Goal.Owner is EnemyUnit enemyUnit && enemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving && !Goal.Skill.SkillDefinition.AreaOfEffectDefinition.IsSingleTarget)
			{
				return !Goal.Skill.SkillDefinition.LockAutoOrientation;
			}
			return false;
		}
	}

	public Goal Goal { get; private set; }

	public GoalController(GoalDefinition goalDefinition, IBehaviorModel owner)
	{
		Goal = new Goal(goalDefinition, this, owner);
		if (!SkillManager.TryGetSkillDefinitionOrDatabase(owner.SkillProgressions, goalDefinition.SkillId, owner.ModifiedDayNumber, out var skillDefinition))
		{
			((CLogger<NightTurnsManager>)TPSingleton<NightTurnsManager>.Instance).LogError((object)("Skill " + goalDefinition.SkillId + " not found!"), (CLogLevel)1, true, true);
		}
		Goal.Skill = new SkillController(skillDefinition, Goal).Skill;
	}

	public SkillTargetedTileInfo ComputeTarget(Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedDamageables = null)
	{
		if (Goal.Cooldown >= 0)
		{
			return null;
		}
		if (Goal.Owner is TheLastStand.Model.Unit.Unit unit && unit.IsStunned && Goal.Owner.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.OnDeath && Goal.GoalDefinition.SkillId != "SkipTurn")
		{
			return null;
		}
		if (Goal.Owner.PreventedSkillsIds.Contains(Goal.Skill.SkillDefinition.Id))
		{
			return null;
		}
		if (!CheckPreconditionGroups())
		{
			return null;
		}
		if (IsOrientationImportant)
		{
			orientationsToCheck = new List<TileObjectSelectionManager.E_Orientation>(4)
			{
				TileObjectSelectionManager.E_Orientation.NORTH,
				TileObjectSelectionManager.E_Orientation.EAST,
				TileObjectSelectionManager.E_Orientation.SOUTH,
				TileObjectSelectionManager.E_Orientation.WEST
			};
		}
		else
		{
			orientationsToCheck = null;
		}
		if (Goal.Skill.SkillAction.SkillActionController is SpawnSkillActionController spawnSkillActionController)
		{
			spawnSkillActionController.ComputeUnitsToSpawn();
		}
		Dictionary<SkillTargetedTileInfo, bool> candidateTargetTiles = GetCandidateTargetTiles(alreadyTargetedDamageables);
		if (candidateTargetTiles.Count == 0)
		{
			return null;
		}
		if (!CheckPostconditionGroups(candidateTargetTiles.Keys.ToList()))
		{
			return null;
		}
		return ComputeBestTarget(candidateTargetTiles, alreadyTargetedDamageables);
	}

	private SkillTargetedTileInfo ComputeBestTarget(Dictionary<SkillTargetedTileInfo, bool> candidateTargetTiles, Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedDamageables = null)
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (candidateTargetTiles.Count < 1)
		{
			return null;
		}
		if (alreadyTargetedDamageables != null)
		{
			TargetingMethodsContainerDefinition targetingMethodsContainer = Goal.GoalDefinition.TargetingMethodsContainer;
			if (targetingMethodsContainer != null && targetingMethodsContainer.AvoidOverkill)
			{
				((CLogger<NightTurnsManager>)TPSingleton<NightTurnsManager>.Instance).Log((object)("currentlyTargetedTiles : " + string.Join(" | ", alreadyTargetedDamageables.Select((KeyValuePair<IDamageable, GroupTargetingInfo> x) => string.Format("{0} {1} - [{2}~{3}] : ({4})", x.Key.DamageableType, (x.Key as IEntity)?.UniqueIdentifier, x.Value.MinDamage, x.Value.MaxDamage, string.Join(", ", x.Value.EntitiesIdTargeting))))), (CLogLevel)0, false, false);
			}
		}
		SkillTargetedTileInfo skillTargetedTileInfo = FilterCandidatesThroughTargetingMethods(candidateTargetTiles);
		if (alreadyTargetedDamageables != null && skillTargetedTileInfo?.Tile.Damageable != null && Goal.Skill.SkillAction.SkillActionController is AttackSkillActionController attackSkillActionController)
		{
			Vector2Int finalDamageRange = attackSkillActionController.ComputeFinalDamageRange(skillTargetedTileInfo.Tile, Goal.Owner).FinalDamageRange;
			if (alreadyTargetedDamageables.ContainsKey(skillTargetedTileInfo.Tile.Damageable))
			{
				alreadyTargetedDamageables[skillTargetedTileInfo.Tile.Damageable].MinDamage += ((Vector2Int)(ref finalDamageRange)).x;
				alreadyTargetedDamageables[skillTargetedTileInfo.Tile.Damageable].MaxDamage += ((Vector2Int)(ref finalDamageRange)).y;
				alreadyTargetedDamageables[skillTargetedTileInfo.Tile.Damageable].EntitiesIdTargeting.Add(Goal.Owner.RandomId);
			}
			else
			{
				alreadyTargetedDamageables.Add(skillTargetedTileInfo.Tile.Damageable, new GroupTargetingInfo(((Vector2Int)(ref finalDamageRange)).x, ((Vector2Int)(ref finalDamageRange)).y, Goal.Owner.RandomId));
			}
		}
		return skillTargetedTileInfo;
	}

	public void StartTurn()
	{
		Goal.Cooldown = Mathf.Max(Goal.Cooldown - 1, -1);
	}

	private bool CheckPostconditionGroup(List<SkillTargetedTileInfo> candidateTargetTiles, GoalConditionDefinition[] conditionGroup)
	{
		int i = 0;
		for (int num = conditionGroup.Length; i < num; i++)
		{
			GoalConditionDefinition goalConditionDefinition = conditionGroup[i];
			if (CheckPostcondition(candidateTargetTiles, goalConditionDefinition))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckPostconditionGroups(List<SkillTargetedTileInfo> candidateTargetTiles)
	{
		if (Goal.GoalDefinition.PostconditionGroups == null)
		{
			return true;
		}
		int i = 0;
		for (int num = Goal.GoalDefinition.PostconditionGroups.Length; i < num; i++)
		{
			if (!CheckPostconditionGroup(candidateTargetTiles, Goal.GoalDefinition.PostconditionGroups[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckPostcondition(List<SkillTargetedTileInfo> candidateTargetTiles, GoalConditionDefinition goalConditionDefinition)
	{
		if (goalConditionDefinition is TargetsCountCondition targetsCountCondition)
		{
			if (candidateTargetTiles.Count >= targetsCountCondition.Min)
			{
				return candidateTargetTiles.Count <= targetsCountCondition.Max;
			}
			return false;
		}
		((CLogger<NightTurnsManager>)TPSingleton<NightTurnsManager>.Instance).LogError((object)"The postcondition has could not be evaluated", (CLogLevel)1, true, true);
		return false;
	}

	private bool CheckPrecondition(GoalConditionDefinition goalConditionDefinition)
	{
		if (Goal.Owner is TheLastStand.Model.Unit.Unit unit && goalConditionDefinition is CasterHasStatusConditionDefinition casterHasStatusConditionDefinition)
		{
			return unit.StatusOwned.HasFlag(casterHasStatusConditionDefinition.StatusType);
		}
		if (Goal.Owner is IDamageable damageable && goalConditionDefinition is CasterHealthConditionDefinition casterHealthConditionDefinition)
		{
			float num = damageable.Health / damageable.HealthTotal;
			if (num >= casterHealthConditionDefinition.Min)
			{
				return num <= casterHealthConditionDefinition.Max;
			}
			return false;
		}
		if (Goal.Owner is EnemyUnit enemyUnit && goalConditionDefinition is InterpretedTurnCondition interpretedTurnCondition)
		{
			return interpretedTurnCondition.Expression.EvalToInt((InterpreterContext)(object)enemyUnit.EnemyUnitController.InterpretedTurnConditionContext) == TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
		}
		if (goalConditionDefinition is PlayableUnitCloseToCasterConditionDefinition)
		{
			return Goal.Owner.TileObjectController.GetAdjacentTilesWithDiagonals().Any((Tile x) => x.Unit is PlayableUnit);
		}
		DamageableAroundConditionDefinition damageableAroundConditionDefinition = goalConditionDefinition as DamageableAroundConditionDefinition;
		if (damageableAroundConditionDefinition != null)
		{
			return Goal.Owner.TileObjectController.GetTilesInRange(damageableAroundConditionDefinition.MaxRange, damageableAroundConditionDefinition.MinRange).Count((Tile x) => x.Damageable?.DamageableType == damageableAroundConditionDefinition.DamageableType) >= damageableAroundConditionDefinition.MinAmount;
		}
		if (goalConditionDefinition is NotInFogCondition notInFogCondition)
		{
			Node nbTurns = notInFogCondition.NbTurns;
			int num2 = ((nbTurns != null) ? nbTurns.EvalToInt((InterpreterContext)(object)Goal) : 0);
			if (!(Goal.Owner is EnemyUnit enemyUnit2))
			{
				return !Goal.Owner.OriginTile.HasFog;
			}
			if (!Goal.Owner.OriginTile.HasFog)
			{
				return TPSingleton<GameManager>.Instance.Game.CurrentNightHour - enemyUnit2.EnemyUnitController.EnemyUnit.LastHourInFog >= num2;
			}
			return false;
		}
		if (goalConditionDefinition is NotInAnyFogCondition notInAnyFogCondition)
		{
			Node nbTurns2 = notInAnyFogCondition.NbTurns;
			int num3 = ((nbTurns2 != null) ? nbTurns2.EvalToInt((InterpreterContext)(object)Goal) : 0);
			if (!(Goal.Owner is EnemyUnit enemyUnit3))
			{
				return !Goal.Owner.OriginTile.HasAnyFog;
			}
			if (!Goal.Owner.OriginTile.HasAnyFog)
			{
				return TPSingleton<GameManager>.Instance.Game.CurrentNightHour - enemyUnit3.EnemyUnitController.EnemyUnit.LastHourInAnyFog >= num3;
			}
			return false;
		}
		if (goalConditionDefinition is SkillProgressionFlagIsToggledConditionDefinition skillProgressionFlagIsToggledConditionDefinition)
		{
			return TPSingleton<GlyphManager>.Instance.SkillProgressionFlag.HasFlag(skillProgressionFlagIsToggledConditionDefinition.SkillProgressionFlag);
		}
		((CLogger<NightTurnsManager>)TPSingleton<NightTurnsManager>.Instance).LogError((object)"The precondition has could not be evaluated", (CLogLevel)1, true, true);
		return false;
	}

	private bool CheckPreconditionGroup(GoalConditionDefinition[] conditionGroup)
	{
		int i = 0;
		for (int num = conditionGroup.Length; i < num; i++)
		{
			GoalConditionDefinition goalConditionDefinition = conditionGroup[i];
			if (CheckPrecondition(goalConditionDefinition))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckPreconditionGroups()
	{
		if (Goal.GoalDefinition.PreconditionGroups == null)
		{
			return true;
		}
		int i = 0;
		for (int num = Goal.GoalDefinition.PreconditionGroups.Length; i < num; i++)
		{
			if (!CheckPreconditionGroup(Goal.GoalDefinition.PreconditionGroups[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckTargetFlagTileCondition(TileFlagDefinition.E_TileFlagTag tileFlagTag, Tile tile, GoalConditionDefinition goalConditionDefinition)
	{
		if (goalConditionDefinition is FlagTagConditionDefinition flagTagConditionDefinition)
		{
			return tileFlagTag == flagTagConditionDefinition.FlagTag;
		}
		if (goalConditionDefinition is TargetInRangeConditionDefinition targetInRangeConditionDefinition)
		{
			int num = TileMapController.DistanceBetweenTiles(Goal.Owner.OriginTile, tile);
			int minEvalToInt = targetInRangeConditionDefinition.GetMinEvalToInt((InterpreterContext)(object)Goal);
			int maxEvalToInt = targetInRangeConditionDefinition.GetMaxEvalToInt((InterpreterContext)(object)Goal);
			if (num >= minEvalToInt)
			{
				return num <= maxEvalToInt;
			}
			return false;
		}
		return true;
	}

	private bool CheckTargetFlagTileConditionGroup(TileFlagDefinition.E_TileFlagTag tileFlagTag, Tile tile, GoalTargetTypeDefinition goalTargetTypeDefinition, GoalConditionDefinition[] conditionGroup)
	{
		int i = 0;
		for (int num = conditionGroup.Length; i < num; i++)
		{
			GoalConditionDefinition goalConditionDefinition = conditionGroup[i];
			if (CheckTargetFlagTileCondition(tileFlagTag, tile, goalConditionDefinition))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckTargetFlagTileConditionGroups(TileFlagDefinition.E_TileFlagTag tileFlagTag, Tile tile, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		if (goalTargetTypeDefinition.ConditionGroups == null)
		{
			return true;
		}
		int i = 0;
		for (int num = goalTargetTypeDefinition.ConditionGroups.Length; i < num; i++)
		{
			if (!CheckTargetFlagTileConditionGroup(tileFlagTag, tile, goalTargetTypeDefinition, goalTargetTypeDefinition.ConditionGroups[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckTargetTileObjectCondition(ITileObject targetTileObject, GoalConditionDefinition goalConditionDefinition, TileObjectSelectionManager.E_Orientation skillOrientation)
	{
		if (goalConditionDefinition is TargetHealthConditionDefinition targetHealthConditionDefinition)
		{
			if (!(targetTileObject is IDamageable damageable) || (targetTileObject is TheLastStand.Model.Building.Building building && building.BlueprintModule.IsIndestructible))
			{
				CLoggerManager.Log((object)"Target isn't a damageable, that is unexpected.", (LogType)2, (CLogLevel)1, true, "StaticLog", false);
				return false;
			}
			float num = damageable.Health / damageable.HealthTotal;
			if (num >= targetHealthConditionDefinition.Min)
			{
				return num <= targetHealthConditionDefinition.Max;
			}
			return false;
		}
		if (goalConditionDefinition is TargetIdConditionDefinition targetIdConditionDefinition)
		{
			if (targetTileObject is PlayableUnit)
			{
				((CLogger<EnemyUnitManager>)TPSingleton<EnemyUnitManager>.Instance).LogWarning((object)"Don't add TargetIdCondition to TargetType PlayableUnit! --'", (CLogLevel)0, true, false);
				return false;
			}
			for (int num2 = targetIdConditionDefinition.TargetIds.Length - 1; num2 >= 0; num2--)
			{
				string text = targetIdConditionDefinition.TargetIds[num2];
				if (targetTileObject.Id == text)
				{
					return !targetIdConditionDefinition.Exclude;
				}
			}
			return targetIdConditionDefinition.Exclude;
		}
		if (goalConditionDefinition is TargetIsNotEliteConditionDefinition)
		{
			return !(targetTileObject is EliteEnemyUnit);
		}
		DamageableCountInAoeConditionDefinition damageableCountInAoe = goalConditionDefinition as DamageableCountInAoeConditionDefinition;
		if (damageableCountInAoe != null)
		{
			List<Tile> affectedTiles = Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.GetAffectedTiles(targetTileObject.OriginTile, alwaysReturnFullPattern: false, skillOrientation);
			bool shouldCheckCanBeDamaged = Goal.Skill.IsAttackOrExecuteOrSurroundingDamage;
			int num3 = affectedTiles.Count((Tile tile) => tile.Damageable != null && damageableCountInAoe.DamageableTypesToCount.Contains(tile.Damageable.DamageableType) && (!shouldCheckCanBeDamaged || tile.Damageable == Goal.Owner || tile.Damageable.CanBeDamaged()));
			if (num3 >= damageableCountInAoe.Min)
			{
				return num3 <= damageableCountInAoe.Max;
			}
			return false;
		}
		ExcludeDamageableTypeInAoeConditionDefinition excludeUnitTypeInAoe = goalConditionDefinition as ExcludeDamageableTypeInAoeConditionDefinition;
		if (excludeUnitTypeInAoe != null)
		{
			return !Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.GetAffectedTiles(targetTileObject.OriginTile, alwaysReturnFullPattern: false, skillOrientation).Any((Tile tile) => tile.Unit != null && excludeUnitTypeInAoe.ExcludeDamageableTypes.Contains(tile.Unit.UnitTemplateDefinition.UnitType));
		}
		GroundCategoryConditionDefinition groundCategoryConditionDefinition = goalConditionDefinition as GroundCategoryConditionDefinition;
		if (groundCategoryConditionDefinition != null)
		{
			return targetTileObject.OccupiedTiles.Any((Tile x) => x.GroundDefinition.GroundCategory == groundCategoryConditionDefinition.GroundCategory);
		}
		TileHasHazardConditionDefinition tileHasHazardConditionDefinition = goalConditionDefinition as TileHasHazardConditionDefinition;
		if (tileHasHazardConditionDefinition != null)
		{
			return targetTileObject.OccupiedTiles.Any((Tile x) => x.HazardOwned.HasFlag(tileHasHazardConditionDefinition.HazardType));
		}
		if (Goal.Owner.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving && goalConditionDefinition is TargetInRangeConditionDefinition targetInRangeConditionDefinition)
		{
			_ = Goal.Owner;
			int minEvalToInt = targetInRangeConditionDefinition.GetMinEvalToInt((InterpreterContext)(object)Goal);
			int maxEvalToInt = targetInRangeConditionDefinition.GetMaxEvalToInt((InterpreterContext)(object)Goal);
			foreach (Tile occupiedTile in Goal.Owner.OccupiedTiles)
			{
				foreach (Tile occupiedTile2 in targetTileObject.OccupiedTiles)
				{
					int num4 = TileMapController.DistanceBetweenTiles(occupiedTile, occupiedTile2);
					if (num4 >= minEvalToInt && num4 <= maxEvalToInt)
					{
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	private bool CheckTargetTileObjectConditionGroup(ITileObject targetTileObject, GoalTargetTypeDefinition goalTargetTypeDefinition, GoalConditionDefinition[] conditionGroup, TileObjectSelectionManager.E_Orientation skillOrientation)
	{
		int i = 0;
		for (int num = conditionGroup.Length; i < num; i++)
		{
			GoalConditionDefinition goalConditionDefinition = conditionGroup[i];
			if (CheckTargetTileObjectCondition(targetTileObject, goalConditionDefinition, skillOrientation))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckTargetTileObjectConditionGroups(ITileObject targetTileObject, GoalTargetTypeDefinition goalTargetTypeDefinition, TileObjectSelectionManager.E_Orientation skillOrientation)
	{
		if (goalTargetTypeDefinition.ConditionGroups == null)
		{
			return true;
		}
		int i = 0;
		for (int num = goalTargetTypeDefinition.ConditionGroups.Length; i < num; i++)
		{
			if (!CheckTargetTileObjectConditionGroup(targetTileObject, goalTargetTypeDefinition, goalTargetTypeDefinition.ConditionGroups[i], skillOrientation))
			{
				return false;
			}
		}
		return true;
	}

	private void AddCandidateTargetBuildings(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		EnemyUnit enemyUnit = Goal.Owner as EnemyUnit;
		for (int num = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num >= 0; num--)
		{
			TheLastStand.Model.Building.Building building = TPSingleton<BuildingManager>.Instance.Buildings[num];
			if (building.DamageableModule != null && (Goal.Owner.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.AfterMoving || Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(building)))
			{
				foreach (Tile occupiedTile in building.OccupiedTiles)
				{
					foreach (TileObjectSelectionManager.E_Orientation item in GetOrientationsToCheck(occupiedTile))
					{
						SkillTargetedTileInfo skillTargetedTileInfo = new SkillTargetedTileInfo(occupiedTile, item);
						if (enemyUnit != null && item != 0 && enemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving)
						{
							Tile tileFromTileToOrientation = TileObjectSelectionManager.GetTileFromTileToOrientation(skillTargetedTileInfo.Tile, skillTargetedTileInfo.Orientation, -1);
							if (tileFromTileToOrientation == null || (!(building is MagicCircle) && !enemyUnit.CanStopOn(tileFromTileToOrientation)))
							{
								continue;
							}
						}
						if (building.IsTargetableByAI() && CheckTargetTileObjectConditionGroups(building, goalTargetTypeDefinition, skillTargetedTileInfo.Orientation))
						{
							candidateTargetTiles.Add(skillTargetedTileInfo);
						}
					}
				}
			}
		}
	}

	private void AddCandidateTargetTiles(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		HashSet<Tile> hashSet = new HashSet<Tile>();
		EnemyUnit enemyUnit = Goal.Owner as EnemyUnit;
		if (goalTargetTypeDefinition.ConditionGroups != null)
		{
			_ = Goal.Owner;
			int num = int.MaxValue;
			int num2 = 0;
			GoalConditionDefinition[][] conditionGroups = goalTargetTypeDefinition.ConditionGroups;
			foreach (GoalConditionDefinition[] array in conditionGroups)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] is TargetInRangeConditionDefinition targetInRangeConditionDefinition)
					{
						int minEvalToInt = targetInRangeConditionDefinition.GetMinEvalToInt((InterpreterContext)(object)Goal);
						int maxEvalToInt = targetInRangeConditionDefinition.GetMaxEvalToInt((InterpreterContext)(object)Goal);
						if (minEvalToInt < num)
						{
							num = minEvalToInt;
						}
						if (maxEvalToInt > num2)
						{
							num2 = maxEvalToInt;
						}
					}
				}
			}
			LinqExtensions.AddRange<Tile>(hashSet, (IEnumerable<Tile>)Goal.Owner.TileObjectController.GetTilesInRange(num2, num));
		}
		else
		{
			Goal.Owner.LogWarning("Trying to target a Tile, but no ConditionGroup has been found. Are you SURE you want to do this? It's a high cost goal.", (CLogLevel)1);
			LinqExtensions.AddRange<Tile>(hashSet, (IEnumerable<Tile>)TPSingleton<TileMapManager>.Instance.TileMap.Tiles);
		}
		foreach (Tile item in hashSet)
		{
			if ((Goal.Owner.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.AfterMoving && !Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(item)) || (goalTargetTypeDefinition.IsTileContentAccepted.HasValue && (item.Unit != null || item.Building != null) != goalTargetTypeDefinition.IsTileContentAccepted))
			{
				continue;
			}
			foreach (TileObjectSelectionManager.E_Orientation item2 in GetOrientationsToCheck(item))
			{
				SkillTargetedTileInfo skillTargetedTileInfo = new SkillTargetedTileInfo(item, item2);
				if (enemyUnit != null && item2 != 0 && enemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving)
				{
					Tile tileFromTileToOrientation = TileObjectSelectionManager.GetTileFromTileToOrientation(skillTargetedTileInfo.Tile, skillTargetedTileInfo.Orientation, -1);
					if (tileFromTileToOrientation == null || !enemyUnit.CanStopOn(tileFromTileToOrientation))
					{
						continue;
					}
				}
				if (CheckTargetTileObjectConditionGroups(item, goalTargetTypeDefinition, skillTargetedTileInfo.Orientation))
				{
					candidateTargetTiles.Add(skillTargetedTileInfo);
				}
			}
		}
	}

	private void AddCandidateTargetEnemyUnits(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		List<EnemyUnit> list = new List<EnemyUnit>(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count + TPSingleton<BossManager>.Instance.BossUnits.Count);
		list.AddRange(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits);
		list.AddRange(TPSingleton<BossManager>.Instance.BossUnits);
		EnemyUnit enemyUnit = Goal.Owner as EnemyUnit;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			EnemyUnit enemyUnit2 = list[num];
			if (Goal.Owner.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.AfterMoving || Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(enemyUnit2))
			{
				Tile tile = ((enemyUnit2.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.BeforeMoving || enemyUnit2 == Goal.Owner || enemyUnit2.TargetTile == null) ? enemyUnit2.OriginTile : enemyUnit2.TargetTile);
				foreach (TileObjectSelectionManager.E_Orientation item in GetOrientationsToCheck(tile))
				{
					SkillTargetedTileInfo skillTargetedTileInfo = new SkillTargetedTileInfo(tile, item);
					if (enemyUnit != null && item != 0 && enemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving)
					{
						Tile tileFromTileToOrientation = TileObjectSelectionManager.GetTileFromTileToOrientation(skillTargetedTileInfo.Tile, skillTargetedTileInfo.Orientation, -1);
						if (tileFromTileToOrientation == null || !enemyUnit.CanStopOn(tileFromTileToOrientation))
						{
							continue;
						}
					}
					if (enemyUnit2.IsTargetableByAI() && CheckTargetTileObjectConditionGroups(enemyUnit2, goalTargetTypeDefinition, skillTargetedTileInfo.Orientation))
					{
						candidateTargetTiles.Add(skillTargetedTileInfo);
					}
				}
			}
		}
	}

	private void AddCandidateTargetPlayableUnits(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		EnemyUnit enemyUnit = Goal.Owner as EnemyUnit;
		for (int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num >= 0; num--)
		{
			PlayableUnit playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num];
			if (Goal.Owner.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.AfterMoving || Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(playableUnit.OriginTile))
			{
				foreach (TileObjectSelectionManager.E_Orientation item in GetOrientationsToCheck(playableUnit.OriginTile))
				{
					SkillTargetedTileInfo skillTargetedTileInfo = new SkillTargetedTileInfo(playableUnit.OriginTile, item);
					if (enemyUnit != null && item != 0 && enemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving)
					{
						Tile tileFromTileToOrientation = TileObjectSelectionManager.GetTileFromTileToOrientation(skillTargetedTileInfo.Tile, skillTargetedTileInfo.Orientation, -1);
						if (tileFromTileToOrientation == null || !enemyUnit.CanStopOn(tileFromTileToOrientation))
						{
							continue;
						}
					}
					if (playableUnit.IsTargetableByAI() && CheckTargetTileObjectConditionGroups(playableUnit, goalTargetTypeDefinition, skillTargetedTileInfo.Orientation))
					{
						candidateTargetTiles.Add(skillTargetedTileInfo);
					}
				}
			}
		}
	}

	private void AddCandidateTargetTileFlags(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		foreach (KeyValuePair<TileFlagDefinition.E_TileFlagTag, List<Tile>> item2 in TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag)
		{
			for (int num = item2.Value.Count - 1; num >= 0; num--)
			{
				Tile tile = item2.Value[num];
				if ((Goal.Owner.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.AfterMoving || Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(tile)) && CheckTargetFlagTileConditionGroups(item2.Key, tile, goalTargetTypeDefinition))
				{
					SkillTargetedTileInfo item = new SkillTargetedTileInfo(tile, TileObjectSelectionManager.E_Orientation.NONE);
					candidateTargetTiles.Add(item);
				}
			}
		}
	}

	private void AddCandidateTargets(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		switch (goalTargetTypeDefinition.TargetType)
		{
		case GoalTargetTypeDefinition.E_TargetType.PlayableUnit:
			AddCandidateTargetPlayableUnits(candidateTargetTiles, goalTargetTypeDefinition);
			break;
		case GoalTargetTypeDefinition.E_TargetType.EnemyUnit:
			AddCandidateTargetEnemyUnits(candidateTargetTiles, goalTargetTypeDefinition);
			break;
		case GoalTargetTypeDefinition.E_TargetType.Building:
			AddCandidateTargetBuildings(candidateTargetTiles, goalTargetTypeDefinition);
			break;
		case GoalTargetTypeDefinition.E_TargetType.Tile:
			AddCandidateTargetTiles(candidateTargetTiles, goalTargetTypeDefinition);
			break;
		case GoalTargetTypeDefinition.E_TargetType.TileFlag:
			AddCandidateTargetTileFlags(candidateTargetTiles, goalTargetTypeDefinition);
			break;
		case GoalTargetTypeDefinition.E_TargetType.Itself:
		{
			SkillTargetedTileInfo item = new SkillTargetedTileInfo(Goal.Owner.OriginTile, TileObjectSelectionManager.E_Orientation.NONE);
			candidateTargetTiles.Add(item);
			break;
		}
		}
	}

	private SkillTargetedTileInfo FilterCandidatesThroughTargetingMethods(Dictionary<SkillTargetedTileInfo, bool> candidateTargetTiles)
	{
		List<SkillTargetedTileInfo> list = ((!Goal.GoalDefinition.TargetingMethodsContainer.AvoidOverkill || !candidateTargetTiles.Any((KeyValuePair<SkillTargetedTileInfo, bool> kvp) => !kvp.Value)) ? candidateTargetTiles.Keys.ToList() : (from kvp in candidateTargetTiles
			where !kvp.Value
			select kvp.Key).ToList());
		for (int i = 0; i < Goal.GoalDefinition.TargetingMethodsContainer.TargetingMethods.Count; i++)
		{
			if (list.Count == 1)
			{
				return list[0];
			}
			TargetingMethodDefinition targetingMethodDefinition = Goal.GoalDefinition.TargetingMethodsContainer.TargetingMethods[i];
			if (targetingMethodDefinition is ClosestTargetingMethodDefinition)
			{
				List<Tuple<SkillTargetedTileInfo, int>> list2 = new List<Tuple<SkillTargetedTileInfo, int>>();
				int minDistance = int.MaxValue;
				for (int num = list.Count - 1; num >= 0; num--)
				{
					int num2 = TileMapController.DistanceBetweenTiles(list[num].Tile, Goal.Owner.OriginTile);
					list2.Add(new Tuple<SkillTargetedTileInfo, int>(list[num], num2));
					if (num2 < minDistance)
					{
						minDistance = num2;
					}
				}
				list = list.Except(from x in list2
					where x.Item2 > minDistance
					select x into tuple
					select tuple.Item1).ToList();
				continue;
			}
			if (targetingMethodDefinition is FarthestTargetingMethodDefinition)
			{
				List<Tuple<SkillTargetedTileInfo, int>> list3 = new List<Tuple<SkillTargetedTileInfo, int>>();
				int maxDistance = int.MinValue;
				for (int num3 = list.Count - 1; num3 >= 0; num3--)
				{
					int num4 = TileMapController.DistanceBetweenTiles(list[num3].Tile, Goal.Owner.OriginTile);
					list3.Add(new Tuple<SkillTargetedTileInfo, int>(list[num3], num4));
					if (num4 > maxDistance)
					{
						maxDistance = num4;
					}
				}
				list = list.Except(from x in list3
					where x.Item2 < maxDistance
					select x into tuple
					select tuple.Item1).ToList();
				continue;
			}
			if (targetingMethodDefinition is FirstTargetTargetingMethodDefinition)
			{
				return list[0];
			}
			OptimalTargetingMethodDefinition optimalTargetingMethodDefinition = targetingMethodDefinition as OptimalTargetingMethodDefinition;
			if (optimalTargetingMethodDefinition != null)
			{
				List<Tuple<SkillTargetedTileInfo, int>> list4 = new List<Tuple<SkillTargetedTileInfo, int>>();
				int maxScore2 = int.MinValue;
				for (int num5 = list.Count - 1; num5 >= 0; num5--)
				{
					List<Tile> affectedTiles = Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.GetAffectedTiles(list[num5].Tile, alwaysReturnFullPattern: false, list[num5].Orientation);
					HashSet<IDamageable> hashSet = new HashSet<IDamageable>();
					foreach (Tile item in affectedTiles)
					{
						if (item.Damageable != null)
						{
							hashSet.Add(item.Damageable);
						}
					}
					int num6 = 0;
					if (optimalTargetingMethodDefinition.DamageableTypesWeight != null)
					{
						num6 = hashSet.Sum((IDamageable d) => optimalTargetingMethodDefinition.DamageableTypesWeight.TryGetValue(d.DamageableType, out var value) ? value : 0);
					}
					if (optimalTargetingMethodDefinition.DamageableIdsWeight != null)
					{
						foreach (IDamageable item2 in hashSet)
						{
							string text = ((item2 is DamageableModule damageableModule) ? damageableModule.BuildingParent.Id : ((!(item2 is TheLastStand.Model.Unit.Unit unit)) ? null : unit.Id));
							string text2 = text;
							if (text2 == null)
							{
								Goal.Owner.LogError($"Tried to do an optimal goal on {item2} but Id comparison is not setup.", (CLogLevel)1);
								continue;
							}
							foreach (var (source, num7) in optimalTargetingMethodDefinition.DamageableIdsWeight)
							{
								if (source.Contains(text2))
								{
									num6 += num7;
								}
							}
						}
					}
					list4.Add(new Tuple<SkillTargetedTileInfo, int>(list[num5], num6));
					if (num6 > maxScore2)
					{
						maxScore2 = num6;
					}
				}
				list = list.Except(from x in list4
					where x.Item2 < maxScore2
					select x into tuple
					select tuple.Item1).ToList();
				continue;
			}
			if (targetingMethodDefinition is RandomTargetingMethodDefinition)
			{
				return list[RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, list.Count - 1)];
			}
			if (!(targetingMethodDefinition is ScoreTargetingMethodDefinition scoreTargetingMethodDefinition))
			{
				continue;
			}
			List<Tuple<SkillTargetedTileInfo, float>> list5 = new List<Tuple<SkillTargetedTileInfo, float>>();
			float maxScore = float.MinValue;
			for (int num8 = list.Count - 1; num8 >= 0; num8--)
			{
				Goal.GoalInterpreterContext.TargetCandidateTile = list[num8].Tile;
				float num9 = scoreTargetingMethodDefinition.Score.EvalToFloat((object)Goal.GoalInterpreterContext);
				list5.Add(new Tuple<SkillTargetedTileInfo, float>(list[num8], num9));
				if (num9 > maxScore)
				{
					maxScore = num9;
				}
			}
			list = list.Except(from x in list5
				where x.Item2 < maxScore
				select x into tuple
				select tuple.Item1).ToList();
		}
		return list[RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, list.Count - 1)];
	}

	private Dictionary<SkillTargetedTileInfo, bool> GetCandidateTargetTiles(Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedDamageables = null)
	{
		List<SkillTargetedTileInfo> list = new List<SkillTargetedTileInfo>();
		for (int num = Goal.GoalDefinition.GoalTargetTypeDefinitions.Length - 1; num >= 0; num--)
		{
			AddCandidateTargets(list, Goal.GoalDefinition.GoalTargetTypeDefinitions[num]);
		}
		Dictionary<SkillTargetedTileInfo, bool> dictionary = list.ToDictionary((SkillTargetedTileInfo key) => key, (SkillTargetedTileInfo value) => false);
		KeyValuePair<IDamageable, GroupTargetingInfo> keyValuePair = default(KeyValuePair<IDamageable, GroupTargetingInfo>);
		for (int num2 = dictionary.Count - 1; num2 >= 0; num2--)
		{
			SkillTargetedTileInfo skillInfo = dictionary.ElementAt(num2).Key;
			if (alreadyTargetedDamageables != null && skillInfo.Tile.Damageable != null && ListExtensions.TryFind<KeyValuePair<IDamageable, GroupTargetingInfo>>((IEnumerable<KeyValuePair<IDamageable, GroupTargetingInfo>>)alreadyTargetedDamageables, (Func<KeyValuePair<IDamageable, GroupTargetingInfo>, bool>)((KeyValuePair<IDamageable, GroupTargetingInfo> x) => x.Key == skillInfo.Tile.Damageable), ref keyValuePair))
			{
				if (keyValuePair.Value.EntitiesIdTargeting.Contains(Goal.Owner.RandomId) && Goal.Owner is BattleModule battleModule && battleModule.BuildingParent.IsTurret)
				{
					dictionary.Remove(skillInfo);
					continue;
				}
				TargetingMethodsContainerDefinition targetingMethodsContainer = Goal.GoalDefinition.TargetingMethodsContainer;
				if (targetingMethodsContainer != null && targetingMethodsContainer.AvoidOverkill && keyValuePair.Value.MinDamage >= keyValuePair.Key.Health + keyValuePair.Key.Armor)
				{
					dictionary[skillInfo] = true;
				}
			}
			if (Goal.Skill.HasManeuver && !Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.IsManeuverValid(skillInfo.Tile, skillInfo.Orientation))
			{
				Goal.Owner.Log($"Removed {skillInfo.Tile}:{skillInfo.Orientation} from candidate target tiles. Reason: Invalid Maneuver", (CLogLevel)1, forcePrintInUnity: true);
				dictionary.Remove(skillInfo);
			}
			else if (!Goal.GoalDefinition.GoalTargetTypeDefinitions.Any((GoalTargetTypeDefinition x) => x.TargetType == GoalTargetTypeDefinition.E_TargetType.Itself) && Goal.Skill.IsAttackOrExecuteOrSurroundingDamage && !Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.GetAffectedTiles(skillInfo.Tile, alwaysReturnFullPattern: false, skillInfo.Orientation).Any((Tile tile) => tile.Damageable != null && (tile.Damageable == Goal.Owner || tile.Damageable.CanBeDamaged())))
			{
				Goal.Owner.Log($"Removed {skillInfo.Tile}:{skillInfo.Orientation} from candidate target tiles. Reason: Not Damaging any target in aoe", (CLogLevel)1, forcePrintInUnity: true);
				dictionary.Remove(skillInfo);
			}
			else if (Goal.Skill.SkillAction.SkillActionController is SpawnSkillActionController spawnSkillActionController && !spawnSkillActionController.ValidateCandidateTargetTile(skillInfo.Tile))
			{
				dictionary.Remove(skillInfo);
			}
		}
		return dictionary;
	}

	private List<TileObjectSelectionManager.E_Orientation> GetOrientationsToCheck(Tile tile)
	{
		return orientationsToCheck ?? new List<TileObjectSelectionManager.E_Orientation>(1) { Goal.Skill.TileDependantOrientation(tile) };
	}
}
