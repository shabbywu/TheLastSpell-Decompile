using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class BarkCutsceneController : CutsceneController
{
	public BarkCutsceneDefinition BarkCutsceneDefinition => base.CutsceneDefinition as BarkCutsceneDefinition;

	public BarkCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		IBarker barker = null;
		switch (BarkCutsceneDefinition.BarkerType)
		{
		case BarkCutsceneDefinition.E_BarkerType.Unit:
			barker = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[VictorySequenceView.barkerIndex % TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count];
			VictorySequenceView.barkerIndex++;
			break;
		case BarkCutsceneDefinition.E_BarkerType.MagicCircle:
			barker = BuildingManager.MagicCircle.BlueprintModule;
			break;
		case BarkCutsceneDefinition.E_BarkerType.BossUnit:
			barker = TPSingleton<BossManager>.Instance.BossUnits.FirstOrDefault((BossUnit x) => x.Id == BarkCutsceneDefinition.BarkerId);
			break;
		case BarkCutsceneDefinition.E_BarkerType.CutsceneUnit:
			barker = cutsceneData.Unit;
			break;
		default:
			CLoggerManager.Log((object)$"Could not find a valid barker for BarkerType {BarkCutsceneDefinition.BarkerType}!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			break;
		}
		if (barker == null)
		{
			yield break;
		}
		Vector3 val = (Vector3)((barker is BuildingModule buildingModule && buildingModule.BuildingParent is MagicCircle) ? new Vector3(0f, TPSingleton<CutsceneManager>.Instance.VictorySequenceView.CameraYOffsetFromCircle) : Vector3.zero);
		switch (BarkCutsceneDefinition.MoveCamera)
		{
		case BarkCutsceneDefinition.E_MoveCamera.IfOutOfScreen:
			if (CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(barker.BarkViewFollowTarget.position))
			{
				ACameraView.MoveTo(barker.BarkViewFollowTarget.position + val, CameraView.AnimationMoveSpeed, (Ease)0);
			}
			break;
		case BarkCutsceneDefinition.E_MoveCamera.Always:
			ACameraView.MoveTo(barker.BarkViewFollowTarget.position + val, CameraView.AnimationMoveSpeed, (Ease)0);
			break;
		}
		TPSingleton<BarkManager>.Instance.AddPotentialBark(BarkCutsceneDefinition.BarkDefinition.Id, barker, 0f, -1, forceSucceed: false, ignoreDeathCheck: true);
		TPSingleton<BarkManager>.Instance.Display();
		switch (BarkCutsceneDefinition.LookAtTarget)
		{
		case BarkCutsceneDefinition.E_LookAtTarget.Barker:
			foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				if (playableUnit != barker)
				{
					playableUnit.UnitController.LookAt(barker.OriginTile);
				}
			}
			break;
		case BarkCutsceneDefinition.E_LookAtTarget.MagicCircle:
			foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				playableUnit2.UnitController.LookAt(BuildingManager.MagicCircle.OriginTile);
			}
			break;
		}
		yield return WaitForBarkDisplaySequence();
	}

	private IEnumerator WaitForBarkDisplaySequence()
	{
		yield return SharedYields.WaitForSeconds(0.5f);
		yield return (object)new WaitUntil((Func<bool>)(() => BarkManager.DisplayedBarksCount == 0));
	}
}
