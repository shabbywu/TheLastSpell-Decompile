using TheLastStand.Framework;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

[RequireComponent(typeof(RectTransform))]
public class HandlerDropdownItem : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	[SerializeField]
	private RectTransform scrollViewport;

	[SerializeField]
	private Scrollbar scrollbar;

	public void OnSelect(BaseEventData eventData)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		if (InputManager.IsLastControllerJoystick)
		{
			GUIHelpers.AdjustScrollViewToFocusedItem((RectTransform)((Component)this).transform, scrollViewport, scrollbar, 0.01f, 0.01f);
		}
	}
}
