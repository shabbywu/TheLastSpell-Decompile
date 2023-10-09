using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace TheLastStand.Framework.UI;

[RequireComponent(typeof(Animator))]
public class BetterToggleGauge : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{
	public enum E_BetterToggleGaugeState
	{
		Disabled,
		Normal,
		Selected,
		Hovered,
		Activated
	}

	public enum E_BetterToggleGaugeTransition
	{
		None,
		Animation
	}

	public E_BetterToggleGaugeState State = E_BetterToggleGaugeState.Normal;

	public E_BetterToggleGaugeTransition Transition;

	private Animator animator;

	[SerializeField]
	private string activatedState = "WorldMapApocalypseLineActivated";

	[SerializeField]
	private string disabledState = "Disabled";

	[SerializeField]
	private string hoveredState = "Highlighted";

	[SerializeField]
	private string normalState = "Normal";

	[SerializeField]
	private string selectedState = "Pressed";

	[SerializeField]
	private UnityEvent onPointerClick = new UnityEvent();

	[SerializeField]
	private UnityEvent onPointerEnter = new UnityEvent();

	[SerializeField]
	private UnityEvent onPointerExit = new UnityEvent();

	[SerializeField]
	private StateEvent onStateHasChanged = new StateEvent();

	[SerializeField]
	private BetterToggleGaugeGroup group;

	[SerializeField]
	private int orderInGroup;

	public Animator Animator
	{
		get
		{
			if ((Object)(object)animator == (Object)null)
			{
				animator = ((Component)this).GetComponent<Animator>();
			}
			return animator;
		}
	}

	public UnityEvent OnPointerClickEvent => onPointerClick;

	public UnityEvent OnPointerEnterEvent => onPointerEnter;

	public UnityEvent OnPointeExitEvent => onPointerExit;

	public StateEvent OnStateHasChanged => onStateHasChanged;

	public void Init(int order, BetterToggleGaugeGroup group)
	{
		this.group = group;
		orderInGroup = order;
		group.RegisterToggle(this, orderInGroup);
		((UnityEvent<E_BetterToggleGaugeState>)OnStateHasChanged).AddListener((UnityAction<E_BetterToggleGaugeState>)delegate
		{
			if (Transition == E_BetterToggleGaugeTransition.Animation)
			{
				Animator.Play(GetTriggerSwitchState(), 0, Random.value);
			}
		});
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		UnityEvent onPointerClickEvent = OnPointerClickEvent;
		if (onPointerClickEvent != null)
		{
			onPointerClickEvent.Invoke();
		}
		switch (State)
		{
		case E_BetterToggleGaugeState.Disabled:
			return;
		case E_BetterToggleGaugeState.Selected:
			State = E_BetterToggleGaugeState.Normal;
			break;
		case E_BetterToggleGaugeState.Normal:
		case E_BetterToggleGaugeState.Hovered:
		case E_BetterToggleGaugeState.Activated:
			State = E_BetterToggleGaugeState.Selected;
			break;
		}
		((UnityEvent<E_BetterToggleGaugeState>)OnStateHasChanged)?.Invoke(State);
		if (State == E_BetterToggleGaugeState.Selected)
		{
			group.NotifyToggleOn(orderInGroup);
		}
		else
		{
			group.NotifyToggleOn(-1);
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		OnPointerClick(null);
	}

	public void OnSelect(BaseEventData eventData)
	{
		OnPointerEnter(null);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		OnPointerExit(null);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		UnityEvent onPointerEnterEvent = OnPointerEnterEvent;
		if (onPointerEnterEvent != null)
		{
			onPointerEnterEvent.Invoke();
		}
		switch (State)
		{
		case E_BetterToggleGaugeState.Normal:
			State = E_BetterToggleGaugeState.Hovered;
			break;
		case E_BetterToggleGaugeState.Disabled:
		case E_BetterToggleGaugeState.Selected:
		case E_BetterToggleGaugeState.Hovered:
		case E_BetterToggleGaugeState.Activated:
			return;
		}
		((UnityEvent<E_BetterToggleGaugeState>)OnStateHasChanged)?.Invoke(State);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		switch (State)
		{
		case E_BetterToggleGaugeState.Disabled:
			return;
		case E_BetterToggleGaugeState.Normal:
			return;
		case E_BetterToggleGaugeState.Selected:
			return;
		case E_BetterToggleGaugeState.Hovered:
			State = E_BetterToggleGaugeState.Normal;
			break;
		case E_BetterToggleGaugeState.Activated:
			return;
		}
		((UnityEvent<E_BetterToggleGaugeState>)OnStateHasChanged)?.Invoke(State);
	}

	public void SetState(E_BetterToggleGaugeState state)
	{
		State = state;
		((UnityEvent<E_BetterToggleGaugeState>)OnStateHasChanged)?.Invoke(State);
	}

	private string GetTriggerSwitchState()
	{
		return State switch
		{
			E_BetterToggleGaugeState.Disabled => disabledState, 
			E_BetterToggleGaugeState.Normal => normalState, 
			E_BetterToggleGaugeState.Selected => selectedState, 
			E_BetterToggleGaugeState.Hovered => hoveredState, 
			E_BetterToggleGaugeState.Activated => activatedState, 
			_ => string.Empty, 
		};
	}
}
