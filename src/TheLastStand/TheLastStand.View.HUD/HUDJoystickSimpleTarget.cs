using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class HUDJoystickSimpleTarget : HUDJoystickTarget
{
	[SerializeField]
	[Tooltip("All selectables belonging to the HUD target. First available will be selected on HUD target selected.")]
	private List<Selectable> selectables;

	public override bool IsSelectable()
	{
		if (base.IsSelectable())
		{
			return (Object)(object)GetFirstAvailableSelectable() != (Object)null;
		}
		return false;
	}

	public override SelectionInfo GetSelectionInfo(Vector2? direction = null)
	{
		if (!IsSelectable())
		{
			LogSelectionNotSelectable();
			return SelectionInfo.Empty;
		}
		Selectable firstAvailableSelectable = GetFirstAvailableSelectable();
		if ((Object)(object)firstAvailableSelectable == (Object)null)
		{
			((CLogger<UIManager>)TPSingleton<UIManager>.Instance).LogWarning((object)("No Selectable has been found for selection on " + ((Object)((Component)this).transform).name + "."), (Object)(object)((Component)this).gameObject, (CLogLevel)1, true, false);
			return SelectionInfo.Empty;
		}
		SelectionInfo result = default(SelectionInfo);
		result.HUDTarget = this;
		result.Selectable = firstAvailableSelectable;
		return result;
	}

	public void ClearSelectables()
	{
		foreach (Selectable selectable in selectables)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.UnregisterSelectable(selectable);
		}
		selectables.Clear();
	}

	public void ClearUnavailableNavigations()
	{
		if ((Object)(object)navigation.Up != (Object)null && !navigation.Up.IsSelectable())
		{
			navigation.Up = null;
		}
		if ((Object)(object)navigation.UpRight != (Object)null && !navigation.UpRight.IsSelectable())
		{
			navigation.UpRight = null;
		}
		if ((Object)(object)navigation.UpLeft != (Object)null && !navigation.UpLeft.IsSelectable())
		{
			navigation.UpLeft = null;
		}
		if ((Object)(object)navigation.Down != (Object)null && !navigation.Down.IsSelectable())
		{
			navigation.Down = null;
		}
		if ((Object)(object)navigation.DownRight != (Object)null && !navigation.DownRight.IsSelectable())
		{
			navigation.DownRight = null;
		}
		if ((Object)(object)navigation.DownLeft != (Object)null && !navigation.DownLeft.IsSelectable())
		{
			navigation.DownLeft = null;
		}
		if ((Object)(object)navigation.Left != (Object)null && !navigation.Left.IsSelectable())
		{
			navigation.Left = null;
		}
		if ((Object)(object)navigation.Right != (Object)null && !navigation.Right.IsSelectable())
		{
			navigation.Right = null;
		}
	}

	public void AddSelectable(Selectable selectable)
	{
		selectables.Add(selectable);
		TPSingleton<HUDJoystickNavigationManager>.Instance.RegisterSelectable(selectable, this);
	}

	public void AddSelectables(IEnumerable<Selectable> selectables)
	{
		foreach (Selectable selectable in selectables)
		{
			AddSelectable(selectable);
			TPSingleton<HUDJoystickNavigationManager>.Instance.RegisterSelectable(selectable, this);
		}
	}

	public void ClearMissingSelectables()
	{
		for (int num = selectables.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)selectables[num] == (Object)null)
			{
				selectables.RemoveAt(num);
			}
		}
	}

	private Selectable GetFirstAvailableSelectable()
	{
		for (int i = 0; i < selectables.Count; i++)
		{
			Selectable val = selectables[i];
			if (!((Object)(object)val == (Object)null) && val.IsInteractable() && ((Component)val).gameObject.activeInHierarchy)
			{
				return val;
			}
		}
		return null;
	}

	private void LogSelectionNotSelectable()
	{
		((CLogger<HUDJoystickNavigationManager>)TPSingleton<HUDJoystickNavigationManager>.Instance).Log((object)("Joystick SelectionInfo of " + ((Object)((Component)this).gameObject).name + " isn't selectable !"), (CLogLevel)0, true, false);
		if (!base.IsSelectable())
		{
			LogTargetIsNotSelectable();
		}
		else if (selectables.Count == 0)
		{
			((CLogger<HUDJoystickNavigationManager>)TPSingleton<HUDJoystickNavigationManager>.Instance).Log((object)"The list of selectable is empty !", (CLogLevel)0, true, false);
		}
	}

	private void Awake()
	{
		for (int i = 0; i < selectables.Count; i++)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.RegisterSelectable(selectables[i], this);
		}
	}

	private void OnDestroy()
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Exist())
		{
			for (int i = 0; i < selectables.Count; i++)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.UnregisterSelectable(selectables[i]);
			}
		}
	}
}
