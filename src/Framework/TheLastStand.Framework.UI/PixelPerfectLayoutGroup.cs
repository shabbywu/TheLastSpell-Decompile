using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI;

[DisallowMultipleComponent]
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public abstract class PixelPerfectLayoutGroup : UIBehaviour, ILayoutElement, ILayoutGroup, ILayoutController
{
	[SerializeField]
	protected RectOffset m_Padding = new RectOffset();

	[SerializeField]
	protected TextAnchor m_ChildAlignment;

	[NonSerialized]
	private RectTransform m_Rect;

	protected DrivenRectTransformTracker m_Tracker;

	private Vector2 m_TotalMinSize = Vector2.zero;

	private Vector2 m_TotalPreferredSize = Vector2.zero;

	private Vector2 m_TotalFlexibleSize = Vector2.zero;

	[NonSerialized]
	private List<RectTransform> m_RectChildren = new List<RectTransform>();

	public RectOffset padding
	{
		get
		{
			return m_Padding;
		}
		set
		{
			SetProperty(ref m_Padding, value);
		}
	}

	public TextAnchor childAlignment
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_ChildAlignment;
		}
		set
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			SetProperty(ref m_ChildAlignment, value);
		}
	}

	protected RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)m_Rect == (Object)null)
			{
				m_Rect = ((Component)this).GetComponent<RectTransform>();
			}
			return m_Rect;
		}
	}

	protected List<RectTransform> rectChildren => m_RectChildren;

	public virtual float minWidth => GetTotalMinSize(0);

	public virtual float preferredWidth => GetTotalPreferredSize(0);

	public virtual float flexibleWidth => GetTotalFlexibleSize(0);

	public virtual float minHeight => GetTotalMinSize(1);

	public virtual float preferredHeight => GetTotalPreferredSize(1);

	public virtual float flexibleHeight => GetTotalFlexibleSize(1);

	public virtual int layoutPriority => 0;

	private bool isRootLayoutGroup
	{
		get
		{
			if ((Object)(object)((Component)this).transform.parent == (Object)null)
			{
				return true;
			}
			return (Object)(object)((Component)((Component)this).transform.parent).GetComponent(typeof(ILayoutGroup)) == (Object)null;
		}
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		m_RectChildren.Clear();
		List<Component> list = ListPool<Component>.Get();
		for (int i = 0; i < ((Transform)rectTransform).childCount; i++)
		{
			Transform child = ((Transform)rectTransform).GetChild(i);
			RectTransform val = (RectTransform)(object)((child is RectTransform) ? child : null);
			if ((Object)(object)val == (Object)null || !((Component)val).gameObject.activeInHierarchy)
			{
				continue;
			}
			((Component)val).GetComponents(typeof(ILayoutIgnorer), list);
			if (list.Count == 0)
			{
				m_RectChildren.Add(val);
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (!((ILayoutIgnorer)list[j]).ignoreLayout)
				{
					m_RectChildren.Add(val);
					break;
				}
			}
		}
		ListPool<Component>.Release(list);
		((DrivenRectTransformTracker)(ref m_Tracker)).Clear();
	}

	public abstract void CalculateLayoutInputVertical();

	public abstract void SetLayoutHorizontal();

	public abstract void SetLayoutVertical();

	protected PixelPerfectLayoutGroup()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		if (m_Padding == null)
		{
			m_Padding = new RectOffset();
		}
	}

	protected override void OnEnable()
	{
		((UIBehaviour)this).OnEnable();
		SetDirty();
	}

	protected override void OnDisable()
	{
		((DrivenRectTransformTracker)(ref m_Tracker)).Clear();
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		((UIBehaviour)this).OnDisable();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		SetDirty();
	}

	protected float GetTotalMinSize(int axis)
	{
		return ((Vector2)(ref m_TotalMinSize))[axis];
	}

	protected float GetTotalPreferredSize(int axis)
	{
		return ((Vector2)(ref m_TotalPreferredSize))[axis];
	}

	protected float GetTotalFlexibleSize(int axis)
	{
		return ((Vector2)(ref m_TotalFlexibleSize))[axis];
	}

	protected float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float num = requiredSpaceWithoutPadding + (float)((axis == 0) ? padding.horizontal : padding.vertical);
		Rect rect = rectTransform.rect;
		Vector2 size = ((Rect)(ref rect)).size;
		float num2 = ((Vector2)(ref size))[axis] - num;
		float alignmentOnAxis = GetAlignmentOnAxis(axis);
		return (float)((axis == 0) ? padding.left : padding.top) + num2 * alignmentOnAxis;
	}

	protected float GetAlignmentOnAxis(int axis)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (axis == 0)
		{
			return (float)(childAlignment % 3) * 0.5f;
		}
		return (float)(childAlignment / 3) * 0.5f;
	}

	protected void SetLayoutInputForAxis(float totalMin, float totalPreferred, float totalFlexible, int axis)
	{
		((Vector2)(ref m_TotalMinSize))[axis] = totalMin;
		((Vector2)(ref m_TotalPreferredSize))[axis] = totalPreferred;
		((Vector2)(ref m_TotalFlexibleSize))[axis] = totalFlexible;
	}

	protected void SetChildAlongAxis(RectTransform rect, int axis, int pos)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rect == (Object)null))
		{
			((DrivenRectTransformTracker)(ref m_Tracker)).Add((Object)(object)this, rect, (DrivenTransformProperties)(0xF00 | ((axis == 0) ? 2 : 4)));
			int num = ((axis != 0) ? 2 : 0);
			float num2 = pos;
			Vector2 sizeDelta = rect.sizeDelta;
			rect.SetInsetAndSizeFromParentEdge((Edge)num, num2, ((Vector2)(ref sizeDelta))[axis]);
		}
	}

	protected void SetChildAlongAxis(RectTransform rect, int axis, int pos, int size)
	{
		if (!((Object)(object)rect == (Object)null))
		{
			((DrivenRectTransformTracker)(ref m_Tracker)).Add((Object)(object)this, rect, (DrivenTransformProperties)(0xF00 | ((axis == 0) ? 4098 : 8196)));
			rect.SetInsetAndSizeFromParentEdge((Edge)((axis != 0) ? 2 : 0), (float)pos, (float)size);
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		((UIBehaviour)this).OnRectTransformDimensionsChange();
		if (isRootLayoutGroup)
		{
			SetDirty();
		}
	}

	protected virtual void OnTransformChildrenChanged()
	{
		SetDirty();
	}

	protected void SetProperty<T>(ref T currentValue, T newValue)
	{
		if ((currentValue != null || newValue != null) && (currentValue == null || !currentValue.Equals(newValue)))
		{
			currentValue = newValue;
			SetDirty();
		}
	}

	protected void SetDirty()
	{
		if (((UIBehaviour)this).IsActive())
		{
			if (!CanvasUpdateRegistry.IsRebuildingLayout())
			{
				LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(DelayedSetDirty(rectTransform));
			}
		}
	}

	private IEnumerator DelayedSetDirty(RectTransform rectTransform)
	{
		yield return null;
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
	}
}
