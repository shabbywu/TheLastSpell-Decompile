using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TPLib;
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
using TheLastStand.Model.Unit.Boss;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class SpawnActorPhaseActionController : ABossPhaseActionController
{
	public SpawnActorPhaseActionDefinition SpawnActorPhaseActionDefinition => base.ABossPhaseActionDefinition as SpawnActorPhaseActionDefinition;

	public SpawnActorPhaseActionController(SpawnActorPhaseActionDefinition spawnActorPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(spawnActorPhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		string bossPhaseActorId = SpawnActorPhaseActionDefinition.UnitCreationSettings.BossPhaseActorId;
		ActorDefinition actorDefinition = TPSingleton<BossManager>.Instance.CurrentBossPhase?.BossPhaseDefinition.ActorDefinitions.GetValueOrDefault(bossPhaseActorId);
		TheLastStand.Framework.Serialization.Definition actorTypedDefinition = actorDefinition?.GetCorrespondingDefinition();
		bool cameraWasZoomed = ACameraView.IsZoomedIn;
		if (SpawnActorPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			ACameraView.Zoom(zoomIn: true);
		}
		if (actorTypedDefinition is BuildingDefinition buildingDefinition && (buildingDefinition.BlueprintModuleDefinition.Category & BuildingDefinition.E_BuildingCategory.LitBrazier) != 0)
		{
			yield return TPSingleton<BuildingManager>.Instance.LitBraziers(SpawnActorPhaseActionDefinition.AmountToSpawn, buildingDefinition, actorDefinition.ActorId);
		}
		else
		{
			for (int i = 0; i < SpawnActorPhaseActionDefinition.AmountToSpawn; i++)
			{
				switch (actorDefinition.ActorType)
				{
				case DamageableType.Enemy:
				{
					EnemyUnitTemplateDefinition enemyUnitTemplateDefinition = actorTypedDefinition as EnemyUnitTemplateDefinition;
					Tile tile3 = TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, (Tile tile) => enemyUnitTemplateDefinition.CanSpawnOn(tile)) ?? TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, delegate(Tile tile)
					{
						if (enemyUnitTemplateDefinition.CanSpawnOn(tile, isPhaseActor: false, ignoreUnits: false, ignoreBuildings: true))
						{
							TheLastStand.Model.Building.Building building2 = tile.Building;
							return building2 == null || building2.IsBarricade || building2.IsTrap || building2.IsTeleporter;
						}
						return false;
					}) ?? TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, (Tile tile) => enemyUnitTemplateDefinition.CanSpawnOn(tile, isPhaseActor: true));
					if (tile3 == null)
					{
						break;
					}
					List<Tile> occupiedTiles = tile3.GetOccupiedTiles(enemyUnitTemplateDefinition);
					TileMapManager.FreeTilesFromPlayableUnits(occupiedTiles);
					TheLastStand.Model.Building.Building building = tile3.Building;
					if (building != null && !building.IsTeleporter && !building.IsTrap)
					{
						TileMapManager.ClearBuildingOnTiles(occupiedTiles);
					}
					TileMapManager.ClearEnemiesOnTiles(occupiedTiles);
					EnemyUnit enemyUnit = EnemyUnitManager.CreateEnemyUnit(enemyUnitTemplateDefinition, tile3, SpawnActorPhaseActionDefinition.UnitCreationSettings);
					if (SpawnActorPhaseActionDefinition.CameraFocus || SpawnActorPhaseActionDefinition.UnitCreationSettings.WaitSpawnAnim)
					{
						yield return (object)new WaitUntil((Func<bool>)(() => enemyUnit.EnemyUnitView.AreAnimationsInitialized));
					}
					if (SpawnActorPhaseActionDefinition.CameraFocus)
					{
						ACameraView.MoveTo(enemyUnit.DamageableView.GameObject.transform.position, 0f, (Ease)0);
					}
					if (SpawnActorPhaseActionDefinition.UnitCreationSettings.WaitSpawnAnim && enemyUnit.EnemyUnitView.HasSpawnAnim)
					{
						yield return enemyUnit.EnemyUnitView.WaitUntilAnimatorStateIsSpawn;
						yield return enemyUnit.EnemyUnitView.WaitUntilAnimatorStateIsIdle;
					}
					break;
				}
				case DamageableType.Boss:
				{
					BossUnitTemplateDefinition bossUnitTemplateDefinition = actorTypedDefinition as BossUnitTemplateDefinition;
					Tile tile2 = TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, (Tile tile) => bossUnitTemplateDefinition.CanSpawnOn(tile)) ?? TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, delegate(Tile tile)
					{
						if (bossUnitTemplateDefinition.CanSpawnOn(tile, isPhaseActor: false, ignoreUnits: false, ignoreBuildings: true))
						{
							TheLastStand.Model.Building.Building building3 = tile.Building;
							return building3 == null || building3.IsBarricade || building3.IsTrap;
						}
						return false;
					}) ?? TileMapManager.GetRandomTileWithFlag(actorDefinition.TileFlagTag, (Tile tile) => bossUnitTemplateDefinition.CanSpawnOn(tile, isPhaseActor: true));
					if (tile2 != null)
					{
						yield return BossManager.CreateBossUnit(bossUnitTemplateDefinition, tile2, SpawnActorPhaseActionDefinition.UnitCreationSettings);
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
}
