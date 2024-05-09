using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Boss;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class SpawnActorPhaseActionController : ABossPhaseActionController
{
	private UnitCreationSettings currentUnitCreationSettings;

	private ActorDefinition currentActorDefinition;

	private TheLastStand.Framework.Serialization.Definition currentActorTypedDefinition;

	private string currentActorId;

	public SpawnActorPhaseActionDefinition SpawnActorPhaseActionDefinition => base.ABossPhaseActionDefinition as SpawnActorPhaseActionDefinition;

	public SpawnActorPhaseActionController(SpawnActorPhaseActionDefinition spawnActorPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(spawnActorPhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		PickRandomActorToSpawn();
		bool cameraWasZoomed = ACameraView.IsZoomedIn;
		if (SpawnActorPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			ACameraView.Zoom(zoomIn: true);
		}
		if (currentActorTypedDefinition is BuildingDefinition buildingDefinition && (buildingDefinition.BlueprintModuleDefinition.Category & BuildingDefinition.E_BuildingCategory.LitBrazier) == BuildingDefinition.E_BuildingCategory.LitBrazier)
		{
			yield return TPSingleton<BuildingManager>.Instance.LitBraziers(SpawnActorPhaseActionDefinition.AmountToSpawn, buildingDefinition, currentActorDefinition.ActorId);
		}
		else
		{
			for (int i = 0; i < SpawnActorPhaseActionDefinition.AmountToSpawn; i++)
			{
				if (i > 0 && SpawnActorPhaseActionDefinition.HasMultipleActorId)
				{
					PickRandomActorToSpawn();
				}
				switch (currentActorDefinition.ActorType)
				{
				case DamageableType.Enemy:
				{
					EnemyUnitTemplateDefinition enemyUnitTemplateDefinition = currentActorTypedDefinition as EnemyUnitTemplateDefinition;
					Tile randomTileSpawn3 = GetRandomTileSpawn(currentActorDefinition, enemyUnitTemplateDefinition, SpawnActorPhaseActionDefinition.PrioritizeWaveSide);
					if (randomTileSpawn3 != null)
					{
						List<Tile> occupiedTiles2 = randomTileSpawn3.GetOccupiedTiles(enemyUnitTemplateDefinition);
						TileMapManager.FreeTilesFromPlayableUnits(occupiedTiles2);
						TheLastStand.Model.Building.Building building = randomTileSpawn3.Building;
						if (building != null && !building.IsTeleporter && !building.IsTrap)
						{
							TileMapManager.ClearBuildingOnTiles(occupiedTiles2);
						}
						TileMapManager.ClearEnemiesOnTiles(occupiedTiles2);
						EnemyUnit enemyUnit = EnemyUnitManager.CreateEnemyUnit(enemyUnitTemplateDefinition, randomTileSpawn3, currentUnitCreationSettings);
						if (SpawnActorPhaseActionDefinition.CameraFocus || currentUnitCreationSettings.WaitSpawnAnim)
						{
							yield return (object)new WaitUntil((Func<bool>)(() => enemyUnit.EnemyUnitView.AreAnimationsInitialized));
						}
						if (SpawnActorPhaseActionDefinition.CameraFocus)
						{
							ACameraView.MoveTo(enemyUnit.DamageableView.GameObject.transform.position, 0f, (Ease)0);
						}
						if (currentUnitCreationSettings.WaitSpawnAnim && enemyUnit.EnemyUnitView.HasSpawnAnim)
						{
							yield return enemyUnit.EnemyUnitView.WaitUntilAnimatorStateIsSpawn;
							yield return enemyUnit.EnemyUnitView.WaitUntilAnimatorStateIsIdle;
						}
					}
					else
					{
						((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogError((object)$"Couldn't spawn an enemy {currentActorDefinition.ActorId} on {currentActorDefinition.TileFlagTag} ! (PrioritizeWaveSide: {SpawnActorPhaseActionDefinition.PrioritizeWaveSide})", (CLogLevel)0, true, true);
					}
					break;
				}
				case DamageableType.Boss:
				{
					BossUnitTemplateDefinition bossUnitTemplateDefinition = currentActorTypedDefinition as BossUnitTemplateDefinition;
					Tile randomTileSpawn2 = GetRandomTileSpawn(currentActorDefinition, bossUnitTemplateDefinition, SpawnActorPhaseActionDefinition.PrioritizeWaveSide);
					if (randomTileSpawn2 != null)
					{
						yield return BossManager.CreateBossUnit(bossUnitTemplateDefinition, randomTileSpawn2, currentUnitCreationSettings);
					}
					else
					{
						((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogError((object)$"Couldn't spawn a boss {currentActorDefinition.ActorId} on {currentActorDefinition.TileFlagTag} ! (PrioritizeWaveSide: {SpawnActorPhaseActionDefinition.PrioritizeWaveSide})", (CLogLevel)0, true, true);
					}
					break;
				}
				case DamageableType.Building:
				{
					BuildingDefinition buildingDefinition2 = currentActorTypedDefinition as BuildingDefinition;
					Tile randomTileSpawn = GetRandomTileSpawn(currentActorDefinition, buildingDefinition2, SpawnActorPhaseActionDefinition.PrioritizeWaveSide);
					if (randomTileSpawn != null)
					{
						List<Tile> occupiedTiles = randomTileSpawn.GetOccupiedTiles(buildingDefinition2.BlueprintModuleDefinition);
						TileMapManager.ClearEnemiesOnTiles(occupiedTiles);
						TileMapManager.ClearBuildingOnTiles(occupiedTiles);
						TheLastStand.Model.Building.Building spawnedBuilding = BuildingManager.CreateBuilding(buildingDefinition2, randomTileSpawn, updateView: true, playSound: true, instantly: false, triggerEvent: true, currentUnitCreationSettings.BossPhaseActorId);
						if (SpawnActorPhaseActionDefinition.CameraFocus)
						{
							ACameraView.MoveTo(((Component)spawnedBuilding.BuildingView).transform.position, CameraView.AnimationMoveSpeed, (Ease)0);
							yield return SharedYields.WaitForSeconds(CameraView.AnimationMoveSpeed);
						}
						if (SpawnActorPhaseActionDefinition.CameraFocus || currentUnitCreationSettings.WaitSpawnAnim)
						{
							yield return spawnedBuilding.BuildingView.WaitUntilConstructionAnimationIsFinished;
						}
					}
					else
					{
						((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogError((object)$"Couldn't spawn a building {currentActorDefinition.ActorId} on {currentActorDefinition.TileFlagTag} ! (PrioritizeWaveSide: {SpawnActorPhaseActionDefinition.PrioritizeWaveSide})", (CLogLevel)0, true, true);
					}
					break;
				}
				}
			}
		}
		if (SpawnActorPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = true;
			ACameraView.AllowUserZoom = true;
			ACameraView.Zoom(cameraWasZoomed);
		}
	}

	private void PickRandomActorToSpawn()
	{
		currentUnitCreationSettings = SpawnActorPhaseActionDefinition.GetUnitCreationSettings();
		currentActorId = currentUnitCreationSettings.BossPhaseActorId;
		currentActorDefinition = TPSingleton<BossManager>.Instance.CurrentBossPhase?.BossPhaseDefinition.ActorDefinitions.GetValueOrDefault(currentActorId);
		currentActorTypedDefinition = currentActorDefinition?.GetCorrespondingDefinition();
	}

	private Tile GetRandomTileSpawn(ActorDefinition actorDefinition, EnemyUnitTemplateDefinition enemyUnitTemplateDefinition, bool prioritizeWaveSide)
	{
		return TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, (Tile tile) => enemyUnitTemplateDefinition.CanSpawnOn(tile) && CheckTileSpawnSettings(prioritizeWaveSide, tile)) ?? TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, delegate(Tile tile)
		{
			if (enemyUnitTemplateDefinition.CanSpawnOn(tile, isPhaseActor: false, ignoreUnits: false, ignoreBuildings: true))
			{
				TheLastStand.Model.Building.Building building = tile.Building;
				if (building == null || building.IsBarricade || building.IsTrap || building.IsTeleporter)
				{
					return CheckTileSpawnSettings(prioritizeWaveSide, tile);
				}
			}
			return false;
		}) ?? TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, (Tile tile) => enemyUnitTemplateDefinition.CanSpawnOn(tile, isPhaseActor: true) && CheckTileSpawnSettings(prioritizeWaveSide, tile));
	}

	private Tile GetRandomTileSpawn(ActorDefinition actorDefinition, BossUnitTemplateDefinition bossUnitTemplateDefinition, bool prioritizeWaveSide)
	{
		return TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, (Tile tile) => bossUnitTemplateDefinition.CanSpawnOn(tile) && CheckTileSpawnSettings(prioritizeWaveSide, tile)) ?? TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, delegate(Tile tile)
		{
			if (bossUnitTemplateDefinition.CanSpawnOn(tile, isPhaseActor: false, ignoreUnits: false, ignoreBuildings: true))
			{
				TheLastStand.Model.Building.Building building = tile.Building;
				if (building == null || building.IsBarricade || building.IsTrap)
				{
					return CheckTileSpawnSettings(prioritizeWaveSide, tile);
				}
			}
			return false;
		}) ?? TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, (Tile tile) => bossUnitTemplateDefinition.CanSpawnOn(tile, isPhaseActor: true) && CheckTileSpawnSettings(prioritizeWaveSide, tile));
	}

	private Tile GetRandomTileSpawn(ActorDefinition actorDefinition, BuildingDefinition buildingDefinition, bool prioritizeWaveSide)
	{
		return TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, (Tile tile) => TileMapController.CanPlaceBuilding(buildingDefinition, tile) && CheckTileSpawnSettings(prioritizeWaveSide, tile)) ?? TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, delegate(Tile tile)
		{
			if (TileMapController.CanPlaceBuilding(buildingDefinition, tile, ignoreUnit: false, ignoreBuilding: true))
			{
				TheLastStand.Model.Building.Building building2 = tile.Building;
				if (building2 == null || building2.IsBarricade || building2.IsTrap)
				{
					return CheckTileSpawnSettings(prioritizeWaveSide, tile);
				}
			}
			return false;
		}) ?? TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, delegate(Tile tile)
		{
			if (TileMapController.CanPlaceBuilding(buildingDefinition, tile, ignoreUnit: true, ignoreBuilding: true))
			{
				TheLastStand.Model.Building.Building building = tile.Building;
				if ((building == null || building.IsBarricade || building.IsTrap) && !(tile.Unit is EnemyUnit { IsBossPhaseActor: not false }))
				{
					return CheckTileSpawnSettings(prioritizeWaveSide, tile);
				}
			}
			return false;
		});
	}

	private bool CheckTileSpawnSettings(bool mustPrioritizeWaveSide, Tile tile)
	{
		if (mustPrioritizeWaveSide)
		{
			return IsTileOnWaveSide(tile);
		}
		return true;
	}

	private bool IsTileOnWaveSide(Tile tile)
	{
		if (tile.HasFog)
		{
			return false;
		}
		return SpawnWaveManager.CurrentSpawnWave.SpawnPointsInfo.Any((SpawnPointInfo x) => x.Direction == tile.DirectionToCenter);
	}
}
