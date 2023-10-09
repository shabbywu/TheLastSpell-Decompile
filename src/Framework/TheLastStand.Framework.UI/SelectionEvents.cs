using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace TheLastStand.Framework.UI;

public class SelectionEvents : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private UnityEvent<RectTransform> onSelect = new UnityEvent<RectTransform>();

	[SerializeField]
	private UnityEvent<RectTransform> onDeselect = new UnityEvent<RectTransform>();

	public void AddSelectListener(UnityAction<RectTransform> onSelectAction)
	{
		onSelect.AddListener(onSelectAction);
	}

	public void RemoveDeselectListener(UnityAction<RectTransform> onDeselectAction)
	{
		onDeselect.RemoveListener(onDeselectAction);
	}

	public void AddDeselectListener(UnityAction<RectTransform> onDeselectAction)
	{
		onDeselect.AddListener(onDeselectAction);
	}

	public void RemoveSelectListener(UnityAction<RectTransform> onSelectAction)
	{
		onSelect.RemoveListener(onSelectAction);
	}

	public void OnSelect(BaseEventData eventData)
	{
		UnityEvent<RectTransform> obj = onSelect;
		Transform transform = ((Component)this).transform;
		obj.Invoke((RectTransform)(object)((transform is RectTransform) ? transform : null));
	}

	public void OnDeselect(BaseEventData eventData)
	{
		UnityEvent<RectTransform> obj = onDeselect;
		Transform transform = ((Component)this).transform;
		obj.Invoke((RectTransform)(object)((transform is RectTransform) ? transform : null));
	}
}
