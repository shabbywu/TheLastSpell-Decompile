using Rewired;
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

	public void RefreshNavigationMode(bool usingJoystick)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = selectable.navigation;
		((Navigation)(ref navigation)).mode = (Mode)(usingJoystick ? ((int)joystickNavigationMode) : 0);
		selectable.navigation = navigation;
	}

	private void OnEnable()
	{
		InputManager.LastActiveControllerChanged += RefreshNavigationMode;
		RefreshNavigationMode(InputManager.IsLastControllerJoystick);
	}

	private void OnDisable()
	{
		InputManager.LastActiveControllerChanged -= RefreshNavigationMode;
	}

	private void Reset()
	{
		selectable = ((Component)this).GetComponent<Selectable>();
	}
}
