using System.Collections;
using Sirenix.OdinInspector;
using TPLib.Yield;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class EffectDisplay : SerializedMonoBehaviour, IDisplayableEffect
{
	public static class Constants
	{
		public const string EffectDisplayPath = "Prefab/Displayable Effect/UI Effect Displays/";
	}

	[SerializeField]
	protected CanvasGroup canvasGroup;

	[SerializeField]
	protected float displayDuration = 0.6f;

	[SerializeField]
	protected float delayBeforeDestruction;

	private bool isBeingDisplayed;

	private FollowElement followElement;

	protected virtual float DisplayDuration => displayDuration;

	public FollowElement FollowElement
	{
		get
		{
			if ((Object)(object)followElement == (Object)null)
			{
				followElement = ((Component)this).GetComponent<FollowElement>();
				if ((Object)(object)followElement == (Object)null)
				{
					followElement = ((Component)this).gameObject.AddComponent<FollowElement>();
				}
			}
			return followElement;
		}
	}

	public bool IsBeingDisplayed => isBeingDisplayed;

	public Coroutine Display()
	{
		if ((Object)(object)canvasGroup != (Object)null)
		{
			canvasGroup.alpha = 1f;
		}
		if ((Object)(object)FollowElement != (Object)null)
		{
			FollowElement.AutoMove();
		}
		return ((MonoBehaviour)this).StartCoroutine(DisplayAndDestroyCoroutine());
	}

	protected virtual void OnEnable()
	{
		if ((Object)(object)canvasGroup != (Object)null)
		{
			canvasGroup.alpha = 0f;
		}
	}

	protected virtual IEnumerator DisplayCoroutine()
	{
		if (displayDuration > 0f)
		{
			yield return SharedYields.WaitForSeconds(displayDuration);
		}
	}

	protected virtual IEnumerator DisplayAndDestroyCoroutine()
	{
		isBeingDisplayed = true;
		yield return ((MonoBehaviour)this).StartCoroutine(DisplayCoroutine());
		if (delayBeforeDestruction > 0f)
		{
			yield return SharedYields.WaitForSeconds(delayBeforeDestruction);
		}
		isBeingDisplayed = false;
		((Component)this).gameObject.SetActive(false);
	}
}
