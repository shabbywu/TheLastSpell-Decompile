using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Tooltip;

public class ObjectDisableTooltipDisplayer : TooltipDisplayer
{
	[SerializeField]
	private GameObject objectToToggle;

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		objectToToggle.SetActive(true);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		objectToToggle.SetActive(false);
	}
}
