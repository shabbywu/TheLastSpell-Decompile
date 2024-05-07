using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Model.Building.Module;

public class BlueprintModule : BuildingModule, IBarker
{
	public RaceDefinition BarkerRaceDefinition => null;

	public BlueprintModuleController BlueprintModuleController => base.BuildingModuleController as BlueprintModuleController;

	public BlueprintModuleDefinition BlueprintModuleDefinition => base.BuildingModuleDefinition as BlueprintModuleDefinition;

	public virtual Transform BarkViewFollowTarget => ((Component)base.BuildingParent.BuildingView).transform;

	public bool HasBark { get; set; }

	public bool IsIndestructible
	{
		get
		{
			if (base.BuildingParent.DamageableModule != null)
			{
				return base.BuildingParent.DamageableModule.HealthTotal == 0f;
			}
			return true;
		}
	}

	public virtual bool IsStoppingLineOfSight => true;

	public List<Tile> OccupiedTiles => base.BuildingParent.OriginTile.GetOccupiedTiles(BlueprintModuleDefinition);

	public Tile OriginTile => base.BuildingParent.OriginTile;

	public BlueprintModule(Building buildingParent, BlueprintModuleDefinition blueprintModuleDefinition, BlueprintModuleController blueprintModuleController)
		: base(buildingParent, blueprintModuleDefinition, blueprintModuleController)
	{
	}

	public static Vector2Int GetRelativeBuildingTilePosition(Tile buildingTile, Tile originTile, BlueprintModuleDefinition blueprintDefinition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int position = buildingTile.Position;
		int x = ((Vector2Int)(ref position)).x;
		position = originTile.Position;
		int num = x - (((Vector2Int)(ref position)).x - blueprintDefinition.OriginX);
		position = buildingTile.Position;
		int y = ((Vector2Int)(ref position)).y;
		position = originTile.Position;
		return new Vector2Int(num, y - (((Vector2Int)(ref position)).y - blueprintDefinition.OriginY));
	}

	public Tile.E_UnitAccess ConvertBuildingTileToUnitAccess(Tile buildingTile)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int relativeBuildingTilePosition = GetRelativeBuildingTilePosition(buildingTile);
		if (BlueprintModuleDefinition.Tiles.Count <= ((Vector2Int)(ref relativeBuildingTilePosition)).y || BlueprintModuleDefinition.Tiles[((Vector2Int)(ref relativeBuildingTilePosition)).y].Count <= ((Vector2Int)(ref relativeBuildingTilePosition)).x)
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)(BlueprintModuleDefinition.BuildingDefinition.Id + " does not contain this tile"), (CLogLevel)0, true, true);
		}
		return BlueprintModuleDefinition.Tiles[((Vector2Int)(ref relativeBuildingTilePosition)).y][((Vector2Int)(ref relativeBuildingTilePosition)).x];
	}

	public Vector2Int GetRelativeBuildingTilePosition(Tile buildingTile)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return GetRelativeBuildingTilePosition(buildingTile, base.BuildingParent.OriginTile, BlueprintModuleDefinition);
	}

	public virtual bool IsTargetableByAI()
	{
		if (!IsIndestructible)
		{
			return base.BuildingParent.DamageableModule.Health > 0f;
		}
		return false;
	}
}
