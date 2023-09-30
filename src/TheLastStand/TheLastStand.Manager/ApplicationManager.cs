using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Modding;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Meta;
using TheLastStand.View;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Manager;

[StringConverter(typeof(StringToTPSingletonConverter<ApplicationManager>))]
public class ApplicationManager : Manager<ApplicationManager>, ISerializable, IDeserializable
{
	public enum E_BuildType
	{
		Debug,
		PreAlpha,
		Alpha,
		Beta,
		Release
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static GetGameVersionFunction _003C_003E9__39_0;

		public static Func<bool> _003C_003E9__41_0;

		internal string _003CAwake_003Eb__39_0()
		{
			return VersionString;
		}

		internal bool _003CQuitCoroutine_003Eb__41_0()
		{
			return SaverLoader.AreSavesCompleted();
		}
	}

	[SerializeField]
	private int majorVersion;

	[SerializeField]
	private int minorVersion = 1;

	[SerializeField]
	private int patchVersion = 1;

	[SerializeField]
	private int hotfixVersion;

	[SerializeField]
	private E_BuildType buildType;

	[SerializeField]
	private int logFilesToKeep = 5;

	[SerializeField]
	private float logBatchingFrequency = 1f;

	[SerializeField]
	private string currentStateName = string.Empty;

	private float lastBatchTime;

	public static Application Application { get; private set; }

	public static E_BuildType BuildType => TPSingleton<ApplicationManager>.Instance.buildType;

	public static string CurrentStateName
	{
		get
		{
			return TPSingleton<ApplicationManager>.Instance.currentStateName;
		}
		set
		{
			TPSingleton<ApplicationManager>.Instance.currentStateName = value;
		}
	}

	public static int LastLoadedVersion { get; private set; }

	public static int MajorVersion => TPSingleton<ApplicationManager>.Instance.majorVersion;

	public static int MinorVersion => TPSingleton<ApplicationManager>.Instance.minorVersion;

	public static int PatchVersion => TPSingleton<ApplicationManager>.Instance.patchVersion;

	public static int HotfixVersion => TPSingleton<ApplicationManager>.Instance.hotfixVersion;

	public static string VersionString => string.Format("v{0}.{1}.{2}.{3}{4}{5}", TPSingleton<ApplicationManager>.Instance.majorVersion, TPSingleton<ApplicationManager>.Instance.minorVersion, TPSingleton<ApplicationManager>.Instance.patchVersion, TPSingleton<ApplicationManager>.Instance.hotfixVersion, TPSingleton<ApplicationManager>.Instance.BuildTypeString, ModManager.GameHasMods ? "_MODDED" : "");

	public string BuildTypeString => buildType switch
	{
		E_BuildType.Debug => "_debug", 
		E_BuildType.PreAlpha => "_pre-alpha", 
		E_BuildType.Alpha => "_alpha", 
		E_BuildType.Beta => "_beta", 
		_ => string.Empty, 
	};

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	public static string DebugDamnedSouls
	{
		get
		{
			return Application.DamnedSouls.ToString();
		}
		set
		{
			uint damnedSouls = Convert.ToUInt32(value, 10);
			Application.DamnedSouls = damnedSouls;
			switch (CurrentStateName)
			{
			case "Game":
				GameView.TopScreenPanel.TurnPanel.PhasePanel.RefreshSoulsText();
				_ = GameManager.CurrentStateName;
				_ = 16;
				if (GameManager.CurrentStateName == Game.E_State.MetaShops)
				{
					TPSingleton<DarkShopManager>.Instance.RefreshTexts();
				}
				break;
			case "MetaShops":
				TPSingleton<DarkShopManager>.Instance.RefreshTexts();
				break;
			}
		}
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	public static string DebugDamnedSoulsObtained => Application.DamnedSoulsObtained.ToString();

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	public static bool DebugHasSeenIntroduction
	{
		get
		{
			return Application.HasSeenIntroduction;
		}
		set
		{
			Application.HasSeenIntroduction = value;
		}
	}

	public static void HandleGameOver(Game.E_GameOverCause cause)
	{
		Application.RunsCompleted++;
		if (cause == Game.E_GameOverCause.MagicSealsCompleted)
		{
			Application.RunsWon++;
		}
		if (Application.RunsCompleted == 1)
		{
			Application.DamnedSouls = (uint)TPSingleton<ResourceDatabase>.Instance.FirstRunDamnedSoulsGain;
			Application.DamnedSoulsObtained = (uint)TPSingleton<ResourceDatabase>.Instance.FirstRunDamnedSoulsGain;
		}
		else
		{
			TPSingleton<TrophyManager>.Instance.AutoGainDamnedSouls(cause != Game.E_GameOverCause.MagicSealsCompleted);
		}
	}

	public static void Quit()
	{
		((MonoBehaviour)TPSingleton<ApplicationManager>.Instance).StartCoroutine(TPSingleton<ApplicationManager>.Instance.QuitCoroutine());
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		LastLoadedVersion = saveVersion;
		SerializedApplicationState serializedApplicationState = container as SerializedApplicationState;
		TPSingleton<AchievementManager>.Instance.Deserialize(serializedApplicationState?.Achievements);
		TPSingleton<MetaUpgradesManager>.Instance.Deserialize((ISerializedData)(object)serializedApplicationState?.MetaUpgrades);
		TPSingleton<MetaConditionManager>.Instance.DeserializeFromAppSave((ISerializedData)(object)serializedApplicationState?.MetaConditions);
		MetaUpgradesManager.ActivateNewAvailableUpgradesInApplication();
		Application.DamnedSouls = serializedApplicationState?.DamnedSouls ?? 0;
		Application.DamnedSoulsObtained = serializedApplicationState?.DamnedSoulsObtained ?? 0;
		Application.HasSeenIntroduction = serializedApplicationState?.HasSeenIntroduction ?? false;
		Application.DaysPlayed = serializedApplicationState?.DaysPlayed ?? 0;
		Application.RunsCompleted = serializedApplicationState?.RunsCompleted ?? 0;
		Application.RunsWon = serializedApplicationState?.RunsWon ?? 0;
		Application.TutorialDone = serializedApplicationState?.TutorialDone ?? false;
		Application.ApplicationQuitInOraculum = serializedApplicationState?.ApplicationQuitInOraculum ?? false;
		Application.TutorialsRead = ((serializedApplicationState?.TutorialsRead != null) ? new List<string>(serializedApplicationState?.TutorialsRead) : new List<string>());
		TPSingleton<ApocalypseManager>.Instance.GlobalDeserialize((ISerializedData)(object)serializedApplicationState?.GlobalApocalypse);
		TPSingleton<WorldMapCityManager>.Instance.DeserializeCities(((int?)serializedApplicationState?.SaveVersion) ?? (-1), (ISerializedData)(object)serializedApplicationState?.Cities);
		TPSingleton<MetaShopsManager>.Instance.Deserialize((ISerializedData)(object)serializedApplicationState?.MetaShops, saveVersion);
		TPSingleton<MetaNarrationsManager>.Instance.Deserialize((ISerializedData)(object)serializedApplicationState?.MetaNarrations, saveVersion);
		TPSingleton<GlyphManager>.Instance.Deserialize(serializedApplicationState, saveVersion);
		if (saveVersion <= 12)
		{
			TPSingleton<AchievementManager>.Instance.TriggerApplicationBackwardCompatibility(saveVersion);
		}
	}

	public ISerializedData Serialize()
	{
		return (ISerializedData)(object)new SerializedApplicationState
		{
			Achievements = TPSingleton<AchievementManager>.Instance.Serialize(),
			DamnedSouls = Application.DamnedSouls,
			DamnedSoulsObtained = Application.DamnedSoulsObtained,
			HasSeenIntroduction = Application.HasSeenIntroduction,
			DaysPlayed = Application.DaysPlayed,
			RunsCompleted = Application.RunsCompleted,
			RunsWon = Application.RunsWon,
			TutorialDone = Application.TutorialDone,
			ApplicationQuitInOraculum = Application.ApplicationQuitInOraculum,
			MetaUpgrades = (TPSingleton<MetaUpgradesManager>.Instance.Serialize() as SerializedMetaUpgrades),
			MetaConditions = (TPSingleton<MetaConditionManager>.Instance.SerializeToAppSave() as SerializedMetaConditions),
			GlobalApocalypse = (TPSingleton<ApocalypseManager>.Instance.GlobalSerialize() as SerializedGlobalApocalypse),
			Cities = (TPSingleton<WorldMapCityManager>.Instance.Serialize() as SerializedCities),
			MetaShops = (TPSingleton<MetaShopsManager>.Instance.Serialize() as SerializedMetaShops),
			MetaNarrations = (TPSingleton<MetaNarrationsManager>.Instance.Serialize() as SerializedNarrations),
			Glyphs = TPSingleton<GlyphManager>.Instance.Serialize(),
			TutorialsRead = new List<string>(Application.TutorialsRead)
		};
	}

	protected override void Awake()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		base.Awake();
		if (!((TPSingleton<ApplicationManager>)(object)this)._IsValid)
		{
			return;
		}
		TPSingleton<DebugManager>.Instance.RegisterAssemblyCommands();
		object obj = _003C_003Ec._003C_003E9__39_0;
		if (obj == null)
		{
			GetGameVersionFunction val = () => VersionString;
			_003C_003Ec._003C_003E9__39_0 = val;
			obj = (object)val;
		}
		TPGameVersion.__GetGameVersion = (GetGameVersionFunction)obj;
		if (logFilesToKeep <= 0)
		{
			((CLogger<ApplicationManager>)this).LogError((object)$"Please do not specify a number of logs to keep inferior or equal with 0! (currently specified: {logFilesToKeep}).\nI put this value to 1 as a precaution.", (CLogLevel)0, true, true);
			logFilesToKeep = 1;
		}
		if (logBatchingFrequency < 0f)
		{
			((CLogger<ApplicationManager>)this).LogError((object)$"Please do not specify a log batching frequency inferior to 0! (currently specified: {logBatchingFrequency}).\nValue automatically set to 0.", (CLogLevel)0, true, true);
			logBatchingFrequency = 0f;
		}
		CLoggerManager.Start("TLS", logBatchingFrequency, (byte)logFilesToKeep);
		Application = new ApplicationController().Application;
		InitState();
	}

	private void InitState()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		string name = ((Scene)(ref activeScene)).name;
		if (Application.ApplicationQuitInOraculum)
		{
			((CLogger<ApplicationManager>)this).Log((object)"Initializing application state, considering last application execution has been quit while in the Oraculum.", (CLogLevel)2, false, false);
		}
		if (name == ScenesManager.SplashSceneName)
		{
			Application.ApplicationController.SetState("SplashScreen");
		}
		else if (name == ScenesManager.MainMenuSceneName)
		{
			Application.ApplicationController.SetState("GameLobby");
		}
		else if (ScenesManager.IsActiveSceneLevel())
		{
			Application.ApplicationController.SetState(GameManager.DisableAutoLoad ? "NewGame" : "LoadGame");
		}
		else if (ScenesManager.IsActiveSceneWorldMap())
		{
			Application.ApplicationController.SetState("WorldMap");
		}
		else if (ScenesManager.IsActiveSceneMetaShop())
		{
			Application.ApplicationController.SetState("MetaShops");
		}
		else if (name == "Level Editor")
		{
			Application.ApplicationController.SetState("LevelEditor");
		}
		else if (name == ScenesManager.CreditsSceneName)
		{
			Application.ApplicationController.SetState("Credits");
		}
		else if (name == ScenesManager.AnimatedCutsceneSceneName)
		{
			Application.ApplicationController.SetState("AnimatedCutscene");
		}
		else
		{
			((CLogger<ApplicationManager>)this).LogError((object)("Unknown scene \"" + name + "\" to initialize application state."), (CLogLevel)2, true, true);
		}
	}

	private IEnumerator QuitCoroutine()
	{
		yield return (object)new WaitUntil((Func<bool>)(() => SaverLoader.AreSavesCompleted()));
		Application.Quit();
	}

	public SerializedApplicationState TryLoad()
	{
		SerializedApplicationState serializedApplicationState = SaverLoader.Load<SerializedApplicationState>(SaveManager.AppSaveFilePath, !SaveManager.IsSaveEncryptionDisabled);
		((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).Log((object)$"Application file save version : {serializedApplicationState.SaveVersion}", (CLogLevel)2, true, false);
		if (serializedApplicationState.SaveVersion < SaveManager.MinimumSupportedAppSaveVersion)
		{
			throw new SaverLoader.WrongSaveVersionException(SaveManager.AppSaveFilePath, shouldMarkAsCorrupted: true);
		}
		return serializedApplicationState;
	}

	public SerializedApplicationState TryLoadBackup(Exception e)
	{
		if (File.Exists(SaveManager.AppSaveBackupFilePath))
		{
			try
			{
				if (e is SaverLoader.SaveLoadingFailedException ex)
				{
					SaveManager.BrokenSavePath = ex.FilePath;
				}
				else
				{
					SaveManager.BrokenSavePath = SaverLoader.MarkFileAsCorrupted(SaveManager.AppSaveFilePath);
				}
				SerializedApplicationState serializedApplicationState = null;
				((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).LogWarning((object)"First AppSave loading failed! Trying to load BACKUP file.", (CLogLevel)2, true, false);
				((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).LogWarning((object)$"Failed loading exception message : {e}", (CLogLevel)2, true, false);
				serializedApplicationState = SaverLoader.Load<SerializedApplicationState>(SaveManager.AppSaveBackupFilePath, !SaveManager.IsSaveEncryptionDisabled);
				((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).Log((object)$"Application Backup file save version : {serializedApplicationState.SaveVersion}", (CLogLevel)2, true, false);
				if (serializedApplicationState.SaveVersion < SaveManager.MinimumSupportedAppSaveVersion)
				{
					throw new SaverLoader.WrongSaveVersionException(SaveManager.AppSaveBackupFilePath, shouldMarkAsCorrupted: true);
				}
				SaverLoader.CopyFileTo(SaveManager.AppSaveBackupFilePath, SaveManager.AppSaveFilePath);
				SaveManager.BackupLoaded = true;
				return serializedApplicationState;
			}
			catch (Exception)
			{
				throw e;
			}
		}
		throw e;
	}

	private void Update()
	{
		if (CLoggerManager.LogBatchingFrequency > 0f && Time.time - lastBatchTime >= CLoggerManager.LogBatchingFrequency)
		{
			CLoggerManager.WriteBatches();
			lastBatchTime = Time.time;
		}
	}

	[DevConsoleCommand("SaveApplication")]
	public static void DebugSaveApplication()
	{
		SaveManager.Save();
	}
}
