using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class VolumesPanel : MonoBehaviour
{
	public static class Consts
	{
		public static class LocalizationKeys
		{
			public const string AmbientVolume = "Settings_AmbientVolume";

			public const string MasterVolume = "Settings_MasterVolume";

			public const string MusicVolume = "Settings_MusicVolume";

			public const string UIVolume = "Settings_UIVolume";
		}
	}

	[SerializeField]
	private TextMeshProUGUI masterVolumeLabelText;

	[SerializeField]
	private VolumeSlider masterVolumeSlider;

	[SerializeField]
	private TextMeshProUGUI musicVolumeLabelText;

	[SerializeField]
	private VolumeSlider musicVolumeSlider;

	[SerializeField]
	private TextMeshProUGUI uiVolumeLabelText;

	[SerializeField]
	private VolumeSlider uiVolumeSlider;

	[SerializeField]
	private TextMeshProUGUI ambientVolumeLabelText;

	[SerializeField]
	private VolumeSlider ambientVolumeSlider;

	public void Refresh()
	{
		masterVolumeSlider.RefreshVolume(TPSingleton<SettingsManager>.Instance.Settings.MasterVolume);
		musicVolumeSlider.RefreshVolume(TPSingleton<SettingsManager>.Instance.Settings.MusicVolume);
		uiVolumeSlider.RefreshVolume(TPSingleton<SettingsManager>.Instance.Settings.UiVolume);
		ambientVolumeSlider.RefreshVolume(TPSingleton<SettingsManager>.Instance.Settings.AmbientVolume);
		RefreshLocalizedTexts();
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		RefreshLocalizedTexts();
	}

	private void RefreshLocalizedTexts()
	{
		((TMP_Text)masterVolumeLabelText).text = Localizer.Get("Settings_MasterVolume");
		((TMP_Text)musicVolumeLabelText).text = Localizer.Get("Settings_MusicVolume");
		((TMP_Text)uiVolumeLabelText).text = Localizer.Get("Settings_UIVolume");
		((TMP_Text)ambientVolumeLabelText).text = Localizer.Get("Settings_AmbientVolume");
	}
}
