using System;
using System.IO;
using Steamworks;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Encryption;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using TheLastStand.Serialization;
using TheLastStand.View.Menus;
using UnityEngine;

namespace TheLastStand.Manager;

public class SaveManager : Manager<SaveManager>
{
	public enum E_BrokenSaveReason
	{
		UNKNOWN,
		WRONG_VERSION,
		LOADING_ERROR,
		FILE_NOT_FOUND,
		MISSING_MOD
	}

	public class Constants
	{
		public const string SaveFolderName = "Save";

		public const string SteamSaveSubFolderName = "Steam";

		public const string LocalSaveSubFolderName = "Local";

		public const string BackupFolderName = "Backups";

		public const string BackupMigrationFolderName = "BackupMigration";
	}

	private const int MaxProfileAmount = 4;

	private static string persistentDataPath;

	[SerializeField]
	private int appVersionTriggeringMigration = 8;

	[SerializeField]
	[Range(1f, 4f)]
	private int profile = 1;

	[SerializeField]
	[Tooltip("Application Save version")]
	private byte appSaveVersion = 1;

	[SerializeField]
	[Tooltip("Game Save version")]
	private byte gameSaveVersion = 1;

	[SerializeField]
	[Tooltip("Settings Save version")]
	private byte settingsSaveVersion = 1;

	[SerializeField]
	[Tooltip("Minimum supported Application save version")]
	private int minimumAppSaveVersion = 1;

	[SerializeField]
	[Tooltip("Minimum supported Game save version")]
	private int minimumGameSaveVersion = 1;

	[SerializeField]
	[Tooltip("Minimum supported Settings save version")]
	private int minimumSettingsSaveVersion = 1;

	[SerializeField]
	[Tooltip("Disable save file encryption")]
	private bool disableSaveEncryption;

	[SerializeField]
	[Tooltip("Save serialization format")]
	private SaverLoader.E_SaveFormat saveFormat;

	[SerializeField]
	[Tooltip("Launching application with one of those version will result in trying to backup save files")]
	private int[] appVersionsTriggeringBackup;

	[SerializeField]
	[Tooltip("Don't compare version of current save")]
	private bool debugDontCompareFileVersion;

	[SerializeField]
	private DevConsole devConsole;

	public static bool NewlyPreloadedGameSave;

	public static bool AppSaveExists
	{
		get
		{
			if (!File.Exists(AppSaveFilePath))
			{
				return File.Exists(AppSaveBackupFilePath);
			}
			return true;
		}
	}

	public static string AppSaveFilePath => PersistentDataPath + "/Save/" + GetSaveSubFolderPath() + "/" + CurrentStringProfile + "/AppSave." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");

	public static string AppSaveBackupFilePath => BackupFolderPath + "/AppSave__BACKUP." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");

	public static string AppSaveTemporaryBackupFilePath => BackupFolderPath + "/AppSave__BACKUP_TMP." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");

	public static byte AppSaveVersion => TPSingleton<SaveManager>.Instance.appSaveVersion;

	public static string BackupFolderPath => PersistentDataPath + "/Save/" + GetSaveSubFolderPath() + "/" + CurrentStringProfile + "/Backups";

	public static string BrokenSavePath { get; set; }

	public static E_BrokenSaveReason BrokenSaveReason { get; set; }

	public static bool BackupLoaded { get; set; }

	public static int CurrentProfileIndex
	{
		get
		{
			return TPSingleton<SaveManager>.Instance.profile;
		}
		set
		{
			TPSingleton<SaveManager>.Instance.profile = value;
		}
	}

	public static string CurrentStringProfile => $"Profile{CurrentProfileIndex}";

	public static string GameSaveBackupFilePath => BackupFolderPath + "/GameSave__BACKUP." + (TPSingleton<SaveManager>.Instance.disableSaveEncryption ? TPSingleton<SaveManager>.Instance.saveFormat.ToString().ToLower() : "bin");

	public static string GameSaveTemporaryBackupFilePath => BackupFolderPath + "/GameSave__BACKUP_TMP." + (TPSingleton<SaveManager>.Instance.disableSaveEncryption ? TPSingleton<SaveManager>.Instance.saveFormat.ToString().ToLower() : "bin");

	public static bool GameSaveExists
	{
		get
		{
			if (!File.Exists(GameSaveFilePath))
			{
				return File.Exists(GameSaveBackupFilePath);
			}
			return true;
		}
	}

	public static string GameSaveFilePath => PersistentDataPath + "/Save/" + GetSaveSubFolderPath() + "/" + CurrentStringProfile + "/GameSave." + (TPSingleton<SaveManager>.Instance.disableSaveEncryption ? TPSingleton<SaveManager>.Instance.saveFormat.ToString().ToLower() : "bin");

	public static byte GameSaveVersion => TPSingleton<SaveManager>.Instance.gameSaveVersion;

	public static bool IsSaveEncryptionDisabled => TPSingleton<SaveManager>.Instance.disableSaveEncryption;

	public static int MinimumSupportedAppSaveVersion => TPSingleton<SaveManager>.Instance.minimumAppSaveVersion;

	public static int MinimumSupportedGameSaveVersion => TPSingleton<SaveManager>.Instance.minimumGameSaveVersion;

	public static int MinimumSupportedSettingsSaveVersion => TPSingleton<SaveManager>.Instance.minimumSettingsSaveVersion;

	public static string PersistentDataPath
	{
		get
		{
			if (string.IsNullOrEmpty(persistentDataPath))
			{
				persistentDataPath = Application.persistentDataPath;
			}
			return persistentDataPath;
		}
	}

	public static string SettingsFilePath => string.Format("{0}/{1}/{2}/SettingsSave.{3}", PersistentDataPath, "Save", GetSaveSubFolderPath(), SaveFormat);

	public static bool SettingsSaveExists => File.Exists(SettingsFilePath);

	public static byte SettingsSaveVersion => TPSingleton<SaveManager>.Instance.settingsSaveVersion;

	public static SaverLoader.E_SaveFormat SaveFormat => TPSingleton<SaveManager>.Instance.saveFormat;

	public string FormerAppSaveFilePath => PersistentDataPath + "/Save/AppSave." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");

	public bool FormerAppSaveExists => File.Exists(FormerAppSaveFilePath);

	public string FormerGameSaveFilePath => PersistentDataPath + "/Save/GameSave." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");

	public bool FormerGameSaveExists => File.Exists(FormerGameSaveFilePath);

	public string FormerKeyRemappingSaveFilePath => PersistentDataPath + "/Save/InputMappingSave.xml";

	public bool FormerKeyRemappingSaveExists => File.Exists(FormerKeyRemappingSaveFilePath);

	public string FormerSettingSaveFilePath => PersistentDataPath + string.Format("/{0}/SettingsSave.{1}", "Save", SaveFormat);

	public bool FormerSettingSaveExists => File.Exists(FormerSettingSaveFilePath);

	public SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> PreloadedGameSave { get; private set; }

	public static void BackupSaveFilesBeforeVersionUpdate()
	{
		if (!DoesAppVersionTriggeredBackup() || !AppSaveExists)
		{
			return;
		}
		try
		{
			SerializedApplicationState serializedApplicationState = SaverLoader.Load<SerializedApplicationState>(AppSaveFilePath, !IsSaveEncryptionDisabled);
			if (!TPSingleton<SaveManager>.Instance.debugDontCompareFileVersion && serializedApplicationState.SaveVersion >= AppSaveVersion)
			{
				return;
			}
			string text = $"{serializedApplicationState.GameMajorVersion}.{serializedApplicationState.GameMinorVersion}.{serializedApplicationState.GamePatchVersion}.{serializedApplicationState.GameHotfixVersion}";
			string text2 = PersistentDataPath + "/Save/" + GetSaveSubFolderPath() + "/Backups/Backup_v" + text;
			bool flag = Directory.Exists(text2);
			if (flag && Directory.GetFiles(text2).Length != 0)
			{
				return;
			}
			((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).Log((object)("Backing up save files from version " + text + "."), (CLogLevel)0, true, false);
			if (!flag)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(text2));
			}
			using (File.Open(AppSaveFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				SaverLoader.CopyFileTo(AppSaveFilePath, text2 + "/AppSave." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin"));
			}
			if (GameSaveExists)
			{
				using (File.Open(GameSaveFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					SaverLoader.CopyFileTo(GameSaveFilePath, text2 + "/GameSave." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin"));
					return;
				}
			}
		}
		catch (Exception ex)
		{
			((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).LogError((object)("Caught exception while backing up save files after major update. Error Message : " + ex.Message), (CLogLevel)2, true, false);
		}
	}

	public static void ChangeCurrentProfile(int profileIndex)
	{
		if (CurrentProfileIndex != profileIndex)
		{
			Save();
			TPSingleton<MetaConditionManager>.Instance.EraseConditionsControllers();
			CurrentProfileIndex = Mathf.Clamp(profileIndex, 1, 4);
			((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).Log((object)("Profile changed to : " + CurrentStringProfile), (CLogLevel)2, false, false);
			Load();
			if (!AppSaveExists)
			{
				Save();
			}
			if (ScenesManager.IsSceneActive(ScenesManager.MainMenuSceneName))
			{
				TPSingleton<MainMenuView>.Instance.Refresh();
			}
			SettingsManager.Save();
		}
	}

	public static string CorruptAppSave()
	{
		string result = SaverLoader.MarkFileAsCorrupted(AppSaveFilePath);
		SaverLoader.MarkFileAsCorrupted(AppSaveBackupFilePath);
		CorruptGameSave();
		return result;
	}

	public static string CorruptGameSave()
	{
		string result = SaverLoader.MarkFileAsCorrupted(GameSaveFilePath);
		SaverLoader.MarkFileAsCorrupted(GameSaveBackupFilePath);
		return result;
	}

	private static bool DoesAppVersionTriggeredBackup()
	{
		bool result = false;
		for (int i = 0; i < TPSingleton<SaveManager>.Instance.appVersionsTriggeringBackup.Length; i++)
		{
			if (TPSingleton<SaveManager>.Instance.appVersionsTriggeringBackup[i] == AppSaveVersion)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static void EraseSave()
	{
		SaverLoader.Erase(AppSaveFilePath);
		SaverLoader.Erase(AppSaveBackupFilePath);
		GameManager.EraseSave();
		TPSingleton<MetaConditionManager>.Instance.EraseConditionsControllers();
	}

	public static E_BrokenSaveReason? GetBrokenSaveReasonFromException(Exception e)
	{
		if (e == null)
		{
			return null;
		}
		return (e is SaverLoader.FileDoesNotExistException) ? E_BrokenSaveReason.FILE_NOT_FOUND : ((e is SaverLoader.WrongSaveVersionException) ? E_BrokenSaveReason.WRONG_VERSION : ((e is SaverLoader.MissingModException) ? E_BrokenSaveReason.MISSING_MOD : ((e is SaverLoader.SaveLoadingFailedException) ? E_BrokenSaveReason.LOADING_ERROR : E_BrokenSaveReason.UNKNOWN)));
	}

	public static string GetSaveSubFolderPath()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return string.Format("{0}/{1}", "Steam", SteamUser.GetSteamID());
	}

	public static void Load()
	{
		SerializedApplicationState serializedApplicationState = null;
		if (AppSaveExists)
		{
			try
			{
				try
				{
					serializedApplicationState = TPSingleton<ApplicationManager>.Instance.TryLoad();
				}
				catch (Exception e)
				{
					serializedApplicationState = TPSingleton<ApplicationManager>.Instance.TryLoadBackup(e);
				}
			}
			catch (SaverLoader.WrongSaveVersionException ex)
			{
				((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).LogError((object)(ex.FilePath + ": Loading failed because of a wrong version, marking game save file corrupted as well"), (CLogLevel)2, true, false);
				BrokenSavePath = ex.FilePath;
				CorruptAppSave();
				serializedApplicationState = null;
			}
			catch (SaverLoader.FileDoesNotExistException ex2)
			{
				((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).LogError((object)(ex2.FilePath + ": Loading failed because of files were not found, marking game save file corrupted as well"), (CLogLevel)2, true, false);
				BrokenSavePath = ex2.FilePath;
				BrokenSaveReason = E_BrokenSaveReason.FILE_NOT_FOUND;
				CorruptAppSave();
				serializedApplicationState = null;
			}
			catch (SaverLoader.SaveLoadingFailedException ex3)
			{
				((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).LogError((object)(ex3.FilePath + ": Could not load the Application save, marking game save file corrupted as well"), (CLogLevel)2, true, false);
				BrokenSavePath = ex3.FilePath;
				CorruptAppSave();
				serializedApplicationState = null;
			}
			catch (Exception ex4)
			{
				((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).LogError((object)("Caught exception while loading appsave. Error Message : " + ex4.Message), (CLogLevel)2, true, false);
				((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).LogError((object)$"A critical error occured while loading the appsave\n{ex4}\nPlease catch this specific exception earlier on and add a proper message for it.", (CLogLevel)0, true, true);
				BrokenSavePath = CorruptAppSave();
				BrokenSaveReason = E_BrokenSaveReason.UNKNOWN;
				serializedApplicationState = null;
			}
		}
		TPSingleton<ApplicationManager>.Instance.Deserialize(serializedApplicationState, ((int?)serializedApplicationState?.SaveVersion) ?? (-1));
		((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).Log((object)"Application save loaded!", (CLogLevel)2, false, false);
	}

	public static void SafeLoadGameSave()
	{
		TPSingleton<SaveManager>.Instance.PreloadedGameSave = null;
		NewlyPreloadedGameSave = true;
		if (GameSaveExists)
		{
			Exception caughtException = null;
			Exception caughtException2 = null;
			SerializedGameState loadedContainer = SaverLoader.SafeLoad<SerializedGameState>(GameSaveFilePath, !IsSaveEncryptionDisabled, out caughtException, MinimumSupportedGameSaveVersion) ?? SaverLoader.SafeLoad<SerializedGameState>(GameSaveBackupFilePath, !IsSaveEncryptionDisabled, out caughtException2, MinimumSupportedGameSaveVersion);
			(E_BrokenSaveReason?, Exception) tuple = (GetBrokenSaveReasonFromException(caughtException), caughtException);
			(E_BrokenSaveReason?, Exception) tuple2 = (GetBrokenSaveReasonFromException(caughtException2), caughtException2);
			TPSingleton<SaveManager>.Instance.PreloadedGameSave = new SaverLoader.SerializedContainerLoadingInfo<SerializedGameState>
			{
				FailedLoadsInfo = new(E_BrokenSaveReason?, Exception)[2] { tuple, tuple2 },
				LoadedContainer = loadedContainer
			};
		}
	}

	public static void Save()
	{
		SaverLoader.EnqueueSave(E_SaveType.App);
	}

	protected override void Awake()
	{
		base.Awake();
		if (((TPSingleton<SaveManager>)(object)this)._IsValid)
		{
			SaveEncoder.Initialize(new byte[32]
			{
				101, 99, 90, 70, 120, 51, 114, 74, 52, 102,
				52, 68, 75, 52, 101, 88, 84, 112, 100, 90,
				112, 90, 68, 76, 51, 107, 77, 85, 101, 53,
				67, 83
			}, new byte[8] { 20, 5, 8, 1, 255, 37, 19, 170 });
			((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).Log((object)$"Application current save versions: [App:{AppSaveVersion}] [Game:{GameSaveVersion}] [Settings:{SettingsSaveVersion}].", (CLogLevel)2, false, false);
			if (FormerAppSaveExists && SaverLoader.Load<SerializedApplicationState>(FormerAppSaveFilePath, !IsSaveEncryptionDisabled).SaveVersion <= appVersionTriggeringMigration)
			{
				ExecuteMultiProfileAndAccountMigration();
			}
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
			BackupSaveFilesBeforeVersionUpdate();
			Load();
			Save();
			if (!(ApplicationManager.Application.State is SplashScreen))
			{
				SafeLoadGameSave();
			}
		}
	}

	public void ExecuteMultiProfileAndAccountMigration()
	{
		((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).Log((object)"Trying to migrate save files to their new locations", (CLogLevel)2, false, false);
		string text = Path.Combine(PersistentDataPath, "Save");
		string text2 = Path.Combine(text, GetSaveSubFolderPath());
		string text3 = text + "_BackupMigration";
		string text4 = Path.Combine(text, "Backups");
		string newPath = text3 + "/AppSave." + (IsSaveEncryptionDisabled ? SaveFormat.ToString() : "bin");
		string newPath2 = text3 + "/GameSave." + (IsSaveEncryptionDisabled ? SaveFormat.ToString().ToLower() : "bin");
		string newPath3 = $"{text3}/SettingsSave.{SaveFormat}";
		string newPath4 = text3 + "/InputMappingSave.xml";
		if (!Directory.Exists(text2))
		{
			((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).Log((object)("Create the new save folder : " + text2), (CLogLevel)2, false, false);
			Directory.CreateDirectory(text2);
		}
		if (!Directory.Exists(text3))
		{
			((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).Log((object)("Create a BackupMigration folder at " + text3 + " ; to copy files outside cloud folders to keep them locally"), (CLogLevel)2, false, false);
			Directory.CreateDirectory(text3);
		}
		if (!AppSaveExists && FormerAppSaveExists)
		{
			SaverLoader.SafeCopyFileTo(FormerAppSaveFilePath, AppSaveFilePath);
			SaverLoader.SafeCopyFileTo(FormerAppSaveFilePath, newPath);
			SaverLoader.SafeDeleteFile(FormerAppSaveFilePath);
		}
		if (!GameSaveExists && FormerGameSaveExists)
		{
			SaverLoader.SafeCopyFileTo(FormerGameSaveFilePath, GameSaveFilePath);
			SaverLoader.SafeCopyFileTo(FormerGameSaveFilePath, newPath2);
			SaverLoader.SafeDeleteFile(FormerGameSaveFilePath);
		}
		if (!SettingsSaveExists && FormerSettingSaveExists)
		{
			SaverLoader.SafeCopyFileTo(FormerSettingSaveFilePath, SettingsFilePath);
			SaverLoader.SafeCopyFileTo(FormerSettingSaveFilePath, newPath3);
			SaverLoader.SafeDeleteFile(FormerSettingSaveFilePath);
		}
		if (!KeyRemappingSaverLoader.KeyRemappingSaveExists && FormerKeyRemappingSaveExists)
		{
			SaverLoader.SafeCopyFileTo(FormerKeyRemappingSaveFilePath, KeyRemappingSaverLoader.KeyRemappingSaveFilePath);
			SaverLoader.SafeCopyFileTo(FormerKeyRemappingSaveFilePath, newPath4);
			SaverLoader.SafeDeleteFile(FormerKeyRemappingSaveFilePath);
		}
		if (!Directory.Exists(Path.Combine(text2, "Backups")) && Directory.Exists(text4))
		{
			SaverLoader.SafeCopyDirectoryTo(text4, text2);
			SaverLoader.SafeCopyDirectoryTo(text4, text3);
			SaverLoader.SafeDeleteDirectory(text4);
		}
	}

	protected override void OnDestroy()
	{
		((CLogger<SaveManager>)this).OnDestroy();
		if (TPSingleton<ApplicationManager>.Exist())
		{
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent -= OnApplicationStateChange;
		}
	}

	private void OnApplicationStateChange(State state)
	{
		if (!(state is WorldMapState))
		{
			if (state is GameLobbyState)
			{
				SafeLoadGameSave();
			}
		}
		else
		{
			PreloadedGameSave = null;
		}
	}

	[DevConsoleCommand("ChangeProfile")]
	public static void DebugChangeProfile(int profile)
	{
		if (ApplicationManager.Application.State.GetName() == "GameLobby" && CurrentProfileIndex != profile)
		{
			ChangeCurrentProfile(profile);
			if ((Object)(object)TPSingleton<SaveManager>.Instance.devConsole != (Object)null)
			{
				TPSingleton<SaveManager>.Instance.devConsole.Log("Profile changed to : " + CurrentStringProfile + " !");
			}
		}
	}

	[DevConsoleCommand("SaveTransferLocalToSteamFolder")]
	public static void DebugTransferLocalSaveToSteamFolder()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!ScenesManager.IsSceneActive(ScenesManager.MainMenuSceneName))
		{
			return;
		}
		if (!SteamAPI.IsSteamRunning())
		{
			CLoggerManager.Log((object)"Can't transfer Local save to Steam folder because Steam isn't launched !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		string path = Path.Combine(PersistentDataPath, "Save");
		if (!Directory.Exists(Path.Combine(path, "Local")))
		{
			return;
		}
		string path2 = PersistentDataPath;
		CSteamID steamID = SteamUser.GetSteamID();
		string text = Path.Combine(path2, "Save", "Steam", ((object)(CSteamID)(ref steamID)).ToString());
		SaverLoader.SafeCopyFileTo(Path.Combine(path, "Local", $"SettingsSave.{SaveFormat}"), Path.Combine(text, $"SettingsSave.{SaveFormat}"));
		SaverLoader.SafeCopyFileTo(Path.Combine(path, "Local", "InputMappingSave.xml"), Path.Combine(text, "InputMappingSave.xml"));
		for (int i = 1; i <= 4; i++)
		{
			string text2 = Path.Combine(path, "Local", $"Profile{i}");
			if (Directory.Exists(text2))
			{
				SaverLoader.SafeCopyDirectoryTo(text2, text);
			}
		}
		TPSingleton<MetaConditionManager>.Instance.EraseConditionsControllers();
		Load();
		TPSingleton<SettingsManager>.Instance.Init();
		TPSingleton<MainMenuView>.Instance.Refresh();
	}
}
