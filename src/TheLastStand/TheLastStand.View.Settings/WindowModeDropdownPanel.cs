using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;

namespace TheLastStand.View.Settings;

public class WindowModeDropdownPanel : DropdownPanel
{
	private const int WindowModesCount = 3;

	private SettingsManager.E_WindowMode windowMode;

	public override void OnDropdownValueChange()
	{
		base.OnDropdownValueChange();
		windowMode = (SettingsManager.E_WindowMode)dropdown.value;
		SettingsController.SetWindowMode(windowMode);
	}

	protected override void InitializeOptionKeys()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		windowMode = TPSingleton<SettingsManager>.Instance.Settings.WindowMode;
		dropdown.options = new List<OptionData>(3);
		for (int i = 0; i < 3; i++)
		{
			dropdown.options.Add(new OptionData($"WindowMode_{(SettingsManager.E_WindowMode)i}"));
		}
		base.InitializeOptionKeys();
		dropdown.value = (int)windowMode;
	}

	public override void Refresh()
	{
		windowMode = TPSingleton<SettingsManager>.Instance.Settings.WindowMode;
		dropdown.value = (int)windowMode;
		base.Refresh();
	}
}
