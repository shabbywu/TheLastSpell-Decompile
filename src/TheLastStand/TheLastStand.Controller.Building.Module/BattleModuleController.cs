using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.View.TileMap;

namespace TheLastStand.Controller.Building.Module;

public class BattleModuleController : BuildingModuleController, IBehaviorController, ISkillCasterController
{
	public BattleModule BattleModule { get; }

	public ISkillCaster SkillCaster => BattleModule;

	public BattleModuleController(BuildingController buildingControllerParent, BattleModuleDefinition battleModuleDefinition)
		: base(buildingControllerParent, battleModuleDefinition)
	{
		BattleModule = base.BuildingModule as BattleModule;
	}

	public void ClearCurrentGoal()
	{
		BattleModule.Log("Cleared current goal", (CLogLevel)0);
		BattleModule.TargetTile = null;
		BattleModule.CurrentGoals = new ComputedGoal[BattleModule.NumberOfGoalsToCompute];
	}

	public void ComputeCurrentGoals(Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedTiles = null)
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		ClearCurrentGoal();
		BattleModule.Log($"Computing current goals out of {BattleModule.BattleModuleDefinition.Behavior.GoalDefinitions.Length} possible goals", (CLogLevel)0);
		for (int i = 0; i < BattleModule.NumberOfGoalsToCompute && (!BattleModule.BuildingParent.IsTrap || BattleModule.RemainingTrapCharges > i); i++)
		{
			int j = 0;
			for (int num = BattleModule.BattleModuleDefinition.Behavior.GoalDefinitions.Length; j < num; j++)
			{
				Goal goal = BattleModule.Goals[j];
				if (goal.GoalDefinition.GoalComputingStep.HasFlag(BattleModule.GoalComputingStep))
				{
					goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(BattleModule);
					SkillTargetedTileInfo skillTargetedTileInfo = goal.GoalController.ComputeTarget(alreadyTargetedTiles);
					if (skillTargetedTileInfo != null)
					{
						BattleModule.Log($"Validated {skillTargetedTileInfo.Tile.Position} Orientation: {skillTargetedTileInfo.Orientation} as best goal", (CLogLevel)1, forcePrintInUnity: true);
						BattleModule.CurrentGoals[i] = new ComputedGoal(goal, skillTargetedTileInfo);
						break;
					}
				}
			}
		}
	}

	public void CreateGoals()
	{
		if (BattleModule?.BehaviourDefinition != null)
		{
			int num = BattleModule.BehaviourDefinition.GoalDefinitions.Length;
			BattleModule.Goals = new Goal[num];
			for (int i = 0; i < num; i++)
			{
				BattleModule.Goals[i] = new GoalController(BattleModule.BehaviourDefinition.GoalDefinitions[i], BattleModule).Goal;
			}
		}
	}

	public void DecrementGoalsCooldown()
	{
		Goal[] goals = BattleModule.Goals;
		for (int i = 0; i < goals.Length; i++)
		{
			goals[i].GoalController.StartTurn();
		}
	}

	public void ExecuteAllGoals()
	{
		if (BattleModule.CurrentGoals == null || BattleModule.CurrentGoals.Length == 0)
		{
			return;
		}
		List<ComputedGoal> list = BattleModule.CurrentGoals.ToList();
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
		BattleModule.Log($"Executing Goal -> target tile is {goalToExecute.TargetTileInfo}, orientation is {goalToExecute.TargetTileInfo.Orientation}", (CLogLevel)0);
		SkillAction skillAction = goalToExecute.Goal.Skill.SkillAction;
		skillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(BattleModule);
		bool num = goalToExecute.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInRange(goalToExecute.TargetTileInfo.Tile);
		bool flag = goalToExecute.Goal.Skill.SkillAction.HasEffect("IgnoreLineOfSight") || goalToExecute.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(goalToExecute.TargetTileInfo.Tile);
		if (num && flag)
		{
			skillAction.SkillActionExecution.TargetTiles.Add(goalToExecute.TargetTileInfo);
			skillAction.SkillActionExecution.SkillExecutionController.ExecuteSkill();
		}
		BattleModule.TargetTile = null;
	}

	public void ExecuteDeathRattle()
	{
		if (BattleModule.IsDeathRattling)
		{
			BattleModule battleModule = BattleModule;
			if (battleModule.TargetTile == null)
			{
				Tile tile = (battleModule.TargetTile = BattleModule.OriginTile);
			}
			ExecuteAllGoals();
		}
	}

	public void HookToModifyingDamagePerks()
	{
		if (((StateMachine)ApplicationManager.Application).State.GetName() == "LevelEditor")
		{
			return;
		}
		for (int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num >= 0; num--)
		{
			foreach (KeyValuePair<string, Perk> perk in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num].Perks)
			{
				if (!perk.Value.Unlocked)
				{
					continue;
				}
				foreach (APerkModule perkModule in perk.Value.PerkModules)
				{
					foreach (APerkEffect perkEffect in perkModule.PerkEffects)
					{
						if (perkEffect is ModifyDefensesDamageEffect item)
						{
							BattleModule.ModifyDefensesDamagePerks.Add(item);
						}
					}
				}
			}
		}
	}

	public void FilterTilesInRange(TilesInRangeInfos tilesInRangeInfos, List<Tile> skillSourceTiles)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> item in tilesInRangeInfos.Range)
		{
			if (item.Key.HasAnyFog && !skillSourceTiles.Contains(item.Key))
			{
				item.Value.HasLineOfSight = false;
				item.Value.TileColor = TileMapView.SkillHiddenRangeTilesColorInvalidOrientation._Color;
			}
		}
	}

	public void PaySkillCost(TheLastStand.Model.Skill.Skill skill)
	{
	}

	public void PrepareForDeathRattle()
	{
		if (BattleModule.BehaviourDefinition == null || !BattleModule.ShouldTriggerDeathRattle)
		{
			return;
		}
		BattleModule.GoalComputingStep = IBehaviorModel.E_GoalComputingStep.OnDeath;
		ComputeCurrentGoals();
		if (BattleModule.CurrentGoals[0]?.Goal != null && BattleModule.CurrentGoals[0].TargetTileInfo != null)
		{
			if (!TPSingleton<BuildingManager>.Instance.BuildingsDeathRattling.Contains(BattleModule))
			{
				TPSingleton<BuildingManager>.Instance.BuildingsDeathRattling.Add(BattleModule);
			}
			BattleModule.TargetTile = BattleModule.OriginTile;
			BattleModule.IsDeathRattling = true;
		}
	}

	public void RefillSkillsOverallUses()
	{
		if (BattleModule.Skills != null && BattleModule.Skills.Count > 0)
		{
			for (int num = BattleModule.Skills.Count - 1; num >= 0; num--)
			{
				BattleModule.Skills[num].OverallUsesRemaining = BattleModule.Skills[num].ComputeTotalUses();
			}
		}
	}

	public void RefillSkillUsesPerTurn()
	{
		if (BattleModule.Skills == null || BattleModule.Skills.Count <= 0)
		{
			return;
		}
		for (int num = BattleModule.Skills.Count - 1; num >= 0; num--)
		{
			if (BattleModule.Skills[num].SkillDefinition.UsesPerTurnCount != -1)
			{
				BattleModule.Skills[num].UsesPerTurnRemaining = BattleModule.Skills[num].SkillDefinition.UsesPerTurnCount;
			}
		}
	}

	public void StartTurn()
	{
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Night:
			if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.EnemyUnits && BattleModule.Goals != null)
			{
				for (int num = BattleModule.Goals.Length - 1; num >= 0; num--)
				{
					BattleModule.Goals[num].GoalController.StartTurn();
				}
			}
			break;
		case Game.E_Cycle.Day:
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
			{
				RefillSkillUsesPerTurn();
				RefillSkillsOverallUses();
			}
			break;
		}
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new BattleModule(building, buildingModuleDefinition as BattleModuleDefinition, this);
	}
}
