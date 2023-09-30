using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlayDeathAnimCutsceneController : CutsceneController
{
	public PlayDeathAnimCutsceneDefinition PlayDeathAnimCutsceneDefinition => base.CutsceneDefinition as PlayDeathAnimCutsceneDefinition;

	public PlayDeathAnimCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (cutsceneData.Unit == null)
		{
			((CLogger<CutsceneManager>)TPSingleton<CutsceneManager>.Instance).LogError((object)"Tried to play a PlayDeathAnimCutsceneController with a null unit.", (CLogLevel)1, true, true);
			yield break;
		}
		cutsceneData.Unit.UnitView.PlayDieAnim();
		if (PlayDeathAnimCutsceneDefinition.WaitDeathAnim)
		{
			yield return cutsceneData.Unit.UnitView.WaitUntilDeathCanBeFinalized;
		}
	}
}
