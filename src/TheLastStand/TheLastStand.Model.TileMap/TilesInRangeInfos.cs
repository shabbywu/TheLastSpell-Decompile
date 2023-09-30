using System.Collections.Generic;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Extensions;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Model.TileMap;

public class TilesInRangeInfos
{
	public class TileDisplayInfos
	{
		public bool HasLineOfSight { get; set; }

		public TileObjectSelectionManager.E_Orientation Orientation { get; private set; }

		public Color TileColor { get; set; }

		public TileDisplayInfos(bool hasLineOfSight, TileObjectSelectionManager.E_Orientation orientation, bool isSkillSelected)
		{
			HasLineOfSight = hasLineOfSight;
			Orientation = orientation;
			SetColorTile(isSkillSelected);
		}

		private void SetColorTile(bool isSkillSelected)
		{
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			if (Orientation != 0)
			{
				Color tileColor = (HasLineOfSight ? TileMapView.SkillRangeTilesColor._Color : TileMapView.SkillHiddenRangeTilesColor._Color);
				if (TileObjectSelectionManager.HasAnythingSelected && isSkillSelected && (SkillManager.SelectedSkill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate) && !Orientation.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection))
				{
					tileColor = (HasLineOfSight ? TileMapView.SkillRangeTilesColorInvalidOrientation._Color : TileMapView.SkillHiddenRangeTilesColorInvalidOrientation._Color);
				}
				TileColor = tileColor;
			}
			else
			{
				TileColor = (HasLineOfSight ? TileMapView.SkillRangeTilesColor._Color : TileMapView.SkillHiddenRangeTilesColor._Color);
			}
		}
	}

	public HashSet<Tile> Exclude;

	public HashSet<Tile> Obstacle;

	public Dictionary<Tile, TileDisplayInfos> Range;

	public bool InfiniteRange;

	public TilesInRangeInfos()
	{
		Exclude = new HashSet<Tile>();
		Obstacle = new HashSet<Tile>();
		Range = new Dictionary<Tile, TileDisplayInfos>();
	}

	public void ClearLonelyTilesInLineOfSight(Tile sourceTile, int minRange, int maxRange)
	{
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Tile, TileDisplayInfos> item in Range)
		{
			if (!item.Value.HasLineOfSight)
			{
				continue;
			}
			List<Tile> adjacentTilesWithDiagonals = item.Key.GetAdjacentTilesWithDiagonals();
			adjacentTilesWithDiagonals.Sort((Tile a, Tile b) => Vector2Int.Distance(b.Position, sourceTile.Position).CompareTo(Vector2Int.Distance(a.Position, sourceTile.Position)));
			bool flag = false;
			for (int i = 0; i < adjacentTilesWithDiagonals.Count; i++)
			{
				flag = false;
				Tile tile = adjacentTilesWithDiagonals[i];
				if (tile != sourceTile)
				{
					if (Range.TryGetValue(tile, out var value) && value.HasLineOfSight)
					{
						item.Value.TileColor = TileMapView.SkillRangeTilesColor._Color;
						break;
					}
					float num = Vector2Int.Distance(tile.Position, sourceTile.Position);
					if (!(num < (float)minRange) && !(num > (float)maxRange))
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				item.Value.HasLineOfSight = false;
				item.Value.TileColor = TileMapView.SkillHiddenRangeTilesColorInvalidOrientation._Color;
			}
		}
	}

	public void Clear()
	{
		Exclude.Clear();
		Obstacle.Clear();
		Range.Clear();
		InfiniteRange = false;
	}

	public Tile GetTileInLineOfSight(ITileObject targetTileObject)
	{
		for (int i = 0; i < targetTileObject.OccupiedTiles.Count; i++)
		{
			if (IsInLineOfSight(targetTileObject.OccupiedTiles[i]))
			{
				return targetTileObject.OccupiedTiles[i];
			}
		}
		return null;
	}

	public bool IsInLineOfSight(Tile targetTile)
	{
		if (Range.TryGetValue(targetTile, out var value))
		{
			return value.HasLineOfSight;
		}
		return false;
	}

	public bool IsInLineOfSight(ITileObject targetTileObject)
	{
		return GetTileInLineOfSight(targetTileObject) != null;
	}

	public bool IsInRange(Tile targetTile)
	{
		if (!InfiniteRange)
		{
			return Range.ContainsKey(targetTile);
		}
		return true;
	}
}
