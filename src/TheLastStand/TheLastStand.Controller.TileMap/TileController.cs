using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Definition;
using TheLastStand.Definition.Building;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Maths;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Controller.TileMap;

public class TileController : ITileObjectController
{
	public enum E_SegmentEndsComputationType
	{
		Center,
		OppositeCorners,
		CenterToOppositeCorner
	}

	public Tile Tile { get; }

	public TileController(int x, int y, GroundDefinition groundDefinition)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		TileView tileView = Object.Instantiate<TileView>(TileMapManager.TileViewPrefab, TileMapManager.TileViewsTransform);
		((Component)tileView).transform.position = TileMapView.GetWorldPosition(new Vector2Int(x, y));
		Tile = new Tile(this, tileView, x, y, groundDefinition);
		tileView.Tile = Tile;
	}

	public static void ComputeSegmentEndsPositions(Tile sourceTile, Tile destTile, E_SegmentEndsComputationType computationType, out Vector2 sourcePos, out Vector2 destPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = destTile.Position - sourceTile.Position;
		switch (computationType)
		{
		case E_SegmentEndsComputationType.OppositeCorners:
			if (((Vector2Int)(ref val)).x > 0)
			{
				if (((Vector2Int)(ref val)).y > 0)
				{
					sourcePos = Vector2Int.op_Implicit(sourceTile.Position);
					destPos = Vector2Int.op_Implicit(destTile.Position + Vector2Int.one);
				}
				else
				{
					sourcePos = Vector2Int.op_Implicit(sourceTile.Position + Vector2Int.up);
					destPos = Vector2Int.op_Implicit(destTile.Position + Vector2Int.right);
				}
			}
			else if (((Vector2Int)(ref val)).y > 0)
			{
				sourcePos = Vector2Int.op_Implicit(sourceTile.Position + Vector2Int.right);
				destPos = Vector2Int.op_Implicit(destTile.Position + Vector2Int.up);
			}
			else
			{
				sourcePos = Vector2Int.op_Implicit(sourceTile.Position + Vector2Int.one);
				destPos = Vector2Int.op_Implicit(destTile.Position);
			}
			break;
		case E_SegmentEndsComputationType.CenterToOppositeCorner:
			sourcePos = Vector2Int.op_Implicit(sourceTile.Position) + Vector2.up * 0.5f + Vector2.right * 0.5f;
			if (((Vector2Int)(ref val)).x > 0)
			{
				if (((Vector2Int)(ref val)).y > 0)
				{
					destPos = Vector2Int.op_Implicit(destTile.Position + Vector2Int.one);
				}
				else
				{
					destPos = Vector2Int.op_Implicit(destTile.Position + Vector2Int.right);
				}
			}
			else if (((Vector2Int)(ref val)).y > 0)
			{
				destPos = Vector2Int.op_Implicit(destTile.Position + Vector2Int.up);
			}
			else
			{
				destPos = Vector2Int.op_Implicit(destTile.Position);
			}
			break;
		default:
			sourcePos = Vector2Int.op_Implicit(sourceTile.Position) + Vector2.up * 0.5f + Vector2.right * 0.5f;
			destPos = Vector2Int.op_Implicit(destTile.Position) + Vector2.up * 0.5f + Vector2.right * 0.5f;
			break;
		}
	}

	public bool TryGenerateBonePile(Dictionary<string, int> percentages)
	{
		if (Tile.HasFog || !Tile.GroundDefinition.IsCrossable || TPSingleton<BuildingManager>.Instance.WillTileBeUsedForRandomBuilding(Tile) || (Tile.Building != null && GenericDatabase.IdsListDefinitions.TryGetValue("BlockingBonePilesBuildings", out var value) && value.Ids.Contains(Tile.Building.Id)))
		{
			return false;
		}
		foreach (KeyValuePair<string, int> building in BonePileDatabase.BonePileGeneratorsDefinition.Buildings)
		{
			if (!percentages.TryGetValue(building.Key, out var value2) || value2 == 0 || value2 < building.Value)
			{
				continue;
			}
			if (TPSingleton<BuildingManager>.Instance.BonePileGenerationLimits.Count > 0)
			{
				int num = TPSingleton<BuildingManager>.Instance.BonePileGenerationLimits[building.Key];
				int value3;
				int num2 = (TPSingleton<BuildingManager>.Instance.BonePileGenerationCounter.TryGetValue(building.Key, out value3) ? value3 : 0);
				if (num > -1 && num2 >= num)
				{
					continue;
				}
			}
			int randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, 100);
			if (value2 >= randomRange)
			{
				GenerateBonePile(building.Key);
				return true;
			}
		}
		return false;
	}

	public void GenerateBonePile(string buildingId)
	{
		TheLastStand.Model.Building.Building building = Tile.Building;
		if (building != null && building.IsTrap && !TPSingleton<BuildingManager>.Instance.BuildingsToRestore.ContainsKey(Tile))
		{
			BuildingToRestore value = new BuildingToRestore(Tile, Tile.Building.Id, Tile.Building.BattleModule.RemainingTrapCharges);
			TPSingleton<BuildingManager>.Instance.BuildingsToRestore.Add(Tile, value);
		}
		BuildingDefinition buildingDefinition = BuildingDatabase.BuildingDefinitions[buildingId];
		if ((Object)(object)Tile.Building?.BuildingView != (Object)null && (Object)(object)Tile.Building.BuildingView.HandledDefensesHUD != (Object)null)
		{
			Tile.Building.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: false);
		}
		TileMapManager.ClearBuildingOnTiles(Tile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition));
		BuildingManager.CreateBuilding(buildingDefinition, Tile, updateView: true, playSound: false, instantly: false, triggerEvent: true, null, recomputeReachableTiles: true, isGeneratingBonePile: true);
		TPSingleton<BuildingManager>.Instance.BonePileGenerationCounter.AddValueOrCreateKey(buildingId, 1, (int a, int b) => a + b);
		CleanDeadBodies(instant: true);
	}

	public void AddDeadBody(EnemyUnit enemy)
	{
		Tile.TileView.InstantiateDeadBody(enemy);
	}

	public void AddDeadBuilding(BuildingDefinition building)
	{
		Tile.TileView.InstantiateDeadBuilding(building);
	}

	public int ComputeDistanceToCity()
	{
		if (Tile.IsCityTile)
		{
			Tile.DistanceToCity = 0;
			return Tile.DistanceToCity;
		}
		Tile closestTileFillingConditions = TileMapManager.GetClosestTileFillingConditions(TPSingleton<TileMapManager>.Instance.TileMap.Width, Tile, (Tile tile) => tile.IsCityTile);
		Tile.DistanceToCity = TileMapController.DistanceBetweenTiles(Tile, closestTileFillingConditions);
		return Tile.DistanceToCity;
	}

	public int ComputeDistanceToMagicCircle()
	{
		if (Tile.Building is MagicCircle)
		{
			Tile.DistanceToMagicCircle = 0;
			return Tile.DistanceToMagicCircle;
		}
		Tile tile = BuildingManager.MagicCircle.OccupiedTiles.OrderBy((Tile o) => Mathf.Abs(Tile.X - o.X) + Mathf.Abs(Tile.Y - o.Y)).First();
		Tile.DistanceToMagicCircle = TileMapController.DistanceBetweenTiles(Tile, tile);
		return Tile.DistanceToMagicCircle;
	}

	public bool CheckIsInBuildingOccupationVolume()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
				Vector2Int position = Tile.Position;
				int x = ((Vector2Int)(ref position)).x + i;
				position = Tile.Position;
				Tile tile = tileMap.GetTile(x, ((Vector2Int)(ref position)).y + j);
				if (tile != null && tile != Tile && tile.Building != null && (tile.Building.BlueprintModule.IsIndestructible || !tile.Building.DamageableModule.IsDead) && tile.Building.BuildingDefinition.ConstructionModuleDefinition.OccupationVolumeType == BuildingDefinition.E_OccupationVolumeType.Adjacent)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CleanDeadBodies(bool instant = false)
	{
		if (Tile.EnemyUnitDeadBodyViews.Count == 0)
		{
			return;
		}
		List<EnemyUnitDeadBodyView> list = new List<EnemyUnitDeadBodyView>(Tile.EnemyUnitDeadBodyViews);
		for (int i = 0; i < list.Count; i++)
		{
			EnemyUnitDeadBodyView enemyUnitDeadBodyView = list[i];
			if (!((Object)(object)enemyUnitDeadBodyView == (Object)null))
			{
				if (instant)
				{
					enemyUnitDeadBodyView.ForceDisappear();
				}
				else
				{
					enemyUnitDeadBodyView.StartWaitDisappear();
				}
			}
		}
	}

	public void FreeOccupiedTiles()
	{
	}

	public List<Tile> GetAdjacentTiles()
	{
		return Tile.GetAdjacentTiles();
	}

	public List<Tile> GetAdjacentTilesWithDiagonals()
	{
		return Tile.GetAdjacentTilesWithDiagonals();
	}

	public List<Tile> GetTilesInRange(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return Tile.OccupiedTiles.GetTilesInRange(maxRange, minRange, cardinalOnly);
	}

	public Dictionary<Tile, Tile> GetTilesInRangeWithClosestOccupiedTile(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return Tile.OccupiedTiles.GetTilesInRangeWithClosestOccupiedTile(maxRange, minRange, cardinalOnly);
	}

	public void SetBuilding(TheLastStand.Model.Building.Building building)
	{
		Tile.Building = building;
		Tile.WillBeReachedBy = null;
	}

	public void SetOccupiedByBuildingVolume(bool isInBuildingVolume)
	{
		Tile.IsInBuildingVolume = isInBuildingVolume;
	}

	public void SetUnit(TheLastStand.Model.Unit.Unit unit)
	{
		if (unit == null)
		{
			Tile.WillBeReachedBy = null;
		}
		Tile.Unit = unit;
	}

	public bool SegmentTileIntersection(Vector2 segmentOrigin, Vector2 segmentDestination, float tolerance = 0f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		Vector2[] array = (Vector2[])(object)new Vector2[8]
		{
			Vector2Int.op_Implicit(Tile.Position),
			Vector2Int.op_Implicit(Tile.Position) + Vector2.up,
			Vector2Int.op_Implicit(Tile.Position),
			Vector2Int.op_Implicit(Tile.Position) + Vector2.right,
			Vector2Int.op_Implicit(Tile.Position) + Vector2.right,
			Vector2Int.op_Implicit(Tile.Position) + Vector2.right + Vector2.up,
			Vector2Int.op_Implicit(Tile.Position) + Vector2.up,
			Vector2Int.op_Implicit(Tile.Position) + Vector2.up + Vector2.right
		};
		Vector2 val = TPHelpers.__VECTOR2_ERROR;
		for (int i = 0; i < 4; i++)
		{
			bool hit;
			Vector2 val2 = Maths.LineSegmentsIntersection(segmentOrigin, segmentDestination, array[i * 2], array[i * 2 + 1], out hit);
			if (hit)
			{
				Vector2 val3 = array[i * 2] - val2;
				float magnitude = ((Vector2)(ref val3)).magnitude;
				val3 = array[i * 2 + 1] - val2;
				float magnitude2 = ((Vector2)(ref val3)).magnitude;
				if (magnitude <= tolerance)
				{
					val2 = array[i * 2];
				}
				else if (magnitude2 <= tolerance)
				{
					val2 = array[i * 2 + 1];
				}
				if ((magnitude > tolerance && magnitude2 > tolerance) || (val2 != val && val != TPHelpers.__VECTOR2_ERROR))
				{
					return true;
				}
				val = val2;
			}
		}
		return false;
	}
}
