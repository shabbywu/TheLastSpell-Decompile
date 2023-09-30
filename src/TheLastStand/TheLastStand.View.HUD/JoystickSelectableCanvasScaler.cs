using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Helpers;
using TheLastStand.Manager;
using TheLastStand.View.MetaShops;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class JoystickSelectableCanvasScaler : MonoBehaviour
{
	[SerializeField]
	private CanvasScaler canvasScaler;

	public void UpdateScale(float scale)
	{
		if (ApplicationManager.CurrentStateName == "WorldMap" || ApplicationManager.CurrentStateName == "MetaShops" || TPSingleton<OraculumView>.Instance.Displayed)
		{
			CanvasHelper.ScaleCanvas(canvasScaler, allowDecimals: false);
		}
		else
		{
			canvasScaler.scaleFactor = scale;
		}
	}

	private void Awake()
	{
		((UnityEvent<float>)(object)TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent).AddListener((UnityAction<float>)UpdateScale);
		ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
		UpdateScale(TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale);
	}

	private void OnApplicationStateChange(State state)
	{
		switch (state.GetName())
		{
		case "MetaShops":
		case "WorldMap":
			UpdateScale(TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale);
			break;
		}
	}

	private void OnDestroy()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			((UnityEvent<float>)(object)TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent).RemoveListener((UnityAction<float>)UpdateScale);
		}
		if (TPSingleton<ApplicationManager>.Exist())
		{
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent -= OnApplicationStateChange;
		}
	}
}
