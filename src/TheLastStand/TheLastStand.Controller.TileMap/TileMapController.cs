using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Definition.Building;
using TheLastStand.Manager;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Controller.TileMap;

public class TileMapController
{
	public TheLastStand.Model.TileMap.TileMap TileMap { get; }

	public TileMapController(XContainer container, TileMapView tileMapView)
	{
		TileMap = new TheLastStand.Model.TileMap.TileMap(container, tileMapView);
	}

	public static bool CanPlaceBuilding(BuildingDefinition buildingDefinition, Tile originTile, bool ignoreUnit = false, bool ignoreBuilding = false, bool ignoreFog = false)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (originTile == null)
		{
			return false;
		}
		List<Tile> occupiedTiles = originTile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		for (int i = 0; i < occupiedTiles.Count; i++)
		{
			Vector2Int relativeBuildingTilePosition = BlueprintModule.GetRelativeBuildingTilePosition(occupiedTiles[i], originTile, buildingDefinition.BlueprintModuleDefinition);
			if (buildingDefinition.ConstructionModuleDefinition.OccupationVolumeType == BuildingDefinition.E_OccupationVolumeType.Adjacent)
			{
				for (int j = -1; j < 2; j++)
				{
					for (int k = -1; k < 2; k++)
					{
						TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
						Vector2Int position = occupiedTiles[i].Position;
						int x = ((Vector2Int)(ref position)).x + j;
						position = occupiedTiles[i].Position;
						Tile tile = tileMap.GetTile(x, ((Vector2Int)(ref position)).y + k);
						if (!occupiedTiles.Contains(tile) && (tile == null || (tile.Building != null && (tile.Building.BlueprintModule.IsIndestructible || !tile.Building.DamageableModule.IsDead) && tile.Building.BuildingDefinition.ConstructionModuleDefinition.OccupationVolumeType != BuildingDefinition.E_OccupationVolumeType.Ignore)))
						{
							return false;
						}
					}
				}
			}
			Tile tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(originTile.X + ((Vector2Int)(ref relativeBuildingTilePosition)).x - buildingDefinition.BlueprintModuleDefinition.OriginX, originTile.Y + ((Vector2Int)(ref relativeBuildingTilePosition)).y - buildingDefinition.BlueprintModuleDefinition.OriginY);
			if (tile2 == null || !IsTileValidForBuilding(buildingDefinition, tile2, buildingDefinition.BlueprintModuleDefinition.Tiles[((Vector2Int)(ref relativeBuildingTilePosition)).y][((Vector2Int)(ref relativeBuildingTilePosition)).x], ignoreUnit, ignoreBuilding, ignoreFog))
			{
				return false;
			}
		}
		return true;
	}

	public static bool CanPlaceBuilding(BuildingDefinition buildingDefinition, int x, int y, bool ignoreUnit = false)
	{
		return CanPlaceBuilding(buildingDefinition, TileMapManager.GetTile(x, y), ignoreUnit);
	}

	public static bool CanPlaceUnit(Tile tile, Tile.E_UnitAccess unitAccessNeeded = Tile.E_UnitAccess.Everyone)
	{
		if (IsTileValidForUnit(tile, unitAccessNeeded))
		{
			return tile.Unit == null;
		}
		return false;
	}

	public static void CleanDeadBodies(bool instant = false)
	{
		int i = 0;
		for (int width = TPSingleton<TileMapManager>.Instance.TileMap.Width; i < width; i++)
		{
			int j = 0;
			for (int height = TPSingleton<TileMapManager>.Instance.TileMap.Height; j < height; j++)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.GetTile(i, j)?.TileController.CleanDeadBodies(instant);
			}
		}
	}

	public static int DistanceBetweenTiles(Tile tile1, Tile tile2)
	{
		return Mathf.Abs(tile2.X - tile1.X) + Mathf.Abs(tile2.Y - tile1.Y);
	}

	public static Tile[] GetEquidistantTilesOnSegment(Tile tileA, Tile tileB, int tilesCount)
	{
		List<Tile> list = new List<Tile>();
		switch (tilesCount)
		{
		case 0:
			((CLogger<TileMapManager>)TPSingleton<TileMapManager>.Instance).LogWarning((object)"Getting equidistant tiles on a segment, with an asked count of 0. Returning null.", (CLogLevel)2, true, false);
			return null;
		case 1:
			((CLogger<TileMapManager>)TPSingleton<TileMapManager>.Instance).Log((object)"Getting equidistant tiles on a segment, with an asked count of 1. Returning middle of the segment.", (CLogLevel)2, false, false);
			return new Tile[1] { TileMapManager.GetTile((tileA.X + tileB.X) / 2, (tileA.Y + tileB.Y) / 2) };
		default:
		{
			int num = Mathf.RoundToInt((float)(Mathf.Abs(tileA.X - tileB.X) / (tilesCount - 1)));
			int num2 = Mathf.RoundToInt((float)(Mathf.Abs(tileA.Y - tileB.Y) / (tilesCount - 1)));
			for (int i = 0; i < tilesCount; i++)
			{
				int x = tileA.X + num * i;
				int y = tileA.Y + num2 * i;
				list.Add(TileMapManager.GetTile(x, y));
			}
			return list.ToArray();
		}
		}
	}

	public static Tile GetCenterTile()
	{
		return TileMapManager.GetTile(TPSingleton<TileMapManager>.Instance.TileMap.Width / 2, TPSingleton<TileMapManager>.Instance.TileMap.Height / 2);
	}

	public static GameDefinition.E_Direction GetDirectionBetweenTiles(Tile sourceTile, Tile targetTile)
	{
		int num = sourceTile.X - targetTile.X;
		int num2 = sourceTile.Y - targetTile.Y;
		int num3 = Mathf.Abs(num);
		int num4 = Mathf.Abs(num2);
		if (num3 > num4)
		{
			if (num > 0)
			{
				return GameDefinition.E_Direction.West;
			}
			if (num < 0)
			{
				return GameDefinition.E_Direction.East;
			}
		}
		else if (num3 < num4)
		{
			if (num2 > 0)
			{
				return GameDefinition.E_Direction.South;
			}
			if (num2 < 0)
			{
				return GameDefinition.E_Direction.North;
			}
		}
		else
		{
			if (num2 > 0)
			{
				return GameDefinition.E_Direction.South;
			}
			if (num2 < 0)
			{
				return GameDefinition.E_Direction.North;
			}
		}
		return GameDefinition.E_Direction.None;
	}

	public static Tile GetRandomUnoccupiedOrBarricadeTile(Vector2Int origin, int radius)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Tile[] tilesInCircle = GetTilesInCircle(origin, radius);
		do
		{
			Tile randomElement = RandomManager.GetRandomElement("TileMapController", tilesInCircle);
			if ((randomElement.Building == null || randomElement.Building.Id == "Barricade") && !randomElement.HasAnyFog && randomElement.GroundDefinition.IsCrossable && randomElement.Unit == null)
			{
				return randomElement;
			}
		}
		while (num++ < 20);
		((CLogger<TileMapManager>)TPSingleton<TileMapManager>.Instance).LogWarning((object)$"TileMapController.GetRandomTile(): didn't manage to find a suitable tile in {20} attempts. Aborting.", (CLogLevel)0, true, false);
		return null;
	}

	public static Tile GetRandomUnoccupiedTile(RectInt area)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Tile[] tilesInRect = GetTilesInRect(area);
		do
		{
			Tile randomElement = RandomManager.GetRandomElement("TileMapController", tilesInRect);
			if (CanPlaceUnit(randomElement))
			{
				return randomElement;
			}
		}
		while (num++ < 20);
		((CLogger<TileMapManager>)TPSingleton<TileMapManager>.Instance).LogWarning((object)$"TileMapController.GetRandomUnoccupiedTile(): didn't manage to find a suitable tile in {20} attempts. Aborting.", (CLogLevel)0, true, false);
		return null;
	}

	public static Tile GetRandomUnoccupiedTile(Vector2Int origin, int radius)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Tile[] tilesInCircle = GetTilesInCircle(origin, radius);
		do
		{
			Tile randomElement = RandomManager.GetRandomElement("TileMapController", tilesInCircle);
			if (CanPlaceUnit(randomElement))
			{
				return randomElement;
			}
		}
		while (num++ < 20);
		((CLogger<TileMapManager>)TPSingleton<TileMapManager>.Instance).LogWarning((object)$"TileMapController.GetRandomUnoccupiedTile(): didn't manage to find a suitable tile in {20} attempts. Aborting.", (CLogLevel)0, true, false);
		return null;
	}

	public static RectInt GetRectFromTileToTile(Tile tileA, Tile tileB)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Min(tileA.X, tileB.X);
		int num2 = Mathf.Min(tileA.Y, tileB.Y);
		int num3 = Mathf.Max(tileA.X, tileB.X);
		int num4 = Mathf.Max(tileA.Y, tileB.Y);
		return new RectInt(num, num2, num3 - num, num4 - num2);
	}

	public static Vector2Int GetRotatedTilemapPosition(Vector2Int offsetFromPivot, Vector2Int pivotTilemapPosition, float angle)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		angle = (float)Math.PI / 2f * Mathf.Round(angle / 90f);
		Vector2Int direction = default(Vector2Int);
		((Vector2Int)(ref direction))._002Ector(Mathf.RoundToInt(Mathf.Cos(angle)), Mathf.RoundToInt(Mathf.Sin(angle)));
		return pivotTilemapPosition + RotateVector2Int(offsetFromPivot, direction);
	}

	public static Tile[] GetTilesInCircle(Vector2Int origin, int radius)
	{
		int num = radius * radius;
		List<Tile> list = new List<Tile>(Mathf.CeilToInt((float)Math.PI * (float)num));
		int num2 = 0;
		int num3 = 0;
		TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
		Tile tile = null;
		int i = -radius;
		for (int num4 = radius + 1; i < num4; i++)
		{
			num2 = ((Vector2Int)(ref origin)).x + i;
			if (!tileMap.IsHorizontalPosValid(num2))
			{
				continue;
			}
			int j = -radius;
			for (int num5 = radius + 1; j < num5; j++)
			{
				num3 = ((Vector2Int)(ref origin)).y + j;
				if (tileMap.IsVerticalPosValid(num3) && i * i + j * j <= num)
				{
					tile = TileMapManager.GetTile(num2, num3);
					if (tile != null)
					{
						list.Add(tile);
					}
				}
			}
		}
		return list.ToArray();
	}

	public static List<Tile> GetTilesInRange(Vector2Int origin, int range, bool includeSelf = true)
	{
		return GetTilesInRange(TPSingleton<TileMapManager>.Instance.TileMap.GetTile(((Vector2Int)(ref origin)).x, ((Vector2Int)(ref origin)).y), range, includeSelf);
	}

	public static List<Tile> GetTilesInRange(Tile originTile, int range, bool includeSelf = true)
	{
		List<Tile> tiles = new List<Tile>();
		TileMapManager.GoThroughTilesInRange(range, originTile, delegate(Tile tile, Vector2Int pos)
		{
			tiles.Add(tile);
		}, cardinalOnly: false, (!includeSelf) ? 1 : 0);
		return tiles;
	}

	public static Tile[] GetTilesInRect(RectInt area, bool includeCenter = true)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		List<Tile> list = new List<Tile>(((RectInt)(ref area)).width * ((RectInt)(ref area)).height);
		TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
		int i = ((RectInt)(ref area)).x;
		for (int num = ((RectInt)(ref area)).xMax + 1; i < num; i++)
		{
			if (!tileMap.IsHorizontalPosValid(i))
			{
				continue;
			}
			int j = ((RectInt)(ref area)).y;
			for (int num2 = ((RectInt)(ref area)).yMax + 1; j < num2; j++)
			{
				if (tileMap.IsVerticalPosValid(j) && (includeCenter || !(Vector2Int.op_Implicit(new Vector2Int(i, j)) == ((RectInt)(ref area)).center)))
				{
					Tile tile = TileMapManager.GetTile(i, j);
					if (tile != null)
					{
						list.Add(tile);
					}
				}
			}
		}
		return list.ToArray();
	}

	public static bool IsAnyTileInRange(Tile originTile, IEnumerable<Tile> comparedTiles, int range)
	{
		foreach (Tile comparedTile in comparedTiles)
		{
			if (DistanceBetweenTiles(originTile, comparedTile) <= range)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsTileValidForBuilding(BuildingDefinition buildingDefinition, Tile tile, Tile.E_UnitAccess relativeTile, bool ignoreUnit = false, bool ignoreBuilding = false, bool ignoreFog = false)
	{
		if (tile != null && (ignoreFog || !tile.HasFog || ApplicationManager.CurrentStateName == "LevelEditor") && buildingDefinition.ConstructionModuleDefinition.GroundCategories.Contains(tile.GroundDefinition.GroundCategory) && (ignoreBuilding || tile.Building == null || (buildingDefinition.UpgradeModuleDefinition != null && buildingDefinition.UpgradeModuleDefinition.GetPreviousUpgrades().Contains(tile.Building.BuildingDefinition.Id))) && (ignoreUnit || tile.Unit == null || (tile.Unit is PlayableUnit && relativeTile.HasFlag(Tile.E_UnitAccess.Hero))))
		{
			if (tile.IsInBuildingVolume)
			{
				return buildingDefinition.ConstructionModuleDefinition.OccupationVolumeType == BuildingDefinition.E_OccupationVolumeType.Ignore;
			}
			return true;
		}
		return false;
	}

	public static bool IsTileValidForUnit(Tile tile, Tile.E_UnitAccess unitAccessNeeded)
	{
		if (tile.CurrentUnitAccess.HasFlag(unitAccessNeeded) && !tile.HasFog)
		{
			return tile.GroundDefinition.IsCrossable;
		}
		return false;
	}

	private static Vector2Int RotateVector2Int(Vector2Int vector, Vector2Int direction)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2Int(((Vector2Int)(ref vector)).x * ((Vector2Int)(ref direction)).x - ((Vector2Int)(ref vector)).y * ((Vector2Int)(ref direction)).y, ((Vector2Int)(ref vector)).x * ((Vector2Int)(ref direction)).y + ((Vector2Int)(ref vector)).y * ((Vector2Int)(ref direction)).x);
	}
}
