using UnityEngine;

namespace TheLastStand.View.Unit;

public class UnitStateMachine : StateMachineBehaviour
{
	private UnitView unitView;

	public void Init(UnitView newUnitView)
	{
		unitView = newUnitView;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
		if (!((Object)(object)unitView == (Object)null))
		{
			((Behaviour)unitView.Animator).enabled = ((Renderer)unitView.BodyFrontRenderer).isVisible || ((Renderer)unitView.BodyBackRenderer).isVisible;
		}
	}
}
