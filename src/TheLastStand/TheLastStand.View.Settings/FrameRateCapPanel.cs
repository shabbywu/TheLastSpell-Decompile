using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class FrameRateCapPanel : MonoBehaviour
{
	public static class Constants
	{
		public static class LocalizationKeys
		{
			public const string FpsLabel = "Settings_FpsLabel";

			public const string UnlimitedFpsLabel = "Settings_UnlimitedFpsLabel";
		}
	}

	[SerializeField]
	private TMP_Dropdown frameRatesDropdown;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private float lockOpacity = 0.5f;

	[SerializeField]
	private List<int> availableFrameRates = new List<int>();

	public void OnValueChanged()
	{
		SetTargetFrameRate(availableFrameRates[frameRatesDropdown.value]);
	}

	public void Refresh()
	{
		if (availableFrameRates.Contains(TPSingleton<SettingsManager>.Instance.Settings.FrameRateCap))
		{
			frameRatesDropdown.value = availableFrameRates.IndexOf(TPSingleton<SettingsManager>.Instance.Settings.FrameRateCap);
		}
		else
		{
			frameRatesDropdown.value = availableFrameRates.IndexOf(-1);
			SettingsController.SetTargetFrameRate(-1);
		}
		SetInteractable(TPSingleton<SettingsManager>.Instance.Settings.UseVSync);
	}

	public void RefreshLocalizedTexts()
	{
		for (int i = 0; i < frameRatesDropdown.options.Count; i++)
		{
			string text = ((availableFrameRates[i] == -1) ? Localizer.Get("Settings_UnlimitedFpsLabel") : Localizer.Format("Settings_FpsLabel", new object[1] { availableFrameRates[i] }));
			frameRatesDropdown.options[i].text = text;
		}
		frameRatesDropdown.RefreshShownValue();
	}

	private void Awake()
	{
		PopulateDropdown();
	}

	private void PopulateDropdown()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		frameRatesDropdown.ClearOptions();
		List<OptionData> list = new List<OptionData>();
		for (int i = 0; i < availableFrameRates.Count; i++)
		{
			string text = ((availableFrameRates[i] == -1) ? Localizer.Get("Settings_UnlimitedFpsLabel") : Localizer.Format("Settings_FpsLabel", new object[1] { availableFrameRates[i] }));
			list.Add(new OptionData(text));
		}
		frameRatesDropdown.AddOptions(list);
	}

	private void SetInteractable(bool state)
	{
		canvasGroup.alpha = (state ? lockOpacity : 1f);
		((Selectable)frameRatesDropdown).interactable = !state;
	}

	private void SetTargetFrameRate(int frameRate)
	{
		SettingsController.SetTargetFrameRate(frameRate);
	}
}
