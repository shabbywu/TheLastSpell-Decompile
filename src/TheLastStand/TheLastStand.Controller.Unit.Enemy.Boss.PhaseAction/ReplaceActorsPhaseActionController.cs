using System;
using System.Collections;
using DG.Tweening;
using TPLib;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Boss;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class ReplaceActorsPhaseActionController : ABossPhaseActionController
{
	public ReplaceActorsPhaseActionDefinition ReplaceActorsPhaseActionDefinition => base.ABossPhaseActionDefinition as ReplaceActorsPhaseActionDefinition;

	public ReplaceActorsPhaseActionController(ABossPhaseActionDefinition aBossPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(aBossPhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		ActorDefinition actorDefinition = TPSingleton<BossManager>.Instance.CurrentBossPhase?.BossPhaseDefinition.ActorDefinitions.GetValueOrDefault(ReplaceActorsPhaseActionDefinition.ReplacementId);
		TheLastStand.Framework.Serialization.Definition actorTypedDefinition = actorDefinition?.GetCorrespondingDefinition();
		if (!TPSingleton<BossManager>.Instance.BossPhaseActors.TryGetValue(ReplaceActorsPhaseActionDefinition.ActorId, out var actors))
		{
			yield break;
		}
		if (ReplaceActorsPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			ACameraView.Zoom(zoomIn: true);
		}
		for (int i = actors.Count - 1; i >= 0; i--)
		{
			Tile tile = actors[i].OriginTile;
			if (ReplaceActorsPhaseActionDefinition.CameraFocus && actors[i] != null)
			{
				ACameraView.MoveTo(actors[i].TileObjectView.GameObject.transform.position, 0f, (Ease)0);
			}
			IBossPhaseActor bossPhaseActor = actors[i];
			if (!(bossPhaseActor is EnemyUnit enemyUnit2))
			{
				if (bossPhaseActor is TheLastStand.Model.Building.Building building)
				{
					BuildingManager.DestroyBuilding(building.OriginTile, updateView: true, addDeadBuilding: false, triggerEvent: true, triggerOnDeathEvent: false);
				}
			}
			else
			{
				enemyUnit2.EnemyUnitController.PrepareForDeath();
				enemyUnit2.EnemyUnitView.PlayDieAnim();
				if (ReplaceActorsPhaseActionDefinition.WaitDeathAnim)
				{
					yield return enemyUnit2.EnemyUnitView.WaitUntilDeathCanBeFinalized;
				}
			}
			switch (actorDefinition.ActorType)
			{
			case DamageableType.Enemy:
			{
				EnemyUnit enemyUnit = EnemyUnitManager.CreateEnemyUnit(actorTypedDefinition as EnemyUnitTemplateDefinition, tile, ReplaceActorsPhaseActionDefinition.UnitCreationSettings);
				if (ReplaceActorsPhaseActionDefinition.UnitCreationSettings.WaitSpawnAnim)
				{
					yield return (object)new WaitUntil((Func<bool>)(() => enemyUnit.EnemyUnitView.AreAnimationsInitialized));
					yield return enemyUnit.EnemyUnitView.WaitUntilAnimatorStateIsIdle;
				}
				break;
			}
			case DamageableType.Boss:
				yield return BossManager.CreateBossUnit(actorTypedDefinition as BossUnitTemplateDefinition, tile, ReplaceActorsPhaseActionDefinition.UnitCreationSettings);
				break;
			case DamageableType.Building:
				BuildingManager.CreateBuilding(actorTypedDefinition as BuildingDefinition, tile);
				break;
			}
		}
		if (ReplaceActorsPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = true;
			ACameraView.AllowUserZoom = true;
		}
	}
}
