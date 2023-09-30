using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class SmartCastPanel : SettingsFieldPanel
{
	[SerializeField]
	private Toggle toggle;

	public override void Refresh()
	{
		base.Refresh();
		toggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.SmartCast;
	}

	protected override void RefreshLocalizedTexts()
	{
		((TMP_Text)labelText).text = Localizer.Get("Settings_SmartCast_Label");
	}

	protected override void Awake()
	{
		base.Awake();
		((UnityEvent<bool>)(object)toggle.onValueChanged).AddListener((UnityAction<bool>)OnToggleValueChanged);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		((UnityEvent<bool>)(object)toggle.onValueChanged).RemoveListener((UnityAction<bool>)OnToggleValueChanged);
	}

	private void OnToggleValueChanged(bool value)
	{
		SettingsController.SetSmartCast(value);
	}
}
