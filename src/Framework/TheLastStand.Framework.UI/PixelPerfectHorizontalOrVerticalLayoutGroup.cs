using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI;

public abstract class PixelPerfectHorizontalOrVerticalLayoutGroup : PixelPerfectLayoutGroup
{
	[SerializeField]
	protected float m_Spacing;

	[SerializeField]
	protected bool m_ChildForceExpandWidth = true;

	[SerializeField]
	protected bool m_ChildForceExpandHeight = true;

	[SerializeField]
	protected bool m_ChildControlWidth = true;

	[SerializeField]
	protected bool m_ChildControlHeight = true;

	public float spacing
	{
		get
		{
			return m_Spacing;
		}
		set
		{
			SetProperty(ref m_Spacing, value);
		}
	}

	public bool childForceExpandWidth
	{
		get
		{
			return m_ChildForceExpandWidth;
		}
		set
		{
			SetProperty(ref m_ChildForceExpandWidth, value);
		}
	}

	public bool childForceExpandHeight
	{
		get
		{
			return m_ChildForceExpandHeight;
		}
		set
		{
			SetProperty(ref m_ChildForceExpandHeight, value);
		}
	}

	public bool childControlWidth
	{
		get
		{
			return m_ChildControlWidth;
		}
		set
		{
			SetProperty(ref m_ChildControlWidth, value);
		}
	}

	public bool childControlHeight
	{
		get
		{
			return m_ChildControlHeight;
		}
		set
		{
			SetProperty(ref m_ChildControlHeight, value);
		}
	}

	protected void CalcAlongAxis(int axis, bool isVertical)
	{
		float num = ((axis == 0) ? base.padding.horizontal : base.padding.vertical);
		bool controlSize = ((axis == 0) ? m_ChildControlWidth : m_ChildControlHeight);
		bool childForceExpand = ((axis == 0) ? childForceExpandWidth : childForceExpandHeight);
		float num2 = num;
		float num3 = num;
		float num4 = 0f;
		bool flag = isVertical ^ (axis == 1);
		for (int i = 0; i < base.rectChildren.Count; i++)
		{
			RectTransform child = base.rectChildren[i];
			GetChildSizes(child, axis, controlSize, childForceExpand, out var min, out var preferred, out var flexible);
			if (flag)
			{
				num2 = Mathf.Max(min + num, num2);
				num3 = Mathf.Max(preferred + num, num3);
				num4 = Mathf.Max(flexible, num4);
			}
			else
			{
				num2 += min + spacing;
				num3 += preferred + spacing;
				num4 += flexible;
			}
		}
		if (!flag && base.rectChildren.Count > 0)
		{
			num2 -= spacing;
			num3 -= spacing;
		}
		num3 = Mathf.Max(num2, num3);
		SetLayoutInputForAxis(num2, num3, num4, axis);
	}

	protected void SetChildrenAlongAxis(int axis, bool isVertical)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = base.rectTransform.rect;
		Vector2 val = ((Rect)(ref rect)).size;
		float num = ((Vector2)(ref val))[axis];
		bool flag = ((axis == 0) ? m_ChildControlWidth : m_ChildControlHeight);
		bool childForceExpand = ((axis == 0) ? childForceExpandWidth : childForceExpandHeight);
		float alignmentOnAxis = GetAlignmentOnAxis(axis);
		if (isVertical ^ (axis == 1))
		{
			float num2 = num - (float)((axis == 0) ? base.padding.horizontal : base.padding.vertical);
			for (int i = 0; i < base.rectChildren.Count; i++)
			{
				RectTransform val2 = base.rectChildren[i];
				GetChildSizes(val2, axis, flag, childForceExpand, out var min, out var preferred, out var flexible);
				int num3 = Mathf.RoundToInt(Mathf.Clamp(num2, min, (flexible > 0f) ? num : preferred));
				float startOffset = GetStartOffset(axis, num3);
				if (flag)
				{
					SetChildAlongAxis(val2, axis, Mathf.RoundToInt(startOffset), num3);
					continue;
				}
				float num4 = num3;
				val = val2.sizeDelta;
				float num5 = (num4 - ((Vector2)(ref val))[axis]) * alignmentOnAxis;
				SetChildAlongAxis(val2, axis, Mathf.RoundToInt(startOffset + num5));
			}
			return;
		}
		float num6 = ((axis == 0) ? base.padding.left : base.padding.top);
		if (GetTotalFlexibleSize(axis) == 0f && GetTotalPreferredSize(axis) < num)
		{
			num6 = GetStartOffset(axis, GetTotalPreferredSize(axis) - (float)((axis == 0) ? base.padding.horizontal : base.padding.vertical));
		}
		float num7 = 0f;
		if (GetTotalMinSize(axis) != GetTotalPreferredSize(axis))
		{
			num7 = Mathf.Clamp01((num - GetTotalMinSize(axis)) / (GetTotalPreferredSize(axis) - GetTotalMinSize(axis)));
		}
		float num8 = 0f;
		if (num > GetTotalPreferredSize(axis) && GetTotalFlexibleSize(axis) > 0f)
		{
			num8 = (num - GetTotalPreferredSize(axis)) / GetTotalFlexibleSize(axis);
		}
		for (int j = 0; j < base.rectChildren.Count; j++)
		{
			RectTransform val3 = base.rectChildren[j];
			GetChildSizes(val3, axis, flag, childForceExpand, out var min2, out var preferred2, out var flexible2);
			float num9 = Mathf.Lerp(min2, preferred2, num7);
			num9 += flexible2 * num8;
			if (flag)
			{
				SetChildAlongAxis(val3, axis, Mathf.RoundToInt(num6), Mathf.RoundToInt(num9));
			}
			else
			{
				float num10 = num9;
				val = val3.sizeDelta;
				float num11 = (num10 - ((Vector2)(ref val))[axis]) * alignmentOnAxis;
				SetChildAlongAxis(val3, axis, Mathf.RoundToInt(num6 + num11));
			}
			num6 += num9 + spacing;
		}
	}

	private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand, out float min, out float preferred, out float flexible)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (!controlSize)
		{
			Vector2 sizeDelta = child.sizeDelta;
			min = ((Vector2)(ref sizeDelta))[axis];
			preferred = min;
			flexible = 0f;
		}
		else
		{
			min = LayoutUtility.GetMinSize(child, axis);
			preferred = LayoutUtility.GetPreferredSize(child, axis);
			flexible = LayoutUtility.GetFlexibleSize(child, axis);
		}
		if (childForceExpand)
		{
			flexible = Mathf.Max(flexible, 1f);
		}
	}
}
