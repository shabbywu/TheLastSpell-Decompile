using System.Collections;
using DG.Tweening;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class FocusMagicCircleCutsceneController : CutsceneController
{
	public FocusMagicCircleCutsceneDefinition FocusMagicCircleCutsceneDefinition => base.CutsceneDefinition as FocusMagicCircleCutsceneDefinition;

	public FocusMagicCircleCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		ACameraView.MoveTo(((Component)BuildingManager.MagicCircle.BuildingView).transform.position + new Vector3(0f, TPSingleton<CutsceneManager>.Instance.VictorySequenceView.CameraYOffsetFromCircle), 0f, (Ease)0);
		if (FocusMagicCircleCutsceneDefinition.ZoomIn)
		{
			ACameraView.Zoom(zoomIn: true);
		}
		yield break;
	}
}
