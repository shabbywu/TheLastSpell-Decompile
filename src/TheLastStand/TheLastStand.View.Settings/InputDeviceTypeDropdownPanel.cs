using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.View.Settings;

public class InputDeviceTypeDropdownPanel : DropdownPanel
{
	private const int InputDeviceTypesCount = 3;

	public override void OnDropdownValueChange()
	{
		base.OnDropdownValueChange();
		SettingsManager.SetInputDeviceType((SettingsManager.E_InputDeviceType)dropdown.value);
	}

	protected override void InitializeOptionKeys()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		dropdown.options = new List<OptionData>(3);
		int valueWithoutNotify = 0;
		for (int i = 0; i < 3; i++)
		{
			string text = $"DeviceType_{(SettingsManager.E_InputDeviceType)i}";
			dropdown.options.Add(new OptionData(text));
			if (i == (int)TPSingleton<SettingsManager>.Instance.Settings.InputDeviceType)
			{
				valueWithoutNotify = i;
			}
		}
		base.InitializeOptionKeys();
		dropdown.SetValueWithoutNotify(valueWithoutNotify);
	}
}
