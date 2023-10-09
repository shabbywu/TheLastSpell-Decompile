using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.Meta;
using TheLastStand.Database;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Helpers;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Item;
using UnityEngine;

namespace TheLastStand.Manager.Item;

public class ItemManager : Manager<ItemManager>
{
	public struct ItemGenerationInfo
	{
		public ItemSlotDefinition.E_ItemSlotId Destination;

		public ItemDefinition ItemDefinition;

		public int Level;

		public ItemDefinition.E_Rarity Rarity;

		public bool SkipMalusAffixes;
	}

	[SerializeField]
	private int nightRewardsCount = 3;

	[SerializeField]
	private int prodRewardsCount = 3;

	public TheLastStand.Model.Item.Item EquippedItemBeingCompared;

	public TheLastStand.Model.Item.Item EquippedItemBeingComparedOffHand;

	public int NightRewardsCount => nightRewardsCount + TPSingleton<GlyphManager>.Instance.NightRewardsCountModifier;

	public int ProdRewardsCount => prodRewardsCount + TPSingleton<GlyphManager>.Instance.ProdRewardsCountModifier;

	public static TheLastStand.Model.Item.Item GenerateItem(ItemGenerationInfo generationInfo)
	{
		((CLogger<ItemManager>)TPSingleton<ItemManager>.Instance).Log((object)$"Generating item {generationInfo.ItemDefinition.Id} (level {generationInfo.Level}, {generationInfo.Rarity.ToString()} rarity, headed to {generationInfo.Destination}.", (CLogLevel)0, false, false);
		TheLastStand.Model.Item.Item item = new ItemController(generationInfo.ItemDefinition, generationInfo.Level, generationInfo.Rarity).Item;
		Dictionary<AffixDefinition, float> dictionary = ComputeAvailableAffixDefinitions(generationInfo, item);
		Dictionary<int, float> dictionary2 = new Dictionary<int, float>(ItemDatabase.AffixLevelsDefinition.AffixLevelsProbas[generationInfo.Level]);
		Dictionary<AffixDefinition, int> dictionary3 = new Dictionary<AffixDefinition, int>();
		int num = ItemDatabase.AffixesCountPerRarity[generationInfo.Rarity];
		while (item.AdditionalAffixes.Count < num && dictionary.Count > 0 && dictionary2.Count > 0)
		{
			int randomItemFromWeights = DictionaryHelpers.GetRandomItemFromWeights(dictionary2, TPSingleton<ItemManager>.Instance);
			Dictionary<AffixDefinition, float> dictionary4 = ComputePotentialAffixDefinitions(dictionary, randomItemFromWeights);
			if (dictionary4.Count == 0)
			{
				dictionary2.Remove(randomItemFromWeights);
				continue;
			}
			AffixDefinition randomItemFromWeights2 = DictionaryHelpers.GetRandomItemFromWeights(dictionary4, TPSingleton<ItemManager>.Instance);
			Affix affix = new AffixController(randomItemFromWeights2).Affix;
			affix.Level = randomItemFromWeights;
			item.AdditionalAffixes.Add(affix);
			dictionary3.AddValueOrCreateKey(randomItemFromWeights2, 1, (int a, int b) => a + b);
			if (randomItemFromWeights2.MaxOccurrences != -1 && dictionary3[randomItemFromWeights2] >= randomItemFromWeights2.MaxOccurrences)
			{
				dictionary.Remove(randomItemFromWeights2);
			}
		}
		if (item.AdditionalAffixes.Count > 0 && item.Rarity == ItemDefinition.E_Rarity.Epic)
		{
			item.AdditionalAffixes[RandomManager.GetRandomRange(TPSingleton<ItemManager>.Instance, 0, item.AdditionalAffixes.Count)].IsEpic = true;
		}
		ApplyMaluses(generationInfo, item);
		switch (generationInfo.Destination)
		{
		case ItemSlotDefinition.E_ItemSlotId.Inventory:
			if (TPSingleton<InventoryManager>.Instance.Inventory.ItemCount < TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots.Count)
			{
				TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.AddItem(item, null, isNewItem: true);
			}
			break;
		case ItemSlotDefinition.E_ItemSlotId.Shop:
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.AddItem(item);
			break;
		}
		return item;
	}

	private static void ApplyMaluses(ItemGenerationInfo generationInfo, TheLastStand.Model.Item.Item item)
	{
		if (generationInfo.SkipMalusAffixes || !ApocalypseManager.CurrentApocalypse.GenerateMalusAffixes || (ItemDefinition.E_Category.Usable & item.ItemDefinition.Category) != 0)
		{
			return;
		}
		AffixMalusDefinition.E_MalusLevel malusLevel = AffixMalusDefinition.E_MalusLevel.Undefined;
		UnitStatDefinition.E_Stat key = UnitStatDefinition.E_Stat.Undefined;
		Dictionary<AffixMalusDefinition.E_MalusLevel, float> dictionary = ItemDatabase.AffixLevelsDefinition.AffixMalusLevelsProbas[item.Level];
		float num = RandomManager.GetRandomRange(max: dictionary.Values.Sum(), caller: TPSingleton<ItemManager>.Instance, min: 0f);
		float num2 = 0f;
		foreach (KeyValuePair<AffixMalusDefinition.E_MalusLevel, float> item2 in dictionary)
		{
			num2 += item2.Value;
			if (num <= num2)
			{
				malusLevel = item2.Key;
				break;
			}
		}
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary2 = ItemDatabase.AffixMalusDefinitions.Where((KeyValuePair<UnitStatDefinition.E_Stat, AffixMalusDefinition> o) => o.Value.IsMalusLevelDefined(malusLevel)).ToDictionary((Func<KeyValuePair<UnitStatDefinition.E_Stat, AffixMalusDefinition>, UnitStatDefinition.E_Stat>)((KeyValuePair<UnitStatDefinition.E_Stat, AffixMalusDefinition> k) => k.Value.Stat), (Func<KeyValuePair<UnitStatDefinition.E_Stat, AffixMalusDefinition>, float>)((KeyValuePair<UnitStatDefinition.E_Stat, AffixMalusDefinition> v) => v.Value.Weight));
		num = RandomManager.GetRandomRange(max: dictionary2.Values.Sum(), caller: TPSingleton<ItemManager>.Instance, min: 0f);
		num2 = 0f;
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item3 in dictionary2)
		{
			num2 += item3.Value;
			if (num <= num2)
			{
				key = item3.Key;
				break;
			}
		}
		AffixMalus affixMalus = new AffixMalusController(ItemDatabase.AffixMalusDefinitions[key]).AffixMalus;
		affixMalus.AffixMalusController.SetLevel(malusLevel);
		item.AdditionalAffixesMalus.Add(affixMalus);
	}

	private static Dictionary<AffixDefinition, float> ComputePotentialAffixDefinitions(Dictionary<AffixDefinition, float> availableAffixDefinitions, int rarity)
	{
		Dictionary<AffixDefinition, float> dictionary = new Dictionary<AffixDefinition, float>();
		foreach (KeyValuePair<AffixDefinition, float> availableAffixDefinition in availableAffixDefinitions)
		{
			if (availableAffixDefinition.Key.LevelDefinitions.ContainsKey(rarity))
			{
				dictionary.Add(availableAffixDefinition.Key, availableAffixDefinition.Value);
			}
		}
		return dictionary;
	}

	private static Dictionary<AffixDefinition, float> ComputeAvailableAffixDefinitions(ItemGenerationInfo generationInfo, TheLastStand.Model.Item.Item item)
	{
		Dictionary<AffixDefinition, float> dictionary = new Dictionary<AffixDefinition, float>();
		string[] lockedAffixesIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedAffixesIds();
		foreach (KeyValuePair<string, AffixDefinition> affixDefinition in ItemDatabase.AffixDefinitions)
		{
			AffixDefinition value = affixDefinition.Value;
			ItemDefinition.E_Category e_Category = ItemDefinition.E_Category.None;
			foreach (KeyValuePair<ItemDefinition.E_Category, float> item2 in value.ItemCategoriesWithWeight)
			{
				if ((item2.Key & item.ItemDefinition.Category) != 0)
				{
					e_Category = item2.Key;
					break;
				}
			}
			if (value.Droppable && generationInfo.Level >= value.LevelMin && generationInfo.Level <= value.LevelMax && e_Category != 0 && !lockedAffixesIds.Contains(value.Id))
			{
				dictionary.Add(affixDefinition.Value, value.ItemCategoriesWithWeight[e_Category]);
			}
		}
		return dictionary;
	}

	public static TheLastStand.Model.Item.Item GenerateItem(ItemSlotDefinition.E_ItemSlotId itemDestination, CreateItemDefinition createItemDefinition, int level)
	{
		ItemDefinition itemDefinition = TakeRandomItemInList(createItemDefinition.ItemsListDefinition);
		int higherExistingLevelFromInitValue = itemDefinition.GetHigherExistingLevelFromInitValue(level);
		int num = 1000;
		while (higherExistingLevelFromInitValue == -1 && --num > 0)
		{
			itemDefinition = TakeRandomItemInList(createItemDefinition.ItemsListDefinition);
			higherExistingLevelFromInitValue = itemDefinition.GetHigherExistingLevelFromInitValue(level);
		}
		ItemGenerationInfo generationInfo = default(ItemGenerationInfo);
		generationInfo.Destination = itemDestination;
		generationInfo.ItemDefinition = itemDefinition;
		generationInfo.Level = higherExistingLevelFromInitValue;
		generationInfo.Rarity = RarityProbabilitiesTreeController.GenerateRarity(createItemDefinition.ItemRaritiesListDefinition);
		return GenerateItem(generationInfo);
	}

	public static void GenerateItems(ItemSlotDefinition.E_ItemSlotId itemDestination, CreateItemDefinition createItemDefinition, int level)
	{
		Node count = createItemDefinition.Count;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<CreateItemModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int i = 0; i < effects.Length; i++)
			{
				if (createItemDefinition.HasID && effects[i].CreateItemId == createItemDefinition.Id)
				{
					count = effects[i].Count;
					break;
				}
			}
		}
		int num = count.EvalToInt(new ItemInterpreterContext());
		if (num == CreateItemDefinition.All)
		{
			GenerateAllItemsInList(itemDestination, createItemDefinition.ItemsListDefinition, level, createItemDefinition.ItemRaritiesListDefinition);
			return;
		}
		for (int j = 0; j < num; j++)
		{
			GenerateItem(itemDestination, createItemDefinition, level);
		}
	}

	public static void GenerateAllItemsInList(ItemSlotDefinition.E_ItemSlotId itemDestination, ItemsListDefinition itemsListDefinition, int level, ProbabilityTreeEntriesDefinition rarityProbability)
	{
		foreach (KeyValuePair<string, int> item in itemsListDefinition.ItemsWithOdd)
		{
			ItemsListDefinition value2;
			if (ItemDatabase.ItemDefinitions.TryGetValue(item.Key, out var value))
			{
				ItemGenerationInfo generationInfo = default(ItemGenerationInfo);
				generationInfo.Destination = itemDestination;
				generationInfo.ItemDefinition = value;
				generationInfo.Level = level;
				generationInfo.Rarity = RarityProbabilitiesTreeController.GenerateRarity(rarityProbability);
				GenerateItem(generationInfo);
			}
			else if (ItemDatabase.ItemsListDefinitions.TryGetValue(item.Key, out value2))
			{
				GenerateAllItemsInList(itemDestination, value2, level, rarityProbability);
			}
			else
			{
				((CLogger<ItemManager>)TPSingleton<ItemManager>.Instance).LogError((object)("Trying to generate all items in list " + itemsListDefinition.Id + " -> Id " + item.Key + " does not refer to an item nor an items list."), (CLogLevel)1, true, true);
			}
		}
	}

	public static float GetItemOddFromItemList(ItemsListDefinition itemsListDefinition, string itemId)
	{
		float num = 1f;
		if (TPSingleton<GlyphManager>.Instance.ItemWeightMultipliers.TryGetValue(itemsListDefinition.Id, out var value) && value.TryGetValue(itemId, out var value2))
		{
			num = value2;
		}
		return (float)itemsListDefinition.ItemsWithOdd[itemId] * num;
	}

	public static bool IsItemsListContentLocked(ItemsListDefinition itemsListDefinition, string[] unavailableIds)
	{
		foreach (KeyValuePair<string, int> item in itemsListDefinition.ItemsWithOdd)
		{
			if (ItemDatabase.ItemDefinitions.TryGetValue(item.Key, out var _) && !unavailableIds.Contains(item.Key))
			{
				return false;
			}
			if (ItemDatabase.ItemsListDefinitions.TryGetValue(item.Key, out var value2) && !IsItemsListContentLocked(value2, unavailableIds))
			{
				return false;
			}
		}
		return true;
	}

	public static bool AnyItemMatchingCondition(ItemsListDefinition itemsListDefinition, Func<ItemDefinition, bool> predicate, List<string> exploredIds = null)
	{
		foreach (KeyValuePair<string, int> item in itemsListDefinition.ItemsWithOdd)
		{
			ItemsListDefinition value2;
			if (ItemDatabase.ItemDefinitions.TryGetValue(item.Key, out var value))
			{
				if (predicate(value))
				{
					return true;
				}
			}
			else if (ItemDatabase.ItemsListDefinitions.TryGetValue(item.Key, out value2))
			{
				if (exploredIds == null)
				{
					exploredIds = new List<string> { value2.Id };
				}
				else
				{
					exploredIds.Add(value2.Id);
				}
				if (AnyItemMatchingCondition(value2, predicate, exploredIds))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static ItemDefinition TakeRandomItemInList(ItemsListDefinition itemsListDefinition, Func<ItemDefinition, bool> predicate = null, List<string> exploredIds = null)
	{
		string[] lockedItemsIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedItemsIds();
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		foreach (string item in itemsListDefinition.ItemsWithOdd.Select((KeyValuePair<string, int> o) => o.Key))
		{
			bool num = lockedItemsIds.Contains(item);
			ItemDefinition value;
			bool flag = ItemDatabase.ItemDefinitions.TryGetValue(item, out value);
			ItemsListDefinition value2;
			bool flag2 = ItemDatabase.ItemsListDefinitions.TryGetValue(item, out value2);
			if (!num && (!flag || predicate == null || predicate(value)) && (!flag2 || (!IsItemsListContentLocked(value2, lockedItemsIds) && (predicate == null || AnyItemMatchingCondition(value2, predicate)))))
			{
				dictionary.Add(item, GetItemOddFromItemList(itemsListDefinition, item));
			}
		}
		string randomItemFromWeights = DictionaryHelpers.GetRandomItemFromWeights(dictionary, TPSingleton<ItemManager>.Instance);
		if (randomItemFromWeights == null)
		{
			return null;
		}
		if (ItemDatabase.ItemDefinitions.TryGetValue(randomItemFromWeights, out var value3))
		{
			return value3;
		}
		if (exploredIds == null)
		{
			exploredIds = new List<string> { randomItemFromWeights };
		}
		else
		{
			exploredIds.Add(randomItemFromWeights);
		}
		return TakeRandomItemInList(ItemDatabase.ItemsListDefinitions[randomItemFromWeights], predicate, exploredIds);
	}

	public static HashSet<string> GetAllItemsInList(ItemsListDefinition itemsListDefinition)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<string, int> item in itemsListDefinition.ItemsWithOdd)
		{
			string key = item.Key;
			if (ItemDatabase.ItemDefinitions.ContainsKey(key))
			{
				hashSet.Add(key);
			}
			else
			{
				hashSet.UnionWith(GetAllItemsInList(ItemDatabase.ItemsListDefinitions[key]));
			}
		}
		return hashSet;
	}

	public Dictionary<UnitStatDefinition.E_Stat, float> GetStatsDiffBetweenItems(TheLastStand.Model.Item.Item baseItem, params TheLastStand.Model.Item.Item[] otherItems)
	{
		Dictionary<UnitStatDefinition.E_Stat, float> allStatBonusesMerged = baseItem.GetAllStatBonusesMerged();
		foreach (TheLastStand.Model.Item.Item item in otherItems)
		{
			if (item == null)
			{
				continue;
			}
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item2 in item.GetAllStatBonusesMerged())
			{
				allStatBonusesMerged.AddValueOrCreateKey(item2.Key, 0f - item2.Value, (float a, float b) => a + b);
			}
		}
		return allStatBonusesMerged;
	}

	public void Init()
	{
		if (TPSingleton<InventoryManager>.Instance.Inventory.ItemCount != 0)
		{
			return;
		}
		foreach (CreateItemDefinition startStockItemDefinition in ItemDatabase.StartStockItemDefinitions)
		{
			GenerateItems(ItemSlotDefinition.E_ItemSlotId.Inventory, startStockItemDefinition, 0);
		}
	}

	[DevConsoleCommand(Name = "GenerateItem")]
	public static void DebugGenerateItem([StringConverter(typeof(TheLastStand.Model.Item.Item.StringToItemIdConverter))] string itemId, int level = 0, ItemDefinition.E_Rarity rarity = ItemDefinition.E_Rarity.None, int amountToGenerate = 1, bool skipMalusAffixes = false)
	{
		amountToGenerate = Mathf.Max(amountToGenerate, 1);
		if (!ItemDatabase.ItemDefinitions.TryGetValue(itemId, out var value))
		{
			TPDebug.LogError((object)("No item found with the Id " + itemId + "!"), (Object)null);
			return;
		}
		bool flag = rarity == ItemDefinition.E_Rarity.None;
		for (int i = 0; i < amountToGenerate; i++)
		{
			if (flag)
			{
				rarity = (ItemDefinition.E_Rarity)RandomManager.GetRandomRange(TPSingleton<ItemManager>.Instance, 1, 5);
			}
			ItemGenerationInfo generationInfo = default(ItemGenerationInfo);
			generationInfo.Destination = ItemSlotDefinition.E_ItemSlotId.Inventory;
			generationInfo.ItemDefinition = value;
			generationInfo.Level = level;
			generationInfo.Rarity = rarity;
			generationInfo.SkipMalusAffixes = skipMalusAffixes;
			GenerateItem(generationInfo);
		}
	}

	[DevConsoleCommand(Name = "GenerateAllPotions")]
	public static void DebugGenerateAllPotions()
	{
		string[] array = new string[7] { "HealthPotion", "ManaPotion", "EnergyPotion", "SpeedPotion", "InvisibilityPotion", "StonePotion", "StrengthPotion" };
		for (int i = 0; i < array.Length; i++)
		{
			if (ItemDatabase.ItemDefinitions.TryGetValue(array[i], out var value))
			{
				for (int j = 0; j <= 5; j++)
				{
					ItemGenerationInfo generationInfo = default(ItemGenerationInfo);
					generationInfo.Destination = ItemSlotDefinition.E_ItemSlotId.Inventory;
					generationInfo.ItemDefinition = value;
					generationInfo.Level = j;
					generationInfo.Rarity = ItemDefinition.E_Rarity.Common;
					GenerateItem(generationInfo);
				}
			}
		}
	}

	[DevConsoleCommand(Name = "GenerateItemByCategory")]
	public static void DebugGenerateItemByCategory(ItemDefinition.E_Category category, int level, ItemDefinition.E_Rarity rarity, int amountToGenerate = 1, bool skipMalusAffixes = false)
	{
		amountToGenerate = Mathf.Max(amountToGenerate, 1);
		List<ItemDefinition> list = new List<ItemDefinition>();
		foreach (KeyValuePair<string, ItemDefinition> itemDefinition2 in ItemDatabase.ItemDefinitions)
		{
			if (category == ItemDefinition.E_Category.None || (itemDefinition2.Value.Category & category) != 0)
			{
				list.Add(itemDefinition2.Value);
			}
		}
		if (list.Count == 0)
		{
			TPDebug.LogError((object)$"No item found with the category {category}!", (Object)null);
			return;
		}
		bool flag = rarity == ItemDefinition.E_Rarity.None;
		for (int i = 0; i < amountToGenerate; i++)
		{
			if (flag)
			{
				rarity = (ItemDefinition.E_Rarity)RandomManager.GetRandomRange(TPSingleton<ItemManager>.Instance, 1, 5);
			}
			list = RandomManager.Shuffle(TPSingleton<ItemManager>.Instance, list).ToList();
			ItemDefinition itemDefinition = list[0];
			level = itemDefinition.GetLowerExistingLevelFromInitValue(level);
			ItemGenerationInfo generationInfo = default(ItemGenerationInfo);
			generationInfo.Destination = ItemSlotDefinition.E_ItemSlotId.Inventory;
			generationInfo.ItemDefinition = itemDefinition;
			generationInfo.Level = list[0].GetLowerExistingLevelFromInitValue(level);
			generationInfo.Rarity = rarity;
			generationInfo.SkipMalusAffixes = skipMalusAffixes;
			GenerateItem(generationInfo);
		}
	}

	[DevConsoleCommand(Name = "ShowAllItemsInList")]
	public static void DebugShowAllItemsInList([StringConverter(typeof(TheLastStand.Model.Item.Item.StringToItemsListIdConverter))] string listId)
	{
		((CLogger<ItemManager>)TPSingleton<ItemManager>.Instance).Log((object)string.Join(", ", GetAllItemsInList(ItemDatabase.ItemsListDefinitions[listId])), (CLogLevel)1, true, false);
	}

	[DevConsoleCommand("AnyUnlockedOneArmedItemInList")]
	public static void AnyUnlockedOneArmedItemInList([StringConverter(typeof(TheLastStand.Model.Item.Item.StringToItemsListIdConverter))] string itemsListDefinitionId)
	{
		List<string> unlockedItemIds = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockEquipmentGenerationMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int i = 0; i < effects.Length; i++)
			{
				unlockedItemIds.Add(effects[i].Id);
			}
		}
		string[] lockedItemsIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedItemsIds();
		if (AnyItemMatchingCondition(ItemDatabase.ItemsListDefinitions[itemsListDefinitionId], (ItemDefinition item) => item.Hands == ItemDefinition.E_Hands.OneHand && (!lockedItemsIds.Contains(item.Id) || unlockedItemIds.Contains(item.Id))))
		{
			Debug.LogError((object)("At least one available OneHand item has been found in list " + itemsListDefinitionId));
		}
		else
		{
			Debug.LogError((object)("No available OneHand item has been found in list " + itemsListDefinitionId));
		}
	}

	[DevConsoleCommand(Name = "GenerateItemsInList")]
	public static void DebugGenerateItemsInList([StringConverter(typeof(TheLastStand.Model.Item.Item.StringToItemsListIdConverter))] string itemsListDefinitionId, int level = 0, [StringConverter(typeof(TheLastStand.Model.Item.Item.StringToRarityProbabilityListIdConverter))] string rarityProbabilityList = "AlwaysCommon", int amountToGenerate = 1, bool skipMalusAffixes = false)
	{
		if (ItemDatabase.ItemsListDefinitions.TryGetValue(itemsListDefinitionId, out var value))
		{
			ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition = ItemDatabase.ItemRaritiesListDefinitions[rarityProbabilityList];
			for (int i = 0; i < amountToGenerate; i++)
			{
				ItemGenerationInfo generationInfo = default(ItemGenerationInfo);
				generationInfo.ItemDefinition = TakeRandomItemInList(value);
				generationInfo.Destination = ItemSlotDefinition.E_ItemSlotId.Inventory;
				generationInfo.Level = level;
				generationInfo.Rarity = RarityProbabilitiesTreeController.GenerateRarity(probabilityTreeEntriesDefinition);
				generationInfo.SkipMalusAffixes = skipMalusAffixes;
				GenerateItem(generationInfo);
			}
		}
	}

	[DevConsoleCommand(Name = "GenerateAllItemsInList")]
	public static void DebugGenerateAllItemsInList([StringConverter(typeof(TheLastStand.Model.Item.Item.StringToItemsListIdConverter))] string itemsListDefinitionId, int level = 0, [StringConverter(typeof(TheLastStand.Model.Item.Item.StringToRarityProbabilityListIdConverter))] string rarityProbabilityList = "AlwaysCommon")
	{
		if (ItemDatabase.ItemsListDefinitions.TryGetValue(itemsListDefinitionId, out var value))
		{
			GenerateAllItemsInList(ItemSlotDefinition.E_ItemSlotId.Inventory, value, level, ItemDatabase.ItemRaritiesListDefinitions[rarityProbabilityList]);
		}
	}
}
