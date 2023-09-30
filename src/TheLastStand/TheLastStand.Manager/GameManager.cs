using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using DG.Tweening;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TPLib.Yield.CustomYieldInstructions;
using TheLastStand.Controller;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Fog;
using TheLastStand.Framework;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Encryption;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Modding;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Trap;
using TheLastStand.Manager.Turret;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Building;
using TheLastStand.Serialization.Item;
using TheLastStand.Serialization.Meta;
using TheLastStand.Serialization.SpawnWave;
using TheLastStand.Serialization.Trophy;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.Building.Construction;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Cursor;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.NightReport;
using TheLastStand.View.Shop;
using TheLastStand.View.TileMap;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TheLastStand.Manager;

public sealed class GameManager : Manager<GameManager>, ISerializable, IDeserializable
{
	public delegate void LateDeserialize();

	public static class Constants
	{
		public class Debug
		{
			public static string LevelsPath = "Assets/DataFiles/Levels/WIP/Runtime/";
		}

		public const string AmbientSoundsFormat = "Sounds/SFX/Ambient/AMB_Towns/AMB_{0}";
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__90_0;

		public static UnityAction _003C_003E9__90_1;

		public static Action _003C_003E9__90_2;

		public static Action _003C_003E9__90_3;

		public static Action _003C_003E9__102_0;

		public static Func<bool> _003C_003E9__129_0;

		internal void _003CHandleEndTurnInput_003Eb__90_0()
		{
			ACameraView.AllowUserPan = true;
		}

		internal void _003CHandleEndTurnInput_003Eb__90_1()
		{
			ACameraView.AllowUserPan = true;
		}

		internal void _003CHandleEndTurnInput_003Eb__90_2()
		{
			GameController.SetState(Game.E_State.Management);
			GameController.EndTurn();
		}

		internal void _003CHandleEndTurnInput_003Eb__90_3()
		{
			GameController.SetState(Game.E_State.Management);
		}

		internal void _003CNightReportToProductionPhaseCoroutine_003Eb__102_0()
		{
			FogController.IncreaseDensity();
			SpawnWaveManager.CurrentSpawnWave.SpawnWaveView.Refresh(onDayStart: false, forceDisplayArrows: true);
		}

		internal bool _003C_002Ector_003Eb__129_0()
		{
			return TPSingleton<GameManager>.Instance.GameInitialized;
		}
	}

	private readonly IEnumerator waitForGameInit = (IEnumerator)new WaitUntil((Func<bool>)(() => TPSingleton<GameManager>.Instance.GameInitialized));

	[SerializeField]
	[Tooltip("Disable auto load (editor only!)")]
	private bool disableAutoLoad;

	[SerializeField]
	[Tooltip("Disable auto save (editor only!)")]
	private bool disableAutoSave;

	[SerializeField]
	[Tooltip("Disables save safety check - ONLY WORKS with encryption")]
	private bool disableSaveCheck;

	[SerializeField]
	[Range(1f, 10f)]
	[Tooltip("Time before the enemies start their turn")]
	private float newNightTransitionDuration = 3f;

	[SerializeField]
	[Range(1f, 10f)]
	[Tooltip("Time before the night report panel show up")]
	private float newDayTransitionDuration = 3f;

	[SerializeField]
	[Range(0f, 10f)]
	[Tooltip("Time waited after bone piles got generated (only applies if there were bone piles to generate)")]
	private float delayAfterBonePiles = 1f;

	[SerializeField]
	private AudioClip winAudioClip;

	[SerializeField]
	private AudioClip defeatAudioClip;

	[SerializeField]
	private AudioClip tutorialDefeatAudioClip;

	[SerializeField]
	private Transform viewTransform;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioSource ambienceAudioSource;

	[SerializeField]
	private AudioClip productionPhaseAudioClip;

	[SerializeField]
	private AudioClip deploymentPhaseAudioClip;

	[SerializeField]
	private AudioClip nightPlayerPhaseAudioClip;

	[SerializeField]
	private AudioClip nightEnemyPhaseAudioClip;

	[SerializeField]
	private float ambientSoundsFadeInDuration = 2f;

	[SerializeField]
	private float ambientSoundsFadeOutDuration = 2f;

	[SerializeField]
	private Game.E_DayTurn debugStartingDayTurn;

	[SerializeField]
	private bool loadLevelEditorCityAssets;

	[SerializeField]
	private Game.E_State currentStateName;

	private bool currentGameIsLoaded;

	private float previousTimeSpent;

	private float timeAtGameStart;

	private Tween ambientSoundFadeTween;

	public static bool DisplayWillBeReachedBy = false;

	public static float AmbientSoundsFadeOutDuration => TPSingleton<GameManager>.Instance.ambientSoundsFadeOutDuration;

	public static AudioSource AudioSource => TPSingleton<GameManager>.Instance.audioSource;

	public static Game.E_State CurrentStateName
	{
		get
		{
			return TPSingleton<GameManager>.Instance.currentStateName;
		}
		set
		{
			TPSingleton<GameManager>.Instance.currentStateName = value;
		}
	}

	public static AudioClip WinAudioClip => TPSingleton<GameManager>.Instance.winAudioClip;

	public static AudioClip DefeatAudioClip => TPSingleton<GameManager>.Instance.defeatAudioClip;

	public static AudioClip TutorialDefeatAudioClip => TPSingleton<GameManager>.Instance.tutorialDefeatAudioClip;

	public static AudioClip DeploymentPhaseAudioClip => TPSingleton<GameManager>.Instance.deploymentPhaseAudioClip;

	public static bool DisableAutoLoad => TPSingleton<GameManager>.Instance.disableAutoLoad;

	public static FormulaInterpreterContext FormulaInterpreterContext { get; private set; }

	public static bool LoadLevelEditorCityAssets
	{
		get
		{
			if (TPSingleton<GameManager>.Instance.loadLevelEditorCityAssets)
			{
				((CLogger<GameManager>)TPSingleton<GameManager>.Instance).LogError((object)"Trying to load city text assets using Level Editor folder while NOT being in editor. Using normal folder instead.", (CLogLevel)2, true, true);
			}
			return false;
		}
	}

	public static AudioClip NightEnemyPhaseAudioClip => TPSingleton<GameManager>.Instance.nightEnemyPhaseAudioClip;

	public static AudioClip NightPlayerPhaseAudioClip => TPSingleton<GameManager>.Instance.nightPlayerPhaseAudioClip;

	public static AudioClip ProductionPhaseAudioClip => TPSingleton<GameManager>.Instance.productionPhaseAudioClip;

	public static Game.E_DayTurn StartingDayTurn
	{
		get
		{
			if (DebugStartingDayTurn != 0)
			{
				return DebugStartingDayTurn;
			}
			return TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.StartingDayTurn;
		}
	}

	public static Game.E_DayTurn DebugStartingDayTurn => TPSingleton<GameManager>.Instance.debugStartingDayTurn;

	public static Transform ViewTransform => TPSingleton<GameManager>.Instance.viewTransform;

	public static IEnumerator WaitForGameInit => TPSingleton<GameManager>.Instance.waitForGameInit;

	public bool IsDebugStartingDayTurnOn => debugStartingDayTurn != Game.E_DayTurn.Undefined;

	public int FogDensityIndex => TPSingleton<FogManager>.Instance.Fog.DensityIndex;

	public int FogDensityValue => TPSingleton<FogManager>.Instance.Fog.DensityValue;

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	public int DayNumber
	{
		get
		{
			return Game.DayNumber;
		}
		set
		{
			int num = value - Game.DayNumber;
			Game.DayNumber = Mathf.Max(0, value);
			SpawnWaveManager.GenerateSpawnWave();
			SpawnWaveManager.SpawnWaveView.Refresh();
			TPSingleton<ToDoListView>.Instance.RefreshAllNotifications();
			GameView.TopScreenPanel.TurnPanel.Refresh();
			TPSingleton<SoundManager>.Instance.ChangePlaylist();
			TPSingleton<SoundManager>.Instance.ChangeMusic();
			if (num > 0)
			{
				ApplicationManager.Application.DaysPlayed += (uint)num;
			}
			TPSingleton<MetaConditionManager>.Instance.RefreshProgression();
		}
	}

	public Game Game { get; private set; }

	public float TotalTimeSpent => previousTimeSpent + Time.unscaledTime - timeAtGameStart;

	public bool GameInitialized
	{
		get
		{
			if (Game != null)
			{
				return Game.State != Game.E_State.Off;
			}
			return false;
		}
	}

	public bool NightReportToDayCoroutineRunning { get; private set; }

	[DevConsoleCommand]
	public static float MoveSpeedMultiplier { get; set; } = 1f;


	[DevConsoleCommand]
	public static float TimeScale
	{
		get
		{
			return Time.timeScale;
		}
		set
		{
			Time.timeScale = value;
		}
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	public static bool FpsEnabled
	{
		get
		{
			return DebugManager.FpsEnabled;
		}
		private set
		{
			DebugManager.FpsEnabled = value;
		}
	}

	public event LateDeserialize FinalizeDeserialize;

	public static void EraseSave()
	{
		SaverLoader.Erase(SaveManager.GameSaveFilePath);
		SaverLoader.Erase(SaveManager.GameSaveBackupFilePath);
	}

	public static void ExileAllEnemies(bool countAsKills, bool resetSpawnWave, bool disableDieAnim = false, List<TheLastStand.Model.Unit.Unit> unitsToSkip = null)
	{
		if (resetSpawnWave)
		{
			SpawnWaveManager.CurrentSpawnWave = null;
		}
		TPSingleton<BossManager>.Instance.ExileAllUnits(countAsKills, disableDieAnim, unitsToSkip);
		TPSingleton<EnemyUnitManager>.Instance.ExileAllUnits(countAsKills, disableDieAnim, unitsToSkip);
	}

	public static bool HandleEndTurnInput()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		if (GameController.CanEndPlayerTurn())
		{
			if (TurnEndValidationManager.EndTurnIsBlocked)
			{
				ACameraView.AllowUserPan = false;
				object obj = _003C_003Ec._003C_003E9__90_0;
				if (obj == null)
				{
					UnityAction val = delegate
					{
						ACameraView.AllowUserPan = true;
					};
					_003C_003Ec._003C_003E9__90_0 = val;
					obj = (object)val;
				}
				GenericBlockingPopup.OpenAsComplex("GenericBlocking_NotYetQuiteReadyYet", "GenericBlocking_BeforeProceeding", (UnityAction)obj);
			}
			else if (TurnEndValidationManager.AnyBlockingPlayableUnitInFog)
			{
				ACameraView.AllowUserPan = false;
				object obj2 = _003C_003Ec._003C_003E9__90_1;
				if (obj2 == null)
				{
					UnityAction val2 = delegate
					{
						ACameraView.AllowUserPan = true;
					};
					_003C_003Ec._003C_003E9__90_1 = val2;
					obj2 = (object)val2;
				}
				GenericBlockingPopup.OpenAsSimple("GenericPopup_TitleMoveUnitOutOfFog", "GenericPopup_MoveUnitOutOfFog", (UnityAction)obj2, smallVersion: true);
			}
			else
			{
				if (TurnEndValidationManager.CanEndTurnWithoutConsentAsking(out var localizedConsentAsk))
				{
					GameController.EndTurn();
					return true;
				}
				ACameraView.AllowUserPan = false;
				GenericConsent.OpenLocalized(localizedConsentAsk, delegate
				{
					GameController.SetState(Game.E_State.Management);
					GameController.EndTurn();
				}, delegate
				{
					GameController.SetState(Game.E_State.Management);
				});
			}
		}
		return false;
	}

	public static void Load()
	{
		if (((StateMachine)ApplicationManager.Application).State.GetName() == "LevelEditor")
		{
			TPSingleton<GameManager>.Instance.Game = new GameController().Game;
			TPSingleton<GlyphManager>.Instance.InitGlyphEffects();
			TPSingleton<ConstructionManager>.Instance.Init();
			TPSingleton<BuildingManager>.Instance.Deserialize(null);
			return;
		}
		try
		{
			try
			{
				SerializedContainer serializedContainer = TPSingleton<SaveManager>.Instance.PreloadedGameSave?.LoadedContainer;
				TPSingleton<GameManager>.Instance.Deserialize((ISerializedData)(object)serializedContainer, ((int?)serializedContainer?.SaveVersion) ?? (-1));
			}
			catch (Exception e)
			{
				SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> preloadedGameSave = TPSingleton<SaveManager>.Instance.PreloadedGameSave;
				if ((preloadedGameSave == null || !preloadedGameSave.FailedLoadsInfo[0].Reason.HasValue) && File.Exists(SaveManager.GameSaveBackupFilePath))
				{
					SerializedContainer serializedContainer2 = TPSingleton<GameManager>.Instance.TryLoadBackup(e);
					TPSingleton<GameManager>.Instance.Deserialize((ISerializedData)(object)serializedContainer2, ((int?)serializedContainer2?.SaveVersion) ?? (-1));
					return;
				}
				throw;
			}
		}
		catch (Exception ex)
		{
			string brokenSavePath = SaveManager.CorruptGameSave();
			ApplicationManager.Application.ApplicationController.SetState("GameLobby");
			SaveManager.BrokenSavePath = brokenSavePath;
			SaveManager.BrokenSaveReason = SaveManager.E_BrokenSaveReason.UNKNOWN;
			((CLogger<GameManager>)TPSingleton<GameManager>.Instance).LogError((object)("Caught exception while loading game. Error Message : " + ex.Message), (CLogLevel)2, true, false);
			((CLogger<GameManager>)TPSingleton<GameManager>.Instance).LogError((object)$"A critical error occured while loading the savegame\n{ex}\nPlease catch this specific exception earlier on and add a proper message for it.", (CLogLevel)0, true, true);
		}
	}

	public static void Save()
	{
		if (!TPSingleton<GameManager>.Instance.IsSaveAllowed())
		{
			((CLogger<GameManager>)TPSingleton<GameManager>.Instance).Log((object)"Tried to save game but it was not allowed.", (CLogLevel)0, false, false);
			return;
		}
		SaverLoader.EnqueueSave(E_SaveType.Game);
		SaveManager.Save();
	}

	public static void TryToSaveAuto()
	{
		if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.Undefined)
		{
			Save();
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		this.FinalizeDeserialize = null;
		SerializedGameState serializedGameState = container as SerializedGameState;
		currentGameIsLoaded = serializedGameState != null;
		BackwardCompatibilityBeforeDeserialize(serializedGameState, saveVersion);
		TPSingleton<ApocalypseManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.Apocalypse);
		TPSingleton<MetaConditionManager>.Instance.DeserializeFromGameSave((ISerializedData)(object)serializedGameState?.MetaConditionsRunContext);
		TPSingleton<GlyphManager>.Instance.InitGlyphEffects();
		TPSingleton<GlyphManager>.Instance.Deserialize(serializedGameState?.SerializedGlyphsContainer, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		Game = new GameController(serializedGameState?.Game).Game;
		FormulaInterpreterContext = new FormulaInterpreterContext(Game);
		previousTimeSpent = serializedGameState?.TotalTimeSpent ?? 0f;
		timeAtGameStart = Time.unscaledTime;
		TPSingleton<InputManager>.Instance.Init();
		TPSingleton<RandomManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.Random);
		TPSingleton<PathfindingManager>.Instance.Init();
		TPSingleton<TileObjectSelectionManager>.Instance.Init();
		TPSingleton<FogManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.Fog);
		CharacterSheetPanel.Init();
		TPSingleton<UnitLevelUpView>.Instance.Init();
		TPSingleton<PanicManager>.Instance.Init();
		ShopView.Init();
		TPSingleton<ConstructionManager>.Instance.Init();
		TPSingleton<ShopManager>.Instance.Init();
		if (currentGameIsLoaded)
		{
			TPSingleton<BuildingManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.Buildings, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		}
		else
		{
			TPSingleton<BuildingManager>.Instance.Deserialize(null);
		}
		TPSingleton<InventoryManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.Inventory);
		TPSingleton<ItemManager>.Instance.Init();
		TPSingleton<ResourceManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.Resources);
		TPSingleton<SpawnWaveManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.SpawnWaveContainer);
		TPSingleton<SpawnWaveManager>.Instance.Init();
		if (serializedGameState?.SpawnWaveContainer?.CurrentSpawnWave == null)
		{
			SpawnWaveManager.GenerateSpawnWave();
		}
		else
		{
			SpawnWaveManager.DeserializeSpawnWave(serializedGameState.SpawnWaveContainer, serializedGameState.SaveVersion);
		}
		CameraView.CameraLutView.Deserialize((ISerializedData)(object)serializedGameState?.SerializedLut);
		TPSingleton<EnemyUnitManager>.Instance.Init();
		TPSingleton<PlayableUnitManagementView>.Instance.Init();
		TPSingleton<PlayableUnitManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.PlayableUnits, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		TPSingleton<PlayableUnitManager>.Instance.NightReport.Deserialize((ISerializedData)(object)serializedGameState?.SerializedNightReport, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		TPSingleton<EnemyUnitManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.EnemyUnits, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		TPSingleton<BossManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.BossData, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count == 0)
		{
			PlayableUnitManager.CreateStartUnits();
		}
		TPSingleton<ConstructionView>.Instance.Init();
		TPSingleton<PanicManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.Panic, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		TPSingleton<TrophyManager>.Instance.Deserialize((ISerializedData)(object)serializedGameState?.Trophies, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		this.FinalizeDeserialize?.Invoke();
		((CLogger<GameManager>)TPSingleton<GameManager>.Instance).Log((object)((serializedGameState != null) ? "Game loaded!" : "New game!"), (CLogLevel)2, false, false);
		BackwardCompatibilityAfterDeserialize(serializedGameState, saveVersion);
		if (serializedGameState != null && ApplicationManager.LastLoadedVersion == 11)
		{
			TPSingleton<AchievementManager>.Instance.TriggerGameBackwardCompatibility();
		}
	}

	public ISerializedData Serialize()
	{
		return (ISerializedData)(object)new SerializedGameState
		{
			Resources = (SerializedResources)(object)TPSingleton<ResourceManager>.Instance.Serialize(),
			Game = (SerializedGame)(object)Game.Serialize(),
			Random = (SerializedRandoms)(object)TPSingleton<RandomManager>.Instance.Serialize(),
			PlayableUnits = (SerializedPlayableUnits)(object)TPSingleton<PlayableUnitManager>.Instance.Serialize(),
			EnemyUnits = ((Game.Cycle == Game.E_Cycle.Night) ? ((SerializedEnemyUnits)(object)TPSingleton<EnemyUnitManager>.Instance.Serialize()) : null),
			BossData = (SerializedBossData)(object)TPSingleton<BossManager>.Instance.Serialize(),
			Buildings = (SerializedBuildings)(object)TPSingleton<BuildingManager>.Instance.Serialize(),
			Inventory = (SerializedItems)(object)TPSingleton<InventoryManager>.Instance.Serialize(),
			Apocalypse = (SerializedApocalypse)(object)TPSingleton<ApocalypseManager>.Instance.Serialize(),
			Fog = (SerializedFog)(object)TPSingleton<FogManager>.Instance.Serialize(),
			Panic = ((Game.Cycle == Game.E_Cycle.Night) ? ((SerializedPanic)(object)TPSingleton<PanicManager>.Instance.Serialize()) : null),
			MetaConditionsRunContext = (SerializedMetaConditionsContext)(object)TPSingleton<MetaConditionManager>.Instance.SerializeToGameSave(),
			SpawnWaveContainer = (SerializedSpawnWaveContainer)(object)TPSingleton<SpawnWaveManager>.Instance.Serialize(),
			TotalTimeSpent = TotalTimeSpent,
			ModsInUse = new List<string>(ModManager.ModIdsInUse),
			SerializedLut = ((Game.Cycle == Game.E_Cycle.Night) ? ((SerializedLUT)(object)CameraView.CameraLutView.Serialize()) : null),
			Trophies = ((Game.Cycle == Game.E_Cycle.Night) ? ((SerializedTrophies)(object)TPSingleton<TrophyManager>.Instance.Serialize()) : null),
			SerializedNightReport = ((Game.Cycle == Game.E_Cycle.Night) ? ((SerializedNightReport)(object)TPSingleton<PlayableUnitManager>.Instance.NightReport.Serialize()) : null),
			SerializedGlyphsContainer = TPSingleton<GlyphManager>.Instance.SerializeGlyphs()
		};
	}

	public void FinalizeDayTransition()
	{
		if (DayNumber < TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount)
		{
			((MonoBehaviour)this).StartCoroutine(NightReportToProductionPhaseCoroutine());
			return;
		}
		GameController.SetState(Game.E_State.CutscenePlaying);
		CutsceneManager.PlayCutscene(TPSingleton<CutsceneManager>.Instance.VictorySequenceView, VictorySequenceCallback);
	}

	public bool IsSaveAllowed()
	{
		return TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver;
	}

	public void OnGameExitButtonClick()
	{
		Application.Quit();
	}

	public IEnumerator WaitNewCycleTransition()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			if (DayNumber >= TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount)
			{
				FinalizeDayTransition();
			}
			else if (StartingDayTurn != Game.E_DayTurn.Production || TPSingleton<GameManager>.Instance.Game.DayNumber != 0)
			{
				yield return SharedYields.WaitForSeconds(TPSingleton<GameManager>.Instance.newDayTransitionDuration);
				TPSingleton<NightReportPanel>.Instance.Open();
			}
		}
		else
		{
			yield return SharedYields.WaitForSeconds(TPSingleton<GameManager>.Instance.newNightTransitionDuration);
			PlayableUnitManager.StartTurn();
			BossManager.StartTurn();
			EnemyUnitManager.StartTurn();
			TurretManager.StartTurn();
			TrapManager.StartTurn();
			NightTurnsManager.StartTurn();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		SaveEncoder.enableHashCheck = !disableSaveCheck;
		DOTween.Init((bool?)null, (bool?)null, (LogBehaviour?)null);
		DOTween.SetTweensCapacity(500, 50);
		TileMapView.DisplayLevel();
		CursorController cursorController = new CursorController();
		Load();
		Game.Cursor = cursorController.Cursor;
		if (!TPSingleton<LevelEditorManager>.Exist())
		{
			ApplicationManager.Application.ApplicationController.SetState("Game");
		}
	}

	private static void TriggerMagicSealsCompleted()
	{
		GameController.TriggerGameOver(Game.E_GameOverCause.MagicSealsCompleted);
	}

	private IEnumerator NightReportToProductionPhaseCoroutine()
	{
		NightReportToDayCoroutineRunning = true;
		SpawnWaveManager.SpawnWaveView.RefreshPosition();
		CameraView.RefreshDayTimeEffects();
		yield return SharedYields.WaitForSeconds(TPSingleton<FogManager>.Instance.FogView.WaitBeforeFogIncreaseSequence);
		bool isDayException = false;
		foreach (FogDefinition.FogDayException fogDayException in TPSingleton<FogManager>.Instance.Fog.FogDefinition.DayExceptions)
		{
			if (fogDayException.DayNumber != TPSingleton<GameManager>.Instance.Game.DayNumber)
			{
				continue;
			}
			isDayException = true;
			if (FogController.IsDensityEqualTo(fogDayException.FogDensityName))
			{
				SpawnWaveManager.CurrentSpawnWave.SpawnWaveView.Refresh(onDayStart: false, forceDisplayArrows: true);
				continue;
			}
			yield return TPSingleton<FogManager>.Instance.MoveCameraToNearestFogWithWave(delegate
			{
				FogController.SetDensity(fogDayException.FogDensityName);
				SpawnWaveManager.CurrentSpawnWave.SpawnWaveView.Refresh(onDayStart: false, forceDisplayArrows: true);
			}, TPSingleton<FogManager>.Instance.FogView.WaitBeforeFogIncreaseShow);
			yield return SharedYields.WaitForSeconds(TPSingleton<FogManager>.Instance.FogView.WaitAfterFogIncreaseShow);
		}
		if (!isDayException)
		{
			if (TPSingleton<GameManager>.Instance.Game.DayNumber % TPSingleton<FogManager>.Instance.Fog.DailyUpdateFrequency == 0 && !FogController.IsDensityAtMaximum())
			{
				yield return TPSingleton<FogManager>.Instance.MoveCameraToNearestFogWithWave(delegate
				{
					FogController.IncreaseDensity();
					SpawnWaveManager.CurrentSpawnWave.SpawnWaveView.Refresh(onDayStart: false, forceDisplayArrows: true);
				}, TPSingleton<FogManager>.Instance.FogView.WaitBeforeFogIncreaseShow);
				yield return SharedYields.WaitForSeconds(TPSingleton<FogManager>.Instance.FogView.WaitAfterFogIncreaseShow);
			}
			else
			{
				SpawnWaveManager.CurrentSpawnWave.SpawnWaveView.Refresh(onDayStart: false, forceDisplayArrows: true);
			}
		}
		yield return ACameraView.Zoom(zoomIn: false);
		ACameraView.MoveTo(((Component)BuildingManager.MagicCircle.BuildingView).transform);
		yield return SharedYields.WaitForSeconds(1f);
		if (TPSingleton<BuildingManager>.Instance.GenerateBonePiles())
		{
			TPSingleton<BuildingManager>.Instance.PlayBonePileConstructionSound();
			yield return SharedYields.WaitForSeconds(delayAfterBonePiles);
		}
		yield return TPSingleton<BuildingManager>.Instance.GenerateRandomBuildingsCoroutine();
		TPSingleton<FogManager>.Instance.GenerateLightFogSpawners();
		yield return TPSingleton<BuildingManager>.Instance.TriggerBuildingPassiveCoroutine();
		TPSingleton<BuildingManager>.Instance.ResetShopRerollIndex();
		PlayableUnitManager.RespawnUnits();
		TPSingleton<ToDoListView>.Instance.Show();
		GameView.BottomScreenPanel.BottomLeftPanel.Refresh();
		GameView.TopScreenPanel.UnitPortraitsPanel.Display(show: true);
		GameView.TopScreenPanel.TurnPanel.Display(show: true);
		NightReportToDayCoroutineRunning = false;
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnProductionStart);
	}

	private void BackwardCompatibilityAfterDeserialize(SerializedGameState saveGame, int saveVersion)
	{
		if (saveGame != null)
		{
			TPSingleton<BuildingManager>.Instance.BackwardCompatibilityAfterDeserialize(saveGame, saveVersion);
		}
	}

	private void BackwardCompatibilityBeforeDeserialize(SerializedGameState saveGame, int saveVersion)
	{
	}

	private IEnumerator StartGame()
	{
		yield return (object)new WaitForFrames(1);
		if (TPSingleton<LevelEditorManager>.Exist())
		{
			GameController.SetState(Game.E_State.LevelEdition);
			yield break;
		}
		ACameraView.MoveTo(Vector2.op_Implicit(TileMapView.GetTileCenter(BuildingManager.MagicCircle.OriginTile)), 0f, (Ease)0);
		StartAmbientSounds();
		TPSingleton<SoundManager>.Instance.TransitionToNormalSnapshot();
		if (currentGameIsLoaded)
		{
			GameController.StartTurnOnLoad(instant: true);
			TPSingleton<AchievementManager>.Instance.HandleRunLoad();
		}
		else
		{
			GameController.StartTurn(instant: true);
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnGameStart);
			TPSingleton<AchievementManager>.Instance.HandleRunStart();
		}
	}

	private void Start()
	{
		if (((StateMachine)ApplicationManager.Application).State.GetName() == "NewGame")
		{
			TryToSaveAuto();
		}
		((MonoBehaviour)this).StartCoroutine(StartGame());
	}

	private SerializedContainer TryLoadBackup(Exception e)
	{
		try
		{
			SaveManager.BrokenSavePath = SaverLoader.MarkFileAsCorrupted(SaveManager.GameSaveFilePath);
			((CLogger<GameManager>)TPSingleton<GameManager>.Instance).LogWarning((object)"First Game loading failed! Trying to load BACKUP file.", (CLogLevel)2, true, false);
			((CLogger<GameManager>)TPSingleton<GameManager>.Instance).LogWarning((object)$"Failed loading exception message : {e}", (CLogLevel)2, true, false);
			SerializedGameState serializedGameState = SaverLoader.Load<SerializedGameState>(SaveManager.GameSaveBackupFilePath, !SaveManager.IsSaveEncryptionDisabled);
			((CLogger<GameManager>)TPSingleton<GameManager>.Instance).Log((object)$"Game file save version : {serializedGameState.SaveVersion}", (CLogLevel)2, true, false);
			if (serializedGameState.SaveVersion < SaveManager.MinimumSupportedGameSaveVersion)
			{
				throw new SaverLoader.WrongSaveVersionException(SaveManager.GameSaveBackupFilePath, shouldMarkAsCorrupted: true);
			}
			SaverLoader.CopyFileTo(SaveManager.GameSaveBackupFilePath, SaveManager.GameSaveFilePath);
			SaveManager.BackupLoaded = true;
			return serializedGameState;
		}
		catch (Exception)
		{
			throw e;
		}
	}

	private void Update()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!GameInitialized)
		{
			return;
		}
		if (InputManager.IsPointerOverWorld || InputManager.IsPointerOverAllowingCursorUI)
		{
			Game.Cursor.CursorController.SetTile();
		}
		else
		{
			Game.Cursor.PreviousTile = Game.Cursor.Tile;
			Game.Cursor.PreviousTilePosition = Game.Cursor.TilePosition;
			if (Game.Cursor.Tile != null)
			{
				CursorView.ClearTiles(Game.Cursor.Tile);
				PlayableUnitManager.OnCursorTileBecomeNull();
				TileObjectSelectionManager.UpdateCursorOrientationFromSelection();
				Game.Cursor.Tile = null;
			}
		}
		if (InputManager.GetButtonDown(5))
		{
			ConstructionManager.OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive);
		}
		else if (InputManager.GetButtonDown(62))
		{
			ConstructionManager.OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Production);
		}
		if (!(((StateMachine)ApplicationManager.Application).State is GameState))
		{
			return;
		}
		if (InputManager.GetButtonDown(7) && GameController.CanEndPlayerTurn() && !InputManager.GetButtonDown(55))
		{
			HandleEndTurnInput();
		}
		else if (SaveManager.BrokenSavePath != null && UIManager.DisplayGameSaveErrorPopUp(SaveManager.BrokenSavePath, SaveManager.BrokenSaveReason, SaveManager.BackupLoaded))
		{
			if (!SaveManager.BackupLoaded)
			{
				((CLogger<ApplicationManager>)TPSingleton<ApplicationManager>.Instance).LogError((object)"===============================[ LOGBAR ]===============================", (CLogLevel)1, true, false);
				((CLogger<ApplicationManager>)TPSingleton<ApplicationManager>.Instance).LogError((object)"Hello again - from now on, you can stop ignoring NullRefs.", (CLogLevel)1, true, false);
			}
			SaveManager.BrokenSavePath = null;
			SaveManager.BackupLoaded = false;
		}
	}

	public static void VictorySequenceCallback()
	{
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		if (selectedCity.CityDefinition.PostVictoryCutscene != null)
		{
			AnimatedCutsceneManager.PlayPostVictoryAnimatedCutscene(selectedCity.CityDefinition.PostVictoryCutscene, TriggerMagicSealsCompleted);
		}
		else
		{
			TriggerMagicSealsCompleted();
		}
	}

	public void StopAmbientSounds()
	{
		SoundManager.FadeOutAudioSource(ambienceAudioSource, ref ambientSoundFadeTween, ambientSoundsFadeOutDuration);
	}

	private void StartAmbientSounds()
	{
		string text = $"Sounds/SFX/Ambient/AMB_Towns/AMB_{TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id}";
		AudioClip val = ResourcePooler.LoadOnce<AudioClip>(text, false);
		if ((Object)(object)val != (Object)null)
		{
			SoundManager.PlayFadeInAudioClip(ambienceAudioSource, ref ambientSoundFadeTween, val, ambientSoundsFadeInDuration);
		}
		else
		{
			((CLogger<GameManager>)this).LogWarning((object)("No ambient sounds found at path " + text + "."), (CLogLevel)1, true, false);
		}
	}

	[DevConsoleCommand("ReloadScene")]
	public static void DebugReloadGameScene()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(((Scene)(ref activeScene)).name);
		GC.Collect();
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	private static void DebugSave()
	{
		if (!TPSingleton<GameManager>.Instance.IsSaveAllowed())
		{
			((CLogger<GameManager>)TPSingleton<GameManager>.Instance).Log((object)"Tried to save game but it was not allowed.", (CLogLevel)0, true, false);
		}
		else
		{
			SaverLoader.EnqueueSave(E_SaveType.Game);
		}
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	private static void DebugTriggerVictorySequence([StringConverter(typeof(StringToCityIdConverter))] string sequenceCityId = "")
	{
		GameController.SetState(Game.E_State.CutscenePlaying);
		GameView.TopScreenPanel.UnitPortraitsPanel.Display(show: false);
		GameView.TopScreenPanel.TurnPanel.Display(show: false);
		TPSingleton<ToDoListView>.Instance.Hide();
		PlayableUnitManager.GatherUnitsForVictorySequence();
		TPSingleton<CutsceneManager>.Instance.VictorySequenceView.debugCityIdOverride = sequenceCityId;
		CutsceneManager.PlayCutscene(TPSingleton<CutsceneManager>.Instance.VictorySequenceView, VictorySequenceCallback);
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	private static void DebugTriggerTutorialSequence()
	{
		CutsceneManager.PlayCutscene(TPSingleton<CutsceneManager>.Instance.TutorialSequenceView);
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	private static void DebugTriggerDefeatSequence()
	{
		BuildingManager.MagicCircle.BuildingController.DamageableModuleController.Demolish();
	}
}
