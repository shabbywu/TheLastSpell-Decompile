using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Tooltip;

public class TooltipDisplayer : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	protected TooltipBase targetTooltip;

	public bool Displayed
	{
		get
		{
			if ((Object)(object)targetTooltip != (Object)null)
			{
				return targetTooltip.Displayed;
			}
			return false;
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		DisplayTooltip();
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		HideTooltip();
	}

	protected virtual bool CanDisplay()
	{
		return true;
	}

	public virtual void DisplayTooltip()
	{
		if (CanDisplay() && (Object)(object)targetTooltip != (Object)null)
		{
			targetTooltip.Display();
		}
	}

	public virtual void HideTooltip()
	{
		if ((Object)(object)targetTooltip != (Object)null)
		{
			targetTooltip.Hide();
		}
	}
}
