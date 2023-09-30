using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View;

public class ImprovedToggle : Toggle
{
	[Serializable]
	public class EventDataUnityEvent : UnityEvent<PointerEventData>
	{
	}

	[SerializeField]
	private bool autoDeselection = true;

	[SerializeField]
	private EventDataUnityEvent onBeforePointerClick = new EventDataUnityEvent();

	[SerializeField]
	private UnityEvent onPointerClick = new UnityEvent();

	[SerializeField]
	private UnityEvent onPointerEnter = new UnityEvent();

	[SerializeField]
	private UnityEvent onPointerExit = new UnityEvent();

	public EventDataUnityEvent OnBeforePointerClickEvent => onBeforePointerClick;

	public UnityEvent OnPointerClickEvent => onPointerClick;

	public UnityEvent OnPointerEnterEvent => onPointerEnter;

	public UnityEvent OnPointerExitEvent => onPointerExit;

	public bool ShouldExecuteOnPointerClickEvent { get; set; } = true;


	public override void OnPointerClick(PointerEventData eventData)
	{
		((UnityEvent<PointerEventData>)onBeforePointerClick).Invoke(eventData);
		if (ShouldExecuteOnPointerClickEvent)
		{
			((Toggle)this).OnPointerClick(eventData);
			if (!((Selectable)this).interactable)
			{
				return;
			}
			onPointerClick.Invoke();
			if (autoDeselection)
			{
				EventSystem current = EventSystem.current;
				if (current != null)
				{
					current.SetSelectedGameObject((GameObject)null);
				}
			}
		}
		else
		{
			ShouldExecuteOnPointerClickEvent = true;
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		((Selectable)this).OnPointerEnter(eventData);
		if (((Selectable)this).interactable)
		{
			onPointerEnter.Invoke();
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		((Selectable)this).OnPointerExit(eventData);
		if (((Selectable)this).interactable)
		{
			onPointerExit.Invoke();
		}
	}
}
