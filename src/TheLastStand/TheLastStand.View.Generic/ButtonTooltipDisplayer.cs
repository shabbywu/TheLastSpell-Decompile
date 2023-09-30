using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class ButtonTooltipDisplayer : GenericTooltipDisplayer
{
	private enum E_DisplayContext
	{
		AlwaysDisplayOnHover,
		OnlyDisplayTooltipWhenInteractable,
		OnlyDisplayTooltipWhenNotInteractable
	}

	[SerializeField]
	private E_DisplayContext displayContext;

	[SerializeField]
	private Button button;

	public override bool CanDisplayTooltip()
	{
		if (base.HasFocus)
		{
			if (displayContext != 0 && (displayContext != E_DisplayContext.OnlyDisplayTooltipWhenInteractable || !((Selectable)button).interactable))
			{
				if (displayContext == E_DisplayContext.OnlyDisplayTooltipWhenNotInteractable)
				{
					return !((Selectable)button).interactable;
				}
				return false;
			}
			return true;
		}
		return false;
	}
}
