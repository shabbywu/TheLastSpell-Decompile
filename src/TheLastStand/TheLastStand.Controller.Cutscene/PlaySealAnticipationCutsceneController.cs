using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.View.Cutscene;
using TheLastStand.View.Sound;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class PlaySealAnticipationCutsceneController : CutsceneController
{
	public PlaySealAnticipationCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		BuildingManager.MagicCircle.MagicCircleView.PlaySealAnticipationAnimation();
		ObjectPooler.GetPooledComponent<OneShotSound>("SFXVictory", BuildingManager.MagicCircle.MagicCircleView.SFXPrefab, (Transform)null, false).Play(GameManager.WinAudioClip, TPSingleton<CutsceneManager>.Instance.VictorySequenceView.VictorySfxDelay);
		yield break;
	}
}
