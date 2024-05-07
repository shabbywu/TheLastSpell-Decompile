using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.HUD;

public class BetterToggleJoystickSelectableTooltipDisplayer : MonoBehaviour
{
	[SerializeField]
	private TooltipDisplayer tooltipDisplayer;

	[SerializeField]
	private FollowElement.FollowDatas followDatas;

	private void OnEnable()
	{
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	private void OnDisable()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (!((Object)(object)this == (Object)null) && !((Object)(object)EventSystem.current.currentSelectedGameObject != (Object)(object)((Component)this).gameObject) && !((Object)(object)tooltipDisplayer == (Object)null) && (Object)(object)tooltipDisplayer != (Object)null)
		{
			if (showTooltips)
			{
				DisplayTooltip();
			}
			else
			{
				HideTooltip();
			}
		}
	}

	public void OnSelect()
	{
		if (InputManager.IsLastControllerJoystick && TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips && (Object)(object)tooltipDisplayer != (Object)null)
		{
			DisplayTooltip();
		}
	}

	public void OnDeselect()
	{
		if (InputManager.IsLastControllerJoystick && (Object)(object)tooltipDisplayer != (Object)null && tooltipDisplayer.Displayed)
		{
			HideTooltip();
		}
	}

	private void DisplayTooltip()
	{
		SetCustomFollowData();
		tooltipDisplayer.DisplayTooltip();
	}

	private void HideTooltip()
	{
		ResetFollowData();
		tooltipDisplayer.HideTooltip();
	}

	private void SetCustomFollowData()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (followDatas != null)
		{
			FollowElement followElement = tooltipDisplayer.GetTooltip().FollowElement;
			followElement.FollowElementDatas.FollowTarget = followDatas.FollowTarget;
			followElement.FollowElementDatas.Offset = followDatas.Offset;
		}
	}

	private void ResetFollowData()
	{
		if (followDatas != null)
		{
			FollowElement followElement = tooltipDisplayer.GetTooltip().FollowElement;
			followElement.FollowElementDatas.FollowTarget = null;
			followElement.RestoreFollowDatasOffset();
		}
	}
}
