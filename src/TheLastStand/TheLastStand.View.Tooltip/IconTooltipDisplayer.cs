using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Tooltip;

public class IconTooltipDisplayer : TooltipDisplayer
{
	protected bool isFromLightShop;

	protected Vector2 oldPivot;

	private FollowElement.FollowDatas oldFollowData;

	[SerializeField]
	private FollowElement.FollowDatas followData;

	public override void DisplayTooltip()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		oldPivot = targetTooltip.RectTransform.pivot;
		oldFollowData = new FollowElement.FollowDatas(targetTooltip.FollowElement.FollowElementDatas);
		targetTooltip.FollowElement.ChangeFollowDatas(followData);
		if (isFromLightShop)
		{
			targetTooltip.FollowElement.ChangeOffset(new Vector3(0f - followData.Offset.x, followData.Offset.y, followData.Offset.z));
		}
		UpdateAnchor();
		base.DisplayTooltip();
	}

	public override void HideTooltip()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		targetTooltip.RectTransform.pivot = oldPivot;
		targetTooltip.FollowElement.ChangeFollowDatas(oldFollowData);
		base.HideTooltip();
	}

	protected void Init(TooltipBase tooltip = null, bool newIsFromLightShop = false)
	{
		if (targetTooltip == null)
		{
			targetTooltip = tooltip;
		}
		isFromLightShop = newIsFromLightShop;
	}

	protected virtual void UpdateAnchor()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		targetTooltip.RectTransform.pivot = (isFromLightShop ? Vector2.one : Vector2.up);
	}
}
