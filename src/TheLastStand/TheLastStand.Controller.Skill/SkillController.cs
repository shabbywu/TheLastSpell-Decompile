using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.Controller.Skill;

public class SkillController
{
	public TheLastStand.Model.Skill.Skill Skill { get; private set; }

	public SkillController(SerializedSkill container, ISkillContainer skillContainer)
	{
		Skill = new TheLastStand.Model.Skill.Skill(container, this, skillContainer);
		CreateSkillEffects();
	}

	public SkillController(SkillDefinition skillDefinition, ISkillContainer skillContainer, int overallUsesCount = -1, int usesPerTurnCount = -1)
	{
		Skill = new TheLastStand.Model.Skill.Skill(skillDefinition, this, skillContainer, overallUsesCount, usesPerTurnCount);
		CreateSkillEffects();
	}

	public bool CanExecuteSkill(float actionPoints, float movePoints, float mana, float health, bool isStun)
	{
		if (!isStun && Skill.UsesPerTurnRemaining != 0 && Skill.OverallUsesRemaining != 0 && ((float)Skill.ActionPointsCost <= actionPoints || actionPoints == -1f) && ((float)Skill.MovePointsCost <= movePoints || movePoints == -1f) && ((float)Skill.HealthCost < health || health == -1f) && ((float)Skill.ManaCost <= mana || mana == -1f))
		{
			return Skill.SkillController.CheckPhaseAllowed();
		}
		return false;
	}

	public bool CheckConditions(PlayableUnit playableUnit, bool dontCheckPhase = false)
	{
		if (CheckContextualConditions(playableUnit) && (dontCheckPhase || CheckPhaseDisplay()))
		{
			return Skill.PerkLocksBuffer <= 0;
		}
		return false;
	}

	public bool CheckContextualConditions(PlayableUnit playableUnit)
	{
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		foreach (SkillConditionDefinition contextualCondition in Skill.SkillDefinition.ContextualConditions)
		{
			Vector2Int position;
			switch (contextualCondition.Name)
			{
			case "BuildingExist":
			{
				BuildingExistConditionDefinition buildingExistConditionDefinition = contextualCondition as BuildingExistConditionDefinition;
				foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
				{
					if (building.BuildingDefinition.Id == buildingExistConditionDefinition.BuildingDefinitionId)
					{
						flag = true;
						break;
					}
				}
				break;
			}
			case "InWatchtower":
				flag = playableUnit.OriginTile.Building != null && playableUnit.OriginTile.Building.IsWatchtower;
				break;
			case "NextToBuilding":
			{
				NextToBuildingConditionDefinition nextToBuildingConditionDefinition = contextualCondition as NextToBuildingConditionDefinition;
				flag = false;
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						if (Mathf.Abs(i) + Mathf.Abs(j) == 1)
						{
							TheLastStand.Model.TileMap.TileMap tileMap2 = TPSingleton<TileMapManager>.Instance.TileMap;
							position = playableUnit.OriginTile.Position;
							int x2 = ((Vector2Int)(ref position)).x + i;
							position = playableUnit.OriginTile.Position;
							Tile tile2 = tileMap2.GetTile(x2, ((Vector2Int)(ref position)).y + j);
							if (tile2 != null && !tile2.HasFog && tile2.Building != null && tile2.Building.BuildingDefinition.Id == nextToBuildingConditionDefinition.BuildingDefinitionId)
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
				break;
			}
			case "NotInBuilding":
				flag = playableUnit.OriginTile.Building == null;
				break;
			case "OntoBuilding":
			{
				OntoBuildingConditionDefinition ontoBuildingConditionDefinition = contextualCondition as OntoBuildingConditionDefinition;
				flag = false;
				TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
				position = playableUnit.OriginTile.Position;
				int x = ((Vector2Int)(ref position)).x;
				position = playableUnit.OriginTile.Position;
				Tile tile = tileMap.GetTile(x, ((Vector2Int)(ref position)).y);
				if (!tile.HasFog && tile.Building != null && tile.Building.BuildingDefinition.Id == ontoBuildingConditionDefinition.BuildingDefinitionId)
				{
					flag = true;
				}
				break;
			}
			}
			if (!flag)
			{
				break;
			}
		}
		return flag & !playableUnit.PerkTree.UnitPerkTreeController.IsSkillLockedByPerks(Skill);
	}

	public bool CheckPhaseAllowed()
	{
		return CheckPhaseFlags(Skill.SkillDefinition.AllowDuringPhase);
	}

	public bool CheckPhaseDisplay()
	{
		return CheckPhaseFlags(Skill.SkillDefinition.DisplayDuringPhase);
	}

	public int ComputeMaxRange()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int range = Skill.SkillDefinition.Range;
		int result = ((Vector2Int)(ref range)).y;
		if (Skill.SkillAction.SkillActionExecution.Caster is TheLastStand.Model.Unit.Unit unit)
		{
			result = unit.UnitController.GetModifiedMaxRange(Skill);
		}
		return result;
	}

	public bool ComputeTargetsAndValidity(ISkillCaster skillCaster, bool shouldUpdateView = false)
	{
		bool result = false;
		if (Skill.Targets == null)
		{
			Skill.Targets = new List<ITileObject>();
		}
		else
		{
			Skill.Targets.Clear();
		}
		if (Skill.SkillDefinition.ValidTargets == null)
		{
			return true;
		}
		if (Skill.SkillDefinition.InfiniteRange)
		{
			Tile[] tiles = TPSingleton<TileMapManager>.Instance.TileMap.Tiles;
			foreach (Tile tile in tiles)
			{
				if (tile.CanAffectThroughFog(skillCaster) && TryAddTarget(tile))
				{
					result = true;
				}
			}
		}
		else
		{
			foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> item in Skill.SkillAction.SkillActionExecution.InRangeTiles.Range)
			{
				if (item.Key != null && item.Value.HasLineOfSight && TryAddTarget(item.Key))
				{
					result = true;
				}
			}
		}
		if (shouldUpdateView)
		{
			foreach (ITileObject target in Skill.Targets)
			{
				if (RequiresTargetValidationFeedback(target.OriginTile))
				{
					target.TileObjectView.ToggleSkillTargeting(display: true);
				}
			}
		}
		return result;
	}

	public bool HasAtLeastOneTileInRange(Tile sourceTile, Tile[] destinationTiles)
	{
		foreach (Tile targetTile in destinationTiles)
		{
			if (Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInRange(targetTile))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsValidatingTargetingConstraints(Tile targetTile, bool isSkillTargetTile = true)
	{
		if (Skill.SkillDefinition.ValidTargets == null)
		{
			return true;
		}
		if (isSkillTargetTile && Skill.Targets != null && Skill.Targets.Count > 0)
		{
			if (!Skill.Targets.Contains(targetTile.Building) && !Skill.Targets.Contains(targetTile.Unit))
			{
				return Skill.Targets.Contains(targetTile);
			}
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.EmptyTiles && targetTile.GroundDefinition.IsCrossable && targetTile.IsEmpty())
		{
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.WalkableTiles && targetTile.GroundDefinition.IsCrossable && targetTile.Unit == null && Skill.SkillAction.SkillActionExecution.Caster is TheLastStand.Model.Unit.Unit unit && unit.CanStopOn(targetTile))
		{
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.UncrossableGrounds && !targetTile.GroundDefinition.IsCrossable && targetTile.Unit == null)
		{
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.PlayableUnits && targetTile.Unit is PlayableUnit)
		{
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.EnemyUnits && targetTile.Unit is EnemyUnit)
		{
			return true;
		}
		if (targetTile.Building != null && Skill.SkillDefinition.ValidTargets.Buildings.TryGetValue(targetTile.Building.Id, out var _))
		{
			return true;
		}
		return false;
	}

	public bool RequiresTargetValidationFeedback(Tile targetTile)
	{
		if (Skill.SkillDefinition.ValidTargets == null)
		{
			return false;
		}
		if ((targetTile.Unit is PlayableUnit && Skill.SkillDefinition.ValidTargets.PlayableUnits) || (targetTile.Unit is EnemyUnit && Skill.SkillDefinition.ValidTargets.EnemyUnits))
		{
			if (Skill.SkillAction is ResupplySkillAction resupplySkillAction && !resupplySkillAction.CheckUnitNeedResupply(targetTile.Unit))
			{
				return false;
			}
			return true;
		}
		if (targetTile.Building != null && Skill.SkillDefinition.ValidTargets.Buildings.Count > 0)
		{
			ValidTargets.Constraints value;
			bool result = (targetTile.Building.BlueprintModule.IsIndestructible || !targetTile.Building.DamageableModule.IsDead) && Skill.SkillDefinition.ValidTargets.Buildings.TryGetValue(targetTile.Building.BuildingDefinition.Id, out value) && (!value.MustBeEmpty || targetTile.Unit == null) && (!value.NeedRepair || (Skill.SkillAction is ResupplySkillAction resupplySkillAction2 && resupplySkillAction2.CheckBuildingNeedRepair(targetTile.Building)));
			TileObjectSelectionManager.E_Orientation specificOrientation = Skill.TileDependantOrientation(targetTile);
			if (!Skill.SkillAction.SkillActionExecution.SkillExecutionController.IsManeuverValid(targetTile, specificOrientation))
			{
				return false;
			}
			return result;
		}
		if (targetTile.Unit == null && targetTile.Building == null && Skill.SkillDefinition.ValidTargets.EmptyTiles)
		{
			return false;
		}
		if (Skill.SkillDefinition.ValidTargets.EmptyTiles && (targetTile.Unit != null || targetTile.Building != null))
		{
			return true;
		}
		if (Skill.SkillDefinition.ValidTargets.UncrossableGrounds && targetTile.IsEmpty() && !targetTile.GroundDefinition.IsCrossable)
		{
			return true;
		}
		return false;
	}

	private bool CheckPhaseFlags(SkillDefinition.E_Phase flags)
	{
		if (SkillManager.DebugSkillsAllowAllPhases)
		{
			return true;
		}
		if (!flags.HasFlag(SkillDefinition.E_Phase.All) && (!flags.HasFlag(SkillDefinition.E_Phase.Night) || TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night) && (!flags.HasFlag(SkillDefinition.E_Phase.Production) || TPSingleton<GameManager>.Instance.Game.DayTurn != Game.E_DayTurn.Production))
		{
			if (flags.HasFlag(SkillDefinition.E_Phase.Deployment))
			{
				return TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Deployment;
			}
			return false;
		}
		return true;
	}

	private void CreateSkillEffects()
	{
		if (Skill.SkillDefinition.SkillActionDefinition is AttackSkillActionDefinition)
		{
			Skill.SkillAction = new AttackSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is GenericSkillActionDefinition)
		{
			Skill.SkillAction = new GenericSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is GoIntoWatchtowerSkillActionDefinition)
		{
			Skill.SkillAction = new GoIntoWatchtowerSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is QuitWatchtowerSkillActionDefinition)
		{
			Skill.SkillAction = new QuitWatchtowerSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is SkipTurnSkillActionDefinition)
		{
			Skill.SkillAction = new SkipTurnSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is SpawnSkillActionDefinition)
		{
			Skill.SkillAction = new SpawnSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is BuildSkillActionDefinition)
		{
			Skill.SkillAction = new BuildSkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else if (Skill.SkillDefinition.SkillActionDefinition is ResupplySkillActionDefinition)
		{
			Skill.SkillAction = new ResupplySkillActionController(Skill.SkillDefinition.SkillActionDefinition, Skill).SkillAction;
		}
		else
		{
			((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).LogError((object)("Unknown skill " + ((object)Skill.SkillDefinition.SkillActionDefinition).GetType().Name), (CLogLevel)2, true, true);
		}
	}

	private bool TryAddTarget(Tile tile)
	{
		TileObjectSelectionManager.E_Orientation specificOrientation = Skill.TileDependantOrientation(tile);
		if (!Skill.SkillAction.SkillActionExecution.SkillExecutionController.IsManeuverValid(tile, specificOrientation))
		{
			return false;
		}
		if (tile.IsEmpty())
		{
			if (Skill.SkillDefinition.ValidTargets.EmptyTiles || (Skill.SkillDefinition.ValidTargets.WalkableTiles && (!(Skill.Owner is TheLastStand.Model.Unit.Unit unit) || unit.CanStopOn(tile))))
			{
				Skill.Targets.Add(tile);
				return true;
			}
			if (Skill.SkillDefinition.ValidTargets.UncrossableGrounds && !tile.GroundDefinition.IsCrossable)
			{
				Skill.Targets.Add(tile);
				return true;
			}
		}
		else
		{
			if (tile.Building != null && (tile.Building.BlueprintModule.IsIndestructible || !tile.Building.DamageableModule.IsDead) && !Skill.Targets.Contains(tile.Building) && Skill.SkillDefinition.ValidTargets != null)
			{
				ResupplySkillAction resupplySkillAction = Skill.SkillAction as ResupplySkillAction;
				if (Skill.SkillDefinition.ValidTargets.Buildings.TryGetValue(tile.Building.BuildingDefinition.Id, out var value) && (!value.MustBeEmpty || tile.Unit == null) && (!value.NeedRepair || (resupplySkillAction != null && resupplySkillAction.CheckBuildingNeedRepair(tile.Building))))
				{
					List<ITileObject> targets = Skill.Targets;
					ITileObject item;
					if (!(Skill.SkillAction.SkillActionExecution.Caster is PlayableUnit))
					{
						ITileObject building = tile.Building;
						item = building;
					}
					else
					{
						ITileObject building = tile;
						item = building;
					}
					targets.Add(item);
					return true;
				}
				if (Skill.SkillDefinition.ValidTargets.WalkableTiles && (!(Skill.Owner is TheLastStand.Model.Unit.Unit unit2) || unit2.CanStopOn(tile)))
				{
					Skill.Targets.Add(tile);
					return true;
				}
			}
			if (tile.Unit != null && !Skill.Targets.Contains(tile.Unit) && Skill.SkillDefinition.ValidTargets != null)
			{
				ResupplySkillAction resupplySkillAction2 = Skill.SkillAction as ResupplySkillAction;
				if (tile.Unit is PlayableUnit && Skill.SkillDefinition.ValidTargets.PlayableUnits && (resupplySkillAction2 == null || resupplySkillAction2.CheckUnitNeedResupply(tile.Unit)))
				{
					Skill.Targets.Add(tile.Unit);
					return true;
				}
				if (tile.Unit is EnemyUnit && Skill.SkillDefinition.ValidTargets.EnemyUnits && Skill.Owner is PlayableUnit playableUnit)
				{
					SkillConditionDefinition skillConditionDefinition = Skill.SkillDefinition.ContextualConditions.Find((SkillConditionDefinition o) => o.Name == "MaxTargetHealthLeft");
					if (skillConditionDefinition != null && (tile.Unit.Health == 0f || tile.Unit.Health > ((MaxTargetHealthLeftConditionDefinition)skillConditionDefinition).HealthThreshold.EvalToFloat((InterpreterContext)(object)playableUnit) || (tile.Unit is EnemyUnit enemyUnit && enemyUnit.EnemyUnitTemplateDefinition.IsInvulnerable)))
					{
						return false;
					}
					Skill.Targets.Add(tile.Unit);
					return true;
				}
			}
		}
		return false;
	}
}
