using System;
using System.Runtime.CompilerServices;
using TPLib;
using TheLastStand.Controller;
using TheLastStand.Definition;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.TileMap;
using UnityEngine.Events;

namespace TheLastStand.Model.Unit.Movement;

public class MoveUnitCommand : UnitCommand
{
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

	private GameDefinition.E_Direction startDirection;

	private readonly bool forceInstant;

	private readonly int movePointsSpent;

	public Task MoveUnitTask { get; private set; }

	public Tile StartTile { get; private set; }

	public MoveUnitCommand(PlayableUnit playableUnit, int movePointsSpent, bool forceInstant)
		: base(playableUnit)
	{
		this.movePointsSpent = movePointsSpent;
		this.forceInstant = forceInstant;
	}

	public override void Compensate()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		base.PlayableUnit.Path.Clear();
		base.PlayableUnit.Path.Add(base.PlayableUnit.OriginTile);
		base.PlayableUnit.Path.Add(StartTile);
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
		base.PlayableUnit.PlayableUnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.MovePoints, movePointsSpent, includeChildStat: false);
		base.PlayableUnit.PlayableUnitController.AddCrossedTiles(-movePointsSpent);
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			base.PlayableUnit.LifetimeStats.LifetimeStatsController.DecreaseTilesCrossed(movePointsSpent);
		}
		base.PlayableUnit.UnitController.LookAtDirection(startDirection);
	}

	public override bool Execute()
	{
		StartTile = base.PlayableUnit.OriginTile;
		startDirection = base.PlayableUnit.LookDirection;
		base.PlayableUnit.PlayableUnitController.SpendMovePoints(movePointsSpent);
		base.PlayableUnit.PlayableUnitController.AddCrossedTiles(movePointsSpent);
		MoveUnitTask = base.PlayableUnit.PlayableUnitController.PrepareForMovement(playWalkAnim: true, followPathOrientation: true, -1f, 0f, TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || forceInstant);
		return true;
	}
}
