using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Framework;

public static class GUIHelpers
{
	public static void AdjustScrollViewToFocusedItem(RectTransform focusedRect, RectTransform scrollViewViewport, Scrollbar scrollbar, float stepValue, float margin, float? snap = null)
	{
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		Vector3[] array2 = (Vector3[])(object)new Vector3[4];
		focusedRect.GetWorldCorners(array);
		scrollViewViewport.GetWorldCorners(array2);
		while (array[1].y > array2[1].y)
		{
			scrollbar.value += stepValue;
			focusedRect.GetWorldCorners(array);
		}
		while (array[0].y < array2[0].y)
		{
			scrollbar.value -= stepValue;
			focusedRect.GetWorldCorners(array);
		}
		if (scrollbar.value - margin < 0f || (snap.HasValue && scrollbar.value < snap.Value))
		{
			scrollbar.value = 0f;
		}
		else if (scrollbar.value + margin > 1f || (snap.HasValue && scrollbar.value > 1f - snap.Value))
		{
			scrollbar.value = 1f;
		}
	}

	public static void AdjustHorizontalScrollViewToFocusedItem(RectTransform focusedRect, RectTransform scrollViewViewport, Scrollbar scrollbar, float stepValue, float margin, float? snap = null)
	{
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		Vector3[] array2 = (Vector3[])(object)new Vector3[4];
		focusedRect.GetWorldCorners(array);
		scrollViewViewport.GetWorldCorners(array2);
		while (array[3].x > array2[3].x)
		{
			scrollbar.value += stepValue;
			focusedRect.GetWorldCorners(array);
		}
		while (array[0].x < array2[0].x)
		{
			scrollbar.value -= stepValue;
			focusedRect.GetWorldCorners(array);
		}
		if (scrollbar.value - margin < 0f || (snap.HasValue && scrollbar.value < snap.Value))
		{
			scrollbar.value = 0f;
		}
		else if (scrollbar.value + margin > 1f || (snap.HasValue && scrollbar.value > 1f - snap.Value))
		{
			scrollbar.value = 1f;
		}
	}

	public static void AdjustHorizontalScrollViewToFocusedItem(RectTransform focusedRect, RectTransform scrollViewViewport, RectTransform parentRect, float stepValue)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		Vector3[] array2 = (Vector3[])(object)new Vector3[4];
		focusedRect.GetWorldCorners(array);
		scrollViewViewport.GetWorldCorners(array2);
		Vector3 val = ((Transform)parentRect).localPosition;
		Vector3 val2 = default(Vector3);
		while (array[2].x > array2[2].x)
		{
			((Vector3)(ref val2))._002Ector(val.x - stepValue, val.y, val.z);
			((Transform)parentRect).localPosition = val2;
			val = val2;
			focusedRect.GetWorldCorners(array);
		}
		while (array[0].x < array2[0].x)
		{
			((Vector3)(ref val2))._002Ector(val.x + stepValue, val.y, val.z);
			((Transform)parentRect).localPosition = val2;
			val = val2;
			focusedRect.GetWorldCorners(array);
		}
	}
}
