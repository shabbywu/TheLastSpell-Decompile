using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class ResolutionDropdownPanel : DropdownPanel
{
	private Resolution resolution;

	public override void OnDropdownValueChange()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		base.OnDropdownValueChange();
		for (int num = SettingsManager.SupportedResolutions.Length - 1; num >= 0; num--)
		{
			Resolution val = SettingsManager.SupportedResolutions[num];
			string text = $"{((Resolution)(ref val)).width}x{((Resolution)(ref val)).height} {((Resolution)(ref val)).refreshRate}Hz";
			if (base.OptionKeys[dropdown.value] == text)
			{
				resolution = val;
				SettingsController.SetResolution(resolution);
				break;
			}
		}
	}

	protected override void InitializeOptionKeys()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		resolution = TPSingleton<SettingsManager>.Instance.Settings.Resolution;
		int num = SettingsManager.SupportedResolutions.Length;
		dropdown.options = new List<OptionData>(num);
		int num2 = -1;
		for (int i = 0; i < num; i++)
		{
			Resolution val = SettingsManager.SupportedResolutions[i];
			dropdown.options.Add(new OptionData($"{((Resolution)(ref val)).width}x{((Resolution)(ref val)).height} {((Resolution)(ref val)).refreshRate}Hz"));
			if (num2 == -1 && ((Resolution)(ref val)).width == ((Resolution)(ref resolution)).width && ((Resolution)(ref val)).height == ((Resolution)(ref resolution)).height && ((Resolution)(ref val)).refreshRate <= ((Resolution)(ref resolution)).refreshRate)
			{
				num2 = i;
			}
		}
		if (num2 == -1)
		{
			((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).LogWarning((object)$"No valid resolution has been found in dropdown options for screen resolution {((Resolution)(ref resolution)).width}x{((Resolution)(ref resolution)).height} {((Resolution)(ref resolution)).refreshRate}Hz.", (CLogLevel)0, true, false);
			num2 = 0;
		}
		base.InitializeOptionKeys();
		dropdown.value = num2;
	}

	public void DebugInitializeOptionKeys()
	{
		InitializeOptionKeys();
	}
}
