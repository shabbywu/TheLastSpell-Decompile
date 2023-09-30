using System;
using TMPro;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class ScreenSettingsPanel : MonoBehaviour
{
	public static class Consts
	{
		public static class LocalizationKeys
		{
			public const string NotImplementedYet = "Settings_NotImplementedYet";

			public const string Resolution = "Settings_Resolution";

			public const string DisplayTitle = "Settings_DisplayTitle";

			public const string DisplayOption = "Settings_DisplayOption";
		}
	}

	[SerializeField]
	private TextMeshProUGUI resolutionLabelText;

	[SerializeField]
	private TextMeshProUGUI displayLabelText;

	[SerializeField]
	private ResolutionDropdownPanel resolutionDropdownPanel;

	[SerializeField]
	private WindowModeDropdownPanel windowModeDropdownPanel;

	[SerializeField]
	private MonitorIndexDropdownPanel monitorIndexDropdownPanel;

	[SerializeField]
	private VSyncPanel vSyncPanel;

	[SerializeField]
	private FrameRateCapPanel frameRateCapPanel;

	[SerializeField]
	private TextMeshProUGUI[] notImplementedLabelTexts;

	public FrameRateCapPanel FrameRateCapPanel => frameRateCapPanel;

	public void Refresh()
	{
		RefreshLocalizedTexts();
		resolutionDropdownPanel.Refresh();
		windowModeDropdownPanel.Refresh();
		monitorIndexDropdownPanel.Refresh();
		vSyncPanel.Refresh();
		frameRateCapPanel.Refresh();
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
		((TMP_Text)resolutionLabelText).text = Localizer.Get("Settings_Resolution");
		if ((Object)(object)displayLabelText != (Object)null)
		{
			((TMP_Text)displayLabelText).text = Localizer.Get("Settings_DisplayTitle");
		}
		if (notImplementedLabelTexts != null)
		{
			for (int num = notImplementedLabelTexts.Length - 1; num >= 0; num--)
			{
				((TMP_Text)notImplementedLabelTexts[num]).text = Localizer.Get("Settings_NotImplementedYet");
			}
		}
		FrameRateCapPanel.RefreshLocalizedTexts();
	}
}
