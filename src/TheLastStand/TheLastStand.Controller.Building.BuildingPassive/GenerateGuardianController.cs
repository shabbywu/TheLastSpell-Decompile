using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class GenerateGuardianController : BuildingPassiveEffectController
{
	public GenerateGuardian GenerateGuardian => base.BuildingPassiveEffect as GenerateGuardian;

	public GenerateGuardianController(PassivesModule buildingPassivesModule, GenerateGuardianDefinition generateGuardianDefinition)
	{
		base.BuildingPassiveEffect = new GenerateGuardian(buildingPassivesModule, generateGuardianDefinition, this);
	}

	public override void Apply()
	{
		if (!TrySpawnGuardian(throughFog: false))
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)"Could not spawn a guardian outside of mist, something went wrong.", (CLogLevel)1, true, true);
			if (!TrySpawnGuardian(throughFog: true))
			{
				((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)"Could not spawn a guardian INSIDE of mist, something went really wrong!", (CLogLevel)1, true, true);
			}
		}
	}

	public override void Unapply()
	{
		EnemyUnit guardian = GenerateGuardian.Guardian;
		if (guardian != null && !guardian.IsDead && !guardian.IsDeathRattling)
		{
			GenerateGuardian.Guardian.UnitController.PrepareForDeath();
			GenerateGuardian.Guardian.UnitView.PlayDieAnim();
		}
	}

	public override void OnDeath()
	{
		base.OnDeath();
		if (GenerateGuardian.Guardian != null)
		{
			GenerateGuardian.Guardian.LinkedBuilding = null;
		}
	}

	private bool TrySpawnGuardian(bool throughFog)
	{
		TheLastStand.Model.Building.Building building = base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent;
		List<Tile> list = building.BlueprintModule.BlueprintModuleController.GetAdjacentTilesWithDiagonals();
		if (!throughFog)
		{
			list = list.Where((Tile tile) => !tile.HasFog).ToList();
		}
		list = RandomManager.Shuffle(TPSingleton<EnemyUnitManager>.Instance, list).ToList();
		Tile magicCircleTile = BuildingManager.MagicCircle.OriginTile;
		list.Split(delegate(Tile adjacentTile)
		{
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector((float)(building.OriginTile.X - magicCircleTile.X), (float)(building.OriginTile.Y - magicCircleTile.Y));
			return Vector2.Dot(new Vector2((float)(adjacentTile.X - building.OriginTile.X), (float)(adjacentTile.Y - building.OriginTile.Y)), val) <= 0f;
		});
		EnemyUnitTemplateDefinition guardianToSpawn = TPSingleton<EnemyUnitManager>.Instance.GetGuardianToSpawn();
		foreach (Tile item in list)
		{
			if (guardianToSpawn.CanSpawnOn(item))
			{
				GenerateGuardian.Guardian = EnemyUnitManager.CreateEnemyUnit(guardianToSpawn, item, new UnitCreationSettings(null, castSpawnSkill: true, playSpawnAnim: true, playSpawnCutscene: true, waitSpawnAnim: false, -1, GenerateGuardian.BuildingPassivesModule.BuildingParent, isGuardian: true));
				return true;
			}
		}
		foreach (Tile item2 in list)
		{
			if (guardianToSpawn.CanSpawnOn(item2, isPhaseActor: false, ignoreUnits: false, ignoreBuildings: true))
			{
				List<Tile> occupiedTiles = item2.GetOccupiedTiles(guardianToSpawn);
				if (!occupiedTiles.Any((Tile occupiedTile) => occupiedTile.Building != null && occupiedTile.Building.BlueprintModule.IsIndestructible))
				{
					TileMapManager.ClearBuildingOnTiles(occupiedTiles);
					GenerateGuardian.Guardian = EnemyUnitManager.CreateEnemyUnit(guardianToSpawn, item2, new UnitCreationSettings(null, castSpawnSkill: true, playSpawnAnim: true, playSpawnCutscene: true, waitSpawnAnim: false, -1, GenerateGuardian.BuildingPassivesModule.BuildingParent, isGuardian: true));
					return true;
				}
			}
		}
		foreach (Tile item3 in list)
		{
			if (guardianToSpawn.CanSpawnOn(item3, isPhaseActor: false, ignoreUnits: true))
			{
				List<Tile> occupiedTiles2 = item3.GetOccupiedTiles(guardianToSpawn);
				TileMapManager.FreeTilesFromPlayableUnits(occupiedTiles2);
				TileMapManager.ClearEnemiesOnTiles(occupiedTiles2);
				GenerateGuardian.Guardian = EnemyUnitManager.CreateEnemyUnit(guardianToSpawn, item3, new UnitCreationSettings(null, castSpawnSkill: true, playSpawnAnim: true, playSpawnCutscene: true, waitSpawnAnim: false, -1, GenerateGuardian.BuildingPassivesModule.BuildingParent, isGuardian: true));
				return true;
			}
		}
		return false;
	}
}
