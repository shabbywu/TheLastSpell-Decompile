using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Framework.Extensions;

public static class ScrollRectExtensions
{
	public static void ScrollTo(this ScrollRect scroll, int index, float duration, Ease easing = 0)
	{
	}

	public static float GetScrollToValue(this ScrollRect scroll, RectTransform target)
	{
		int siblingIndex = ((Component)target).transform.GetSiblingIndex();
		float num = 1f - (float)siblingIndex / (float)((Component)scroll.content).transform.childCount;
		if ((double)num < 0.4)
		{
			float num2 = 1f / (float)((Component)scroll.content).transform.childCount;
			num -= num2;
		}
		return num;
	}
}
