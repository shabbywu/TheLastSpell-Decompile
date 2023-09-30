using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Brazier;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheLastStand.Database.Building;

public class BuildingDatabase : Database<BuildingDatabase>
{
	[SerializeField]
	private TextAsset buildingGaugeEffectDefinitions;

	[SerializeField]
	private TextAsset buildingPassiveDefinitions;

	[SerializeField]
	private TextAsset buildingActionDefinitions;

	[SerializeField]
	private TextAsset buildingUpgradeDefinitions;

	[SerializeField]
	private TextAsset buildingSkillSoundDefinitions;

	[SerializeField]
	private IEnumerable<TextAsset> individualBuildingDefinitions;

	[SerializeField]
	private IEnumerable<TextAsset> groupBuildingDefinitions;

	[SerializeField]
	private TextAsset shopDefinition;

	[SerializeField]
	private TextAsset brazierDefinition;

	[SerializeField]
	private TextAsset randomBuildingsGenerationsDefinitions;

	[SerializeField]
	private TextAsset randomBuildingsDirectionsDefinitions;

	[SerializeField]
	private TextAsset randomBuildingsPerDayDefinitions;

	[SerializeField]
	private DataColor validColor;

	[SerializeField]
	private DataColor invalidColor;

	[SerializeField]
	private TileBySpriteDictionary tileBySpriteDictionary;

	public static ShopDefinition ShopDefinition;

	public static BraziersDefinition BraziersDefinition;

	public static Dictionary<string, BuildingActionDefinition> BuildingActionDefinitions { get; private set; }

	public static Dictionary<string, BuildingDefinition> BuildingDefinitions { get; private set; }

	public static Dictionary<string, BuildingLimitGroupDefinition> BuildingLimitGroupDefinitions { get; private set; }

	public static Dictionary<string, BuildingGaugeEffectDefinition> BuildingGaugeEffectDefinitions { get; private set; }

	public static Dictionary<string, BuildingPassiveDefinition> BuildingPassiveDefinitions { get; private set; }

	public static Dictionary<string, BuildingUpgradeDefinition> BuildingUpgradeDefinitions { get; private set; }

	public static Dictionary<string, BuildingSkillSoundDefinition> BuildingSkillSoundDefinitions { get; private set; }

	public static Dictionary<string, RandomBuildingsGenerationDefinition> RandomBuildingsGenerationDefinitions { get; private set; }

	public static Dictionary<string, RandomBuildingsDirectionsDefinition> RandomBuildingsDirectionsDefinitions { get; private set; }

	public static Dictionary<string, RandomBuildingsPerDayDefinition> RandomBuildingsPerDayDefinitions { get; private set; }

	public static Color InvalidColor => TPSingleton<BuildingDatabase>.Instance.invalidColor._Color;

	public static Dictionary<ItemDefinition.E_Category, HashSet<string>> ShopItemsByCategory { get; private set; }

	public static TileBySpriteDictionary TileBySpriteDictionary => TPSingleton<BuildingDatabase>.Instance.tileBySpriteDictionary;

	public static Color ValidColor => TPSingleton<BuildingDatabase>.Instance.validColor._Color;

	public override void Deserialize(XContainer container = null)
	{
		DeserializeShop();
		DeserializeGaugeEffects();
		DeserializePassive();
		DeserializeActions();
		DeserializeUpgrades();
		DeserializeBuildingDefinitions();
		DeserializeSkillSoundDefinitions();
		ComputeShopItemsByCategory();
		DeserializeBraziers();
		DeserializeRandomBuildingsGenerations();
		DeserializeRandomBuildingsDirections();
		DeserializeRandomBuildingsPerDay();
	}

	private void DeserializeBraziers()
	{
		if (BraziersDefinition == null)
		{
			XElement val = ((XContainer)XDocument.Parse(brazierDefinition.text, (LoadOptions)2)).Element(XName.op_Implicit("BraziersDefinition"));
			if (val == null)
			{
				CLoggerManager.Log((object)"BraziersDefinition document must have a BraziersDefinition Element", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				BraziersDefinition = new BraziersDefinition((XContainer)(object)val);
			}
		}
	}

	public TileBase LoadTileFromResourcesOnce(string path)
	{
		return ResourcePooler<TileBase>.LoadOnce(path, false);
	}

	private void ComputeShopItemsByCategory()
	{
		if (!BuildingDefinitions.TryGetValue("Shop", out var value))
		{
			CLoggerManager.Log((object)"Shop definition is missing !", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			return;
		}
		GenerateNewItemsRosterDefinition generateNewItemsRosterDefinition = null;
		foreach (BuildingPassiveDefinition buildingPassiveDefinition in value.PassivesModuleDefinition.BuildingPassiveDefinitions)
		{
			generateNewItemsRosterDefinition = buildingPassiveDefinition.PassiveEffectDefinitions.FirstOrDefault((BuildingPassiveEffectDefinition e) => e is GenerateNewItemsRosterDefinition) as GenerateNewItemsRosterDefinition;
			if (generateNewItemsRosterDefinition != null)
			{
				break;
			}
		}
		ShopItemsByCategory = new Dictionary<ItemDefinition.E_Category, HashSet<string>>();
		foreach (CreateRosterItemDefinition createItemRosterDefinition in generateNewItemsRosterDefinition.CreateItemRosterDefinitions)
		{
			foreach (KeyValuePair<string, int> item in createItemRosterDefinition.CreateItemDefinition.ItemsListDefinition.ItemsWithOdd)
			{
				if (item.Value != 0)
				{
					AddItemToShopItemsByCategory(item.Key);
				}
			}
		}
	}

	private void AddItemToShopItemsByCategory(string itemId)
	{
		if (!ItemDatabase.ItemDefinitions.TryGetValue(itemId, out var value))
		{
			if (ItemDatabase.ItemsListDefinitions.TryGetValue(itemId, out var value2))
			{
				foreach (KeyValuePair<string, int> item in value2.ItemsWithOdd)
				{
					if (item.Value != 0)
					{
						AddItemToShopItemsByCategory(item.Key);
					}
				}
				return;
			}
			CLoggerManager.Log((object)("item or itemsList definition with id " + itemId + " is missing !"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		else
		{
			ItemDefinition.E_Category category = value.Category;
			if (!ShopItemsByCategory.ContainsKey(category))
			{
				ShopItemsByCategory.Add(category, new HashSet<string> { itemId });
			}
			else
			{
				ShopItemsByCategory[category].Add(itemId);
			}
		}
	}

	private void DeserializeActions()
	{
		if (BuildingActionDefinitions != null)
		{
			return;
		}
		BuildingActionDefinitions = new Dictionary<string, BuildingActionDefinition>();
		foreach (XElement item in ((XContainer)((XContainer)XDocument.Parse(buildingActionDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("BuildingActionDefinitions"))).Elements(XName.op_Implicit("BuildingActionDefinition")))
		{
			BuildingActionDefinition buildingActionDefinition = new BuildingActionDefinition((XContainer)(object)item);
			BuildingActionDefinitions.Add(buildingActionDefinition.Id, buildingActionDefinition);
		}
	}

	private void DeserializeBuildingDefinitions()
	{
		if (BuildingDefinitions != null)
		{
			return;
		}
		BuildingDefinitions = new Dictionary<string, BuildingDefinition>();
		BuildingLimitGroupDefinitions = new Dictionary<string, BuildingLimitGroupDefinition>();
		string name = typeof(BuildingDefinition).Name;
		string text = name + "s";
		ConcurrentQueue<XElement> xDefinitionsList = new ConcurrentQueue<XElement>();
		ConcurrentQueue<XElement> xBuildingLimitGroupDefinitionsList = new ConcurrentQueue<XElement>();
		foreach (TextAsset groupBuildingDefinition in groupBuildingDefinitions)
		{
			XDocument obj = XDocument.Parse(groupBuildingDefinition.text, (LoadOptions)2);
			((XContainer)((XContainer)obj).Element(XName.op_Implicit(text))).Elements(XName.op_Implicit("BuildingLimitGroupDefinition")).All(delegate(XElement o)
			{
				xBuildingLimitGroupDefinitionsList.Enqueue(o);
				return true;
			});
			((XContainer)((XContainer)obj).Element(XName.op_Implicit(text))).Elements(XName.op_Implicit(name)).All(delegate(XElement o)
			{
				xDefinitionsList.Enqueue(o);
				return true;
			});
		}
		foreach (TextAsset individualBuildingDefinition in individualBuildingDefinitions)
		{
			try
			{
				XDocument obj2 = XDocument.Parse(individualBuildingDefinition.text, (LoadOptions)2);
				XElement item = ((obj2 != null) ? ((XContainer)obj2).Element(XName.op_Implicit(name)) : null);
				xDefinitionsList.Enqueue(item);
			}
			catch (InvalidOperationException)
			{
				CLoggerManager.Log((object)("Invalid  template definition: " + ((Object)individualBuildingDefinition).name + ". Please check the XML thoroughly. Loading of this building will be skipped."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			catch (ArgumentNullException)
			{
				CLoggerManager.Log((object)("Please check the " + ((object)this).GetType().Name + " prefab: a NULL building has been linked as part of it."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		XElement result;
		while (xBuildingLimitGroupDefinitionsList.TryDequeue(out result))
		{
			BuildingLimitGroupDefinition buildingLimitGroupDefinition = new BuildingLimitGroupDefinition((XContainer)(object)result);
			try
			{
				BuildingLimitGroupDefinitions.Add(buildingLimitGroupDefinition.Id, buildingLimitGroupDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log((object)("Duplicate BuildingLimitGroupDefinition found for ID " + buildingLimitGroupDefinition.Id + "."), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		XElement result2;
		while (xDefinitionsList.TryDequeue(out result2))
		{
			BuildingDefinition buildingDefinition = ((!(result2.Attribute(XName.op_Implicit("Id")).Value == "MagicCircle")) ? new BuildingDefinition((XContainer)(object)result2) : new MagicCircleDefinition((XContainer)(object)result2));
			try
			{
				BuildingDefinitions.Add(buildingDefinition.Id, buildingDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log((object)("Duplicate " + name + " found for ID " + buildingDefinition.Id + ": the individual files will have PRIORITY over the all-in-one template file."), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}

	private void DeserializeGaugeEffects()
	{
		if (BuildingGaugeEffectDefinitions != null)
		{
			return;
		}
		BuildingGaugeEffectDefinitions = new Dictionary<string, BuildingGaugeEffectDefinition>();
		XElement val = ((XContainer)XDocument.Parse(buildingGaugeEffectDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("BuildingGaugeEffectDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"Document must have BuildingGaugeEffectDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("BuildingGaugeEffectDefinition")))
		{
			BuildingGaugeEffectDefinition buildingGaugeEffectDefinition = null;
			XAttribute val2 = item.Attribute(XName.op_Implicit("Id"));
			if (XDocumentExtensions.IsNullOrEmpty(val2))
			{
				CLoggerManager.Log((object)"BuildingGaugeEffectDefinition must have Id", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			string value = val2.Value;
			buildingGaugeEffectDefinition = value switch
			{
				"CreateItem" => new CreateItemGaugeEffectDefinition((XContainer)(object)item), 
				"GainGold" => new GainGoldDefinition((XContainer)(object)item), 
				"GainMaterials" => new GainMaterialsDefinition((XContainer)(object)item), 
				"OpenMagicSeal" => new OpenMagicSealDefinition((XContainer)(object)item), 
				"GlobalUpgradeStat" => new UpgradeStatGaugeEffectDefinition((XContainer)(object)item), 
				_ => null, 
			};
			if (buildingGaugeEffectDefinition == null)
			{
				CLoggerManager.Log((object)("BuildingGaugeEffectDefinition " + value + " not found!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				BuildingGaugeEffectDefinitions.Add(value, buildingGaugeEffectDefinition);
			}
		}
	}

	private void DeserializePassive()
	{
		if (BuildingPassiveDefinitions != null)
		{
			return;
		}
		BuildingPassiveDefinitions = new Dictionary<string, BuildingPassiveDefinition>();
		foreach (XElement item in ((XContainer)((XContainer)XDocument.Parse(buildingPassiveDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("BuildingPassiveDefinitions"))).Elements(XName.op_Implicit("BuildingPassiveDefinition")))
		{
			BuildingPassiveDefinition buildingPassiveDefinition = new BuildingPassiveDefinition((XContainer)(object)item);
			BuildingPassiveDefinitions.Add(buildingPassiveDefinition.Id, buildingPassiveDefinition);
		}
	}

	private void DeserializeRandomBuildingsGenerations()
	{
		if (RandomBuildingsGenerationDefinitions != null)
		{
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(randomBuildingsGenerationsDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("RandomBuildingsGenerationsDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"randomBuildingsGenerationsDefinitions document must have a RandomBuildingsGenerationsDefinitions Element", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		RandomBuildingsGenerationDefinitions = new Dictionary<string, RandomBuildingsGenerationDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("RandomBuildingsGenerationDefinition")))
		{
			RandomBuildingsGenerationDefinition randomBuildingsGenerationDefinition = new RandomBuildingsGenerationDefinition((XContainer)(object)item);
			RandomBuildingsGenerationDefinitions.Add(randomBuildingsGenerationDefinition.Id, randomBuildingsGenerationDefinition);
		}
	}

	private void DeserializeRandomBuildingsDirections()
	{
		if (RandomBuildingsDirectionsDefinitions != null)
		{
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(randomBuildingsDirectionsDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("RandomBuildingsDirectionsDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"randomBuildingsDirectionsDefinitions document must have a RandomBuildingsDirectionsDefinitions Element", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		RandomBuildingsDirectionsDefinitions = new Dictionary<string, RandomBuildingsDirectionsDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("RandomBuildingsDirectionsDefinition")))
		{
			RandomBuildingsDirectionsDefinition randomBuildingsDirectionsDefinition = new RandomBuildingsDirectionsDefinition((XContainer)(object)item);
			RandomBuildingsDirectionsDefinitions.Add(randomBuildingsDirectionsDefinition.Id, randomBuildingsDirectionsDefinition);
		}
	}

	private void DeserializeRandomBuildingsPerDay()
	{
		if (RandomBuildingsPerDayDefinitions != null)
		{
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(randomBuildingsPerDayDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("RandomBuildingsPerDayDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"randomBuildingsPerDayDefinitions document must have a RandomBuildingsGenerationsDefinitions Element", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		RandomBuildingsPerDayDefinitions = new Dictionary<string, RandomBuildingsPerDayDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("RandomBuildingsPerDayDefinition")))
		{
			RandomBuildingsPerDayDefinition randomBuildingsPerDayDefinition = new RandomBuildingsPerDayDefinition((XContainer)(object)item);
			RandomBuildingsPerDayDefinitions.Add(randomBuildingsPerDayDefinition.Id, randomBuildingsPerDayDefinition);
		}
	}

	private void DeserializeShop()
	{
		if (ShopDefinition == null)
		{
			XElement val = ((XContainer)XDocument.Parse(shopDefinition.text, (LoadOptions)2)).Element(XName.op_Implicit("ShopDefinition"));
			if (val == null)
			{
				CLoggerManager.Log((object)"ShopDefinition document must have a ShopDefinition Element", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				ShopDefinition = new ShopDefinition((XContainer)(object)val);
			}
		}
	}

	private void DeserializeSkillSoundDefinitions()
	{
		if (BuildingSkillSoundDefinitions != null)
		{
			return;
		}
		XElement obj = ((XContainer)XDocument.Parse(buildingSkillSoundDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("BuildingSkillSoundDefinitions"));
		BuildingSkillSoundDefinitions = new Dictionary<string, BuildingSkillSoundDefinition>();
		foreach (XElement item in ((XContainer)obj).Elements(XName.op_Implicit("BuildingSkillSoundDefinition")))
		{
			BuildingSkillSoundDefinition buildingSkillSoundDefinition = new BuildingSkillSoundDefinition((XContainer)(object)item);
			BuildingSkillSoundDefinitions.Add(buildingSkillSoundDefinition.BuildingTemplateDefinitionId, buildingSkillSoundDefinition);
		}
	}

	private void DeserializeUpgrades()
	{
		if (BuildingUpgradeDefinitions != null)
		{
			return;
		}
		BuildingUpgradeDefinitions = new Dictionary<string, BuildingUpgradeDefinition>();
		foreach (XElement item in ((XContainer)((XContainer)XDocument.Parse(buildingUpgradeDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("BuildingUpgradeDefinitions"))).Elements(XName.op_Implicit("BuildingUpgradeDefinition")))
		{
			BuildingUpgradeDefinition buildingUpgradeDefinition = new BuildingUpgradeDefinition((XContainer)(object)item);
			BuildingUpgradeDefinitions.Add(buildingUpgradeDefinition.Id, buildingUpgradeDefinition);
		}
	}
}
