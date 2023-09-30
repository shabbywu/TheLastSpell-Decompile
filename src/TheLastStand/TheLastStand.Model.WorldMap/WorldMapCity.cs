using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.WorldMap;
using TheLastStand.Database.Meta;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Serialization;
using TheLastStand.View.WorldMap;
using UnityEngine;

namespace TheLastStand.Model.WorldMap;

public class WorldMapCity : ISerializable, IDeserializable
{
	public int CurrentGlyphPoints;

	public int MaxApocalypsePassed = -1;

	public int MaxNightReached;

	public int NumberOfRuns;

	public int NumberOfWins;

	public WorldMapCityView WorldMapCityView;

	public GlyphsConfig GlyphsConfig;

	public bool HaveWon => NumberOfWins > 0;

	public CityDefinition CityDefinition { get; private set; }

	public bool IsSelectable { get; private set; }

	public bool IsUnlocked { get; private set; }

	public WorldMapCityController WorldMapCityController { get; private set; }

	public WorldMapCity(CityDefinition definition, WorldMapCityController controller, WorldMapCityView view)
	{
		CityDefinition = definition;
		WorldMapCityController = controller;
		WorldMapCityView = view;
		GlyphsConfig = new GlyphsConfig
		{
			SelectedGlyphs = new List<GlyphDefinition>()
		};
		RefreshIsUnlocked();
	}

	public int GetCustomModeBonusPoints()
	{
		return Mathf.Max(0, CurrentGlyphPoints - CityDefinition.MaxGlyphPoints);
	}

	public bool RefreshIsSelectable()
	{
		IsSelectable = !CityDefinition.Hidden && (!CityDefinition.IsTutorialMap || TPSingleton<WorldMapCityManager>.Instance.Cities.Count((WorldMapCity c) => !c.CityDefinition.Hidden && c.IsUnlocked && c != this) <= 0);
		return IsSelectable;
	}

	public bool RefreshIsUnlocked()
	{
		IsUnlocked = !TPSingleton<MetaUpgradesManager>.Instance.GetLockedCitiesIds().Contains(CityDefinition.Id);
		return IsUnlocked;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (!(container is SerializedCity serializedCity))
		{
			return;
		}
		MaxApocalypsePassed = Mathf.Min(serializedCity.MaxApoPassed, TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable);
		if (saveVersion >= 0 && saveVersion <= 11)
		{
			MaxNightReached = ((serializedCity.NumberOfWins > 1) ? CityDefinition.VictoryDaysCount : 0);
		}
		else
		{
			MaxNightReached = serializedCity.MaxNightReached;
		}
		NumberOfRuns = serializedCity.NumberOfRuns;
		if (serializedCity.NumberOfWins > 0)
		{
			NumberOfWins = serializedCity.NumberOfWins;
		}
		else
		{
			NumberOfWins = ((MaxApocalypsePassed >= 0) ? 1 : 0);
			if (NumberOfWins > 0)
			{
				TPSingleton<MetaConditionManager>.Instance.IncreaseRunsCompleted(isGameOver: false, this, MaxApocalypsePassed);
			}
		}
		foreach (string selectedGlyph in serializedCity.SelectedGlyphs)
		{
			if (GlyphDatabase.GlyphDefinitions.TryGetValue(selectedGlyph, out var value))
			{
				GlyphsConfig.SelectedGlyphs.Add(value);
			}
		}
		GlyphsConfig.CustomModeEnabled = serializedCity.CustomModeEnabled;
		CurrentGlyphPoints = GlyphsConfig.SelectedGlyphs.Sum((GlyphDefinition o) => o.Cost);
	}

	public ISerializedData Serialize()
	{
		return (ISerializedData)(object)new SerializedCity
		{
			Id = CityDefinition.Id,
			MaxApoPassed = MaxApocalypsePassed,
			MaxNightReached = MaxNightReached,
			NumberOfRuns = NumberOfRuns,
			NumberOfWins = NumberOfWins,
			SelectedGlyphs = GlyphsConfig.SelectedGlyphs.Select((GlyphDefinition glyph) => glyph.Id).ToList(),
			CustomModeEnabled = GlyphsConfig.CustomModeEnabled
		};
	}
}
