using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlayRippleEffectCutsceneController : CutsceneController
{
	public PlayRippleEffectCutsceneDefinition PlayRippleEffectCutsceneDefinition => base.CutsceneDefinition as PlayRippleEffectCutsceneDefinition;

	public PlayRippleEffectCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (!cutsceneData.Position.HasValue)
		{
			((CLogger<CutsceneManager>)TPSingleton<CutsceneManager>.Instance).LogError((object)"Tried to play a PlayRippleEffectCutsceneController but position is null.", (CLogLevel)1, true, true);
		}
		else
		{
			CameraView.RippleEffect.RippleAtWorldPosition(cutsceneData.Position.Value);
		}
		yield break;
	}
}
