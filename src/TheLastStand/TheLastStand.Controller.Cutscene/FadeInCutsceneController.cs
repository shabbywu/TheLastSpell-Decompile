using System.Collections;
using DG.Tweening;
using TPLib.Yield;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class FadeInCutsceneController : CutsceneController
{
	public FadeInCutsceneDefinition FadeInCutsceneDefinition => base.CutsceneDefinition as FadeInCutsceneDefinition;

	public FadeInCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		CanvasFadeManager.FadeIn(FadeInCutsceneDefinition.Color, FadeInCutsceneDefinition.Duration, -1, (Ease)0);
		if (FadeInCutsceneDefinition.WaitDuration)
		{
			yield return SharedYields.WaitForSeconds(FadeInCutsceneDefinition.Duration);
		}
	}
}
