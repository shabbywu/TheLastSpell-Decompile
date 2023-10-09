using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace TheLastStand.Framework.UI;

public class PointerEventsListener : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField]
	private UnityEvent onPointerEnter;

	[SerializeField]
	private UnityEvent onPointerExit;

	[SerializeField]
	private UnityEvent onPointerDown;

	[SerializeField]
	private UnityEvent onPointerUp;

	public UnityEvent OnPointerEnterEvent => onPointerEnter;

	public UnityEvent OnPointerExitEvent => onPointerExit;

	public UnityEvent OnPointerDownEvent => onPointerDown;

	public UnityEvent OnPointerUpEvent => onPointerUp;

	public void OnPointerEnter(PointerEventData eventData)
	{
		UnityEvent obj = onPointerEnter;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		UnityEvent obj = onPointerExit;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		UnityEvent obj = onPointerDown;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		UnityEvent obj = onPointerUp;
		if (obj != null)
		{
			obj.Invoke();
		}
	}
}
