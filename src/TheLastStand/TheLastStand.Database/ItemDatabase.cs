using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Definition.DLC;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager.Item;
using UnityEngine;

namespace TheLastStand.Database;

public class ItemDatabase : Database<ItemDatabase>
{
	[SerializeField]
	private TextAsset affixDefinitions;

	[SerializeField]
	private TextAsset affixLevelsDefinition;

	[SerializeField]
	private TextAsset affixMalusDefinitions;

	[SerializeField]
	private TextAsset[] itemListsDefinitions;

	[SerializeField]
	private TextAsset itemGenerationModifierListDefinitions;

	[SerializeField]
	private TextAsset itemRaritiesListDefinitions;

	[SerializeField]
	private TextAsset[] groupItemDefinitions;

	[SerializeField]
	private DLCTextAssetDefinition[] dlcGroupItemDefinitions;

	[SerializeField]
	private TextAsset[] individualItemDefinitions;

	[SerializeField]
	private TextAsset[] groupItemsArtDefinitions;

	[SerializeField]
	private DLCTextAssetDefinition[] dlcGroupItemsArtDefinitions;

	[SerializeField]
	private TextAsset[] individualArtItemDefinitions;

	[SerializeField]
	private TextAsset itemRestrictionCategoriesCollectionDefinitions;

	[SerializeField]
	private TextAsset itemSlotDefinitions;

	[SerializeField]
	private TextAsset itemConfig;

	[SerializeField]
	private TextAsset startStockItemDefinitions;

	public static Dictionary<string, AffixDefinition> AffixDefinitions { get; private set; }

	public static Dictionary<ItemDefinition.E_Rarity, int> AffixesCountPerRarity { get; private set; }

	public static Dictionary<UnitStatDefinition.E_Stat, AffixMalusDefinition> AffixMalusDefinitions { get; private set; }

	public static AffixLevelsDefinition AffixLevelsDefinition { get; private set; }

	public static Dictionary<string, ItemDefinition> AllItemsDefinitions { get; private set; }

	public static Dictionary<string, ItemDefinition> ItemDefinitions { get; private set; }

	public static Dictionary<string, ProbabilityTreeEntriesDefinition> ItemGenerationModifierListDefinitions { get; private set; }

	public static Dictionary<string, ItemsListDefinition> ItemsListDefinitions { get; private set; }

	public static Dictionary<string, ProbabilityTreeEntriesDefinition> ItemRaritiesListDefinitions { get; private set; }

	public static Dictionary<string, ItemRestrictionCategoriesCollectionDefinition> ItemRestrictionCategoriesCollectionDefinitions { get; private set; }

	public static Dictionary<string, ItemRestrictionFamilyDefinition> ItemRestrictionFamiliesDefinitions { get; private set; }

	public static Dictionary<ItemSlotDefinition.E_ItemSlotId, ItemSlotDefinition> ItemSlotDefinitions { get; private set; }

	public static Dictionary<string, List<string>> ItemsByTag { get; } = new Dictionary<string, List<string>>();


	public static Node ItemPriceEquation { get; private set; }

	public static float ItemPriceEquationConstant1 { get; private set; }

	public static float ItemPriceEquationConstant2 { get; private set; }

	public static float ItemPriceEquationPowerConstant { get; private set; }

	public static List<CreateItemDefinition> StartStockItemDefinitions { get; set; }

	public override void Deserialize(XContainer container = null)
	{
		DeserializeAffixes();
		DeserializeAffixesMalus();
		DeserializeItems();
		DeserializeItemLists();
		DeserializeItemConfig();
		DeserializeItemRaritiesList();
		DeserializeModifierLists();
		DeserializeStartStockItemDefinitions();
		CleanEmptyListsAndMissingItems();
		DeserializeItemRestrictions();
	}

	private void DeserializeAffixes()
	{
		AffixDefinitions = new Dictionary<string, AffixDefinition>();
		XElement val = ((XContainer)XDocument.Parse(affixDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("AffixDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"No AffixDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("AffixDefinition")))
		{
			AffixDefinition affixDefinition = new AffixDefinition((XContainer)(object)item);
			AffixDefinitions.Add(affixDefinition.Id, affixDefinition);
		}
		XElement val2 = ((XContainer)XDocument.Parse(affixLevelsDefinition.text, (LoadOptions)2)).Element(XName.op_Implicit("AffixLevelsDefinition"));
		if (val2 == null)
		{
			CLoggerManager.Log((object)("No AffixLevelsDefinition in TextAsset " + ((Object)affixLevelsDefinition).name + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			AffixLevelsDefinition = new AffixLevelsDefinition((XContainer)(object)val2);
		}
	}

	private void DeserializeAffixesMalus()
	{
		AffixMalusDefinitions = new Dictionary<UnitStatDefinition.E_Stat, AffixMalusDefinition>(UnitStatDefinition.SharedStatComparer);
		XElement val = ((XContainer)XDocument.Parse(affixMalusDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("AffixMalusDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)("No AffixMalusDefinition in TextAsset " + ((Object)affixMalusDefinitions).name + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("AffixMalusDefinition")))
		{
			AffixMalusDefinition affixMalusDefinition = new AffixMalusDefinition((XContainer)(object)item);
			AffixMalusDefinitions.Add(affixMalusDefinition.Stat, affixMalusDefinition);
		}
	}

	private void DeserializeItemLists()
	{
		if (ItemsListDefinitions != null)
		{
			return;
		}
		ItemsListDefinitions = new Dictionary<string, ItemsListDefinition>();
		Queue<XElement> queue = GatherElements(itemListsDefinitions, null, "ItemsListDefinition");
		while (queue.Count > 0)
		{
			ItemsListDefinition itemsListDefinition = new ItemsListDefinition((XContainer)(object)queue.Dequeue());
			try
			{
				ItemsListDefinitions.Add(itemsListDefinition.Id, itemsListDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log((object)("Duplicate ItemsListDefinition found for ID " + itemsListDefinition.Id + ": the individual files will have PRIORITY over the all-in-one template file."), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}

	private void DeserializeItemRaritiesList()
	{
		if (ItemRaritiesListDefinitions != null)
		{
			return;
		}
		ItemRaritiesListDefinitions = new Dictionary<string, ProbabilityTreeEntriesDefinition>();
		XElement val = ((XContainer)XDocument.Parse(itemRaritiesListDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("ItemRaritiesListDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"The document must have ItemRaritiesListDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("ItemRaritiesListDefinition")))
		{
			ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition = new ProbabilityTreeEntriesDefinition((XContainer)(object)item);
			ItemRaritiesListDefinitions.Add(probabilityTreeEntriesDefinition.Id, probabilityTreeEntriesDefinition);
		}
	}

	private void DeserializeItemRestrictions()
	{
		if (ItemRestrictionCategoriesCollectionDefinitions != null)
		{
			return;
		}
		ItemRestrictionCategoriesCollectionDefinitions = new Dictionary<string, ItemRestrictionCategoriesCollectionDefinition>();
		ItemRestrictionFamiliesDefinitions = new Dictionary<string, ItemRestrictionFamilyDefinition>();
		XElement val = ((XContainer)XDocument.Parse(itemRestrictionCategoriesCollectionDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("ItemRestrictionCategoriesCollectionDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"The document must have ItemRestrictionCategoriesCollectionDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("ItemRestrictionCategoriesCollectionDefinition")))
		{
			ItemRestrictionCategoriesCollectionDefinition itemRestrictionCategoriesCollectionDefinition = new ItemRestrictionCategoriesCollectionDefinition((XContainer)(object)item);
			ItemRestrictionCategoriesCollectionDefinitions.Add(itemRestrictionCategoriesCollectionDefinition.Id, itemRestrictionCategoriesCollectionDefinition);
		}
	}

	private void DeserializeItems()
	{
		if (ItemDefinitions != null)
		{
			return;
		}
		AllItemsDefinitions = new Dictionary<string, ItemDefinition>();
		ItemDefinitions = new Dictionary<string, ItemDefinition>();
		List<TextAsset> list = new List<TextAsset>();
		list.AddRange(groupItemDefinitions);
		int count = list.Count;
		list.AddRange(GenericDatabase.GetDLCTextAssets(dlcGroupItemDefinitions));
		int num = list.Count - count;
		List<TextAsset> list2 = new List<TextAsset>();
		list2.AddRange(groupItemsArtDefinitions);
		list2.AddRange(GenericDatabase.GetDLCTextAssets(dlcGroupItemsArtDefinitions));
		Queue<XElement> queue = GatherElements(list, individualItemDefinitions, "ItemDefinition");
		Queue<XElement> queue2 = GatherElements(list2, individualArtItemDefinitions, "ItemArtDefinition");
		while (queue.Count > 0)
		{
			ItemDefinition itemDefinition = new ItemDefinition((XContainer)(object)queue.Dequeue());
			try
			{
				ItemDefinitions.Add(itemDefinition.Id, itemDefinition);
				AllItemsDefinitions.Add(itemDefinition.Id, itemDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log((object)("Duplicate ItemDefinition found for ID " + itemDefinition.Id + ": the individual files will have PRIORITY over the all-in-one template file."), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		list.Clear();
		list.AddRange(GenericDatabase.GetDLCTextAssets(dlcGroupItemDefinitions, forceGetAllTextAssetDefinitions: true));
		Queue<XElement> queue3 = GatherElements(list, individualItemDefinitions, "ItemDefinition");
		if (num != list.Count)
		{
			while (queue3.Count > 0)
			{
				ItemDefinition itemDefinition2 = new ItemDefinition((XContainer)(object)queue3.Dequeue());
				if (!AllItemsDefinitions.ContainsKey(itemDefinition2.Id))
				{
					AllItemsDefinitions.Add(itemDefinition2.Id, itemDefinition2);
				}
			}
		}
		while (queue2.Count > 0)
		{
			XElement val = queue2.Dequeue();
			XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
			try
			{
				ItemDefinitions[val2.Value].DeserializeArtRelatedDatas((XContainer)(object)val);
			}
			catch (KeyNotFoundException)
			{
				CLoggerManager.Log((object)$"ID {val2} found in an item art definition but could not find the actual item definition to link.", (LogType)2, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		ItemSlotDefinitions = new Dictionary<ItemSlotDefinition.E_ItemSlotId, ItemSlotDefinition>();
		XElement val3 = ((XContainer)XDocument.Parse(itemSlotDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("ItemSlotDefinitions"));
		if (val3 == null)
		{
			CLoggerManager.Log((object)"No ItemSlotDefinitions!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item in ((XContainer)val3).Elements(XName.op_Implicit("ItemSlotDefinition")))
		{
			ItemSlotDefinition itemSlotDefinition = new ItemSlotDefinition((XContainer)(object)item);
			ItemSlotDefinitions.Add(itemSlotDefinition.Id, itemSlotDefinition);
		}
	}

	private void DeserializeItemConfig()
	{
		if (AffixesCountPerRarity != null)
		{
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(itemConfig.text, (LoadOptions)2)).Element(XName.op_Implicit("ItemConfig"));
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("AffixesCountPerRarity"));
		AffixesCountPerRarity = new Dictionary<ItemDefinition.E_Rarity, int>(ItemDefinition.SharedRarityComparer);
		foreach (XElement item in ((XContainer)obj).Elements(XName.op_Implicit("Rarity")))
		{
			int result2;
			if (!Enum.TryParse<ItemDefinition.E_Rarity>(item.Attribute(XName.op_Implicit("Id")).Value, out var result) || result < ItemDefinition.E_Rarity.Common || result > ItemDefinition.E_Rarity.Epic)
			{
				CLoggerManager.Log((object)"The rarity has an invalid Id!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else if (!int.TryParse(item.Value, out result2))
			{
				CLoggerManager.Log((object)$"The rarity {result} has an invalid AffixesCount!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				AffixesCountPerRarity.Add(result, result2);
			}
		}
		ItemPriceEquation = Parser.Parse(((XContainer)val).Element(XName.op_Implicit("ItemPriceEquation")).Value);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("ItemPriceEquationConstant1"));
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("ItemPriceEquationConstant2"));
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("ItemPriceEquationPowerConstant"));
		if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
		{
			((CLogger<ItemManager>)TPSingleton<ItemManager>.Instance).LogError((object)("Could not cast the item price equation constant 1 into a float. Current value : " + val2.Value), (CLogLevel)1, true, true);
			return;
		}
		if (!float.TryParse(val3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
		{
			((CLogger<ItemManager>)TPSingleton<ItemManager>.Instance).LogError((object)("Could not cast the item price equation constant 2 into a float. Current value : " + val3.Value), (CLogLevel)1, true, true);
			return;
		}
		if (!float.TryParse(val4.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result5))
		{
			((CLogger<ItemManager>)TPSingleton<ItemManager>.Instance).LogError((object)("Could not cast the item price equation power constant into a float. Current value : " + val4.Value), (CLogLevel)1, true, true);
			return;
		}
		ItemPriceEquationConstant1 = result3;
		ItemPriceEquationConstant2 = result4;
		ItemPriceEquationPowerConstant = result5;
	}

	private void DeserializeModifierLists()
	{
		if (ItemGenerationModifierListDefinitions != null)
		{
			return;
		}
		ItemGenerationModifierListDefinitions = new Dictionary<string, ProbabilityTreeEntriesDefinition>();
		XElement val = ((XContainer)XDocument.Parse(itemGenerationModifierListDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("ItemGenerationModifiersListDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"The document must have xItemGenerationModifierListDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("ItemGenerationModifiersListDefinition")))
		{
			ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition = new ProbabilityTreeEntriesDefinition((XContainer)(object)item);
			ItemGenerationModifierListDefinitions.Add(probabilityTreeEntriesDefinition.Id, probabilityTreeEntriesDefinition);
		}
	}

	private void DeserializeStartStockItemDefinitions()
	{
		if (StartStockItemDefinitions != null)
		{
			return;
		}
		XElement obj = ((XContainer)XDocument.Parse(startStockItemDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("StartInventoryItemDefinitions"));
		StartStockItemDefinitions = new List<CreateItemDefinition>();
		foreach (XElement item2 in ((XContainer)obj).Elements(XName.op_Implicit("CreateItem")))
		{
			CreateItemDefinition item = new CreateItemDefinition((XContainer)(object)item2);
			StartStockItemDefinitions.Add(item);
		}
	}

	protected void CleanEmptyListsAndMissingItems()
	{
		RemoveEmptyLists();
		RemovedMissingDefinitionsFromLists();
	}

	private void RemoveEmptyLists()
	{
		List<string> list = new List<string>();
		foreach (ItemsListDefinition value in ItemsListDefinitions.Values)
		{
			if (value.IsEmpty)
			{
				list.Add(value.Id);
			}
		}
		foreach (string item in list)
		{
			if (ItemsListDefinitions.ContainsKey(item))
			{
				ItemsListDefinitions.Remove(item);
			}
		}
	}

	private void RemovedMissingDefinitionsFromLists()
	{
		foreach (ItemsListDefinition value3 in ItemsListDefinitions.Values)
		{
			if (value3.IsEmpty)
			{
				continue;
			}
			List<string> list = new List<string>();
			foreach (string key in value3.ItemsWithOdd.Keys)
			{
				if (!ItemDefinitions.TryGetValue(key, out var _) && !ItemsListDefinitions.TryGetValue(key, out var _))
				{
					list.Add(key);
				}
			}
			foreach (string item in list)
			{
				value3.ItemsWithOdd.Remove(item);
			}
		}
	}
}
