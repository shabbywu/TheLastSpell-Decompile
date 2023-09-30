using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs.Feedback;

public class UnlockedGlyphFeedback : MonoBehaviour
{
	[SerializeField]
	private GlyphDisplay glyphDisplayParent;

	public Animator UnlockAnimator;

	public Image UnlockImage;

	public void StopAnimation()
	{
		((Behaviour)UnlockImage).enabled = false;
		((Behaviour)UnlockAnimator).enabled = false;
	}

	public void TriggerUnlockAnimation()
	{
		((Behaviour)UnlockAnimator).enabled = true;
	}

	public void GlyphDisplayParentRefreshLockedFeedback()
	{
		glyphDisplayParent.RefreshLockedFeedback(hideFeedback: false);
	}
}
