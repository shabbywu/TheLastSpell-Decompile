using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class VSyncPanel : MonoBehaviour
{
	public static class Consts
	{
		public static class LocalizationKeys
		{
			public const string VSync = "Settings_VSync";
		}
	}

	[SerializeField]
	private TextMeshProUGUI vSyncLabelText;

	[SerializeField]
	private BetterToggle vSyncToggle;

	public void OnValueChanged()
	{
		SettingsController.SetVSync(((Toggle)vSyncToggle).isOn);
		TPSingleton<SettingsManager>.Instance.SettingsPanel.RefreshFrameRateCap();
	}

	public void Refresh()
	{
		((Toggle)vSyncToggle).isOn = TPSingleton<SettingsManager>.Instance.Settings.UseVSync;
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
		((TMP_Text)vSyncLabelText).text = Localizer.Get("Settings_VSync");
	}
}
