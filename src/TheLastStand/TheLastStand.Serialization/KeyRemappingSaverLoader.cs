using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Rewired;
using Rewired.Data;
using Rewired.Utils.Libraries.TinyJson;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Serialization;

public class KeyRemappingSaverLoader : UserDataStore
{
	public class Constants
	{
		public static readonly XNamespace XmlNamespace = XNamespace.op_Implicit("http://guavaman.com/rewired");

		public const string SaveElementLocalName = "InputMappingSave";

		public const string CategoryIdElementName = "categoryId";

		public const string LayoutIdElementName = "layoutId";

		public const string InputBehaviorElementName = "InputBehavior";

		public const string IdElementName = "id";

		public const string EditorLoadedMessage = "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.";

		public const int ControllerMapPPKeyVersionOriginal = 0;

		public const int ControllerMapPPKeyVersionIncludeDuplicateJoystickIndex = 1;

		public const int ControllerMapPPKeyVersionSupportDisconnectedControllers = 2;

		public const int ControllerMapPPKeyVersionIncludeFormatVersion = 2;

		public const int ControllerMapPPKeyVersion = 2;
	}

	private class ControllerAssignmentSaveInfo
	{
		public class PlayerInfo
		{
			public bool HasKeyboard { get; set; }

			public bool HasMouse { get; set; }

			public int Id { get; set; }

			public int JoystickCount
			{
				get
				{
					if (Joysticks == null)
					{
						return 0;
					}
					return Joysticks.Length;
				}
			}

			public JoystickInfo[] Joysticks { get; set; }

			public int IndexOfJoystick(int joystickId)
			{
				for (int i = 0; i < JoystickCount; i++)
				{
					if (Joysticks[i] != null && Joysticks[i].Id == joystickId)
					{
						return i;
					}
				}
				return -1;
			}

			public bool ContainsJoystick(int joystickId)
			{
				return IndexOfJoystick(joystickId) >= 0;
			}
		}

		public class JoystickInfo
		{
			public string HardwareIdentifier { get; set; }

			public int Id { get; set; }

			public Guid InstanceGuid { get; set; }
		}

		public int PlayerCount
		{
			get
			{
				if (Players == null)
				{
					return 0;
				}
				return Players.Length;
			}
		}

		public PlayerInfo[] Players { get; set; }

		public ControllerAssignmentSaveInfo()
		{
		}

		public ControllerAssignmentSaveInfo(int playerCount)
		{
			Players = new PlayerInfo[playerCount];
			for (int i = 0; i < playerCount; i++)
			{
				Players[i] = new PlayerInfo();
			}
		}

		public int IndexOfPlayer(int playerId)
		{
			for (int i = 0; i < PlayerCount; i++)
			{
				if (Players[i] != null && Players[i].Id == playerId)
				{
					return i;
				}
			}
			return -1;
		}

		public bool ContainsPlayer(int playerId)
		{
			return IndexOfPlayer(playerId) >= 0;
		}
	}

	private class JoystickAssignmentHistoryInfo
	{
		public readonly Joystick Joystick;

		public readonly int OldJoystickId;

		public JoystickAssignmentHistoryInfo(Joystick joystick, int oldJoystickId)
		{
			if (joystick == null)
			{
				throw new ArgumentNullException("joystick");
			}
			Joystick = joystick;
			OldJoystickId = oldJoystickId;
		}
	}

	[SerializeField]
	[Tooltip("Should this script be used? If disabled, nothing will be saved or loaded.")]
	private bool isEnabled = true;

	[SerializeField]
	[Tooltip("Should saved data be loaded on start?")]
	private bool loadDataOnStart = true;

	[SerializeField]
	[Tooltip("Should Player Joystick assignments be saved and loaded? This is not totally reliable for all Joysticks on all platforms. Some platforms/input sources do not provide enough information to reliably save assignments from session to session and reboot to reboot.")]
	private bool loadJoystickAssignments = true;

	[SerializeField]
	[Tooltip("Should Player Keyboard assignments be saved and loaded?")]
	private bool loadKeyboardAssignments = true;

	[SerializeField]
	[Tooltip("Should Player Mouse assignments be saved and loaded?")]
	private bool loadMouseAssignments = true;

	[HideInInspector]
	[SerializeField]
	[Tooltip("The PlayerPrefs key prefix. Change this to change how keys are stored in PlayerPrefs. Changing this will make saved data already stored with the old key no longer accessible.")]
	private string playerPrefsKeyPrefix = "RewiredSaveData";

	[SerializeField]
	private UserDataStore_PlayerPrefs playerPrefsSaverLoader;

	private XElement saveElement;

	private XElement loadElement;

	[NonSerialized]
	private bool allowImpreciseJoystickAssignmentMatching = true;

	[NonSerialized]
	private bool deferredJoystickAssignmentLoadPending;

	[NonSerialized]
	private bool wasJoystickEverDetected;

	[NonSerialized]
	private List<int> allActionIds;

	[NonSerialized]
	private string allActionIdsString;

	public static string KeyRemappingSaveFilePath => SaveManager.PersistentDataPath + "/Save/" + SaveManager.GetSaveSubFolderPath() + "/InputMappingSave.xml";

	public static bool KeyRemappingSaveExists => File.Exists(KeyRemappingSaveFilePath);

	public bool IsEnabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			isEnabled = value;
		}
	}

	public bool LoadDataOnStart
	{
		get
		{
			return loadDataOnStart;
		}
		set
		{
			loadDataOnStart = value;
		}
	}

	public bool LoadJoystickAssignments
	{
		get
		{
			return loadJoystickAssignments;
		}
		set
		{
			loadJoystickAssignments = value;
		}
	}

	public bool LoadKeyboardAssignments
	{
		get
		{
			return loadKeyboardAssignments;
		}
		set
		{
			loadKeyboardAssignments = value;
		}
	}

	public bool LoadMouseAssignments
	{
		get
		{
			return loadMouseAssignments;
		}
		set
		{
			loadMouseAssignments = value;
		}
	}

	public string PlayerPrefsKeyPrefix
	{
		get
		{
			return playerPrefsKeyPrefix;
		}
		set
		{
			playerPrefsKeyPrefix = value;
		}
	}

	private string PlayerPrefsKeyControllerAssignments => string.Format("{0}_{1}", playerPrefsKeyPrefix, "ControllerAssignments");

	private bool LoadControllerAssignments
	{
		get
		{
			if (!loadKeyboardAssignments && !loadMouseAssignments)
			{
				return loadJoystickAssignments;
			}
			return true;
		}
	}

	private List<int> AllActionIds
	{
		get
		{
			if (allActionIds != null)
			{
				return allActionIds;
			}
			List<int> list = new List<int>();
			IList<InputAction> actions = ReInput.mapping.Actions;
			for (int i = 0; i < actions.Count; i++)
			{
				list.Add(actions[i].id);
			}
			allActionIds = list;
			return list;
		}
	}

	public override void Save()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if (!IsEnabled)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader is disabled and will not save any data.", (Object)(object)((Component)this).gameObject, (CLogLevel)1, true, false);
			return;
		}
		saveElement = new XElement(XName.op_Implicit("InputMappingSave"));
		try
		{
			SaveAll();
		}
		catch (Exception ex)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)("KeyRemappingSaverLoader could not save input mapping. Exception message : " + ex.Message), (Object)(object)((Component)this).gameObject, (CLogLevel)1, true, false);
			return;
		}
		((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)"KeyRemappingSaverLoader saved all user data to XML.", (CLogLevel)0, false, false);
	}

	public override void SaveControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEnabled)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader is disabled and will not save any data.", (Object)(object)this, (CLogLevel)1, true, false);
			return;
		}
		SaveControllerDataNow(playerId, controllerType, controllerId);
		((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)string.Format("{0} saved {1} {2} data for Player {3} to XML.", "KeyRemappingSaverLoader", controllerType, controllerId, playerId), (CLogLevel)1, false, false);
	}

	public override void SaveControllerData(ControllerType controllerType, int controllerId)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEnabled)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader is disabled and will not save any data.", (Object)(object)this, (CLogLevel)1, true, false);
			return;
		}
		SaveControllerDataNow(controllerType, controllerId);
		((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)string.Format("{0} saved {1} {2} data to XML.", "KeyRemappingSaverLoader", controllerType, controllerId), (CLogLevel)1, false, false);
	}

	public override void SavePlayerData(int playerId)
	{
		if (!IsEnabled)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader is disabled and will not save any data.", (Object)(object)this, (CLogLevel)1, true, false);
			return;
		}
		SavePlayerDataNow(playerId);
		((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)string.Format("{0} saved all user data for Player {1} to XML.", "KeyRemappingSaverLoader", playerId), (CLogLevel)1, false, false);
	}

	public override void SaveInputBehavior(int playerId, int behaviorId)
	{
		if (!IsEnabled)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader is disabled and will not save any data.", (Object)(object)this, (CLogLevel)1, true, false);
			return;
		}
		SaveInputBehaviorNow(playerId, behaviorId);
		((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)string.Format("{0} saved Input Behavior data for Player {1} to XML.", "KeyRemappingSaverLoader", playerId), (CLogLevel)1, false, false);
	}

	public override void Load()
	{
		((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)"KeyRemappingSaverLoader load.", (Object)(object)this, (CLogLevel)1, false, false);
		if (!IsEnabled)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader is disabled and will not load any data.", (Object)(object)this, (CLogLevel)1, true, false);
			return;
		}
		if (SaveManager.SettingsSaveVersion >= 9 && !KeyRemappingSaveExists)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)"KeyRemappingSaverLoader could not find xml file, trying to load from PlayerPrefs (this should be due to backward compatibility or if a new player loads for the first time).", (Object)(object)((Component)this).gameObject, (CLogLevel)0, false, false);
			((UserDataStore)playerPrefsSaverLoader).Load();
			playerPrefsSaverLoader.IsEnabled = false;
			return;
		}
		try
		{
			XDocument val = SaverLoader.LoadXml(KeyRemappingSaveFilePath);
			if (val != null)
			{
				loadElement = ((XContainer)val).Element(XName.op_Implicit("InputMappingSave"));
				if (loadElement == null)
				{
					((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader has found a save file but no KeyRemapping element has been found. \n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.", (CLogLevel)1, true, false);
				}
				else if (LoadAll() > 0)
				{
					((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)"KeyRemappingSaverLoader loaded all user data from XML. \n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.", (CLogLevel)1, true, false);
				}
			}
		}
		catch (Exception arg)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogError((object)$"Error caught while loading InputMappingSave, resetting remapping. Message: {arg}", (CLogLevel)2, true, true);
			TPSingleton<KeyRemappingManager>.Instance.ResetAll();
		}
	}

	public override void LoadControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEnabled)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader is disabled and will not load any data.", (Object)(object)this, (CLogLevel)1, true, false);
		}
		else if (LoadControllerDataNow(playerId, controllerType, controllerId) > 0)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)(string.Format("{0} loaded user data for {1} {2} for Player {3} from XML. ", "KeyRemappingSaverLoader", controllerType, controllerId, playerId) + "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component."), (CLogLevel)1, true, false);
		}
	}

	public override void LoadControllerData(ControllerType controllerType, int controllerId)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEnabled)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader is disabled and will not load any data.", (Object)(object)this, (CLogLevel)1, true, false);
		}
		else if (LoadControllerDataNow(controllerType, controllerId) > 0)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)(string.Format("{0} loaded user data for {1} {2} from XML. ", "KeyRemappingSaverLoader", controllerType, controllerId) + "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component."), (CLogLevel)1, true, false);
		}
	}

	public override void LoadPlayerData(int playerId)
	{
		if (!IsEnabled)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader is disabled and will not load any data.", (Object)(object)this, (CLogLevel)1, true, false);
		}
		else if (LoadPlayerDataNow(playerId) > 0)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)(string.Format("{0} loaded Player {1} user data from XML. ", "KeyRemappingSaverLoader", playerId) + "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component."), (CLogLevel)1, true, false);
		}
	}

	public override void LoadInputBehavior(int playerId, int behaviorId)
	{
		if (!IsEnabled)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader is disabled and will not load any data.", (Object)(object)this, (CLogLevel)1, true, false);
		}
		else if (LoadInputBehaviorNow(playerId, behaviorId) > 0)
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)(string.Format("{0} loaded Player {1} InputBehavior data from XML. ", "KeyRemappingSaverLoader", playerId) + "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component."), (CLogLevel)1, true, false);
		}
	}

	protected override void OnInitialize()
	{
		if (LoadDataOnStart)
		{
			((UserDataStore)this).Load();
			if (LoadControllerAssignments && ReInput.controllers.joystickCount > 0)
			{
				SaveControllerAssignments();
			}
		}
	}

	protected override void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if (IsEnabled && (int)args.controllerType == 2)
		{
			if (LoadJoystickData(args.controllerId) > 0)
			{
				((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)(string.Format("{0} loaded Joystick {1} ({2}) data from XML. ", "KeyRemappingSaverLoader", args.controllerId, ((Controller)ReInput.controllers.GetJoystick(args.controllerId)).hardwareName) + "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component."), (CLogLevel)1, true, false);
			}
			if (LoadDataOnStart && LoadJoystickAssignments && !wasJoystickEverDetected)
			{
				((MonoBehaviour)this).StartCoroutine(LoadJoystickAssignmentsDeferred());
			}
			if (LoadJoystickAssignments && !deferredJoystickAssignmentLoadPending)
			{
				SaveControllerAssignments();
			}
			wasJoystickEverDetected = true;
		}
	}

	protected override void OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if (IsEnabled && (int)args.controllerType == 2)
		{
			SaveJoystickData(args.controllerId);
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)string.Format("{0} saved Joystick {1} ({2}) data to XML.", "KeyRemappingSaverLoader", args.controllerId, ((Controller)ReInput.controllers.GetJoystick(args.controllerId)).hardwareName), (CLogLevel)1, false, false);
		}
	}

	protected override void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (IsEnabled && LoadControllerAssignments)
		{
			SaveControllerAssignments();
		}
	}

	public override void SaveControllerMap(int playerId, ControllerMap controllerMap)
	{
		if (controllerMap != null)
		{
			SaveControllerMap(controllerMap);
		}
	}

	public override ControllerMap LoadControllerMap(int playerId, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return null;
		}
		return LoadControllerMap(player, controllerIdentifier, categoryId, layoutId);
	}

	private int LoadAll()
	{
		int num = 0;
		if (LoadControllerAssignments && LoadControllerAssignmentsNow())
		{
			num++;
		}
		return num + LoadPlayerDataNow(TPSingleton<InputManager>.Instance.Player);
	}

	private int LoadPlayerDataNow(int playerId)
	{
		return LoadPlayerDataNow(ReInput.players.GetPlayer(playerId));
	}

	private int LoadPlayerDataNow(Player player)
	{
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		num += LoadInputBehaviors(player.id);
		num += LoadControllerMaps(player.id, (ControllerType)0, 0);
		num += LoadControllerMaps(player.id, (ControllerType)1, 0);
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			num += LoadControllerMaps(player.id, (ControllerType)2, ((Controller)joystick).id);
		}
		RefreshLayoutManager(player.id);
		return num;
	}

	private int LoadAllJoystickCalibrationData()
	{
		int num = 0;
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			num += LoadJoystickCalibrationData(joysticks[i]);
		}
		return num;
	}

	private int LoadJoystickCalibrationData(Joystick joystick)
	{
		if (joystick == null)
		{
			return 0;
		}
		if (!((ControllerWithAxes)joystick).ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(joystick)))
		{
			return 0;
		}
		return 1;
	}

	private int LoadJoystickCalibrationData(int joystickId)
	{
		return LoadJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	private int LoadJoystickData(int joystickId)
	{
		int num = 0;
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player val = allPlayers[i];
			if (val.controllers.ContainsController((ControllerType)2, joystickId))
			{
				num += LoadControllerMaps(val.id, (ControllerType)2, joystickId);
				RefreshLayoutManager(val.id);
			}
		}
		return num + LoadJoystickCalibrationData(joystickId);
	}

	private int LoadControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		int num = 0 + LoadControllerMaps(playerId, controllerType, controllerId);
		RefreshLayoutManager(playerId);
		return num + LoadControllerDataNow(controllerType, controllerId);
	}

	private int LoadControllerDataNow(ControllerType controllerType, int controllerId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		int num = 0;
		if ((int)controllerType == 2)
		{
			num += LoadJoystickCalibrationData(controllerId);
		}
		return num;
	}

	private int LoadControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return num;
		}
		Controller controller = ReInput.controllers.GetController(controllerType, controllerId);
		if (controller == null)
		{
			return num;
		}
		IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
		for (int i = 0; i < mapCategories.Count; i++)
		{
			InputMapCategory val = mapCategories[i];
			if (!((InputCategory)val).userAssignable)
			{
				continue;
			}
			IList<InputLayout> list = ReInput.mapping.MapLayouts(controller.type);
			for (int j = 0; j < list.Count; j++)
			{
				InputLayout val2 = list[j];
				ControllerMap val3 = LoadControllerMap(player, controller.identifier, ((InputCategory)val).id, val2.id);
				if (val3 != null)
				{
					player.controllers.maps.AddMap(controller, val3);
					num++;
				}
			}
		}
		return num;
	}

	private ControllerMap LoadControllerMap(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (player == null)
		{
			return null;
		}
		string controllerMapXml = GetControllerMapXml(controllerIdentifier, categoryId, layoutId);
		if (string.IsNullOrEmpty(controllerMapXml))
		{
			return null;
		}
		ControllerMap val = ControllerMap.CreateFromXml(((ControllerIdentifier)(ref controllerIdentifier)).controllerType, controllerMapXml);
		if (val == null)
		{
			return null;
		}
		List<int> controllerMapKnownActionIds = GetControllerMapKnownActionIds(player, controllerIdentifier, categoryId, layoutId);
		AddDefaultMappingsForNewActions(controllerIdentifier, val, controllerMapKnownActionIds);
		return val;
	}

	private int LoadInputBehaviors(int playerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		IList<InputBehavior> inputBehaviors = ReInput.mapping.GetInputBehaviors(player.id);
		for (int i = 0; i < inputBehaviors.Count; i++)
		{
			num += LoadInputBehaviorNow(inputBehaviors[i]);
		}
		return num;
	}

	private int LoadInputBehaviorNow(int playerId, int behaviorId)
	{
		if (ReInput.players.GetPlayer(playerId) == null)
		{
			return 0;
		}
		InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
		if (inputBehavior == null)
		{
			return 0;
		}
		return LoadInputBehaviorNow(inputBehavior);
	}

	private int LoadInputBehaviorNow(InputBehavior inputBehavior)
	{
		if (inputBehavior == null)
		{
			return 0;
		}
		string inputBehaviorXml = GetInputBehaviorXml(inputBehavior.id);
		if (string.IsNullOrEmpty(inputBehaviorXml))
		{
			return 0;
		}
		if (!inputBehavior.ImportXmlString(inputBehaviorXml))
		{
			return 0;
		}
		return 1;
	}

	private bool LoadControllerAssignmentsNow()
	{
		try
		{
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = LoadControllerAssignmentData();
			if (controllerAssignmentSaveInfo == null)
			{
				return false;
			}
			if (loadKeyboardAssignments || loadMouseAssignments)
			{
				LoadKeyboardAndMouseAssignmentsNow(controllerAssignmentSaveInfo);
			}
			if (loadJoystickAssignments)
			{
				LoadJoystickAssignmentsNow(controllerAssignmentSaveInfo);
			}
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader loaded controller assignments from PlayerPrefs.", (CLogLevel)1, true, false);
		}
		catch
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogError((object)"KeyRemappingSaverLoader encountered an error loading controller assignments from PlayerPrefs.", (CLogLevel)1, true, true);
		}
		return true;
	}

	private bool LoadKeyboardAndMouseAssignmentsNow(ControllerAssignmentSaveInfo data)
	{
		try
		{
			if (data == null && (data = LoadControllerAssignmentData()) == null)
			{
				return false;
			}
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				if (data.ContainsPlayer(allPlayer.id))
				{
					ControllerAssignmentSaveInfo.PlayerInfo playerInfo = data.Players[data.IndexOfPlayer(allPlayer.id)];
					if (loadKeyboardAssignments)
					{
						allPlayer.controllers.hasKeyboard = playerInfo.HasKeyboard;
					}
					if (loadMouseAssignments)
					{
						allPlayer.controllers.hasMouse = playerInfo.HasMouse;
					}
				}
			}
		}
		catch
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogError((object)"KeyRemappingSaverLoader encountered an error loading keyboard and/or mouse assignments from PlayerPrefs.", (CLogLevel)1, true, true);
		}
		return true;
	}

	private bool LoadJoystickAssignmentsNow(ControllerAssignmentSaveInfo data)
	{
		try
		{
			if (ReInput.controllers.joystickCount == 0)
			{
				return false;
			}
			if (data == null && (data = LoadControllerAssignmentData()) == null)
			{
				return false;
			}
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				allPlayer.controllers.ClearControllersOfType((ControllerType)2);
			}
			List<JoystickAssignmentHistoryInfo> list = (loadJoystickAssignments ? new List<JoystickAssignmentHistoryInfo>() : null);
			foreach (Player allPlayer2 in ReInput.players.AllPlayers)
			{
				if (!data.ContainsPlayer(allPlayer2.id))
				{
					continue;
				}
				ControllerAssignmentSaveInfo.PlayerInfo playerInfo = data.Players[data.IndexOfPlayer(allPlayer2.id)];
				for (int i = 0; i < playerInfo.JoystickCount; i++)
				{
					ControllerAssignmentSaveInfo.JoystickInfo joystickInfo2 = playerInfo.Joysticks[i];
					if (joystickInfo2 == null)
					{
						continue;
					}
					Joystick joystick = FindJoystickPrecise(joystickInfo2);
					if (joystick != null)
					{
						if (list.Find((JoystickAssignmentHistoryInfo x) => x.Joystick == joystick) == null)
						{
							list.Add(new JoystickAssignmentHistoryInfo(joystick, joystickInfo2.Id));
						}
						allPlayer2.controllers.AddController((Controller)(object)joystick, false);
					}
				}
			}
			if (allowImpreciseJoystickAssignmentMatching)
			{
				foreach (Player allPlayer3 in ReInput.players.AllPlayers)
				{
					if (!data.ContainsPlayer(allPlayer3.id))
					{
						continue;
					}
					ControllerAssignmentSaveInfo.PlayerInfo playerInfo2 = data.Players[data.IndexOfPlayer(allPlayer3.id)];
					for (int j = 0; j < playerInfo2.JoystickCount; j++)
					{
						ControllerAssignmentSaveInfo.JoystickInfo joystickInfo = playerInfo2.Joysticks[j];
						if (joystickInfo == null)
						{
							continue;
						}
						Joystick val = null;
						int num = list.FindIndex((JoystickAssignmentHistoryInfo x) => x.OldJoystickId == joystickInfo.Id);
						if (num >= 0)
						{
							val = list[num].Joystick;
						}
						else
						{
							if (!TryFindJoysticksImprecise(joystickInfo, out var matches))
							{
								continue;
							}
							foreach (Joystick match in matches)
							{
								if (list.Find((JoystickAssignmentHistoryInfo x) => x.Joystick == match) == null)
								{
									val = match;
									break;
								}
							}
							if (val == null)
							{
								continue;
							}
							list.Add(new JoystickAssignmentHistoryInfo(val, joystickInfo.Id));
						}
						allPlayer3.controllers.AddController((Controller)(object)val, false);
					}
				}
			}
		}
		catch
		{
		}
		if (ReInput.configuration.autoAssignJoysticks)
		{
			ReInput.controllers.AutoAssignJoysticks();
		}
		return true;
	}

	private ControllerAssignmentSaveInfo LoadControllerAssignmentData()
	{
		try
		{
			if (!PlayerPrefs.HasKey(PlayerPrefsKeyControllerAssignments))
			{
				return null;
			}
			string @string = PlayerPrefs.GetString(PlayerPrefsKeyControllerAssignments);
			if (string.IsNullOrEmpty(@string))
			{
				return null;
			}
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = JsonParser.FromJson<ControllerAssignmentSaveInfo>(@string);
			if (controllerAssignmentSaveInfo == null || controllerAssignmentSaveInfo.PlayerCount == 0)
			{
				return null;
			}
			return controllerAssignmentSaveInfo;
		}
		catch
		{
			return null;
		}
	}

	private IEnumerator LoadJoystickAssignmentsDeferred()
	{
		deferredJoystickAssignmentLoadPending = true;
		yield return (object)new WaitForEndOfFrame();
		if (ReInput.isReady)
		{
			if (LoadJoystickAssignmentsNow(null))
			{
				((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)"KeyRemappingSaverLoader loaded joystick assignments from PlayerPrefs.", (CLogLevel)1, true, false);
			}
			SaveControllerAssignments();
			deferredJoystickAssignmentLoadPending = false;
		}
	}

	private void SaveAll()
	{
		SavePlayerDataNow(TPSingleton<InputManager>.Instance.Player);
		SaverLoader.SaveXmlSync((XContainer)(object)saveElement, KeyRemappingSaveFilePath);
	}

	private void SavePlayerDataNow(int playerId)
	{
		SavePlayerDataNow(ReInput.players.GetPlayer(playerId));
		PlayerPrefs.Save();
	}

	private void SavePlayerDataNow(Player player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (player != null)
		{
			PlayerSaveData saveData = player.GetSaveData(true);
			SaveInputBehaviors(player, saveData);
			SaveControllerMaps(player, saveData);
		}
	}

	private void SaveAllJoystickCalibrationData()
	{
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			SaveJoystickCalibrationData(joysticks[i]);
		}
	}

	private void SaveJoystickCalibrationData(int joystickId)
	{
		SaveJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	private void SaveJoystickCalibrationData(Joystick joystick)
	{
		if (joystick != null)
		{
			JoystickCalibrationMapSaveData calibrationMapSaveData = joystick.GetCalibrationMapSaveData();
			PlayerPrefs.SetString(GetJoystickCalibrationMapPlayerPrefsKey(joystick), ((CalibrationMapSaveData)calibrationMapSaveData).map.ToXmlString());
		}
	}

	private void SaveJoystickData(int joystickId)
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player val = allPlayers[i];
			if (val.controllers.ContainsController((ControllerType)2, joystickId))
			{
				SaveControllerMaps(val.id, (ControllerType)2, joystickId);
			}
		}
		SaveJoystickCalibrationData(joystickId);
	}

	private void SaveControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		SaveControllerMaps(playerId, controllerType, controllerId);
		SaveControllerDataNow(controllerType, controllerId);
		PlayerPrefs.Save();
	}

	private void SaveControllerDataNow(ControllerType controllerType, int controllerId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)controllerType == 2)
		{
			SaveJoystickCalibrationData(controllerId);
		}
		PlayerPrefs.Save();
	}

	private void SaveControllerMaps(Player player, PlayerSaveData playerSaveData)
	{
		foreach (ControllerMapSaveData allControllerMapSaveDatum in ((PlayerSaveData)(ref playerSaveData)).AllControllerMapSaveData)
		{
			SaveControllerMap(allControllerMapSaveDatum.map);
		}
	}

	private void SaveControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null || !player.controllers.ContainsController(controllerType, controllerId))
		{
			return;
		}
		ControllerMapSaveData[] mapSaveData = player.controllers.maps.GetMapSaveData(controllerType, controllerId, true);
		if (mapSaveData != null)
		{
			for (int i = 0; i < mapSaveData.Length; i++)
			{
				SaveControllerMap(mapSaveData[i].map);
			}
		}
	}

	private void SaveControllerMap(ControllerMap controllerMap)
	{
		XDocument val = XDocument.Parse(controllerMap.ToXmlString());
		((XContainer)saveElement).Add((object)((XContainer)val).Elements().First());
	}

	private void SaveInputBehaviors(Player player, PlayerSaveData playerSaveData)
	{
		if (player != null)
		{
			InputBehavior[] inputBehaviors = ((PlayerSaveData)(ref playerSaveData)).inputBehaviors;
			for (int i = 0; i < inputBehaviors.Length; i++)
			{
				SaveInputBehaviorNow(inputBehaviors[i]);
			}
		}
	}

	private void SaveInputBehaviorNow(int playerId, int behaviorId)
	{
		if (ReInput.players.GetPlayer(playerId) != null)
		{
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
			if (inputBehavior != null)
			{
				SaveInputBehaviorNow(inputBehavior);
			}
		}
	}

	private void SaveInputBehaviorNow(InputBehavior inputBehavior)
	{
		if (inputBehavior != null)
		{
			XDocument val = XDocument.Parse(inputBehavior.ToXmlString());
			((XContainer)saveElement).Add((object)((XContainer)val).Elements().First());
		}
	}

	private bool SaveControllerAssignments()
	{
		try
		{
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = new ControllerAssignmentSaveInfo(ReInput.players.allPlayerCount);
			for (int i = 0; i < ReInput.players.allPlayerCount; i++)
			{
				Player val = ReInput.players.AllPlayers[i];
				ControllerAssignmentSaveInfo.PlayerInfo playerInfo = new ControllerAssignmentSaveInfo.PlayerInfo();
				controllerAssignmentSaveInfo.Players[i] = playerInfo;
				playerInfo.Id = val.id;
				playerInfo.HasKeyboard = val.controllers.hasKeyboard;
				playerInfo.HasMouse = val.controllers.hasMouse;
				ControllerAssignmentSaveInfo.JoystickInfo[] array2 = (playerInfo.Joysticks = new ControllerAssignmentSaveInfo.JoystickInfo[val.controllers.joystickCount]);
				for (int j = 0; j < val.controllers.joystickCount; j++)
				{
					Joystick val2 = val.controllers.Joysticks[j];
					ControllerAssignmentSaveInfo.JoystickInfo joystickInfo = new ControllerAssignmentSaveInfo.JoystickInfo
					{
						InstanceGuid = ((Controller)val2).deviceInstanceGuid,
						Id = ((Controller)val2).id,
						HardwareIdentifier = ((Controller)val2).hardwareIdentifier
					};
					array2[j] = joystickInfo;
				}
			}
			PlayerPrefs.SetString(PlayerPrefsKeyControllerAssignments, JsonWriter.ToJson((object)controllerAssignmentSaveInfo));
			PlayerPrefs.Save();
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).Log((object)"KeyRemappingSaverLoader saved controller assignments to PlayerPrefs.", (CLogLevel)1, false, false);
		}
		catch
		{
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogError((object)"KeyRemappingSaverLoader encountered an error saving controller assignments to PlayerPrefs.", (CLogLevel)1, true, true);
		}
		return true;
	}

	private bool ControllerAssignmentSaveDataExists()
	{
		if (!PlayerPrefs.HasKey(PlayerPrefsKeyControllerAssignments))
		{
			return false;
		}
		if (string.IsNullOrEmpty(PlayerPrefs.GetString(PlayerPrefsKeyControllerAssignments)))
		{
			return false;
		}
		return true;
	}

	private string GetBasePlayerPrefsKey(Player player)
	{
		return playerPrefsKeyPrefix + "|playerName=" + player.name;
	}

	private string GetControllerMapKnownActionIdsPlayerPrefsKey(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return string.Concat(GetBasePlayerPrefsKey(player) + "|dataType=ControllerMap_KnownActionIds", GetControllerMapPlayerPrefsKeyCommonSuffix(player, controllerIdentifier, categoryId, layoutId, ppKeyVersion));
	}

	private static string GetControllerMapPlayerPrefsKeyCommonSuffix(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Invalid comparison between Unknown and I4
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Invalid comparison between Unknown and I4
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		string text = string.Empty;
		if (ppKeyVersion >= 2)
		{
			text = text + "|kv=" + ppKeyVersion;
		}
		text = text + "|controllerMapType=" + GetControllerMapType(((ControllerIdentifier)(ref controllerIdentifier)).controllerType).Name;
		text = text + "|categoryId=" + categoryId + "|layoutId=" + layoutId;
		if (ppKeyVersion >= 2)
		{
			text = text + "|hardwareGuid=" + ((ControllerIdentifier)(ref controllerIdentifier)).hardwareTypeGuid;
			if (((ControllerIdentifier)(ref controllerIdentifier)).hardwareTypeGuid == Guid.Empty)
			{
				text = text + "|hardwareIdentifier=" + ((ControllerIdentifier)(ref controllerIdentifier)).hardwareIdentifier;
			}
			if ((int)((ControllerIdentifier)(ref controllerIdentifier)).controllerType == 2)
			{
				text = text + "|duplicate=" + GetDuplicateIndex(player, controllerIdentifier);
			}
		}
		else
		{
			text = text + "|hardwareIdentifier=" + ((ControllerIdentifier)(ref controllerIdentifier)).hardwareIdentifier;
			if ((int)((ControllerIdentifier)(ref controllerIdentifier)).controllerType == 2)
			{
				text = text + "|hardwareGuid=" + ((ControllerIdentifier)(ref controllerIdentifier)).hardwareTypeGuid;
				if (ppKeyVersion >= 1)
				{
					text = text + "|duplicate=" + GetDuplicateIndex(player, controllerIdentifier);
				}
			}
		}
		return text;
	}

	private string GetJoystickCalibrationMapPlayerPrefsKey(Joystick joystick)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		string text = playerPrefsKeyPrefix + "|dataType=CalibrationMap";
		ControllerType type = ((Controller)joystick).type;
		return string.Concat(string.Concat(text + "|controllerType=" + ((object)(ControllerType)(ref type)).ToString(), "|hardwareIdentifier=", ((Controller)joystick).hardwareIdentifier), "|hardwareGuid=", ((Controller)joystick).hardwareTypeGuid.ToString());
	}

	private string GetControllerMapXml(ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		foreach (XElement item in ((XContainer)loadElement).Elements().Where(delegate(XElement o)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			string localName = o.Name.LocalName;
			ControllerType controllerType = ((ControllerIdentifier)(ref controllerIdentifier)).controllerType;
			return localName.Contains(((object)(ControllerType)(ref controllerType)).ToString());
		}))
		{
			XElement val = ((XContainer)item).Element(Constants.XmlNamespace + "categoryId");
			XElement val2 = ((XContainer)item).Element(Constants.XmlNamespace + "layoutId");
			if (val.Value == categoryId.ToString() && val2.Value == layoutId.ToString())
			{
				return ((object)item).ToString();
			}
		}
		return null;
	}

	private List<int> GetControllerMapKnownActionIds(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		List<int> list = new List<int>();
		string text = null;
		bool flag = false;
		for (int num = 2; num >= 0; num--)
		{
			text = GetControllerMapKnownActionIdsPlayerPrefsKey(player, controllerIdentifier, categoryId, layoutId, num);
			if (PlayerPrefs.HasKey(text))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return list;
		}
		string @string = PlayerPrefs.GetString(text);
		if (string.IsNullOrEmpty(@string))
		{
			return list;
		}
		string[] array = @string.Split(new char[1] { ',' });
		for (int i = 0; i < array.Length; i++)
		{
			if (!string.IsNullOrEmpty(array[i]) && int.TryParse(array[i], out var result))
			{
				list.Add(result);
			}
		}
		return list;
	}

	private string GetJoystickCalibrationMapXml(Joystick joystick)
	{
		string joystickCalibrationMapPlayerPrefsKey = GetJoystickCalibrationMapPlayerPrefsKey(joystick);
		if (!PlayerPrefs.HasKey(joystickCalibrationMapPlayerPrefsKey))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(joystickCalibrationMapPlayerPrefsKey);
	}

	private string GetInputBehaviorXml(int id)
	{
		return ((object)(from o in ((XContainer)loadElement).Elements(Constants.XmlNamespace + "InputBehavior")
			where ((XContainer)o).Element(Constants.XmlNamespace + "id").Value == id.ToString()
			select o).FirstOrDefault())?.ToString() ?? string.Empty;
	}

	private void AddDefaultMappingsForNewActions(ControllerIdentifier controllerIdentifier, ControllerMap controllerMap, List<int> knownActionIds)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		if (controllerMap == null || knownActionIds == null || knownActionIds == null || knownActionIds.Count == 0)
		{
			return;
		}
		ControllerMap controllerMapInstance = ReInput.mapping.GetControllerMapInstance(controllerIdentifier, controllerMap.categoryId, controllerMap.layoutId);
		if (controllerMapInstance == null)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (int allActionId in AllActionIds)
		{
			if (!knownActionIds.Contains(allActionId))
			{
				list.Add(allActionId);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		ElementAssignment val = default(ElementAssignment);
		foreach (ActionElementMap allMap in controllerMapInstance.AllMaps)
		{
			if (list.Contains(allMap.actionId) && !controllerMap.DoesElementAssignmentConflict(allMap))
			{
				((ElementAssignment)(ref val))._002Ector(controllerMap.controllerType, allMap.elementType, allMap.elementIdentifierId, allMap.axisRange, allMap.keyCode, allMap.modifierKeyFlags, allMap.actionId, allMap.axisContribution, allMap.invert);
				controllerMap.CreateElementMap(val);
			}
		}
	}

	private Joystick FindJoystickPrecise(ControllerAssignmentSaveInfo.JoystickInfo joystickInfo)
	{
		if (joystickInfo == null)
		{
			return null;
		}
		if (joystickInfo.InstanceGuid == Guid.Empty)
		{
			return null;
		}
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			if (((Controller)joysticks[i]).deviceInstanceGuid == joystickInfo.InstanceGuid)
			{
				return joysticks[i];
			}
		}
		return null;
	}

	private bool TryFindJoysticksImprecise(ControllerAssignmentSaveInfo.JoystickInfo joystickInfo, out List<Joystick> matches)
	{
		matches = null;
		if (joystickInfo == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(joystickInfo.HardwareIdentifier))
		{
			return false;
		}
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			if (string.Equals(((Controller)joysticks[i]).hardwareIdentifier, joystickInfo.HardwareIdentifier, StringComparison.OrdinalIgnoreCase))
			{
				if (matches == null)
				{
					matches = new List<Joystick>();
				}
				matches.Add(joysticks[i]);
			}
		}
		return matches != null;
	}

	private static int GetDuplicateIndex(Player player, ControllerIdentifier controllerIdentifier)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		Controller controller = ReInput.controllers.GetController(controllerIdentifier);
		if (controller == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Controller controller2 in player.controllers.Controllers)
		{
			if (controller2.type != controller.type)
			{
				continue;
			}
			bool flag = false;
			if ((int)controller.type == 2)
			{
				if (((controller2 is Joystick) ? controller2 : null).hardwareTypeGuid != controller.hardwareTypeGuid)
				{
					continue;
				}
				if (controller.hardwareTypeGuid != Guid.Empty)
				{
					flag = true;
				}
			}
			if (flag || !(controller2.hardwareIdentifier != controller.hardwareIdentifier))
			{
				if (controller2 == controller)
				{
					return num;
				}
				num++;
			}
		}
		return num;
	}

	private void RefreshLayoutManager(int playerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player != null)
		{
			player.controllers.maps.layoutManager.Apply();
		}
	}

	private static Type GetControllerMapType(ControllerType controllerType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		switch ((int)controllerType)
		{
		default:
			if ((int)controllerType == 20)
			{
				return typeof(CustomControllerMap);
			}
			((CLogger<InputManager>)TPSingleton<InputManager>.Instance).LogWarning((object)$" Unknown ControllerType {controllerType}", (CLogLevel)1, true, false);
			return null;
		case 2:
			return typeof(JoystickMap);
		case 0:
			return typeof(KeyboardMap);
		case 1:
			return typeof(MouseMap);
		}
	}
}
