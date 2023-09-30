using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Skill.SkillAction;
using UnityEngine;

namespace TheLastStand.Controller.Building.Module;

public class BlueprintModuleController : BuildingModuleController, ITileObjectController, IEffectTargetSkillActionController
{
	private Coroutine displayEffectsCoroutine;

	public BlueprintModule BlueprintModule { get; }

	public BlueprintModuleController(BuildingController buildingControllerParent, BlueprintModuleDefinition blueprintModuleDefinition)
		: base(buildingControllerParent, blueprintModuleDefinition)
	{
		BlueprintModule = base.BuildingModule as BlueprintModule;
	}

	public void AddEffectDisplay(IDisplayableEffect displayableEffect)
	{
		base.BuildingControllerParent.Building.BuildingView.AddSkillEffectDisplay(displayableEffect);
		EffectManager.Register(this);
	}

	public void DisplayEffects(float delay = 0f)
	{
		if (displayEffectsCoroutine == null)
		{
			displayEffectsCoroutine = ((MonoBehaviour)TPSingleton<GameManager>.Instance).StartCoroutine(DisplayEffectsCoroutine(delay));
		}
	}

	public void FreeOccupiedTiles()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < BlueprintModule.OccupiedTiles.Count; i++)
		{
			Tile tile = BlueprintModule.OccupiedTiles[i];
			tile.TileController.SetBuilding(null);
			tile.Unit?.UnitView.UpdatePosition();
			tile.CurrentUnitAccess = Tile.E_UnitAccess.Everyone;
			if (base.BuildingControllerParent.Building.BuildingDefinition.ConstructionModuleDefinition.OccupationVolumeType != BuildingDefinition.E_OccupationVolumeType.Adjacent)
			{
				continue;
			}
			for (int j = -1; j < 2; j++)
			{
				for (int k = -1; k < 2; k++)
				{
					TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
					Vector2Int position = tile.Position;
					int x = ((Vector2Int)(ref position)).x + j;
					position = tile.Position;
					Tile tile2 = tileMap.GetTile(x, ((Vector2Int)(ref position)).y + k);
					if (tile2 != null && !BlueprintModule.OccupiedTiles.Contains(tile2) && !tile2.TileController.CheckIsInBuildingOccupationVolume())
					{
						tile2.TileController.SetOccupiedByBuildingVolume(isInBuildingVolume: false);
					}
				}
			}
		}
	}

	public List<Tile> GetAdjacentTiles()
	{
		HashSet<Tile> tiles = new HashSet<Tile>();
		foreach (Tile occupiedTile in BlueprintModule.OccupiedTiles)
		{
			occupiedTile.GetAdjacentTiles().ForEach(delegate(Tile o)
			{
				if (!BlueprintModule.OccupiedTiles.Contains(o))
				{
					tiles.Add(o);
				}
			});
		}
		return tiles.ToList();
	}

	public List<Tile> GetAdjacentTilesWithDiagonals()
	{
		HashSet<Tile> tiles = new HashSet<Tile>();
		foreach (Tile occupiedTile in BlueprintModule.OccupiedTiles)
		{
			occupiedTile.GetAdjacentTilesWithDiagonals().ForEach(delegate(Tile o)
			{
				if (!BlueprintModule.OccupiedTiles.Contains(o))
				{
					tiles.Add(o);
				}
			});
		}
		return tiles.ToList();
	}

	public int GetEffectsCount()
	{
		return base.BuildingControllerParent.BuildingView.SkillEffectDisplays.Count;
	}

	public List<Tile> GetTilesInRange(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return BlueprintModule.OccupiedTiles.GetTilesInRange(maxRange, minRange, cardinalOnly);
	}

	public Dictionary<Tile, Tile> GetTilesInRangeWithClosestOccupiedTile(int maxRange, int minRange = 0, bool cardinalOnly = false)
	{
		return BlueprintModule.OccupiedTiles.GetTilesInRangeWithClosestOccupiedTile(maxRange, minRange, cardinalOnly);
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new BlueprintModule(building, buildingModuleDefinition as BlueprintModuleDefinition, this);
	}

	private IEnumerator DisplayEffectsCoroutine(float delay)
	{
		yield return base.BuildingControllerParent.BuildingView.DisplaySkillEffects(delay);
		EffectManager.Unregister(this);
		displayEffectsCoroutine = null;
	}
}
