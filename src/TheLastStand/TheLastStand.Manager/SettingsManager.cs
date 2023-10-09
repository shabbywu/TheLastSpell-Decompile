using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Modding;
using TheLastStand.Model;
using TheLastStand.Model.Settings;
using TheLastStand.Serialization;
using TheLastStand.View.Generic;
using TheLastStand.View.Menus;
using TheLastStand.View.MetaShops;
using TheLastStand.View.Settings;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Manager;

public class SettingsManager : Manager<SettingsManager>, ISerializable, IDeserializable
{
	public delegate void SettingsDeserialzedHandler();

	public delegate void OnResolutionChange(Resolution resolution);

	public delegate void OnWindowModeChange(E_WindowMode windowMode);

	public delegate void OnInputDeviceTypeChange(E_InputDeviceType inputDeviceType);

	public enum E_KeyboardLayout
	{
		QWERTY,
		AZERTY,
		QWERTZ
	}

	public enum E_WindowMode
	{
		FullScreen,
		BorderlessWindow,
		Windowed
	}

	public enum E_EndTurnWarning
	{
		RESOURCES_UNUSED,
		RESOURCES_THRESHOLD,
		WORKERS_LEFT,
		FREE_ACTIONS_LEFT,
		ACTIONS_POINTS_LEFT,
		UNMOVED_HEROES,
		PERK_POINTS_LEFT
	}

	public enum E_SpeedMode
	{
		Toggle,
		OnPress
	}

	public enum E_InputDeviceType
	{
		Auto,
		MouseKeyboard,
		Controller
	}

	public struct SettingsChangesNoteLocalizationKeys
	{
		public string TitleKey;

		public string TextKey;

		public string[] FormatArgs;
	}

	public static class Constants
	{
		public static class SettingsChangesLocaKeys
		{
			public const string SettingsChangesVersionKeyFormat = "SettingsChangesNotes_SaveVersion{0}";

			public const string SettingsChangesDefault = "SettingsChangesNotes_Default";

			public const string SettingsChangesDefaultKeyRemapping = "SettingsChangesNotes_Default_KeyRemapping";
		}

		public const int ScreenMinWidth = 1024;

		public const int ScreenMinHeight = 720;
	}

	private static bool hasBeenInitialized;

	[SerializeField]
	[Range(0f, 1f)]
	private float masterVolume = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float uiVolume = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float musicVolume = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float ambientVolume = 1f;

	[SerializeField]
	private string[] supportedKeyboardLayouts = new string[3] { "QWERTY", "AZERTY", "QWERTZ" };

	[SerializeField]
	private float gameAccelerationMaxValue = 10f;

	[SerializeField]
	private float gameAccelerationMinValue = 1.5f;

	private FloatEvent ambientVolumeSettingChangeEvent;

	private FloatEvent masterVolumeSettingChangeEvent;

	private FloatEvent musicVolumeSettingChangeEvent;

	private FloatEvent uiScaleSettingChangeEvent;

	private FloatEvent uiVolumeSettingChangeEvent;

	public static float GameAccelerationMaxValue => TPSingleton<SettingsManager>.Instance.gameAccelerationMaxValue;

	public static float GameAccelerationMinValue => TPSingleton<SettingsManager>.Instance.gameAccelerationMinValue;

	public static Resolution[] SupportedResolutions { get; private set; }

	public FloatEvent AmbientVolumeSettingChangeEvent
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			if (ambientVolumeSettingChangeEvent == null)
			{
				ambientVolumeSettingChangeEvent = new FloatEvent();
			}
			return ambientVolumeSettingChangeEvent;
		}
	}

	public bool HasClosedSettingsThisFrame { get; private set; }

	public FloatEvent MasterVolumeSettingChangeEvent
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			if (masterVolumeSettingChangeEvent == null)
			{
				masterVolumeSettingChangeEvent = new FloatEvent();
			}
			return masterVolumeSettingChangeEvent;
		}
	}

	public FloatEvent MusicVolumeSettingChangeEvent
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			if (musicVolumeSettingChangeEvent == null)
			{
				musicVolumeSettingChangeEvent = new FloatEvent();
			}
			return musicVolumeSettingChangeEvent;
		}
	}

	public SettingsPanel SettingsPanel { get; private set; }

	public List<SettingsChangesNoteLocalizationKeys> SettingsChangesWarningsToDisplay { get; } = new List<SettingsChangesNoteLocalizationKeys>();


	public FloatEvent UiScaleSettingChangeEvent
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			if (uiScaleSettingChangeEvent == null)
			{
				uiScaleSettingChangeEvent = new FloatEvent();
			}
			return uiScaleSettingChangeEvent;
		}
	}

	public FloatEvent UiVolumeSettingChangeEvent
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			if (uiVolumeSettingChangeEvent == null)
			{
				uiVolumeSettingChangeEvent = new FloatEvent();
			}
			return uiVolumeSettingChangeEvent;
		}
	}

	public Settings Settings { get; private set; }

	public bool IsTimeScaleAccelerated { get; set; }

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	private float DebugAmbientVolume
	{
		get
		{
			return Settings.AmbientVolume;
		}
		set
		{
			SettingsController.SetAmbientVolume(Mathf.Clamp01(value));
		}
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	private float DebugMasterVolume
	{
		get
		{
			return Settings.MasterVolume;
		}
		set
		{
			SettingsController.SetMasterVolume(Mathf.Clamp01(value));
		}
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	private float DebugMusicVolume
	{
		get
		{
			return Settings.MusicVolume;
		}
		set
		{
			SettingsController.SetMusicVolume(Mathf.Clamp01(value));
		}
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	private float DebugUiVolume
	{
		get
		{
			return Settings.UiVolume;
		}
		set
		{
			SettingsController.SetUIVolume(Mathf.Clamp01(value));
		}
	}

	public event SettingsDeserialzedHandler OnSettingsDeserializedEvent;

	public event OnResolutionChange OnResolutionChangeEvent;

	public event OnWindowModeChange OnWindowModeChangeEvent;

	public event OnInputDeviceTypeChange OnInputDeviceTypeChangeEvent;

	public static bool CanCloseSettings()
	{
		if (ApplicationManager.Application.State is SettingsState && (!TPSingleton<GameManager>.Exist() || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Settings) && !TPSingleton<KeyRemappingManager>.Instance.RemappingInProgress && !TPSingleton<KeyRemappingManager>.Instance.ResetAllInProgress && !GenericPopUp.IsOpen && !GenericConsent.IsWaitingForInput())
		{
			return !GenericBlockingPopup.IsWaitingForInput();
		}
		return false;
	}

	public static bool CanOpenSettings()
	{
		switch (ApplicationManager.Application.State.GetName())
		{
		case "GameLobby":
			if (!GenericPopUp.IsOpen && !GenericConsent.IsWaitingForInput())
			{
				return !GenericBlockingPopup.IsWaitingForInput();
			}
			return false;
		case "Game":
			if ((TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction) && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.EnemyUnits && !TPSingleton<OraculumView>.Instance.Displayed && !TPSingleton<OraculumView>.Instance.OpeningOrClosing && !GenericConsent.IsWaitingForInput())
			{
				return !GenericBlockingPopup.IsWaitingForInput();
			}
			return false;
		default:
			return false;
		}
	}

	public static void CloseSettings(bool backToGame = true)
	{
		if (TPSingleton<SettingsManager>.Instance.SettingsPanel.State == SettingsPanel.E_State.Opened)
		{
			TPSingleton<SettingsManager>.Instance.SettingsPanel.Close(backToGame);
			if (TPSingleton<ToDoListView>.Exist())
			{
				TPSingleton<ToDoListView>.Instance.RefreshToDoList();
			}
			ApplicationManager.Application.ApplicationController.BackToPreviousState();
		}
	}

	public static E_KeyboardLayout GetKeyboardLayoutFromSystemLanguage()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected I4, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected I4, but got Unknown
		SystemLanguage systemLanguage = Application.systemLanguage;
		switch (systemLanguage - 14)
		{
		default:
			switch (systemLanguage - 29)
			{
			case 0:
			case 2:
			case 4:
				break;
			default:
				goto end_IL_000a;
			}
			goto case 1;
		case 0:
			return E_KeyboardLayout.AZERTY;
		case 1:
		case 4:
			return E_KeyboardLayout.QWERTZ;
		case 2:
		case 3:
			break;
			end_IL_000a:
			break;
		}
		return E_KeyboardLayout.QWERTY;
	}

	public static void Load()
	{
		((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).Log((object)"Loading settings.", (CLogLevel)1, false, false);
		SerializedSettings serializedSettings = null;
		if (File.Exists(SaveManager.SettingsFilePath))
		{
			try
			{
				serializedSettings = SaverLoader.Load<SerializedSettings>(SaveManager.SettingsFilePath, useEncryption: false);
				((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).Log((object)$"Settings file save version : {serializedSettings.SaveVersion}", (CLogLevel)2, true, false);
				if (serializedSettings.SaveVersion < SaveManager.MinimumSupportedSettingsSaveVersion)
				{
					throw new SaverLoader.WrongSaveVersionException(SaveManager.SettingsFilePath, shouldMarkAsCorrupted: true);
				}
			}
			catch (SaverLoader.WrongSaveVersionException ex)
			{
				((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).LogError((object)(ex.FilePath + ": Loading failed because of a wrong version"), (CLogLevel)2, true, false);
				serializedSettings = null;
			}
			catch (SaverLoader.SaveLoadingFailedException ex2)
			{
				((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).LogError((object)("Error while trying to load settings:\n" + ex2.Message), (CLogLevel)2, true, true);
			}
			catch (Exception ex3)
			{
				((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).LogError((object)("Unknown error while trying to load settings:\n" + ex3.Message), (CLogLevel)2, true, true);
			}
		}
		InputManager.LoadMap();
		TPSingleton<SettingsManager>.Instance.Deserialize(serializedSettings, ((int?)serializedSettings?.SaveVersion) ?? (-1));
		((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).Log((object)"Settings loaded!", (CLogLevel)1, false, false);
	}

	public static void OpenSettings()
	{
		TPSingleton<SettingsManager>.Instance.SettingsPanel.Open();
	}

	public static void Save()
	{
		SaverLoader.EnqueueSave(E_SaveType.Settings);
		InputManager.SaveMap();
	}

	public static void SetInputDeviceType(E_InputDeviceType inputDeviceType)
	{
		TPSingleton<SettingsManager>.Instance.Settings.InputDeviceType = inputDeviceType;
		TPSingleton<SettingsManager>.Instance.OnInputDeviceTypeChangeEvent?.Invoke(TPSingleton<SettingsManager>.Instance.Settings.InputDeviceType);
		InputManager.OnInputDeviceTypeChanged(inputDeviceType);
	}

	public void Init()
	{
		Load();
		Save();
		hasBeenInitialized = true;
		this.OnSettingsDeserializedEvent?.Invoke();
	}

	public IEnumerator WaitForFullScreenModeChange()
	{
		FullScreenMode targetFullScreenMode = (FullScreenMode)(Settings.WindowMode switch
		{
			E_WindowMode.Windowed => 3, 
			E_WindowMode.FullScreen => 0, 
			E_WindowMode.BorderlessWindow => 1, 
			_ => 3, 
		});
		while (Screen.fullScreenMode != targetFullScreenMode)
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		this.OnWindowModeChangeEvent?.Invoke(Settings.WindowMode);
		Cursor.lockState = (CursorLockMode)(Settings.IsCursorRestricted ? 2 : 0);
	}

	public IEnumerator WaitForResolutionChange()
	{
		while (true)
		{
			int width = Screen.width;
			Resolution resolution = Settings.Resolution;
			if (width == ((Resolution)(ref resolution)).width)
			{
				break;
			}
			yield return SharedYields.WaitForEndOfFrame;
		}
		this.OnResolutionChangeEvent?.Invoke(Settings.Resolution);
		Cursor.lockState = (CursorLockMode)(Settings.IsCursorRestricted ? 2 : 0);
	}

	protected override void Awake()
	{
		base.Awake();
		if (((TPSingleton<SettingsManager>)(object)this)._IsValid)
		{
			Settings = new SettingsController().Settings;
			SceneManager.sceneLoaded += OnSceneLoaded;
			SaverLoader.RegisterUnkownXMLElementHandler<SerializedSettings>(SerializedSettings.HandleUnknownXMLElement);
			if (!hasBeenInitialized && !TPSingleton<ModManager>.Exist())
			{
				Init();
			}
		}
	}

	protected override void OnDestroy()
	{
		((CLogger<SettingsManager>)this).OnDestroy();
		if (((TPSingleton<SettingsManager>)(object)this)._IsValid)
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}

	private void CheckWarningsToDisplay(int oldestVersion, int currentVersion)
	{
		if (oldestVersion < 7 && currentVersion >= 7)
		{
			string text = $"SettingsChangesNotes_SaveVersion{7}";
			string[] localizedHotkeysForAction = InputManager.GetLocalizedHotkeysForAction("ConstructionDefensive");
			string[] localizedHotkeysForAction2 = InputManager.GetLocalizedHotkeysForAction("ConstructionProduction");
			SettingsChangesWarningsToDisplay.Add(new SettingsChangesNoteLocalizationKeys
			{
				TitleKey = "KeyRemappingChanges_Disclaimer_Title",
				TextKey = (Localizer.Exists(text) ? text : "SettingsChangesNotes_Default_KeyRemapping"),
				FormatArgs = new string[2]
				{
					(localizedHotkeysForAction != null) ? localizedHotkeysForAction[0] : "NA",
					(localizedHotkeysForAction2 != null) ? localizedHotkeysForAction2[0] : "NA"
				}
			});
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SettingsPanel = Object.FindObjectOfType<SettingsPanel>();
	}

	private void Update()
	{
		HasClosedSettingsThisFrame = false;
		if (!TPSingleton<InputManager>.Exist())
		{
			return;
		}
		if (InputManager.GetButtonDown(29) && CanCloseSettings())
		{
			HasClosedSettingsThisFrame = true;
			CloseSettings();
		}
		else if (InputManager.GetButtonDown(54) && CanOpenSettings())
		{
			ApplicationManager.Application.ApplicationController.SetState("Settings");
		}
		else if (InputManager.GetButtonDown(55))
		{
			SettingsController.SetWindowMode((Settings.WindowMode != E_WindowMode.Windowed) ? E_WindowMode.Windowed : E_WindowMode.FullScreen);
			if (TPSingleton<SettingsManager>.Instance.SettingsPanel.State == SettingsPanel.E_State.Opened)
			{
				TPSingleton<SettingsManager>.Instance.SettingsPanel.Refresh();
			}
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedSettings
		{
			AlwaysDisplayMaxStatValue = Settings.AlwaysDisplayMaxStatValue,
			EdgePan = Settings.EdgePan,
			EdgePanOverUI = Settings.EdgePanOverUI,
			FocusCamOnSelections = Settings.FocusCamOnSelections,
			ScreenSettings = SerializeScreenSettings(),
			SoundSettings = SerializeSoundSettings(),
			Language = ((Settings.LanguageAfterRestart != string.Empty) ? Settings.LanguageAfterRestart : Localizer.language),
			EndTurnWarnings = Settings.EndTurnWarnings,
			ShowSkillsHotkeys = Settings.ShowSkillsHotkeys,
			SpeedScale = Settings.SpeedScale,
			SpeedMode = Settings.SpeedMode,
			InputDeviceType = Settings.InputDeviceType,
			SmartCast = Settings.SmartCast,
			CurrentProfile = SaveManager.CurrentProfileIndex,
			HideCompendium = Settings.HideCompendium,
			AlwaysDisplayUnitPortraitAttribute = Settings.AlwaysDisplayUnitPortraitAttribute
		};
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedSettings serializedSettings = container as SerializedSettings;
		DeserializeScreenSettings(serializedSettings, serializedSettings?.ScreenSettings);
		DeserializeSoundSettings(serializedSettings?.SoundSettings);
		DeserializeLanguageSettings(serializedSettings?.Language);
		DeserializeEndTurnWarnings(serializedSettings?.SaveVersion, serializedSettings?.EndTurnWarnings);
		DeserializeSpeedSettings(serializedSettings?.SpeedScale, serializedSettings?.SpeedMode);
		DeserializeShowSkillsHotkeys(serializedSettings?.ShowSkillsHotkeys);
		DeserializeInputDeviceType(serializedSettings?.SaveVersion, serializedSettings?.InputDeviceType);
		DeserializeCurrentProfile(serializedSettings?.CurrentProfile);
		DeserializeSmartCast(serializedSettings?.SaveVersion, serializedSettings?.SmartCast);
		DeserializeAlwaysDisplayMaxStatValue(serializedSettings?.AlwaysDisplayMaxStatValue);
		DeserializeEdgePan(serializedSettings);
		DeserializeFocusCamOnSelections(serializedSettings);
		DeserializeHideCompendium(serializedSettings?.HideCompendium);
		DeserializeAlwaysDisplayUnitPortraitAttribute(serializedSettings?.AlwaysDisplayUnitPortraitAttribute);
		if (serializedSettings != null)
		{
			CheckWarningsToDisplay(serializedSettings.SaveVersion, SaveManager.SettingsSaveVersion);
		}
		if (saveVersion < 11 && SaveManager.SettingsSaveVersion >= 11)
		{
			((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).Log((object)"Added tutorial popups since last save version -> resetting input mapping.", (CLogLevel)0, true, false);
			TPSingleton<KeyRemappingManager>.Instance.ResetAll();
		}
		else if (saveVersion < 13 && SaveManager.SettingsSaveVersion >= 13)
		{
			((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).Log((object)"Skill bar hotkeys rework -> resetting input mapping.", (CLogLevel)0, true, false);
			TPSingleton<KeyRemappingManager>.Instance.ResetAll();
		}
	}

	private void DeserializeAlwaysDisplayMaxStatValue(bool? alwaysDisplayMaxStatValue)
	{
		Settings.AlwaysDisplayMaxStatValue = alwaysDisplayMaxStatValue.GetValueOrDefault();
	}

	private void DeserializeAlwaysDisplayUnitPortraitAttribute(bool? alwaysDisplayUnitPortraitAttribute)
	{
		Settings.AlwaysDisplayUnitPortraitAttribute = alwaysDisplayUnitPortraitAttribute.GetValueOrDefault();
	}

	private void DeserializeCurrentProfile(int? currentProfile)
	{
		SaveManager.ChangeCurrentProfile(currentProfile ?? 1);
	}

	private void DeserializeEdgePan(SerializedSettings settingsElement)
	{
		Settings.EdgePan = settingsElement?.EdgePan ?? true;
		Settings.EdgePanOverUI = settingsElement?.EdgePanOverUI ?? true;
	}

	private void DeserializeFocusCamOnSelections(SerializedSettings settingsElement)
	{
		Settings.FocusCamOnSelections = settingsElement?.FocusCamOnSelections ?? true;
	}

	private void DeserializeFrameRateCap(int? frameRate)
	{
		SettingsController.SetTargetFrameRate(frameRate ?? (-1));
	}

	private void DeserializeHideCompendium(bool? hideCompendium)
	{
		Settings.HideCompendium = hideCompendium.GetValueOrDefault();
	}

	private void DeserializeLanguageSettings(string language = null)
	{
		if (language == null)
		{
			Localizer.language = Localizer.knownLanguages[0];
			return;
		}
		for (int num = Localizer.knownLanguages.Length - 1; num >= 0; num--)
		{
			string text = Localizer.knownLanguages[num];
			if (language == text)
			{
				Localizer.language = text;
				break;
			}
		}
	}

	private void DeserializeMonitorIndex(int? monitorIndex)
	{
		Settings.MonitorIndex = monitorIndex.GetValueOrDefault();
	}

	private void DeserializeResolution(SerializableResolution? resolutionElement)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		List<Resolution> list = new List<Resolution>(Screen.resolutions.Length);
		for (int num = Screen.resolutions.Length - 1; num >= 0; num--)
		{
			Resolution item = Screen.resolutions[num];
			if (((Resolution)(ref item)).width >= 1024 && ((Resolution)(ref item)).height >= 720)
			{
				list.Add(item);
			}
		}
		SupportedResolutions = list.ToArray();
		if (resolutionElement.HasValue)
		{
			SettingsController.SetResolution(resolutionElement.Value.Deserialize());
			return;
		}
		SettingsController.SetResolution(Screen.currentResolution);
		Resolution resolution = Settings.Resolution;
		object arg = ((Resolution)(ref resolution)).width;
		resolution = Settings.Resolution;
		object arg2 = ((Resolution)(ref resolution)).height;
		resolution = Settings.Resolution;
		((CLogger<SettingsManager>)this).Log((object)$"Settings initial resolution to {arg}x{arg2} {((Resolution)(ref resolution)).refreshRate}Hz.", (CLogLevel)0, false, false);
	}

	private void DeserializeRestrictedCursor(bool? restrictedCursorElement = null)
	{
		if (!restrictedCursorElement.HasValue)
		{
			SettingsController.SetRestrictedCursor(isCursorRestricted: true);
		}
		else
		{
			SettingsController.SetRestrictedCursor(restrictedCursorElement.Value);
		}
	}

	private void DeserializeRunInBackground(bool? runInBackgroundElement = null)
	{
		if (!runInBackgroundElement.HasValue)
		{
			SettingsController.SetRunInBackground(runInBackground: true);
		}
		else
		{
			SettingsController.SetRunInBackground(runInBackgroundElement.Value);
		}
	}

	private void DeserializeScreenSettings(SerializedSettings settingsElement, SerializedScreenSettings screenSettingsElement = null)
	{
		DeserializeResolution(screenSettingsElement?.Resolution);
		DeserializeMonitorIndex(screenSettingsElement?.MonitorIndex);
		DeserializeRestrictedCursor(screenSettingsElement?.IsCursorRestricted);
		DeserializeRunInBackground(screenSettingsElement?.RunInBackground);
		DeserializeUISize(settingsElement?.SaveVersion, screenSettingsElement?.UISize);
		DeserializeScreenShakes(settingsElement?.SaveVersion, screenSettingsElement?.ScreenShakes);
		DeserializeWindowMode(screenSettingsElement?.WindowMode);
		DeserializeVSync(screenSettingsElement?.VSync);
		DeserializeFrameRateCap(screenSettingsElement?.FrameRateCap);
	}

	private void DeserializeSoundSettings(SerializedSoundSettings soundSettingsElement = null)
	{
		if (soundSettingsElement == null)
		{
			SettingsController.SetMasterVolume(masterVolume);
			SettingsController.SetMusicVolume(musicVolume);
			SettingsController.SetUIVolume(uiVolume);
			SettingsController.SetAmbientVolume(ambientVolume);
		}
		else
		{
			SettingsController.SetMasterVolume(soundSettingsElement.MasterVolume);
			SettingsController.SetMusicVolume(soundSettingsElement.MusicVolume);
			SettingsController.SetUIVolume(soundSettingsElement.UIVolume);
			SettingsController.SetAmbientVolume(soundSettingsElement.AmbientVolume);
		}
	}

	private void DeserializeSpeedSettings(float? speedScale, E_SpeedMode? speedMode)
	{
		SettingsController.SetSpeedScale(speedScale ?? 5f);
		SettingsController.SetSpeedMode(speedMode.GetValueOrDefault());
	}

	private void DeserializeShowSkillsHotkeys(bool? showSkillsHotkeys)
	{
		SettingsController.SetShowSkillsHotkeys(showSkillsHotkeys ?? true);
	}

	private void DeserializeInputDeviceType(int? saveVersion, E_InputDeviceType? inputDeviceType)
	{
		SetInputDeviceType(inputDeviceType.GetValueOrDefault());
	}

	private void DeserializeSmartCast(int? saveVersion, bool? value)
	{
		SettingsController.SetSmartCast(value.GetValueOrDefault());
	}

	private void DeserializeUISize(int? saveVersion, float? uiSizeElement = null)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (saveVersion.HasValue && saveVersion <= 3 && SaveManager.SettingsSaveVersion > saveVersion)
		{
			((CLogger<SettingsManager>)this).Log((object)$"Loading a v3 save with the application being v{SaveManager.SettingsSaveVersion}, forcing Big Screen UI option reset.", (CLogLevel)0, false, false);
			Resolution resolution = Settings.Resolution;
			SettingsController.SetUISizeScale((((Resolution)(ref resolution)).height >= 2160) ? 2f : 1f);
		}
		else
		{
			SettingsController.SetUISizeScale(uiSizeElement.GetValueOrDefault());
		}
	}

	private void DeserializeEndTurnWarnings(int? saveVersion, bool[] endTurnWarnings)
	{
		if (endTurnWarnings != null)
		{
			Settings.EndTurnWarnings = endTurnWarnings;
			return;
		}
		Settings.EndTurnWarnings = new bool[Enum.GetNames(typeof(E_EndTurnWarning)).Length];
		for (int i = 0; i < Settings.EndTurnWarnings.Length; i++)
		{
			Settings.EndTurnWarnings[i] = true;
		}
	}

	private void DeserializeScreenShakes(int? saveVersion, float? uiScreenShakesElement = null)
	{
		if (saveVersion.HasValue && saveVersion <= 4 && SaveManager.SettingsSaveVersion > saveVersion)
		{
			((CLogger<SettingsManager>)this).Log((object)$"Loading a v4 save with the application being v{SaveManager.SettingsSaveVersion}, forcing uiScreenShakes value to 1.", (CLogLevel)0, false, false);
			SettingsController.SetScreenShakes(1f);
		}
		else
		{
			SettingsController.SetScreenShakes(uiScreenShakesElement ?? 1f);
		}
	}

	private void DeserializeVSync(bool? vSyncElement = null)
	{
		SettingsController.SetVSync(vSyncElement ?? (QualitySettings.vSyncCount > 0));
	}

	private void DeserializeWindowMode(E_WindowMode? windowModeElement = null)
	{
		if (!windowModeElement.HasValue)
		{
			SettingsController.SetWindowMode(Settings.WindowMode);
		}
		else
		{
			SettingsController.SetWindowMode(windowModeElement.Value);
		}
	}

	private SerializedScreenSettings SerializeScreenSettings()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return new SerializedScreenSettings
		{
			IsCursorRestricted = Settings.IsCursorRestricted,
			Resolution = new SerializableResolution(Settings.Resolution),
			MonitorIndex = Settings.MonitorIndex,
			RunInBackground = Settings.RunInBackground,
			ScreenShakes = Settings.ScreenShakesValue,
			UISize = Settings.UiSizeScale,
			WindowMode = Settings.WindowMode,
			VSync = Settings.UseVSync,
			FrameRateCap = Settings.FrameRateCap
		};
	}

	private SerializedSoundSettings SerializeSoundSettings()
	{
		return new SerializedSoundSettings
		{
			MasterVolume = Settings.MasterVolume,
			MusicVolume = Settings.MusicVolume,
			UIVolume = Settings.UiVolume,
			AmbientVolume = Settings.AmbientVolume
		};
	}

	[DevConsoleCommand("AlwaysDisplayMaxStat")]
	private static void DebugAlwaysDisplayMaxStat(bool newValue)
	{
		TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayMaxStatValue = newValue;
	}

	[DevConsoleCommand("AlwaysDisplayUnitPortraitAttribute")]
	private static void DebugAlwaysDisplayUnitPortraitAttribute(bool newValue)
	{
		TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayUnitPortraitAttribute = newValue;
	}

	[DevConsoleCommand("CompendiumHide")]
	private static void DebugHideCompendium(bool newValue)
	{
		TPSingleton<SettingsManager>.Instance.Settings.HideCompendium = newValue;
	}

	[DevConsoleCommand(Name = "UIScale")]
	private static void DebugUiScale(float newValue)
	{
		SettingsController.SetUISizeScale(Mathf.Clamp(newValue, 1f, 2f));
	}

	[DevConsoleCommand(Name = "ToggleEndTurnWarning")]
	private static void DebugEndTurnWarning(E_EndTurnWarning warningType, bool value)
	{
		TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[(int)warningType] = value;
		((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).Log((object)"DebugToggle EndTurnWarning list:", (CLogLevel)1, true, false);
		for (E_EndTurnWarning e_EndTurnWarning = E_EndTurnWarning.RESOURCES_UNUSED; (int)e_EndTurnWarning < TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings.Length; e_EndTurnWarning++)
		{
			((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).Log((object)$"{e_EndTurnWarning} : {TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[(int)e_EndTurnWarning]}", (CLogLevel)1, true, false);
		}
	}

	[DevConsoleCommand(Name = "DisplaySettingsChangesWarnings")]
	private static void DebugDisplaySettingsChangesWarnings(int oldestVersion, int currentVerion)
	{
		TPSingleton<SettingsManager>.Instance.CheckWarningsToDisplay(oldestVersion, currentVerion);
		MainMenuView.OpenSettingsChangesWarning();
	}

	[DevConsoleCommand(Name = "AllowAllResolutions")]
	private static void DebugAllowAllResolutions()
	{
		SupportedResolutions = Screen.resolutions;
		SupportedResolutions = SupportedResolutions.Reverse().ToArray();
		if ((Object)(object)TPSingleton<SettingsManager>.Instance.SettingsPanel.ResolutionDropdownPanel != (Object)null)
		{
			TPSingleton<SettingsManager>.Instance.SettingsPanel.ResolutionDropdownPanel.DebugInitializeOptionKeys();
		}
	}
}
