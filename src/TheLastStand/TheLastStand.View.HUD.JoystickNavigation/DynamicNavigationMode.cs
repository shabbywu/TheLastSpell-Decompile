using Rewired;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.JoystickNavigation;

public class DynamicNavigationMode : MonoBehaviour
{
	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private Mode joystickNavigationMode = (Mode)4;

	public void RefreshNavigationMode(ControllerType controllerType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		RefreshNavigationMode((int)controllerType == 2);
	}

	public unsafe void RefreshNavigationMode(bool usingJoystick)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a->IL001a: Incompatible stack types: Ref vs I4
		//IL_0014->IL001a: Incompatible stack types: I4 vs Ref
		//IL_0014->IL001a: Incompatible stack types: Ref vs I4
		Navigation navigation = selectable.navigation;
		ref Navigation reference = ref navigation;
		int num;
		if (usingJoystick)
		{
			reference = ref *(Navigation*)joystickNavigationMode;
			num = (int)(ref reference);
		}
		else
		{
			num = 0;
			reference = ref *(Navigation*)num;
			num = (int)(ref reference);
		}
		((Navigation)num).mode = (Mode)(ref reference);
		selectable.navigation = navigation;
	}

	private void OnEnable()
	{
		TPSingleton<InputManager>.Instance.LastActiveControllerChanged += RefreshNavigationMode;
		RefreshNavigationMode(InputManager.IsLastControllerJoystick);
	}

	private void OnDisable()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.LastActiveControllerChanged -= RefreshNavigationMode;
		}
	}

	private void Reset()
	{
		selectable = ((Component)this).GetComponent<Selectable>();
	}
}
