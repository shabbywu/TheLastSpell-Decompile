using System;
using System.Runtime.CompilerServices;
using TPLib;
using TheLastStand.Controller;
using TheLastStand.Definition;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Building;
using UnityEngine.Events;

namespace TheLastStand.Model.Unit.Movement;

public class SkillCommand : UnitCommand
{
	public struct UndoneSkillData
	{
		public float ActionPointsSpent;

		public float MovePointsSpent;

		public float ManaSpent;

		public float HealthSpent;
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__12_0;

		internal void _003CCompensate_003Eb__12_0()
		{
			GameController.SetState(Game.E_State.Management);
		}
	}

	private readonly TheLastStand.Model.Skill.Skill skill;

	private float startActionPoints;

	private float startMovePoints;

	private float startHealth;

	private float startMana;

	private Tile startTile;

	private GameDefinition.E_Direction startDirection;

	private int startTilesCrossedThisTurn;

	private int startMomentumTilesCrossed;

	private AttackSkillActionDefinition.E_AttackType startAttackType;

	public SkillCommand(PlayableUnit playableUnit, TheLastStand.Model.Skill.Skill skill)
		: base(playableUnit)
	{
		this.skill = skill;
	}

	public override void Compensate()
	{
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Expected O, but got Unknown
		if (skill.UsesPerTurnRemaining != -1)
		{
			skill.UsesPerTurnRemaining++;
		}
		if (skill.OverallUsesRemaining != -1)
		{
			skill.OverallUsesRemaining++;
		}
		if (base.PlayableUnit.OriginTile != startTile)
		{
			if (base.PlayableUnit.OriginTile.Building?.BuildingView is WatchtowerView || startTile.Building?.BuildingView is WatchtowerView)
			{
				base.PlayableUnit.OriginTile.Unit = null;
				if (base.PlayableUnit.OriginTile.Building != null && !(base.PlayableUnit.OriginTile.Building.BuildingView is WatchtowerView))
				{
					TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuilding(base.PlayableUnit.OriginTile.Building, base.PlayableUnit.OriginTile.Building.OriginTile);
				}
				base.PlayableUnit.OriginTile = startTile;
				startTile.Unit = base.PlayableUnit;
				base.PlayableUnit.UnitView.UpdatePosition();
				if (startTile.Building != null && !(startTile.Building.BuildingView is WatchtowerView))
				{
					TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuilding(startTile.Building, startTile.Building.OriginTile);
				}
			}
			else
			{
				base.PlayableUnit.Path.Clear();
				base.PlayableUnit.Path.Add(base.PlayableUnit.OriginTile);
				base.PlayableUnit.Path.Add(startTile);
				Task task = base.PlayableUnit.PlayableUnitController.PrepareForMovement(playWalkAnim: false, followPathOrientation: false, float.PositiveInfinity, 0f, isMovementInstant: true);
				if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
				{
					PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
				}
				PlayableUnitManager.MovePath.MovePathController.Clear();
				GameController.SetState(Game.E_State.Wait);
				PlayableUnitManager instance = TPSingleton<PlayableUnitManager>.Instance;
				object obj = _003C_003Ec._003C_003E9__12_0;
				if (obj == null)
				{
					UnityAction val = delegate
					{
						GameController.SetState(Game.E_State.Management);
					};
					_003C_003Ec._003C_003E9__12_0 = val;
					obj = (object)val;
				}
				instance.MoveUnitsTaskGroup = new TaskGroup((UnityAction)obj);
				TPSingleton<PlayableUnitManager>.Instance.MoveUnitsTaskGroup.AddTask(task);
				TPSingleton<PlayableUnitManager>.Instance.MoveUnitsTaskGroup.Run();
			}
		}
		base.PlayableUnit.UnitController.LookAtDirection(startDirection);
		float num = startActionPoints - base.PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ActionPoints).Base;
		float num2 = startMovePoints - base.PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).Base;
		float num3 = startMana - base.PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).Base;
		float num4 = startHealth - base.PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Health).Base;
		if (num != 0f)
		{
			base.PlayableUnit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.ActionPoints, startActionPoints);
		}
		if (num2 != 0f)
		{
			base.PlayableUnit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.MovePoints, startMovePoints);
		}
		if (num3 != 0f)
		{
			base.PlayableUnit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.Mana, startMana);
		}
		if (num4 != 0f)
		{
			base.PlayableUnit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.Health, startHealth);
		}
		int num5 = startTilesCrossedThisTurn - base.PlayableUnit.TilesCrossedThisTurn;
		if (num5 != 0)
		{
			bool ignoreMomentum = startMomentumTilesCrossed == base.PlayableUnit.TotalMomentumTilesCrossedThisTurn;
			base.PlayableUnit.PlayableUnitController.AddCrossedTiles(num5, ignoreMomentum);
		}
		base.PlayableUnit.LastSkillType = startAttackType;
		PerkDataContainer obj2 = new PerkDataContainer
		{
			UndoneSkillData = new UndoneSkillData
			{
				ActionPointsSpent = num,
				MovePointsSpent = num2,
				ManaSpent = num3,
				HealthSpent = num4
			},
			Caster = base.PlayableUnit,
			Skill = skill,
			IsTriggeredByPerk = false
		};
		base.PlayableUnit.Events.GetValueOrDefault(E_EffectTime.OnSkillUndo)?.Invoke(obj2);
	}

	public override bool Execute()
	{
		startTile = base.PlayableUnit.OriginTile;
		startDirection = base.PlayableUnit.LookDirection;
		startActionPoints = base.PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ActionPoints).Base;
		startMovePoints = base.PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).Base;
		startMana = base.PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).Base;
		startHealth = base.PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Health).Base;
		startTilesCrossedThisTurn = base.PlayableUnit.TilesCrossedThisTurn;
		startMomentumTilesCrossed = base.PlayableUnit.TotalMomentumTilesCrossedThisTurn;
		startAttackType = base.PlayableUnit.LastSkillType;
		return true;
	}
}
