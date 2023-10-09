using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI;

[AddComponentMenu("UI/BetterSlider", 33)]
[RequireComponent(typeof(RectTransform))]
public class BetterSlider : Selectable, IPointerClickHandler, IEventSystemHandler, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
{
	public enum Direction
	{
		LeftToRight,
		RightToLeft,
		BottomToTop,
		TopToBottom
	}

	[Serializable]
	public class SliderEvent : UnityEvent<float>
	{
	}

	private enum Axis
	{
		Horizontal,
		Vertical
	}

	[SerializeField]
	private RectTransform m_FillRect;

	[SerializeField]
	private RectTransform m_HandleRect;

	[Space]
	[SerializeField]
	private Direction m_Direction;

	[SerializeField]
	private float m_MinValue;

	[SerializeField]
	private float m_MaxValue = 1f;

	[SerializeField]
	private bool m_WholeNumbers;

	[SerializeField]
	private bool m_forcedStep;

	[SerializeField]
	private float m_step = 0.1f;

	[SerializeField]
	protected float m_Value;

	[Space]
	[SerializeField]
	private SliderEvent m_OnValueChanged = new SliderEvent();

	[SerializeField]
	private UnityEvent m_onPointerEnter = new UnityEvent();

	[SerializeField]
	private UnityEvent m_onPointerClick = new UnityEvent();

	private BetterImage m_FillImage;

	private Transform m_FillTransform;

	private RectTransform m_FillContainerRect;

	private Transform m_HandleTransform;

	private RectTransform m_HandleContainerRect;

	private Vector2 m_Offset = Vector2.zero;

	private DrivenRectTransformTracker m_Tracker;

	public RectTransform fillRect
	{
		get
		{
			return m_FillRect;
		}
		set
		{
			if (SetPropertyUtility.SetClass(ref m_FillRect, value))
			{
				UpdateCachedReferences();
				UpdateVisuals();
			}
		}
	}

	public RectTransform handleRect
	{
		get
		{
			return m_HandleRect;
		}
		set
		{
			if (SetPropertyUtility.SetClass(ref m_HandleRect, value))
			{
				UpdateCachedReferences();
				UpdateVisuals();
			}
		}
	}

	public Direction direction
	{
		get
		{
			return m_Direction;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_Direction, value))
			{
				UpdateVisuals();
			}
		}
	}

	public float minValue
	{
		get
		{
			return m_MinValue;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_MinValue, value))
			{
				Set(m_Value);
				UpdateVisuals();
			}
		}
	}

	public float maxValue
	{
		get
		{
			return m_MaxValue;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_MaxValue, value))
			{
				Set(m_Value);
				UpdateVisuals();
			}
		}
	}

	public bool wholeNumbers
	{
		get
		{
			return m_WholeNumbers;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_WholeNumbers, value))
			{
				Set(m_Value);
				UpdateVisuals();
			}
		}
	}

	public bool forcedSteps
	{
		get
		{
			return m_forcedStep;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_forcedStep, value))
			{
				Set(m_Value);
				UpdateVisuals();
			}
		}
	}

	public virtual float value
	{
		get
		{
			if (wholeNumbers)
			{
				return Mathf.Round(m_Value);
			}
			return m_Value;
		}
		set
		{
			Set(value);
		}
	}

	public float normalizedValue
	{
		get
		{
			if (Mathf.Approximately(minValue, maxValue))
			{
				return 0f;
			}
			return Mathf.InverseLerp(minValue, maxValue, value);
		}
		set
		{
			this.value = Mathf.Lerp(minValue, maxValue, value);
		}
	}

	public SliderEvent onValueChanged
	{
		get
		{
			return m_OnValueChanged;
		}
		set
		{
			m_OnValueChanged = value;
		}
	}

	public UnityEventBase OnPointerEnterEvent => (UnityEventBase)(object)m_onPointerEnter;

	public UnityEventBase OnPointerClickEvent => (UnityEventBase)(object)m_onPointerClick;

	public float Step
	{
		get
		{
			return m_step;
		}
		set
		{
			m_step = value;
		}
	}

	private float stepSize
	{
		get
		{
			if (!wholeNumbers)
			{
				return (maxValue - minValue) * m_step;
			}
			return 1f;
		}
	}

	private Axis axis
	{
		get
		{
			if (m_Direction != 0 && m_Direction != Direction.RightToLeft)
			{
				return Axis.Vertical;
			}
			return Axis.Horizontal;
		}
	}

	private bool reverseValue
	{
		get
		{
			if (m_Direction != Direction.RightToLeft)
			{
				return m_Direction == Direction.TopToBottom;
			}
			return true;
		}
	}

	protected BetterSlider()
	{
	}//IL_0022: Unknown result type (might be due to invalid IL or missing references)
	//IL_002c: Expected O, but got Unknown
	//IL_002d: Unknown result type (might be due to invalid IL or missing references)
	//IL_0037: Expected O, but got Unknown
	//IL_0038: Unknown result type (might be due to invalid IL or missing references)
	//IL_003d: Unknown result type (might be due to invalid IL or missing references)


	public virtual void Rebuild(CanvasUpdate executing)
	{
	}

	public virtual void LayoutComplete()
	{
	}

	public void SetValueWithoutNotify(float input)
	{
		Set(input, sendCallback: false);
	}

	public virtual void GraphicUpdateComplete()
	{
	}

	protected override void OnEnable()
	{
		((Selectable)this).OnEnable();
		UpdateCachedReferences();
		Set(m_Value, sendCallback: false);
		UpdateVisuals();
	}

	protected override void OnDisable()
	{
		((DrivenRectTransformTracker)(ref m_Tracker)).Clear();
		((Selectable)this).OnDisable();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		m_Value = ClampValue(m_Value);
		float num = normalizedValue;
		Vector2 val;
		if ((Object)(object)m_FillContainerRect != (Object)null)
		{
			if ((Object)(object)m_FillImage != (Object)null && m_FillImage.type == BetterImage.Type.Filled)
			{
				num = m_FillImage.fillAmount;
			}
			else
			{
				float num2;
				if (!reverseValue)
				{
					val = m_FillRect.anchorMax;
					num2 = ((Vector2)(ref val))[(int)axis];
				}
				else
				{
					val = m_FillRect.anchorMin;
					num2 = 1f - ((Vector2)(ref val))[(int)axis];
				}
				num = num2;
			}
		}
		else if ((Object)(object)m_HandleContainerRect != (Object)null)
		{
			float num3;
			if (!reverseValue)
			{
				val = m_HandleRect.anchorMin;
				num3 = ((Vector2)(ref val))[(int)axis];
			}
			else
			{
				val = m_HandleRect.anchorMin;
				num3 = 1f - ((Vector2)(ref val))[(int)axis];
			}
			num = num3;
		}
		UpdateVisuals();
		if (num != normalizedValue)
		{
			UISystemProfilerApi.AddMarker("BetterSlider.value", (Object)(object)this);
			((UnityEvent<float>)onValueChanged).Invoke(m_Value);
		}
	}

	private void UpdateCachedReferences()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)m_FillRect) && (Object)(object)m_FillRect != (Object)(RectTransform)((Component)this).transform)
		{
			m_FillTransform = ((Component)m_FillRect).transform;
			m_FillImage = ((Component)m_FillRect).GetComponent<BetterImage>();
			if ((Object)(object)m_FillTransform.parent != (Object)null)
			{
				m_FillContainerRect = ((Component)m_FillTransform.parent).GetComponent<RectTransform>();
			}
		}
		else
		{
			m_FillRect = null;
			m_FillContainerRect = null;
			m_FillImage = null;
		}
		if (Object.op_Implicit((Object)(object)m_HandleRect) && (Object)(object)m_HandleRect != (Object)(RectTransform)((Component)this).transform)
		{
			m_HandleTransform = ((Component)m_HandleRect).transform;
			if ((Object)(object)m_HandleTransform.parent != (Object)null)
			{
				m_HandleContainerRect = ((Component)m_HandleTransform.parent).GetComponent<RectTransform>();
			}
		}
		else
		{
			m_HandleRect = null;
			m_HandleContainerRect = null;
		}
	}

	private float ClampValue(float input)
	{
		float num = Mathf.Clamp(input, minValue, maxValue);
		if (wholeNumbers)
		{
			num = Mathf.Round(num);
		}
		else if (forcedSteps)
		{
			num = Mathf.Clamp(Mathf.Round(num / m_step) * m_step, minValue, maxValue);
		}
		return num;
	}

	private void Set(float input)
	{
		Set(input, sendCallback: true);
	}

	protected virtual void Set(float input, bool sendCallback)
	{
		float num = ClampValue(input);
		if (m_Value != num)
		{
			m_Value = num;
			UpdateVisuals();
			if (sendCallback)
			{
				UISystemProfilerApi.AddMarker("BetterSlider.value", (Object)(object)this);
				((UnityEvent<float>)m_OnValueChanged).Invoke(num);
			}
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		((UIBehaviour)this).OnRectTransformDimensionsChange();
		if (((UIBehaviour)this).IsActive())
		{
			UpdateVisuals();
		}
	}

	private void UpdateVisuals()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		((DrivenRectTransformTracker)(ref m_Tracker)).Clear();
		if ((Object)(object)m_FillContainerRect != (Object)null)
		{
			((DrivenRectTransformTracker)(ref m_Tracker)).Add((Object)(object)this, m_FillRect, (DrivenTransformProperties)3840);
			Vector2 zero = Vector2.zero;
			Vector2 one = Vector2.one;
			if ((Object)(object)m_FillImage != (Object)null && m_FillImage.type == BetterImage.Type.Filled)
			{
				m_FillImage.fillAmount = normalizedValue;
			}
			else if (reverseValue)
			{
				((Vector2)(ref zero))[(int)axis] = 1f - normalizedValue;
			}
			else
			{
				((Vector2)(ref one))[(int)axis] = normalizedValue;
			}
			m_FillRect.anchorMin = zero;
			m_FillRect.anchorMax = one;
		}
		if ((Object)(object)m_HandleContainerRect != (Object)null)
		{
			((DrivenRectTransformTracker)(ref m_Tracker)).Add((Object)(object)this, m_HandleRect, (DrivenTransformProperties)3840);
			Vector2 zero2 = Vector2.zero;
			Vector2 one2 = Vector2.one;
			Axis num = axis;
			float num3 = (((Vector2)(ref one2))[(int)axis] = (reverseValue ? (1f - normalizedValue) : normalizedValue));
			((Vector2)(ref zero2))[(int)num] = num3;
			m_HandleRect.anchorMin = zero2;
			m_HandleRect.anchorMax = one2;
		}
	}

	private void UpdateDrag(PointerEventData eventData, Camera cam)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = m_HandleContainerRect ?? m_FillContainerRect;
		if ((Object)(object)val != (Object)null)
		{
			Rect rect = val.rect;
			Vector2 val2 = ((Rect)(ref rect)).size;
			Vector2 val3 = default(Vector2);
			if (((Vector2)(ref val2))[(int)axis] > 0f && RectTransformUtility.ScreenPointToLocalPointInRectangle(val, eventData.position, cam, ref val3))
			{
				Vector2 val4 = val3;
				rect = val.rect;
				val3 = val4 - ((Rect)(ref rect)).position;
				val2 = val3 - m_Offset;
				float num = ((Vector2)(ref val2))[(int)axis];
				rect = val.rect;
				val2 = ((Rect)(ref rect)).size;
				float num2 = Mathf.Clamp01(num / ((Vector2)(ref val2))[(int)axis]);
				normalizedValue = (reverseValue ? (1f - num2) : num2);
			}
		}
	}

	private bool MayDrag(PointerEventData eventData)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		if (((UIBehaviour)this).IsActive() && ((Selectable)this).IsInteractable())
		{
			return (int)eventData.button == 0;
		}
		return false;
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!MayDrag(eventData))
		{
			return;
		}
		((Selectable)this).OnPointerDown(eventData);
		m_Offset = Vector2.zero;
		if ((Object)(object)m_HandleContainerRect != (Object)null && RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.position, eventData.enterEventCamera))
		{
			Vector2 offset = default(Vector2);
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, ref offset))
			{
				m_Offset = offset;
			}
		}
		else
		{
			UpdateDrag(eventData, eventData.pressEventCamera);
		}
	}

	public virtual void OnDrag(PointerEventData eventData)
	{
		if (MayDrag(eventData))
		{
			UpdateDrag(eventData, eventData.pressEventCamera);
		}
	}

	public override void OnMove(AxisEventData eventData)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected I4, but got Unknown
		if (!((UIBehaviour)this).IsActive() || !((Selectable)this).IsInteractable())
		{
			((Selectable)this).OnMove(eventData);
			return;
		}
		MoveDirection moveDir = eventData.moveDir;
		switch ((int)moveDir)
		{
		case 0:
			if (axis == Axis.Horizontal && (Object)(object)((Selectable)this).FindSelectableOnLeft() == (Object)null)
			{
				Set(reverseValue ? (value + stepSize) : (value - stepSize));
			}
			else
			{
				((Selectable)this).OnMove(eventData);
			}
			break;
		case 2:
			if (axis == Axis.Horizontal && (Object)(object)((Selectable)this).FindSelectableOnRight() == (Object)null)
			{
				Set(reverseValue ? (value - stepSize) : (value + stepSize));
			}
			else
			{
				((Selectable)this).OnMove(eventData);
			}
			break;
		case 1:
			if (axis == Axis.Vertical && (Object)(object)((Selectable)this).FindSelectableOnUp() == (Object)null)
			{
				Set(reverseValue ? (value - stepSize) : (value + stepSize));
			}
			else
			{
				((Selectable)this).OnMove(eventData);
			}
			break;
		case 3:
			if (axis == Axis.Vertical && (Object)(object)((Selectable)this).FindSelectableOnDown() == (Object)null)
			{
				Set(reverseValue ? (value + stepSize) : (value - stepSize));
			}
			else
			{
				((Selectable)this).OnMove(eventData);
			}
			break;
		}
	}

	public override Selectable FindSelectableOnLeft()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		Navigation navigation = ((Selectable)this).navigation;
		if ((int)((Navigation)(ref navigation)).mode == 3 && axis == Axis.Horizontal)
		{
			return null;
		}
		return ((Selectable)this).FindSelectableOnLeft();
	}

	public override Selectable FindSelectableOnRight()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		Navigation navigation = ((Selectable)this).navigation;
		if ((int)((Navigation)(ref navigation)).mode == 3 && axis == Axis.Horizontal)
		{
			return null;
		}
		return ((Selectable)this).FindSelectableOnRight();
	}

	public override Selectable FindSelectableOnUp()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		Navigation navigation = ((Selectable)this).navigation;
		if ((int)((Navigation)(ref navigation)).mode == 3 && axis == Axis.Vertical)
		{
			return null;
		}
		return ((Selectable)this).FindSelectableOnUp();
	}

	public override Selectable FindSelectableOnDown()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		Navigation navigation = ((Selectable)this).navigation;
		if ((int)((Navigation)(ref navigation)).mode == 3 && axis == Axis.Vertical)
		{
			return null;
		}
		return ((Selectable)this).FindSelectableOnDown();
	}

	public virtual void OnInitializePotentialDrag(PointerEventData eventData)
	{
		eventData.useDragThreshold = false;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		((Selectable)this).OnPointerEnter(eventData);
		if (((Selectable)this).interactable)
		{
			m_onPointerEnter.Invoke();
		}
	}

	public void SetDirection(Direction direction, bool includeRectLayouts)
	{
		Axis axis = this.axis;
		bool flag = reverseValue;
		this.direction = direction;
		if (includeRectLayouts)
		{
			if (this.axis != axis)
			{
				Transform transform = ((Component)this).transform;
				RectTransformUtility.FlipLayoutAxes((RectTransform)(object)((transform is RectTransform) ? transform : null), true, true);
			}
			if (reverseValue != flag)
			{
				Transform transform2 = ((Component)this).transform;
				RectTransformUtility.FlipLayoutOnAxis((RectTransform)(object)((transform2 is RectTransform) ? transform2 : null), (int)this.axis, true, true);
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (((Selectable)this).interactable)
		{
			m_onPointerClick.Invoke();
		}
	}

	[SpecialName]
	Transform ICanvasElement.get_transform()
	{
		return ((Component)this).transform;
	}
}
