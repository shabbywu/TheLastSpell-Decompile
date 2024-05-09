using Rewired;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

[DisallowMultipleComponent]
public class GamepadInputDisplay : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private GamepadButtonsSet gamepadButtonsSet;

	public void Display(bool show)
	{
		((Behaviour)image).enabled = show;
	}

	private void Reset()
	{
		if ((Object)(object)image == (Object)null)
		{
			image = ((Component)this).GetComponent<Image>();
		}
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)controllerType > 1)
		{
			if ((int)controllerType == 2)
			{
				UpdateIcon(gamepadButtonsSet);
				Display(show: true);
				return;
			}
			if ((int)controllerType == 20)
			{
			}
		}
		Display(show: false);
	}

	private void OnInputDeviceTypeChanged(SettingsManager.E_InputDeviceType inputDeviceType)
	{
		switch (inputDeviceType)
		{
		case SettingsManager.E_InputDeviceType.MouseKeyboard:
			Display(show: false);
			break;
		case SettingsManager.E_InputDeviceType.Controller:
			Display(show: true);
			break;
		case SettingsManager.E_InputDeviceType.Auto:
			break;
		}
	}

	private void OnEnable()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		TPSingleton<SettingsManager>.Instance.OnInputDeviceTypeChangeEvent += OnInputDeviceTypeChanged;
		OnLastActiveControllerChanged(InputManager.GetLastControllerType());
	}

	private void OnDisable()
	{
		InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnInputDeviceTypeChangeEvent -= OnInputDeviceTypeChanged;
		}
	}

	private void UpdateIcon(GamepadButtonsSet buttonsSet)
	{
		Sprite icon = buttonsSet.GetIcon();
		image.sprite = icon;
		((Graphic)image).SetNativeSize();
	}
}
