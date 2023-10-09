using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Building;
using TheLastStand.Database.Meta;
using TheLastStand.Database.Unit;
using TheLastStand.Database.WorldMap;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class MetaDatabase : Database<MetaDatabase>
{
	[SerializeField]
	private TextAsset[] metaUpgradesDefinitions;

	public static Dictionary<string, MetaUpgradeDefinition> MetaUpgradesDefinitions { get; private set; }

	public static Dictionary<Type, List<string>> DefaultEnabledIdsByMetaEffectType { get; private set; } = new Dictionary<Type, List<string>>();


	public override void Deserialize(XContainer container = null)
	{
		Queue<XElement> queue = GatherElements(metaUpgradesDefinitions, null, "MetaUpgradeDefinition");
		MetaUpgradesDefinitions = new Dictionary<string, MetaUpgradeDefinition>();
		int num = 0;
		while (queue.Count > 0)
		{
			MetaUpgradeDefinition metaUpgradeDefinition = new MetaUpgradeDefinition((XContainer)(object)queue.Dequeue(), num++);
			try
			{
				MetaUpgradesDefinitions.Add(metaUpgradeDefinition.Id, metaUpgradeDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log((object)("Duplicate MetaUpgradeDefinition found for Id " + metaUpgradeDefinition.Id + "."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		ComputeDefaultsActivatedIds();
	}

	private void ComputeDefaultsActivatedIds()
	{
		DefaultEnabledIdsByMetaEffectType.Add(typeof(UnlockAffixesMetaEffectDefinition), ItemDatabase.AffixDefinitions.Keys.ToList());
		DefaultEnabledIdsByMetaEffectType.Add(typeof(UnlockBuildingMetaEffectDefinition), BuildingDatabase.BuildingDefinitions.Keys.ToList());
		DefaultEnabledIdsByMetaEffectType.Add(typeof(UnlockBuildingUpgradeMetaEffectDefinition), BuildingDatabase.BuildingUpgradeDefinitions.Keys.ToList());
		DefaultEnabledIdsByMetaEffectType.Add(typeof(UnlockCitiesMetaEffectDefinition), CityDatabase.CityDefinitions.Keys.ToList());
		DefaultEnabledIdsByMetaEffectType.Add(typeof(UnlockItemsMetaEffectDefinition), ItemDatabase.ItemDefinitions.Keys.ToList());
		DefaultEnabledIdsByMetaEffectType.Add(typeof(NewEnemyMetaEffectDefinition), EnemyUnitDatabase.EnemyUnitTemplateDefinitions.Keys.ToList());
		DefaultEnabledIdsByMetaEffectType.Add(typeof(UnlockTraitsMetaEffectDefinition), PlayableUnitDatabase.UnitTraitDefinitions.Keys.ToList());
		DefaultEnabledIdsByMetaEffectType.Add(typeof(UnlockWavesMetaEffectDefinition), SpawnWaveDatabase.WaveDefinitions.Keys.ToList());
		DefaultEnabledIdsByMetaEffectType.Add(typeof(UnlockGlyphsMetaEffectDefinition), GlyphDatabase.GlyphDefinitions.Keys.ToList());
		foreach (string key in MetaUpgradesDefinitions.Keys)
		{
			for (int num = MetaUpgradesDefinitions[key].UpgradeEffectDefinitions.Count - 1; num >= 0; num--)
			{
				if (DefaultEnabledIdsByMetaEffectType.TryGetValue(MetaUpgradesDefinitions[key].UpgradeEffectDefinitions[num].GetType(), out var value))
				{
					List<string> relatedIdsFromUpgrade = GetRelatedIdsFromUpgrade(MetaUpgradesDefinitions[key].UpgradeEffectDefinitions[num]);
					for (int num2 = relatedIdsFromUpgrade.Count - 1; num2 >= 0; num2--)
					{
						if (value.Contains(relatedIdsFromUpgrade[num2]))
						{
							value.Remove(relatedIdsFromUpgrade[num2]);
						}
					}
				}
			}
		}
	}

	private List<string> GetRelatedIdsFromUpgrade(MetaEffectDefinition metaEffectDefinition)
	{
		if (!(metaEffectDefinition is UnlockAffixesMetaEffectDefinition unlockAffixesMetaEffectDefinition))
		{
			if (!(metaEffectDefinition is UnlockBuildingMetaEffectDefinition unlockBuildingMetaEffectDefinition))
			{
				if (!(metaEffectDefinition is UnlockBuildingUpgradeMetaEffectDefinition unlockBuildingUpgradeMetaEffectDefinition))
				{
					if (!(metaEffectDefinition is UnlockCitiesMetaEffectDefinition unlockCitiesMetaEffectDefinition))
					{
						if (!(metaEffectDefinition is UnlockItemsMetaEffectDefinition unlockItemsMetaEffectDefinition))
						{
							if (!(metaEffectDefinition is UnlockTraitsMetaEffectDefinition unlockTraitsMetaEffectDefinition))
							{
								if (!(metaEffectDefinition is UnlockWavesMetaEffectDefinition unlockWavesMetaEffectDefinition))
								{
									if (!(metaEffectDefinition is NewEnemyMetaEffectDefinition newEnemyMetaEffectDefinition))
									{
										if (metaEffectDefinition is UnlockGlyphsMetaEffectDefinition unlockGlyphsMetaEffectDefinition)
										{
											return unlockGlyphsMetaEffectDefinition.GlyphIds;
										}
										return null;
									}
									return new List<string> { newEnemyMetaEffectDefinition.EnemyId };
								}
								return unlockWavesMetaEffectDefinition.WavesToUnlock;
							}
							return unlockTraitsMetaEffectDefinition.TraitsToUnlock;
						}
						return unlockItemsMetaEffectDefinition.ItemsToUnlock;
					}
					return unlockCitiesMetaEffectDefinition.CitiesToUnlock;
				}
				return new List<string> { unlockBuildingUpgradeMetaEffectDefinition.UpgradeId };
			}
			return new List<string> { unlockBuildingMetaEffectDefinition.BuildingId };
		}
		return unlockAffixesMetaEffectDefinition.AffixesToUnlock;
	}
}
