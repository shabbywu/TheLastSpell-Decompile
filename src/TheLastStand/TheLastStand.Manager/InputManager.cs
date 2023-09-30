using System;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using Rewired.Data;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Framework.Automaton;
using TheLastStand.Model;
using TheLastStand.Serialization;
using TheLastStand.View;
using TheLastStand.View.Cursor;
using TheLastStand.View.Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.Manager;

public class InputManager : Manager<InputManager>
{
	public delegate void LastActiveControllerChangedHandler(ControllerType controllerType);

	[SerializeField]
	private WorldInputsView worldInputsView;

	[SerializeField]
	private UserDataStore keyRemappingSaverLoader;

	[SerializeField]
	private JoystickConfig joystickConfig;

	private int allowingCursorUICount;

	private int currentKeyboardLayoutId = -1;

	private bool hasBeenInitialized;

	private Player player;

	public static bool IsLastControllerJoystick => (int)GetLastControllerType() == 2;

	public static bool IsPointerOverAllowingCursorUI
	{
		get
		{
			return TPSingleton<InputManager>.Instance.allowingCursorUICount > 0;
		}
		set
		{
			TPSingleton<InputManager>.Instance.allowingCursorUICount += (value ? 1 : (-1));
		}
	}

	public static bool IsPointerOverWorld => TPSingleton<InputManager>.Instance.worldInputsView.IsPointerOverWorld;

	public static Vector3 MousePosition => Input.mousePosition;

	public static JoystickConfig JoystickConfig => TPSingleton<InputManager>.Instance.joystickConfig;

	public static Vector3 JoystickCursorPosition => TPSingleton<CursorView>.Instance.JoystickCursorPosition;

	public bool JoysticksEnabled { get; private set; } = true;


	public bool KeyboardEnabled { get; private set; } = true;


	public Player Player
	{
		get
		{
			if (player == null)
			{
				player = ReInput.players.GetPlayer("Player");
			}
			return player;
		}
	}

	public Guid JoystickGuid
	{
		get
		{
			if (ReInput.controllers.Joysticks.Count <= 0)
			{
				return Guid.Empty;
			}
			return ((Controller)ReInput.controllers.Joysticks[0]).hardwareTypeGuid;
		}
	}

	public ControllerType? DebugDisplayedControllerType { get; private set; }

	public event LastActiveControllerChangedHandler LastActiveControllerChanged;

	public static float GetAxis(int axisId)
	{
		if (TPSingleton<InputManager>.Instance.Player.isPlaying)
		{
			return TPSingleton<InputManager>.Instance.Player.GetAxis(axisId);
		}
		return 0f;
	}

	public static bool GetButton(int buttonId)
	{
		if (TPSingleton<InputManager>.Instance.Player.isPlaying)
		{
			return TPSingleton<InputManager>.Instance.Player.GetButton(buttonId);
		}
		return false;
	}

	public static bool GetButtonDown(int buttonId)
	{
		if (TPSingleton<InputManager>.Instance.Player.isPlaying)
		{
			return TPSingleton<InputManager>.Instance.Player.GetButtonDown(buttonId);
		}
		return false;
	}

	public static bool GetButtonUp(int buttonId)
	{
		if (TPSingleton<InputManager>.Instance.Player.isPlaying)
		{
			return TPSingleton<InputManager>.Instance.Player.GetButtonUp(buttonId);
		}
		return false;
	}

	public static string[] GetLocalizedHotkeysForAction(string actionKey)
	{
		InputAction val = null;
		List<InputCategory> list = ReInput.mapping.ActionCategories.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			val = ReInput.mapping.ActionsInCategory(list[i].id, true).ToList().Find((InputAction o) => o.name == actionKey);
			if (val != null)
			{
				break;
			}
		}
		if (val == null)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)("No Rewired Action has been found for Id (string) " + actionKey + "."), (CLogLevel)1, true, false);
			return null;
		}
		return GetLocalizedHotkeysForAction(val);
	}

	public static string[] GetLocalizedHotkeysForAction(int actionId)
	{
		InputAction val = null;
		List<InputCategory> list = ReInput.mapping.ActionCategories.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			val = ReInput.mapping.ActionsInCategory(list[i].id, true).ToList().Find((InputAction o) => o.id == actionId);
			if (val != null)
			{
				break;
			}
		}
		if (val == null)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)$"No Rewired Action has been found for Id (int) {actionId}.", (CLogLevel)1, true, false);
			return null;
		}
		return GetLocalizedHotkeysForAction(val);
	}

	public static string[] GetLocalizedHotkeysForAction(InputAction action)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		List<ActionElementMap> list = new List<ActionElementMap>();
		foreach (ControllerMap allMap in TPSingleton<InputManager>.Instance.Player.controllers.maps.GetAllMaps())
		{
			if (((int)allMap.controllerType != 2 || IsLastControllerJoystick) && (((int)allMap.controllerType != 0 && (int)allMap.controllerType != 1) || !IsLastControllerJoystick))
			{
				List<ActionElementMap> list2 = new List<ActionElementMap>();
				allMap.GetButtonMapsWithAction(action.id, list2);
				list.AddRange(list2);
			}
		}
		if (list.Count > 0)
		{
			string[] array = new string[list.Count];
			int i = 0;
			string text = default(string);
			for (int count = list.Count; i < count; i++)
			{
				array[i] = (Localizer.TryGet($"KeyCode_{list[i].keyCode}", ref text) ? text : list[i].elementIdentifierName);
			}
			return array;
		}
		return null;
	}

	public static ControllerType GetLastControllerType()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Controller lastActiveController = TPSingleton<InputManager>.Instance.Player.controllers.GetLastActiveController();
		if (lastActiveController == null)
		{
			return (ControllerType)0;
		}
		return lastActiveController.type;
	}

	public static bool GetSubmitButtonDown()
	{
		if (!Input.GetMouseButtonDown(0))
		{
			return GetButtonDown(79);
		}
		return true;
	}

	public static void LoadMap()
	{
		if ((Object)(object)TPSingleton<InputManager>.Instance.keyRemappingSaverLoader == (Object)null)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"keyRemappingSaverLoader reference is missing, trying to find it with FindObjectOfType.", (CLogLevel)2, true, false);
			TPSingleton<InputManager>.Instance.keyRemappingSaverLoader = (UserDataStore)(object)Object.FindObjectOfType<KeyRemappingSaverLoader>();
			if ((Object)(object)TPSingleton<InputManager>.Instance.keyRemappingSaverLoader == (Object)null)
			{
				((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"keyRemappingSaverLoader reference is missing, cannot load input mapping.", (CLogLevel)2, true, false);
				return;
			}
		}
		TPSingleton<InputManager>.Instance.keyRemappingSaverLoader.Load();
	}

	public static void OnDropdownClose()
	{
		if (TPSingleton<GameManager>.Exist())
		{
			OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		}
		else if (TPSingleton<ApplicationManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.OnApplicationStateChange(((StateMachine)ApplicationManager.Application).State);
		}
	}

	public static void OnDropdownOpen()
	{
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetAllMapsEnabled(false);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 15);
	}

	public static void OnGameStateChange(Game.E_State state)
	{
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetAllMapsEnabled(false);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 0);
		switch (state)
		{
		case Game.E_State.Management:
		case Game.E_State.UnitPreparingSkill:
		case Game.E_State.UnitExecutingSkill:
		case Game.E_State.BuildingPreparingAction:
		case Game.E_State.BuildingExecutingAction:
		case Game.E_State.BuildingPreparingSkill:
		case Game.E_State.BuildingExecutingSkill:
		case Game.E_State.Construction:
		case Game.E_State.PlaceUnit:
		case Game.E_State.BuildingUpgrade:
		case Game.E_State.Wait:
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 3);
			if (IsPointerOverWorld)
			{
				TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 4);
			}
			break;
		case Game.E_State.CharacterSheet:
		case Game.E_State.Recruitment:
		case Game.E_State.Shopping:
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
		case Game.E_State.Settings:
		case Game.E_State.GameOver:
		case Game.E_State.MetaShops:
		case Game.E_State.UnitCustomisation:
		case Game.E_State.ConsentPopup:
		case Game.E_State.BlockingPopup:
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 11);
			break;
		case Game.E_State.HowToPlay:
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 17);
			break;
		case Game.E_State.LevelEdition:
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 3);
			if (IsPointerOverWorld)
			{
				TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 4);
			}
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 7);
			break;
		case Game.E_State.CutscenePlaying:
			break;
		}
	}

	public static void OnGenericConsentViewToggled(bool state)
	{
		if (TPSingleton<GameManager>.Exist())
		{
			OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		}
		else if (state)
		{
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetAllMapsEnabled(false);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 0);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 11);
		}
		else if (((StateMachine)ApplicationManager.Application).State.GetName() == "WorldMap")
		{
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 3);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 4);
		}
	}

	public static void OnItemTooltipDisplayedChange(bool isVisible)
	{
	}

	public static void OnTutorialPopupOpen()
	{
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetAllMapsEnabled(false);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 17);
	}

	public static void OnTutorialPopupClosed()
	{
		if (TPSingleton<GameManager>.Exist())
		{
			OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		}
		else if (TPSingleton<ApplicationManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.OnApplicationStateChange(((StateMachine)ApplicationManager.Application).State);
		}
	}

	public static void OnInputDeviceTypeChanged(SettingsManager.E_InputDeviceType inputDeviceType)
	{
		switch (inputDeviceType)
		{
		case SettingsManager.E_InputDeviceType.MouseKeyboard:
			TPSingleton<InputManager>.Instance.JoysticksEnabled = false;
			TPSingleton<InputManager>.Instance.KeyboardEnabled = true;
			Cursor.visible = true;
			if (TPSingleton<HUDJoystickNavigationManager>.Exist() && !TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode())
			{
				EventSystem.current.SetSelectedGameObject((GameObject)null);
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			}
			break;
		case SettingsManager.E_InputDeviceType.Controller:
		{
			int count = ReInput.controllers.Joysticks.Count;
			if (count == 0)
			{
				((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)$"Setting InputDeviceType to {inputDeviceType} though no controller is registered! Registering setting but keeping Mouse/KB enabled.", (CLogLevel)1, true, false);
			}
			TPSingleton<InputManager>.Instance.JoysticksEnabled = true;
			TPSingleton<InputManager>.Instance.KeyboardEnabled = count == 0;
			Cursor.visible = count == 0;
			break;
		}
		case SettingsManager.E_InputDeviceType.Auto:
			TPSingleton<InputManager>.Instance.JoysticksEnabled = true;
			TPSingleton<InputManager>.Instance.KeyboardEnabled = true;
			break;
		default:
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogError((object)$"Unhandled InputDeviceType {inputDeviceType}!", (CLogLevel)1, true, true);
			return;
		}
		UpdateJoystickControllersEnabled();
		UpdateKeyboardControllerEnabled();
	}

	public static void SaveMap()
	{
		if ((Object)(object)TPSingleton<InputManager>.Instance.keyRemappingSaverLoader == (Object)null)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"keyRemappingSaverLoader reference is missing, trying to find it with FindObjectOfType.", (CLogLevel)2, true, false);
			TPSingleton<InputManager>.Instance.keyRemappingSaverLoader = (UserDataStore)(object)Object.FindObjectOfType<KeyRemappingSaverLoader>();
			if ((Object)(object)TPSingleton<InputManager>.Instance.keyRemappingSaverLoader == (Object)null)
			{
				((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"keyRemappingSaverLoader reference is missing, cannot save input mapping.", (CLogLevel)2, true, false);
				return;
			}
		}
		TPSingleton<InputManager>.Instance.keyRemappingSaverLoader.Save();
	}

	public static int GetKeyboardLayoutId()
	{
		switch (TPSingleton<SettingsManager>.Instance.Settings.KeyboardLayout)
		{
		case SettingsManager.E_KeyboardLayout.AZERTY:
			return 2;
		case SettingsManager.E_KeyboardLayout.QWERTY:
			return 3;
		case SettingsManager.E_KeyboardLayout.QWERTZ:
			return 4;
		default:
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogError((object)$"Keyboard layout {TPSingleton<SettingsManager>.Instance.Settings.KeyboardLayout} not handled, please add it to the switch", (CLogLevel)1, true, true);
			return 3;
		}
	}

	public static void RefreshRewiredKeyboardLayout()
	{
		int keyboardLayoutId = GetKeyboardLayoutId();
		((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)$"Refreshing keyboard layout using Layout Id {keyboardLayoutId} (= {(SettingsManager.E_KeyboardLayout)keyboardLayoutId})", (CLogLevel)1, false, false);
		if (keyboardLayoutId == TPSingleton<InputManager>.Instance.currentKeyboardLayoutId)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)$"Layout Id {keyboardLayoutId} (= {(SettingsManager.E_KeyboardLayout)keyboardLayoutId}) used for refresh is the current one -> aborting.", (CLogLevel)1, false, false);
			return;
		}
		TPSingleton<InputManager>.Instance.Player.controllers.maps.RemoveMap((ControllerType)0, 0, 0, TPSingleton<InputManager>.Instance.currentKeyboardLayoutId);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.RemoveMap((ControllerType)0, 0, 3, TPSingleton<InputManager>.Instance.currentKeyboardLayoutId);
		TPSingleton<InputManager>.Instance.currentKeyboardLayoutId = keyboardLayoutId;
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadMap((ControllerType)0, 0, 0, TPSingleton<InputManager>.Instance.currentKeyboardLayoutId);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadMap((ControllerType)0, 0, 3, TPSingleton<InputManager>.Instance.currentKeyboardLayoutId);
		if (TPSingleton<GameManager>.Exist() && TPSingleton<GameManager>.Instance.Game != null)
		{
			OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		}
	}

	public void Init()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		if (!hasBeenInitialized)
		{
			if (TPSingleton<ApplicationManager>.Exist())
			{
				ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
			}
			Player.controllers.AddLastActiveControllerChangedDelegate(new PlayerActiveControllerChangedDelegate(OnLastActiveControllerChanged));
			Player.isPlaying = true;
			if (TPSingleton<DebugManager>.Exist())
			{
				DebugManager.DevConsoleDisplayed += OnDevConsoleDisplayedChange;
			}
			if (TPSingleton<GameManager>.Exist())
			{
				worldInputsView.PointerOverWorldChangeEvent += OnPointerOverWorldChanged;
			}
			if (!Application.isEditor && !Application.isConsolePlatform && !JoystickConfig.BuildEnabled)
			{
				DebugToggleJoystickControllers();
			}
			Player.controllers.maps.LoadDefaultMaps((ControllerType)2);
			ReInput.ControllerConnectedEvent += OnControllerConnected;
			ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
			hasBeenInitialized = true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	protected override void OnDestroy()
	{
		((CLogger<InputManager>)this).OnDestroy();
		if (TPSingleton<ApplicationManager>.Exist())
		{
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent -= OnApplicationStateChange;
		}
		this.LastActiveControllerChanged = null;
		if (TPSingleton<DebugManager>.Exist())
		{
			DebugManager.DevConsoleDisplayed -= OnDevConsoleDisplayedChange;
		}
		ReInput.ControllerConnectedEvent -= OnControllerConnected;
		ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
	}

	private void OnApplicationStateChange(State state)
	{
		((CLogger<InputManager>)this).LogWarning((object)$"OnApplicationStateChange: {state}", (CLogLevel)1, true, false);
		Player.controllers.maps.SetAllMapsEnabled(false);
		Player.controllers.maps.SetMapsEnabled(true, 0);
		switch (state.GetName())
		{
		case "WorldMap":
			Player.controllers.maps.SetMapsEnabled(true, 3);
			Player.controllers.maps.SetMapsEnabled(true, 4);
			break;
		case "Game":
			if (TPSingleton<GameManager>.Exist())
			{
				OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
				if (TPSingleton<TutorialView>.Instance.DisplayCoroutineRunning)
				{
					OnTutorialPopupOpen();
				}
			}
			break;
		case "Settings":
			Player.controllers.maps.SetMapsEnabled(true, 11);
			break;
		}
	}

	private void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		SettingsManager.E_InputDeviceType inputDeviceType = TPSingleton<SettingsManager>.Instance.Settings.InputDeviceType;
		if (inputDeviceType == SettingsManager.E_InputDeviceType.Controller)
		{
			OnInputDeviceTypeChanged(inputDeviceType);
		}
	}

	private void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		OnInputDeviceTypeChanged(TPSingleton<SettingsManager>.Instance.Settings.InputDeviceType);
	}

	private static void UpdateJoystickControllersEnabled()
	{
		foreach (Joystick joystick in TPSingleton<InputManager>.Instance.player.controllers.Joysticks)
		{
			((Controller)joystick).enabled = TPSingleton<InputManager>.Instance.JoysticksEnabled;
		}
	}

	private static void UpdateKeyboardControllerEnabled()
	{
		((Controller)TPSingleton<InputManager>.Instance.player.controllers.Keyboard).enabled = TPSingleton<InputManager>.Instance.KeyboardEnabled;
	}

	private void OnDevConsoleDisplayedChange(bool isVisible)
	{
		Player.isPlaying = !isVisible;
	}

	private void OnPointerOverWorldChanged(bool isPointerOverWorld)
	{
		Player.controllers.maps.SetMapsEnabled(isPointerOverWorld, 4);
	}

	private void OnLastActiveControllerChanged(Player player, Controller controller)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		ControllerType type = controller.type;
		if ((int)type > 1)
		{
			if ((int)type == 2)
			{
				Cursor.visible = false;
				goto IL_0024;
			}
			if ((int)type == 20)
			{
			}
		}
		Cursor.visible = true;
		goto IL_0024;
		IL_0024:
		this.LastActiveControllerChanged?.Invoke(controller.type);
	}

	[DevConsoleCommand("ToggleJoystickControllers")]
	private static void DebugToggleJoystickControllers()
	{
		TPSingleton<InputManager>.Instance.JoysticksEnabled = !TPSingleton<InputManager>.Instance.JoysticksEnabled;
		UpdateJoystickControllersEnabled();
	}

	public static void DebugOnMetaConditionDebugViewToggled(bool state)
	{
		if (state)
		{
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetAllMapsEnabled(false);
			TPSingleton<InputManager>.Instance.Player.controllers.maps.SetMapsEnabled(true, 11);
		}
		else
		{
			OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		}
	}

	[DevConsoleCommand("ForceDisplayedControllerTypeReset")]
	public static void ForceDisplayedControllerTypeReset()
	{
		TPSingleton<InputManager>.Instance.DebugDisplayedControllerType = null;
	}

	[DevConsoleCommand("ForceDisplayedControllerType")]
	public static void ForceDisplayedControllerType(ControllerType controllerType)
	{
		TPSingleton<InputManager>.Instance.DebugDisplayedControllerType = controllerType;
	}
}
