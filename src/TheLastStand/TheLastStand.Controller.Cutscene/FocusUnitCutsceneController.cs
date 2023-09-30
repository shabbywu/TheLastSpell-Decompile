using System.Collections;
using DG.Tweening;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class FocusUnitCutsceneController : CutsceneController
{
	public FocusUnitCutsceneDefinition FocusUnitCutsceneDefinition => base.CutsceneDefinition as FocusUnitCutsceneDefinition;

	public FocusUnitCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (cutsceneData.Unit == null)
		{
			((CLogger<CutsceneManager>)TPSingleton<CutsceneManager>.Instance).LogError((object)"Tried to play a FocusUnitCutsceneController with a null unit.", (CLogLevel)1, true, true);
			yield break;
		}
		if (FocusUnitCutsceneDefinition.ZoomIn)
		{
			ACameraView.Zoom(zoomIn: true);
		}
		ACameraView.MoveTo(((Component)cutsceneData.Unit.OriginTile.TileView).transform.position, CameraView.AnimationMoveSpeed, (Ease)0);
		yield return SharedYields.WaitForSeconds(CameraView.AnimationMoveSpeed);
	}
}
