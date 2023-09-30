using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Fog;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Model.Apocalypse;

public class Apocalypse
{
	public Dictionary<string, Dictionary<UnitStatDefinition.E_Stat, float>> EnemiesStatsModifiers = new Dictionary<string, Dictionary<UnitStatDefinition.E_Stat, float>>();

	public List<ApocalypseEffectDefinition> AllEffects { get; } = new List<ApocalypseEffectDefinition>();


	public float FogSpawnersApocalypseMultiplier { get; private set; }

	public int Id { get; }

	public int DailyFogUpdateFrequencyModifier
	{
		get
		{
			int num = 0;
			for (int i = 0; i < AllEffects.Count; i++)
			{
				if (AllEffects[i] is IncreaseDailyFogUpdateFrequencyApocalypseEffectDefinition increaseDailyFogUpdateFrequencyApocalypseEffectDefinition)
				{
					num += increaseDailyFogUpdateFrequencyApocalypseEffectDefinition.Value;
				}
			}
			return num;
		}
	}

	public int ExtraPercentageOfEnemies
	{
		get
		{
			int num = 0;
			for (int i = 0; i < AllEffects.Count; i++)
			{
				if (AllEffects[i] is IncreaseEnemiesNumberApocalypseEffectDefinition increaseEnemiesNumberApocalypseEffectDefinition)
				{
					num += increaseEnemiesNumberApocalypseEffectDefinition.Value;
				}
			}
			return num;
		}
	}

	public bool GenerateMalusAffixes => AllEffects.Count((ApocalypseEffectDefinition o) => o is GenerateMalusAffixesApocalypseEffectDefinition) > 0;

	public int StartingFogDensityModifier
	{
		get
		{
			int num = 0;
			for (int i = 0; i < AllEffects.Count; i++)
			{
				if (AllEffects[i] is IncreaseStartingFogDensityApocalypseEffectDefinition increaseStartingFogDensityApocalypseEffectDefinition)
				{
					num += increaseStartingFogDensityApocalypseEffectDefinition.Value;
				}
			}
			return num;
		}
	}

	private ApocalypseDefinition[] ApocalypseDefinitions { get; }

	public Apocalypse(ApocalypseDefinition[] apocalypseDefinitions, int id)
	{
		ApocalypseDefinitions = apocalypseDefinitions;
		Id = id;
		ComputeAllEffects();
		ComputeEnemiesStatsModifiers();
		ComputeFogSpawnersMultiplier();
	}

	private void ComputeFogSpawnersMultiplier()
	{
		FogSpawnersApocalypseMultiplier = 0f;
		foreach (ApocalypseEffectDefinition allEffect in AllEffects)
		{
			if (allEffect is GenerateFogSpawnersApocalypseEffectDefinition generateFogSpawnersApocalypseEffectDefinition)
			{
				FogSpawnersApocalypseMultiplier = generateFogSpawnersApocalypseEffectDefinition.Multiplier;
			}
		}
	}

	public int ExtraPercentageForCosts(ResourceManager.E_PriceModifierType type)
	{
		int num = 0;
		for (int i = 0; i < AllEffects.Count; i++)
		{
			if (AllEffects[i] is IncreasePricesApocalypseEffectDefinition increasePricesApocalypseEffectDefinition && increasePricesApocalypseEffectDefinition.Type.HasFlag(type))
			{
				num += increasePricesApocalypseEffectDefinition.Value;
			}
		}
		return num;
	}

	public int GetModifiedLightFogSpawnersCount(int count)
	{
		if (!FogDatabase.LightFogDefinition.LightFogSpawnersMultipliers.TryGetValue(TPSingleton<FogManager>.Instance.Fog.DensityName, out var value))
		{
			((CLogger<ApocalypseManager>)TPSingleton<ApocalypseManager>.Instance).LogError((object)("LightFogSpawners multiplier isn't defined for density : \"" + TPSingleton<FogManager>.Instance.Fog.DensityName + "\"."), (CLogLevel)1, true, false);
			value = 1f;
		}
		return Mathf.RoundToInt((float)count * FogSpawnersApocalypseMultiplier * value);
	}

	private void ComputeAllEffects()
	{
		ApocalypseDefinition[] apocalypseDefinitions = ApocalypseDefinitions;
		foreach (ApocalypseDefinition apocalypseDefinition in apocalypseDefinitions)
		{
			if (apocalypseDefinition.Effects != null || apocalypseDefinition.Effects.Count != 0)
			{
				AllEffects.AddRange(apocalypseDefinition.Effects);
			}
		}
	}

	private void ComputeEnemiesStatsModifiers()
	{
		for (int num = AllEffects.Count - 1; num >= 0; num--)
		{
			if (AllEffects[num] is EnemiesStatModifierApocalyseEffectDefinition enemiesStatModifierApocalyseEffectDefinition)
			{
				for (int num2 = enemiesStatModifierApocalyseEffectDefinition.AffectedEnemies.Count - 1; num2 >= 0; num2--)
				{
					if (!EnemiesStatsModifiers.ContainsKey(enemiesStatModifierApocalyseEffectDefinition.AffectedEnemies[num2]))
					{
						EnemiesStatsModifiers.Add(enemiesStatModifierApocalyseEffectDefinition.AffectedEnemies[num2], new Dictionary<UnitStatDefinition.E_Stat, float>());
					}
					if (!EnemiesStatsModifiers[enemiesStatModifierApocalyseEffectDefinition.AffectedEnemies[num2]].ContainsKey(enemiesStatModifierApocalyseEffectDefinition.Stat))
					{
						EnemiesStatsModifiers[enemiesStatModifierApocalyseEffectDefinition.AffectedEnemies[num2]].Add(enemiesStatModifierApocalyseEffectDefinition.Stat, enemiesStatModifierApocalyseEffectDefinition.Value);
					}
					else
					{
						EnemiesStatsModifiers[enemiesStatModifierApocalyseEffectDefinition.AffectedEnemies[num2]][enemiesStatModifierApocalyseEffectDefinition.Stat] += enemiesStatModifierApocalyseEffectDefinition.Value;
					}
				}
			}
		}
	}
}
