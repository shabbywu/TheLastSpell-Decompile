using System;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

[DisallowMultipleComponent]
public abstract class HUDJoystickTarget : MonoBehaviour
{
	[Serializable]
	public struct Navigation
	{
		public HUDJoystickTarget Up;

		public HUDJoystickTarget UpRight;

		public HUDJoystickTarget UpLeft;

		public HUDJoystickTarget Down;

		public HUDJoystickTarget DownRight;

		public HUDJoystickTarget DownLeft;

		public HUDJoystickTarget Left;

		public HUDJoystickTarget Right;
	}

	public struct SelectionInfo
	{
		public HUDJoystickTarget HUDTarget;

		public Selectable Selectable;

		public static SelectionInfo Empty => default(SelectionInfo);
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	public bool NavigationEnabled = true;

	[SerializeField]
	protected Navigation navigation;

	[SerializeField]
	private UnityEvent onSelect;

	[SerializeField]
	private UnityEvent onDeselect;

	public abstract SelectionInfo GetSelectionInfo(Vector2? direction = null);

	public virtual bool IsSelectable()
	{
		if (NavigationEnabled && ((Component)this).gameObject.activeInHierarchy)
		{
			if (!((Object)(object)canvas == (Object)null))
			{
				return ((Behaviour)canvas).enabled;
			}
			return true;
		}
		return false;
	}

	public void RaiseSelectEvent()
	{
		UnityEvent obj = onSelect;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	public void RaiseDeselectEvent()
	{
		UnityEvent obj = onDeselect;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	public HUDJoystickTarget GetNextPanelForDirection(Vector2 direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt((Vector2.SignedAngle(direction, Vector2.down) + 180f + 22.5f) / 45f) % 8;
		bool flag = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
		HUDJoystickTarget hUDJoystickTarget = num switch
		{
			0 => ((Object)(object)navigation.Up != (Object)null) ? navigation.Up : ((direction.x > 0f) ? navigation.UpRight : navigation.UpLeft), 
			1 => ((Object)(object)navigation.UpRight != (Object)null) ? navigation.UpRight : (flag ? navigation.Right : navigation.Up), 
			2 => ((Object)(object)navigation.Right != (Object)null) ? navigation.Right : ((direction.y > 0f) ? navigation.UpRight : navigation.DownRight), 
			3 => ((Object)(object)navigation.DownRight != (Object)null) ? navigation.DownRight : (flag ? navigation.Right : navigation.Down), 
			4 => ((Object)(object)navigation.Down != (Object)null) ? navigation.Down : ((direction.x > 0f) ? navigation.DownRight : navigation.DownLeft), 
			5 => ((Object)(object)navigation.DownLeft != (Object)null) ? navigation.DownLeft : (flag ? navigation.Left : navigation.Down), 
			6 => ((Object)(object)navigation.Left != (Object)null) ? navigation.Left : ((direction.y > 0f) ? navigation.UpLeft : navigation.DownLeft), 
			7 => ((Object)(object)navigation.UpLeft != (Object)null) ? navigation.UpLeft : (flag ? navigation.Left : navigation.Up), 
			_ => null, 
		};
		if ((Object)(object)hUDJoystickTarget != (Object)null && !hUDJoystickTarget.IsSelectable())
		{
			return hUDJoystickTarget.GetNextPanelForDirection(direction);
		}
		return hUDJoystickTarget;
	}

	public void LogTargetIsNotSelectable()
	{
		if (!NavigationEnabled)
		{
			((CLogger<HUDJoystickNavigationManager>)TPSingleton<HUDJoystickNavigationManager>.Instance).Log((object)"The navigation of the target is not enabled !", (CLogLevel)0, true, false);
		}
		else if (!((Component)this).gameObject.activeInHierarchy)
		{
			((CLogger<HUDJoystickNavigationManager>)TPSingleton<HUDJoystickNavigationManager>.Instance).Log((object)"The gameObject of the target is not active in the hierarchy !", (CLogLevel)0, true, false);
		}
		else if ((Object)(object)canvas == (Object)null || ((Behaviour)canvas).enabled)
		{
			((CLogger<HUDJoystickNavigationManager>)TPSingleton<HUDJoystickNavigationManager>.Instance).Log((object)"The canvas of the target is null or not enabled !", (CLogLevel)0, true, false);
		}
	}
}
