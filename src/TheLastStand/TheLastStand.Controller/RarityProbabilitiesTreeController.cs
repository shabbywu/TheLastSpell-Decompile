using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;

namespace TheLastStand.Controller;

public class RarityProbabilitiesTreeController
{
	public static ItemDefinition.E_Rarity GenerateRarity(ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition, HashSet<int> allowedValues = null)
	{
		return GenerateRarity(ComputeRarityProbabilities(probabilityTreeEntriesDefinition), allowedValues);
	}

	public static ItemDefinition.E_Rarity GenerateRarity(Dictionary<int, int> probabilities, HashSet<int> allowedValues = null)
	{
		int num = allowedValues?.Max() ?? 4;
		int num2 = allowedValues?.Min() ?? 1;
		for (int num3 = num; num3 >= num2; num3--)
		{
			if (probabilities.ContainsKey(num3) && RandomManager.GetRandomRange("RarityProbabilitiesTreeController", 0, 100) < probabilities[num3])
			{
				return (ItemDefinition.E_Rarity)num3;
			}
		}
		return (ItemDefinition.E_Rarity)num2;
	}

	public static Dictionary<int, int> ComputeRarityProbabilities(ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(probabilityTreeEntriesDefinition.ProbabilityLevels);
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		if (TPSingleton<GlyphManager>.Instance.RarityProbabilityTreeModifiers.TryGetValue(probabilityTreeEntriesDefinition.Id, out var value))
		{
			dictionary = DictionaryExtensions.Add(dictionary, value);
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<ItemRaritiesMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int num = effects.Length - 1; num >= 0; num--)
			{
				ItemRaritiesMetaEffectDefinition itemRaritiesMetaEffectDefinition = effects[num];
				if (itemRaritiesMetaEffectDefinition.RarityTreeId == probabilityTreeEntriesDefinition.Id)
				{
					foreach (KeyValuePair<int, int> item in itemRaritiesMetaEffectDefinition.WeightBonusByRarityLevel)
					{
						if (dictionary2.ContainsKey(item.Key))
						{
							dictionary2[item.Key] += item.Value;
						}
						else
						{
							dictionary2.Add(item.Key, item.Value);
						}
					}
				}
			}
		}
		return DictionaryExtensions.Add(dictionary, dictionary2);
	}
}
