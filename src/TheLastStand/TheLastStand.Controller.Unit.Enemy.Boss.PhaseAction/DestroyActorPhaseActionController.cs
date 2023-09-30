using System.Collections;
using DG.Tweening;
using TPLib;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Boss;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class DestroyActorPhaseActionController : ABossPhaseActionController
{
	public DestroyActorPhaseActionDefinition DestroyActorPhaseActionDefinition => base.ABossPhaseActionDefinition as DestroyActorPhaseActionDefinition;

	public DestroyActorPhaseActionController(DestroyActorPhaseActionDefinition destroyActorPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(destroyActorPhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		string actorId = DestroyActorPhaseActionDefinition.ActorId;
		if (!TPSingleton<BossManager>.Instance.BossPhaseActors.TryGetValue(actorId, out var actors))
		{
			yield break;
		}
		bool cameraWasZoomed = ACameraView.IsZoomedIn;
		if (DestroyActorPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			ACameraView.Zoom(zoomIn: true);
		}
		BossPhase currentBossPhase = TPSingleton<BossManager>.Instance.CurrentBossPhase;
		if (((currentBossPhase != null) ? DictionaryExtensions.GetValueOrDefault<string, ActorDefinition>(currentBossPhase.BossPhaseDefinition.ActorDefinitions, actorId) : null)?.GetCorrespondingDefinition() is BuildingDefinition buildingDefinition && (buildingDefinition.BlueprintModuleDefinition.Category & BuildingDefinition.E_BuildingCategory.LitBrazier) != 0)
		{
			TPSingleton<BuildingManager>.Instance.ExtinguishBraziers();
		}
		else
		{
			for (int i = actors.Count - 1; i >= 0; i--)
			{
				if (actors[i] is EnemyUnit enemyUnit)
				{
					if (DestroyActorPhaseActionDefinition.CameraFocus)
					{
						ACameraView.MoveTo(enemyUnit.DamageableView.GameObject.transform.position, 0f, (Ease)0);
					}
					enemyUnit.EnemyUnitController.PrepareForDeath();
					enemyUnit.EnemyUnitView.PlayDieAnim();
					if (DestroyActorPhaseActionDefinition.WaitDeathAnim)
					{
						yield return enemyUnit.UnitView.WaitUntilDeathCanBeFinalized;
					}
				}
			}
		}
		if (DestroyActorPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = true;
			ACameraView.AllowUserZoom = true;
			ACameraView.Zoom(cameraWasZoomed);
		}
	}
}
