using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.WorldMap;
using TheLastStand.Database.WorldMap;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using TheLastStand.Model;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.MetaShops;
using TheLastStand.View.WorldMap;
using TheLastStand.View.WorldMap.Glyphs;
using UnityEngine;

namespace TheLastStand.Manager.WorldMap;

public class WorldMapCityManager : Manager<WorldMapCityManager>, ISerializable, IDeserializable
{
	[SerializeField]
	private WorldMapCityView cityViewPrefab;

	private WorldMapCity selectedCity;

	private bool hasBeenInitialized;

	public LastRunInfo LastRunInfo { get; private set; }

	public WorldMapCity SelectedCity
	{
		get
		{
			return TPSingleton<WorldMapCityManager>.Instance.selectedCity;
		}
		set
		{
			if (TPSingleton<WorldMapCityManager>.Instance.selectedCity != value)
			{
				TPSingleton<WorldMapCityManager>.Instance.selectedCity?.WorldMapCityView.OnDeselection();
				TPSingleton<WorldMapCityManager>.Instance.selectedCity = value;
				TPSingleton<WorldMapCityManager>.Instance.selectedCity?.WorldMapCityView.OnSelection();
				WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.FOCUSED);
			}
		}
	}

	public List<WorldMapCity> Cities { get; private set; }

	private static WorldMapCity CreateCity(CityDefinition cityDefinition, int saveVersion, SerializedCity cityElement = null)
	{
		WorldMapCity worldMapCity = new WorldMapCityController(cityElement, cityDefinition, null, saveVersion).WorldMapCity;
		InitCity(worldMapCity);
		return worldMapCity;
	}

	private static WorldMapCityView CreateCityView(CityDefinition cityDefinition, Transform parent)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		WorldMapCityView worldMapCityView = Object.Instantiate<WorldMapCityView>(TPSingleton<WorldMapCityManager>.Instance.cityViewPrefab, parent);
		((Component)worldMapCityView).gameObject.transform.localPosition = Vector2.op_Implicit(cityDefinition.WorldMapPosition);
		((Object)((Component)worldMapCityView).gameObject).name = cityDefinition.Id;
		return worldMapCityView;
	}

	private static void InitCity(WorldMapCity city)
	{
		TPSingleton<WorldMapCityManager>.Instance.Cities.Add(city);
	}

	private void GenerateInitialCities(SerializedCities serializedCities, int saveVersion)
	{
		foreach (KeyValuePair<string, CityDefinition> cityDefinition in CityDatabase.CityDefinitions)
		{
			try
			{
				SerializedCity cityElement = serializedCities?.Cities.FirstOrDefault((SerializedCity city) => city.Id == cityDefinition.Key);
				CreateCity(cityDefinition.Value, saveVersion, cityElement);
			}
			catch (MissingAssetException<CityDatabase> arg)
			{
				((CLogger<WorldMapCityManager>)this).LogError((object)$"{arg}\nThis asset will be skipped.", (CLogLevel)1, true, true);
			}
		}
		if (serializedCities != null)
		{
			selectedCity = Cities.FirstOrDefault((WorldMapCity x) => x.CityDefinition.Id == serializedCities.SelectedCityId);
		}
	}

	private void Init()
	{
		if (!hasBeenInitialized)
		{
			hasBeenInitialized = true;
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
		}
	}

	private void CreateCityViews()
	{
		Cities.ForEach(delegate(WorldMapCity x)
		{
			x.RefreshIsUnlocked();
		});
		foreach (WorldMapCity city in Cities)
		{
			city.WorldMapCityView = CreateCityView(city.CityDefinition, TPSingleton<WorldMapRefsManager>.Instance.CitiesParent);
			city.WorldMapCityView.WorldMapCity = city;
			city.WorldMapCityView.Init();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		TPSingleton<WorldMapCityManager>.Instance.Init();
	}

	public static void OnStateChange()
	{
		WorldMapStateManager.WorldMapState currentState = TPSingleton<WorldMapStateManager>.Instance.CurrentState;
		if (currentState != WorldMapStateManager.WorldMapState.EXPLORATION)
		{
			_ = 2;
			return;
		}
		TPSingleton<WorldMapCityManager>.Instance.selectedCity?.WorldMapCityView.OnDeselection();
		TPSingleton<WorldMapCityManager>.Instance.selectedCity = null;
	}

	public static void StartNewGame()
	{
		if (TPSingleton<OraculumView>.Instance.OpeningOrClosing || TPSingleton<GlyphSelectionPanel>.Instance.OpeningOrClosing)
		{
			return;
		}
		ApocalypseManager.SetApocalypse(WorldMapUIManager.GetApocalypseSelectedLevel());
		if (!ApplicationManager.Application.HasSeenIntroduction)
		{
			ApplicationManager.Application.HasSeenIntroduction = true;
			AnimatedCutsceneManager.PlayPreGameAnimatedCutscene("NewGame");
			return;
		}
		for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
		{
			TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeOut(TPSingleton<CanvasFadeManager>.Instance.FadeDuration);
		}
		TPSingleton<WorldMapCityManager>.Instance.ResetLastRunInfo();
		if (TPSingleton<WorldMapCityManager>.Instance.selectedCity.CityDefinition.PreGameCutscene != null && TPSingleton<WorldMapCityManager>.Instance.selectedCity.NumberOfRuns < 1)
		{
			AnimatedCutsceneManager.PlayPreGameAnimatedCutscene("NewGame", TPSingleton<WorldMapCityManager>.Instance.selectedCity.CityDefinition.PreGameCutscene);
		}
		else
		{
			ApplicationManager.Application.ApplicationController.SetState("NewGame");
		}
	}

	public WorldMapCity GetPreviousCity(IEnumerable<WorldMapCity> cities, WorldMapCity city)
	{
		return cities.TakeWhile((WorldMapCity x) => x != city).DefaultIfEmpty(cities.ElementAt(cities.Count() - 1)).LastOrDefault();
	}

	public WorldMapCity GetNextCity(IEnumerable<WorldMapCity> cities, WorldMapCity city)
	{
		return cities.SkipWhile((WorldMapCity x) => x != city).Skip(1).DefaultIfEmpty(cities.ElementAt(0))
			.FirstOrDefault();
	}

	public void HandleGameOver(Game.E_GameOverCause cause)
	{
		if (selectedCity == null)
		{
			((CLogger<WorldMapCityManager>)this).LogWarning((object)"Called HandleGameOver but the selected city is null. Something might have been wrong in the game flow. Skipping", (CLogLevel)1, true, true);
			return;
		}
		selectedCity.NumberOfRuns++;
		bool num = SelectedCity.CityDefinition.IsTutorialMap && (TPSingleton<CutsceneManager>.Instance.TutorialSequenceView.IsPlaying || cause == Game.E_GameOverCause.Abandon);
		LastRunInfo = new LastRunInfo(selectedCity, cause);
		if (num || cause == Game.E_GameOverCause.MagicSealsCompleted)
		{
			selectedCity.NumberOfWins++;
			ApocalypseManager.TryIncreaseMaxApocalypseIndexAvailable();
			selectedCity.MaxApocalypsePassed = Mathf.Max(selectedCity.MaxApocalypsePassed, ApocalypseManager.CurrentApocalypseIndex);
		}
	}

	public bool IsCurrentSelectedCityLastPlayedCity()
	{
		if (LastRunInfo != null)
		{
			return LastRunInfo.CityId == SelectedCity.CityDefinition.Id;
		}
		return false;
	}

	public void ResetLastRunInfo()
	{
		LastRunInfo = null;
	}

	public void SelectNextCity()
	{
		if (selectedCity == null)
		{
			SelectedCity = Cities.FirstOrDefault((WorldMapCity x) => x.IsUnlocked && x.IsSelectable);
		}
		else
		{
			SelectedCity = GetNextCity(Cities.Where((WorldMapCity x) => x.IsUnlocked && x.IsSelectable), selectedCity);
		}
	}

	public void SelectPreviousCity()
	{
		if (selectedCity == null)
		{
			SelectedCity = Cities.FirstOrDefault((WorldMapCity x) => x.IsUnlocked && x.IsSelectable);
		}
		else
		{
			SelectedCity = GetPreviousCity(Cities.Where((WorldMapCity x) => x.IsUnlocked && x.IsSelectable), selectedCity);
		}
	}

	public void SelectCity(WorldMapCity city)
	{
		if (city.IsSelectable && SelectedCity == null && !ACameraView.IsZooming)
		{
			SelectedCity = city;
		}
	}

	public void OnApplicationStateChange(State state)
	{
		string name = state.GetName();
		if (name != null && name == "WorldMap")
		{
			TPSingleton<WorldMapCityManager>.Instance.selectedCity = null;
			CreateCityViews();
			TPSingleton<GlyphManager>.Instance.RemoveOverflowingGlyphs();
			for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
			{
				TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeIn();
			}
		}
	}

	private void Update()
	{
		if (!(((StateMachine)ApplicationManager.Application).State.GetName() == "WorldMap"))
		{
			return;
		}
		if (InputManager.GetButtonDown(97) && WorldMapUIManager.CanAccessMetaShops && !TPSingleton<OraculumView>.Instance.Displayed && !TPSingleton<OraculumView>.Instance.OpeningOrClosing && !TPSingleton<GlyphSelectionPanel>.Instance.Displayed && !TPSingleton<GlyphSelectionPanel>.Instance.OpeningOrClosing)
		{
			TPSingleton<MetaShopsManager>.Instance.OpenShops();
		}
		if (TPSingleton<OraculumView>.Instance.Displayed || TPSingleton<OraculumView>.Instance.OpeningOrClosing || TPSingleton<GlyphSelectionPanel>.Instance.Displayed || TPSingleton<GlyphSelectionPanel>.Instance.OpeningOrClosing)
		{
			return;
		}
		if (InputManager.GetButtonDown(100))
		{
			if (SelectedCity != null && SelectedCity.WorldMapCityView.ValidApocalypseStateToStart && !GenericConsent.IsOpen)
			{
				for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
				{
					TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeOut(TPSingleton<CanvasFadeManager>.Instance.FadeDuration);
				}
				GameConfigurationsView.StartNewGameIfEnoughGlyph();
			}
		}
		else if (InputManager.GetButtonDown(107))
		{
			SelectNextCity();
		}
		else if (InputManager.GetButtonDown(108))
		{
			SelectPreviousCity();
		}
		else
		{
			if (!InputManager.IsLastControllerJoystick || !InputManager.GetButtonDown(79))
			{
				return;
			}
			foreach (WorldMapCity city in Cities)
			{
				if (city.WorldMapCityView.IsHovered)
				{
					SelectCity(city);
					break;
				}
			}
		}
	}

	public void DeserializeCities(int saveVersion, ISerializedData container = null)
	{
		Cities = new List<WorldMapCity>();
		SerializedCities serializedCities = container as SerializedCities;
		GenerateInitialCities(serializedCities, saveVersion);
		LastRunInfo = serializedCities?.LastRunInfo;
		if (container == null)
		{
			selectedCity = Cities.FirstOrDefault((WorldMapCity o) => MetaUpgradesManager.IsThisCityUnlockedByDefault(o.CityDefinition.Id));
			((CLogger<WorldMapCityManager>)TPSingleton<WorldMapCityManager>.Instance).Log((object)("No city were selected on deserialization, probably due to new AppSave creation, selecting " + selectedCity.CityDefinition.Id + " by default."), (CLogLevel)0, false, false);
		}
		if (selectedCity == null)
		{
			selectedCity = Cities.FirstOrDefault();
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
	}

	public ISerializedData Serialize()
	{
		SerializedCities serializedCities = new SerializedCities
		{
			SelectedCityId = selectedCity?.CityDefinition.Id,
			LastRunInfo = LastRunInfo
		};
		foreach (WorldMapCity city in Cities)
		{
			serializedCities.Cities.Add(city.Serialize() as SerializedCity);
		}
		return (ISerializedData)(object)serializedCities;
	}

	[DevConsoleCommand("ShowAllCities")]
	public static void ShowAllCities()
	{
		foreach (WorldMapCity city in TPSingleton<WorldMapCityManager>.Instance.Cities)
		{
			((Component)city.WorldMapCityView).gameObject.SetActive(true);
		}
	}
}
