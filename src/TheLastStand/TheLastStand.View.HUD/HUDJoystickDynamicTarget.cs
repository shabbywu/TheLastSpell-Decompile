using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.HUD;

public class HUDJoystickDynamicTarget : HUDJoystickTarget
{
	[SerializeField]
	private HUDJoystickTarget[] targets;

	[SerializeField]
	private HUDJoystickTarget defaultTarget;

	public override bool IsSelectable()
	{
		if (base.IsSelectable())
		{
			if (!((Object)(object)GetFirstAvailableTarget() != (Object)null))
			{
				return (Object)(object)defaultTarget != (Object)null;
			}
			return true;
		}
		return false;
	}

	public override SelectionInfo GetSelectionInfo(Vector2? direction = null)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!IsSelectable())
		{
			LogSelectionNotSelectable();
			return SelectionInfo.Empty;
		}
		HUDJoystickTarget hUDJoystickTarget = GetFirstAvailableTarget();
		if ((Object)(object)hUDJoystickTarget == (Object)null)
		{
			if (direction.HasValue)
			{
				hUDJoystickTarget = GetNextPanelForDirection(direction.Value);
			}
			if ((Object)(object)hUDJoystickTarget == (Object)null)
			{
				hUDJoystickTarget = defaultTarget;
			}
		}
		return hUDJoystickTarget.GetSelectionInfo(direction);
	}

	private void LogSelectionNotSelectable()
	{
		((CLogger<HUDJoystickNavigationManager>)TPSingleton<HUDJoystickNavigationManager>.Instance).Log((object)("Joystick SelectionInfo of " + ((Object)((Component)this).gameObject).name + " isn't selectable !"), (CLogLevel)0, true, false);
		if (!base.IsSelectable())
		{
			LogTargetIsNotSelectable();
		}
		else if (targets.Length != 0)
		{
			((CLogger<HUDJoystickNavigationManager>)TPSingleton<HUDJoystickNavigationManager>.Instance).Log((object)"The list of target is empty !", (CLogLevel)0, true, false);
		}
		else if ((Object)(object)defaultTarget != (Object)null)
		{
			((CLogger<HUDJoystickNavigationManager>)TPSingleton<HUDJoystickNavigationManager>.Instance).Log((object)"The default target is null !", (CLogLevel)0, true, false);
		}
	}

	private HUDJoystickTarget GetFirstAvailableTarget()
	{
		for (int i = 0; i < targets.Length; i++)
		{
			HUDJoystickTarget hUDJoystickTarget = targets[i];
			if (hUDJoystickTarget.IsSelectable())
			{
				return hUDJoystickTarget;
			}
		}
		return null;
	}
}
