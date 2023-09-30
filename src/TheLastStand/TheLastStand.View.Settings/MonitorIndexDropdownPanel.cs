using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class MonitorIndexDropdownPanel : DropdownPanel
{
	public static class Constants
	{
		public const string DisplayChangedPopupTitle = "Settings_DisplayChangedPopupTitle";

		public const string DisplayChangedPopupContent = "Settings_DisplayChangedPopupContent";
	}

	private int monitorIndex;

	private bool canTriggerPopup;

	public override void OnDropdownValueChange()
	{
		base.OnDropdownValueChange();
		if (!canTriggerPopup)
		{
			canTriggerPopup = true;
		}
		else
		{
			GenericPopUp.Open("Settings_DisplayChangedPopupTitle", "Settings_DisplayChangedPopupContent");
		}
		monitorIndex = dropdown.value;
		SettingsController.SetMonitorIndex(monitorIndex);
	}

	protected override void InitializeOptionKeys()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		monitorIndex = TPSingleton<SettingsManager>.Instance.Settings.MonitorIndex;
		int num = Display.displays.Length;
		dropdown.options = new List<OptionData>(num);
		for (int i = 0; i < num; i++)
		{
			dropdown.options.Add(new OptionData("Settings_DisplayOption"));
		}
		base.InitializeOptionKeys();
		dropdown.value = monitorIndex;
	}

	public override void Refresh()
	{
		monitorIndex = TPSingleton<SettingsManager>.Instance.Settings.MonitorIndex;
		dropdown.value = monitorIndex;
		base.Refresh();
	}

	protected override void RefreshOptionTexts()
	{
		if (optionsCount != 0)
		{
			for (int i = 0; i < optionsCount; i++)
			{
				dropdown.options[i].text = (useLocalization ? Localizer.Format(base.OptionKeys[i], new object[1] { i + 1 }) : base.OptionKeys[i]);
			}
			dropdown.RefreshShownValue();
		}
	}
}
