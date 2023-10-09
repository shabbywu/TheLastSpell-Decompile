using UnityEngine;

namespace TheLastStand.Framework.Extensions;

public static class RectTransformExtensions
{
	public static void ClampTo(this RectTransform rectTransform, RectTransform otherRectTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Transform)rectTransform).localPosition;
		Rect rect = rectTransform.rect;
		Rect rect2 = otherRectTransform.rect;
		Vector3 val = Vector2.op_Implicit(((Rect)(ref rect2)).min - ((Rect)(ref rect)).min);
		Vector3 val2 = Vector2.op_Implicit(((Rect)(ref rect2)).max - ((Rect)(ref rect)).max);
		localPosition.x = Mathf.Clamp(localPosition.x, val.x, val2.x);
		localPosition.y = Mathf.Clamp(localPosition.y, val.y, val2.y);
		if (((Transform)rectTransform).localPosition != localPosition)
		{
			((Transform)rectTransform).localPosition = localPosition;
		}
	}

	public static void ClampToParent(this RectTransform rectTransform)
	{
		if ((Object)(object)((Transform)rectTransform).parent == (Object)null)
		{
			Debug.LogError((object)"The RectTransform has no parent!");
			return;
		}
		RectTransform component = ((Component)((Transform)rectTransform).parent).GetComponent<RectTransform>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)"The parent of the RectTransform has no RectTransform!");
		}
		else
		{
			rectTransform.ClampTo(component);
		}
	}

	public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = rectTransform.rect;
		Vector2 size = ((Rect)(ref rect)).size;
		Vector2 val = rectTransform.pivot - pivot;
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(val.x * size.x, val.y * size.y);
		rectTransform.pivot = pivot;
		((Transform)rectTransform).localPosition = ((Transform)rectTransform).localPosition - val2;
	}

	public static void SetAnchorMin(this RectTransform rectTransform, Vector2 anchorMin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Transform)rectTransform).localPosition;
		rectTransform.anchorMin = anchorMin;
		((Transform)rectTransform).localPosition = localPosition;
	}

	public static void SetAnchorMax(this RectTransform rectTransform, Vector2 anchorMax)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Transform)rectTransform).localPosition;
		rectTransform.anchorMax = anchorMax;
		((Transform)rectTransform).localPosition = localPosition;
	}

	public static void SetAnchors(this RectTransform rectTransform, Vector2 anchors)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Transform)rectTransform).localPosition;
		rectTransform.anchorMin = anchors;
		rectTransform.anchorMax = anchors;
		((Transform)rectTransform).localPosition = localPosition;
	}

	public static Rect GetWorldRect(this RectTransform rectTransform)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Vector3 val = array[0];
		float x = ((Transform)rectTransform).lossyScale.x;
		Rect rect = rectTransform.rect;
		float num = x * ((Rect)(ref rect)).size.x;
		float y = ((Transform)rectTransform).lossyScale.y;
		rect = rectTransform.rect;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(num, y * ((Rect)(ref rect)).size.y);
		return new Rect(Vector2.op_Implicit(val), val2);
	}
}
