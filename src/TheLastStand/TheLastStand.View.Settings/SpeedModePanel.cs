using System;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.View.Settings;

public class SpeedModePanel : SettingsFieldPanel
{
	[SerializeField]
	private TMP_Dropdown dropdown;

	public override void Refresh()
	{
		base.Refresh();
		dropdown.SetValueWithoutNotify((int)TPSingleton<SettingsManager>.Instance.Settings.SpeedMode);
	}

	protected override void RefreshLocalizedTexts()
	{
		((TMP_Text)labelText).text = Localizer.Get("Settings_SpeedMode_Label");
	}

	protected override void Awake()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		base.Awake();
		Array values = Enum.GetValues(typeof(SettingsManager.E_SpeedMode));
		List<OptionData> list = new List<OptionData>();
		foreach (int item in values)
		{
			list.Add(new OptionData($"Settings_SpeedMode_{(SettingsManager.E_SpeedMode)item}"));
		}
		dropdown.AddOptions(list);
		((UnityEvent<int>)(object)dropdown.onValueChanged).AddListener((UnityAction<int>)OnDropdownValueChanged);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		((UnityEvent<int>)(object)dropdown.onValueChanged).RemoveListener((UnityAction<int>)OnDropdownValueChanged);
	}

	private void OnDropdownValueChanged(int optionIndex)
	{
		SettingsController.SetSpeedMode((SettingsManager.E_SpeedMode)optionIndex);
	}
}
