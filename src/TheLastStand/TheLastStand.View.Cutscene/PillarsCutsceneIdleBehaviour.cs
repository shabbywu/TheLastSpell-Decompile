using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Cutscene;

public class PillarsCutsceneIdleBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
		TPSingleton<CutsceneManager>.Instance.PillarsCutsceneView.animatorIdleStateEnter = true;
	}
}
