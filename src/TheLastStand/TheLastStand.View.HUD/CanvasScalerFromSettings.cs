using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerFromSettings : MonoBehaviour
{
	private CanvasScaler canvasScaler;

	private void UpdateScale(float scale)
	{
		canvasScaler.scaleFactor = scale;
	}

	private void Awake()
	{
		canvasScaler = ((Component)this).GetComponent<CanvasScaler>();
		((UnityEvent<float>)(object)TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent).AddListener((UnityAction<float>)UpdateScale);
		UpdateScale(TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale);
	}

	private void OnDestroy()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			((UnityEvent<float>)(object)TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent).RemoveListener((UnityAction<float>)UpdateScale);
		}
	}
}
