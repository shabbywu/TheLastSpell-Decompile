using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs.Feedback;

public class CompletedGlyphFeedback : MonoBehaviour
{
	[SerializeField]
	private Image completedImage;

	[SerializeField]
	private Animator completionAnimator;

	public void DisplayCompletedImage()
	{
		((Behaviour)completedImage).enabled = true;
	}

	public void SetActiveCompletedImage(bool value)
	{
		((Behaviour)completedImage).enabled = value;
	}

	public void SetActiveCompletionAnimation(bool value)
	{
		((Behaviour)completionAnimator).enabled = value;
	}
}
