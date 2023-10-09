using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI;

public class BetterToggle : Toggle
{
	[SerializeField]
	private bool autoDeselection = true;

	[SerializeField]
	private UnityEvent onPointerClick = new UnityEvent();

	[SerializeField]
	private UnityEvent onPointerEnter = new UnityEvent();

	[SerializeField]
	private UnityEvent onPointerExit = new UnityEvent();

	public UnityEvent OnPointerClickEvent => onPointerClick;

	public UnityEvent OnPointerEnterEvent => onPointerEnter;

	public UnityEvent OnPointeExitEvent => onPointerExit;

	public override void OnPointerClick(PointerEventData eventData)
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
