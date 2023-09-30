using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class RestrictedCursorPanel : SettingsFieldPanel
{
	[SerializeField]
	private BetterToggle restrictedCursorToggle;

	public void OnValueChanged()
	{
		SettingsController.SetRestrictedCursor(((Toggle)restrictedCursorToggle).isOn);
	}

	public override void Refresh()
	{
		base.Refresh();
		((Toggle)restrictedCursorToggle).isOn = TPSingleton<SettingsManager>.Instance.Settings.IsCursorRestricted;
	}

	protected override void RefreshLocalizedTexts()
	{
		((TMP_Text)labelText).text = Localizer.Get("Settings_RestrictedCursor");
	}

	protected override void Awake()
	{
		base.Awake();
		Refresh();
	}
}
