using TheLastStand.Manager;
using TheLastStand.View.Tooltip;
using UnityEngine;

namespace TheLastStand.View.Resource;

public class ResourceTooltipDisplayer : TooltipDisplayer
{
	[SerializeField]
	private string resourceId = string.Empty;

	public override void DisplayTooltip()
	{
		ResourceTooltip obj = (targetTooltip as ResourceTooltip) ?? ResourceManager.ResourceTooltip;
		obj.SetContent(resourceId);
		obj.FollowElement.ChangeTarget(((Component)this).transform);
		obj.Display();
	}

	public override void HideTooltip()
	{
		ResourceTooltip resourceTooltip = (targetTooltip as ResourceTooltip) ?? ResourceManager.ResourceTooltip;
		if ((Object)(object)resourceTooltip != (Object)null)
		{
			resourceTooltip.Hide();
		}
	}
}
