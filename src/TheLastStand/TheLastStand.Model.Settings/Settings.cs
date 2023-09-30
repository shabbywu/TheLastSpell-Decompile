using System;
using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.Model.Settings;

public class Settings
{
	public static class Consts
	{
		public static class Values
		{
			public static class GameAcceleration
			{
				public const float DefaultValue = 5f;
			}
		}
	}

	private bool edgePan;

	private bool edgePanOverUI;

	public SettingsController SettingsController { get; }

	public bool AlwaysDisplayMaxStatValue { get; set; }

	public bool AlwaysDisplayUnitPortraitAttribute { get; set; }

	public float AmbientVolume { get; set; }

	public bool EdgePan
	{
		get
		{
			return edgePan;
		}
		set
		{
			edgePan = value;
			if (TPSingleton<ACameraView>.Exist() && (Object)(object)TPSingleton<ACameraView>.Instance.CamPanner != (Object)null)
			{
				TPSingleton<ACameraView>.Instance.CamPanner.UsePanByMoveToEdges = value;
			}
		}
	}

	public bool EdgePanOverUI
	{
		get
		{
			return edgePanOverUI;
		}
		set
		{
			edgePanOverUI = value;
			if (TPSingleton<ACameraView>.Exist() && (Object)(object)TPSingleton<ACameraView>.Instance.CamPanner != (Object)null)
			{
				TPSingleton<ACameraView>.Instance.CamPanner.IgnoreUIOnEdges = value;
			}
		}
	}

	public bool FocusCamOnSelections { get; set; }

	public bool HideCompendium { get; set; }

	public bool SmartCast { get; set; }

	public bool[] EndTurnWarnings { get; set; } = new bool[Enum.GetNames(typeof(SettingsManager.E_EndTurnWarning)).Length];


	public int FrameRateCap { get; set; }

	public bool IsCursorRestricted { get; set; } = true;


	public SettingsManager.E_KeyboardLayout KeyboardLayout { get; set; }

	public SettingsManager.E_InputDeviceType InputDeviceType { get; set; }

	public string LanguageAfterRestart { get; set; } = string.Empty;


	public float MasterVolume { get; set; }

	public int MonitorIndex { get; set; }

	public float MusicVolume { get; set; }

	public Resolution Resolution { get; set; }

	public bool RunInBackground { get; set; }

	public float ScreenShakesValue { get; set; }

	public bool ShowSkillsHotkeys { get; set; }

	public SettingsManager.E_SpeedMode SpeedMode { get; set; }

	public float SpeedScale { get; set; }

	public float UiSizeScale { get; set; }

	public float UiVolume { get; set; }

	public bool UseVSync { get; set; }

	public SettingsManager.E_WindowMode WindowMode { get; set; }

	public Settings(SettingsController settingsController)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Resolution val = default(Resolution);
		((Resolution)(ref val)).width = 1280;
		((Resolution)(ref val)).height = 720;
		Resolution = val;
		RunInBackground = true;
		ScreenShakesValue = 1f;
		ShowSkillsHotkeys = true;
		SpeedScale = 5f;
		UiSizeScale = 1f;
		WindowMode = SettingsManager.E_WindowMode.BorderlessWindow;
		base._002Ector();
		SettingsController = settingsController;
	}
}
