using TPLib;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.Manager;

public class SelectableJoystickTooltipDisplayer : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private GenericTooltipDisplayer tooltipDisplayer;

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

	private void OnTooltipsToggled(bool state)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)EventSystem.current.currentSelectedGameObject != (Object)(object)((Component)this).gameObject) && !((Object)(object)tooltipDisplayer == (Object)null))
		{
			FollowElement tooltipFollowElement = tooltipDisplayer.GetTooltipFollowElement();
			if (state && tooltipDisplayer.CanDisplayTooltip())
			{
				tooltipFollowElement.FollowElementDatas.FollowTarget = followDatas.FollowTarget;
				tooltipFollowElement.FollowElementDatas.Offset = followDatas.Offset;
				tooltipDisplayer.DisplayTooltip();
			}
			else
			{
				tooltipFollowElement.FollowElementDatas.FollowTarget = null;
				tooltipFollowElement.RestoreFollowDatasOffset();
				tooltipDisplayer.HideTooltip();
			}
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips && tooltipDisplayer.CanDisplayTooltip())
		{
			FollowElement tooltipFollowElement = tooltipDisplayer.GetTooltipFollowElement();
			tooltipFollowElement.FollowElementDatas.FollowTarget = followDatas.FollowTarget;
			tooltipFollowElement.FollowElementDatas.Offset = followDatas.Offset;
			tooltipDisplayer.DisplayTooltip();
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		FollowElement tooltipFollowElement = tooltipDisplayer.GetTooltipFollowElement();
		tooltipFollowElement.FollowElementDatas.FollowTarget = null;
		tooltipFollowElement.RestoreFollowDatasOffset();
		tooltipDisplayer.HideTooltip();
	}
}
