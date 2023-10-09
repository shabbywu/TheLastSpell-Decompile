using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.PlayableUnitGeneration;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Unit;
using UnityEngine;

namespace TheLastStand.Database.Unit;

public class PlayableUnitDatabase : Database<PlayableUnitDatabase>
{
	[SerializeField]
	private List<DataColor> portraitBackgroundColors = new List<DataColor>();

	[SerializeField]
	private Dictionary<int, int> perksPointsPerLevel = new Dictionary<int, int>();

	[SerializeField]
	private TextAsset playableUnitConfigTextAsset;

	[SerializeField]
	private TextAsset unitLevelUpStats;

	[SerializeField]
	private TextAsset unitLevelUpSettings;

	[SerializeField]
	private TextAsset unitEquipmentSlotsDefinitions;

	[SerializeField]
	private TextAsset unitTraitDefinitions;

	[SerializeField]
	private TextAsset unitTraitGenerationDefinition;

	[SerializeField]
	private TextAsset playableUnitGenerationDefinitionTextAsset;

	[SerializeField]
	private TextAsset playableUnitTemplateDefinition;

	[SerializeField]
	private TextAsset[] unitsGenerationsStartDefinitionsTextAssets;

	[SerializeField]
	private TextAsset unitGenerationLevelDefinitions;

	[SerializeField]
	private TextAsset recruitmentDefinition;

	[SerializeField]
	private TextAsset unitPerkTemplateDefinition;

	[SerializeField]
	private TextAsset unitPerkCollectionDefinitions;

	[SerializeField]
	private TextAsset perkDefinitions;

	[SerializeField]
	private TextAsset unitMaleHeads;

	[SerializeField]
	private TextAsset unitFemaleHeads;

	[SerializeField]
	private TextAsset unitNakedBodyPartDefinitions;

	[SerializeField]
	private TextAsset unitHairColorDefinitions;

	[SerializeField]
	private TextAsset unitSkinColorDefinitions;

	[SerializeField]
	private TextAsset unitEyesColorDefinitions;

	[SerializeField]
	private TextAsset unitLinkHairSkinColorDefinitions;

	[SerializeField]
	private TextAsset unitMaleNames;

	[SerializeField]
	private TextAsset unitFemaleNames;

	[SerializeField]
	private int startingUnitsSpawnAreaSize = 8;

	[Tooltip("Areas are currently on the left AND and on the right of the circle, so this value should be rather small, like 2 or 3.")]
	[SerializeField]
	private int victoryUnitsGatherAreaSize = 2;

	[SerializeField]
	[Range(0f, 100f)]
	private float unitMoveSpeed = 15f;

	public static Node ExperienceNeededToNextLevel { get; private set; }

	public static float KillerBonusExperienceFactor { get; private set; }

	public static Dictionary<int, int> PerksPointsPerLevel => TPSingleton<PlayableUnitDatabase>.Instance.perksPointsPerLevel;

	public static UnitFaceIdDefinitions PlayableFemaleUnitFaceIds { get; set; }

	public static UnitFaceIdDefinitions PlayableMaleUnitFaceIds { get; set; }

	public static Dictionary<string, PlayableUnitGenerationDefinition> PlayableUnitGenerationDefinitions { get; private set; }

	public static Dictionary<string, ColorSwapPaletteDefinition> PlayableUnitHairColorDefinitions { get; private set; }

	public static Dictionary<string, BodyPartDefinition> PlayableUnitNakedBodyPartsDefinitions { get; private set; }

	public static Dictionary<string, ColorSwapPaletteDefinition> PlayableUnitSkinColorDefinitions { get; private set; }

	public static Dictionary<string, ColorSwapPaletteDefinition> PlayableUnitEyesColorDefinitions { get; private set; }

	public static PlayableUnitTemplateDefinition PlayableUnitTemplateDefinition { get; private set; }

	public static List<DataColor> PortraitBackgroundColors => TPSingleton<PlayableUnitDatabase>.Instance.portraitBackgroundColors;

	public static List<string> SecondaryTraitIds { get; private set; }

	public static List<int> SecondaryTraitCost { get; private set; }

	public static int StartingUnitsSpawnAreaSize => TPSingleton<PlayableUnitDatabase>.Instance.startingUnitsSpawnAreaSize;

	public static Dictionary<ItemSlotDefinition.E_ItemSlotId, UnitEquipmentSlotDefinition> UnitEquipmentSlotDefinitions { get; set; }

	public static Dictionary<string, UnitGenerationLevelDefinition> UnitGenerationLevelDefinitions { get; set; }

	public static Dictionary<string, List<UnitGenerationDefinition>> UnitsGenerationStartDefinitions { get; set; }

	public static Dictionary<UnitStatDefinition.E_Stat, UnitLevelUpStatDefinition> UnitLevelUpMainStatDefinitions { get; private set; }

	public static Dictionary<UnitStatDefinition.E_Stat, UnitLevelUpStatDefinition> UnitLevelUpSecondaryStatDefinitions { get; private set; }

	public static UnitLevelUpDefinition UnitLevelUpDefinition { get; private set; }

	public static UnitLinkHairSkin UnitLinkHairSkin { get; private set; }

	public static float UnitMoveSpeed => TPSingleton<PlayableUnitDatabase>.Instance.unitMoveSpeed;

	public static Dictionary<string, UnitPerkCollectionDefinition> UnitPerkCollectionDefinitions { get; private set; }

	public static Dictionary<string, PerkDefinition> PerkDefinitions { get; private set; }

	public static UnitPerkTemplateDefinition UnitPerkTemplateDefinition { get; private set; }

	public static Dictionary<string, UnitTraitDefinition> UnitTraitDefinitions { get; private set; }

	public static Dictionary<int, string> UnitTraitTiersId { get; private set; }

	public static Dictionary<int, string> UnitBackgroundTraitTiersId { get; private set; }

	public static UnitTraitGenerationDefinition UnitTraitGenerationDefinition { get; private set; }

	private static List<string> PlayableFemaleUnitNames { get; set; }

	private static List<string> PlayableMaleUnitNames { get; set; }

	public static RecruitmentDefinition RecruitmentDefinition { get; private set; }

	public static int VictoryUnitsGatherAreaSize => TPSingleton<PlayableUnitDatabase>.Instance.victoryUnitsGatherAreaSize;

	public static List<string> GetFaceIdsForGender(string gender)
	{
		return gender switch
		{
			"Female" => PlayableFemaleUnitFaceIds.ToStringList(), 
			"Male" => PlayableMaleUnitFaceIds.ToStringList(), 
			_ => null, 
		};
	}

	public static List<string> GetNamesForGender(string gender)
	{
		return gender switch
		{
			"Female" => PlayableFemaleUnitNames, 
			"Male" => PlayableMaleUnitNames, 
			_ => null, 
		};
	}

	public override void Deserialize(XContainer container = null)
	{
		DeserializePlayableUnitConfig();
		DeserializeUnitFaceIds();
		DeserializeUnitNakedBodyParts();
		DeserializeUnitNames();
		DeserializeUnitHairColors();
		DeserializeUnitSkinColors();
		DeserializeUnitEyesColors();
		DeserializeUnitLinkHairSkinColors();
		DeserializePerks();
		DeserializeUnitPerksCollections();
		DeserializeUnitPerksTemplate();
		DeserializeUnitEquipmentSlots();
		DeserializeUnitTraits();
		DeserializeUnitsGeneration();
		DeserializePlayableUnitTemplate();
		DeserializeUnitLevelUpStats();
	}

	private static int CompareTraitsByCost(string traitId1, string traitId2)
	{
		return UnitTraitDefinitions[traitId1].Cost.CompareTo(UnitTraitDefinitions[traitId2].Cost);
	}

	private static void LoadUnitFaceIds(XDocument idsDocument, string gender)
	{
		switch (gender)
		{
		case "Male":
			PlayableMaleUnitFaceIds = new UnitFaceIdDefinitions(idsDocument);
			break;
		case "Female":
			PlayableFemaleUnitFaceIds = new UnitFaceIdDefinitions(idsDocument);
			break;
		}
	}

	private static void LoadUnitNames(string names, string gender)
	{
		List<string> namesForGender = GetNamesForGender(gender);
		string[] array = names.Split(new char[1] { '\n' });
		for (int num = array.Length - 1; num >= 0; num--)
		{
			array[num] = array[num].Trim();
			if (array[num] != string.Empty)
			{
				string text = string.Empty;
				for (int i = 0; i < array[num].Length; i++)
				{
					text += array[num][i];
				}
				namesForGender.Add(text);
			}
		}
	}

	private void DeserializePlayableUnitConfig()
	{
		XElement obj = ((XContainer)XDocument.Parse(playableUnitConfigTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("PlayableUnitConfig"));
		ExperienceNeededToNextLevel = Parser.Parse(((XContainer)obj).Element(XName.op_Implicit("ExperienceNeededToNextLevel")).Value);
		XElement val = ((XContainer)obj).Element(XName.op_Implicit("BonusExperiencePerKill"));
		if (!float.TryParse(val.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log((object)("Could not parse BonusExperiencePerKill element value \"" + val.Value + "\" to a valid float value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			KillerBonusExperienceFactor = result;
		}
	}

	private void DeserializePlayableUnitTemplate()
	{
		if (PlayableUnitTemplateDefinition == null)
		{
			PlayableUnitTemplateDefinition = new PlayableUnitTemplateDefinition((XContainer)(object)((XContainer)XDocument.Parse(playableUnitTemplateDefinition.text, (LoadOptions)2)).Element(XName.op_Implicit("PlayableUnitTemplateDefinition")));
		}
	}

	private void DeserializeUnitLinkHairSkinColors()
	{
		UnitLinkHairSkin = new UnitLinkHairSkin(((XContainer)XDocument.Parse(unitLinkHairSkinColorDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitLinkHairSkinColorDefinition")));
	}

	private void DeserializeUnitFaceIds()
	{
		if (PlayableFemaleUnitFaceIds == null && PlayableMaleUnitFaceIds == null)
		{
			XDocument idsDocument = XDocument.Parse(TPSingleton<PlayableUnitDatabase>.Instance.unitMaleHeads.text, (LoadOptions)2);
			XDocument idsDocument2 = XDocument.Parse(TPSingleton<PlayableUnitDatabase>.Instance.unitFemaleHeads.text, (LoadOptions)2);
			LoadUnitFaceIds(idsDocument, "Male");
			LoadUnitFaceIds(idsDocument2, "Female");
		}
	}

	private void DeserializeUnitEquipmentSlots()
	{
		UnitEquipmentSlotDefinitions = new Dictionary<ItemSlotDefinition.E_ItemSlotId, UnitEquipmentSlotDefinition>();
		foreach (XElement item in ((XContainer)((XContainer)XDocument.Parse(unitEquipmentSlotsDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitEquipmentSlotDefinitions"))).Elements(XName.op_Implicit("UnitEquipmentSlotDefinition")))
		{
			if (item.Attribute(XName.op_Implicit("Id")).IsNullOrEmpty())
			{
				CLoggerManager.Log((object)"The UnitEquipmentSlotDefinition must have a valid id", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			UnitEquipmentSlotDefinition unitEquipmentSlotDefinition = new UnitEquipmentSlotDefinition((XContainer)(object)item);
			UnitEquipmentSlotDefinitions.Add(unitEquipmentSlotDefinition.Id, unitEquipmentSlotDefinition);
		}
	}

	private void DeserializeUnitsGeneration()
	{
		if (UnitGenerationLevelDefinitions != null)
		{
			return;
		}
		UnitGenerationLevelDefinitions = new Dictionary<string, UnitGenerationLevelDefinition>();
		XElement val = ((XContainer)XDocument.Parse(unitGenerationLevelDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitGenerationLevelDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"The document must have UnitGenerationLevelDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("UnitGenerationLevelDefinition")))
		{
			UnitGenerationLevelDefinition unitGenerationLevelDefinition = new UnitGenerationLevelDefinition((XContainer)(object)item2);
			UnitGenerationLevelDefinitions.Add(unitGenerationLevelDefinition.Id, unitGenerationLevelDefinition);
		}
		UnitsGenerationStartDefinitions = new Dictionary<string, List<UnitGenerationDefinition>>();
		Queue<XElement> queue = GatherElements(unitsGenerationsStartDefinitionsTextAssets, null, "UnitsGenerationStartDefinitions", "UnitsGenerationsStartDefinitions");
		while (queue.Count > 0)
		{
			XElement obj = queue.Dequeue();
			XAttribute val2 = obj.Attribute(XName.op_Implicit("Id"));
			UnitsGenerationStartDefinitions.Add(val2.Value, new List<UnitGenerationDefinition>());
			foreach (XElement item3 in ((XContainer)obj).Elements(XName.op_Implicit("UnitGenerationStartDefinition")))
			{
				UnitGenerationDefinition item = new UnitGenerationDefinition((XContainer)(object)item3);
				UnitsGenerationStartDefinitions[val2.Value].Add(item);
			}
		}
		XElement val3 = ((XContainer)XDocument.Parse(recruitmentDefinition.text, (LoadOptions)2)).Element(XName.op_Implicit("RecruitmentDefinition"));
		if (val3 == null)
		{
			CLoggerManager.Log((object)"The document must have RecruitmentDefinition", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		RecruitmentDefinition = new RecruitmentDefinition((XContainer)(object)val3);
		XElement val4 = ((XContainer)XDocument.Parse(playableUnitGenerationDefinitionTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("PlayableUnitGenerationDefinitions"));
		if (val4 == null)
		{
			CLoggerManager.Log((object)"The playableUnitGenerationDefinitionsDocument must have an element PlayableUnitGenerationDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		PlayableUnitGenerationDefinitions = new Dictionary<string, PlayableUnitGenerationDefinition>();
		foreach (XElement item4 in ((XContainer)val4).Elements(XName.op_Implicit("PlayableUnitGenerationDefinition")))
		{
			PlayableUnitGenerationDefinition playableUnitGenerationDefinition = new PlayableUnitGenerationDefinition((XContainer)(object)item4);
			PlayableUnitGenerationDefinitions.Add(playableUnitGenerationDefinition.ArchetypeId, playableUnitGenerationDefinition);
		}
	}

	private void DeserializeUnitHairColors()
	{
		if (PlayableUnitHairColorDefinitions != null)
		{
			return;
		}
		if ((Object)(object)unitHairColorDefinitions == (Object)null)
		{
			TPDebug.LogError((object)"The document unitHairColorDefinitions can't be null", (Object)(object)this);
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(unitHairColorDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitHairColorDefinitions"));
		if (val == null)
		{
			TPDebug.LogError((object)"The document unitHairColorDefinitions must define UnitHairColorDefinitions", (Object)(object)this);
			return;
		}
		PlayableUnitHairColorDefinitions = new Dictionary<string, ColorSwapPaletteDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("ColorSwapPalette")))
		{
			ColorSwapPaletteDefinition colorSwapPaletteDefinition = new ColorSwapPaletteDefinition((XContainer)(object)item);
			PlayableUnitHairColorDefinitions.Add(colorSwapPaletteDefinition.Id, colorSwapPaletteDefinition);
		}
	}

	private void DeserializeUnitLevelUpStats()
	{
		XElement val = ((XContainer)XDocument.Parse(unitLevelUpStats.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitLevelUpStatDefinitions"));
		UnitLevelUpMainStatDefinitions = new Dictionary<UnitStatDefinition.E_Stat, UnitLevelUpStatDefinition>(UnitStatDefinition.SharedStatComparer);
		UnitLevelUpSecondaryStatDefinitions = new Dictionary<UnitStatDefinition.E_Stat, UnitLevelUpStatDefinition>(UnitStatDefinition.SharedStatComparer);
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("MainStats"))).Elements(XName.op_Implicit("UnitLevelUpStatDefinition")))
		{
			UnitLevelUpStatDefinition unitLevelUpStatDefinition = new UnitLevelUpStatDefinition((XContainer)(object)item);
			UnitLevelUpMainStatDefinitions.Add(unitLevelUpStatDefinition.Stat, unitLevelUpStatDefinition);
		}
		foreach (XElement item2 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("SecondaryStats"))).Elements(XName.op_Implicit("UnitLevelUpStatDefinition")))
		{
			UnitLevelUpStatDefinition unitLevelUpStatDefinition2 = new UnitLevelUpStatDefinition((XContainer)(object)item2);
			UnitLevelUpSecondaryStatDefinitions.Add(unitLevelUpStatDefinition2.Stat, unitLevelUpStatDefinition2);
		}
		UnitLevelUpDefinition = new UnitLevelUpDefinition((XContainer)(object)((XContainer)XDocument.Parse(unitLevelUpSettings.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitLevelUpDefinition")));
	}

	private void DeserializeUnitNakedBodyParts()
	{
		if (PlayableUnitNakedBodyPartsDefinitions != null)
		{
			return;
		}
		if ((Object)(object)unitNakedBodyPartDefinitions == (Object)null)
		{
			TPDebug.LogError((object)"The document nakedBodyPartsDefinitions can't be null", (Object)(object)this);
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(unitNakedBodyPartDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitNakedBodyPartDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"The document nakedBodyPartsDefinitions must define xBodyPartDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		PlayableUnitNakedBodyPartsDefinitions = new Dictionary<string, BodyPartDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("BodyPartDefinition")))
		{
			BodyPartDefinition bodyPartDefinition = new BodyPartDefinition((XContainer)(object)item);
			PlayableUnitNakedBodyPartsDefinitions.Add(bodyPartDefinition.Id, bodyPartDefinition);
		}
	}

	private void DeserializeUnitNames()
	{
		if (PlayableFemaleUnitNames == null && PlayableMaleUnitNames == null)
		{
			PlayableFemaleUnitNames = new List<string>();
			PlayableMaleUnitNames = new List<string>();
			LoadUnitNames(TPSingleton<PlayableUnitDatabase>.Instance.unitMaleNames.text, "Male");
			LoadUnitNames(TPSingleton<PlayableUnitDatabase>.Instance.unitFemaleNames.text, "Female");
		}
	}

	private void DeserializePerks()
	{
		if (PerkDefinitions != null)
		{
			return;
		}
		PerkDefinitions = new Dictionary<string, PerkDefinition>();
		foreach (XElement item in ((XContainer)((XContainer)XDocument.Parse(perkDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("PerkDefinitions"))).Elements(XName.op_Implicit("PerkDefinition")))
		{
			PerkDefinitions[item.Attribute(XName.op_Implicit("Id")).Value] = new PerkDefinition((XContainer)(object)item);
		}
	}

	private void DeserializeUnitPerksCollections()
	{
		XElement obj = ((XContainer)XDocument.Parse(unitPerkCollectionDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitPerkCollectionDefinitions"));
		UnitPerkCollectionDefinitions = new Dictionary<string, UnitPerkCollectionDefinition>();
		foreach (XElement item in ((XContainer)obj).Elements(XName.op_Implicit("UnitPerkCollectionDefinition")))
		{
			UnitPerkCollectionDefinition unitPerkCollectionDefinition = new UnitPerkCollectionDefinition((XContainer)(object)item);
			if (UnitPerkCollectionDefinitions.ContainsKey(unitPerkCollectionDefinition.Id))
			{
				CLoggerManager.Log((object)("Perk collection \"" + unitPerkCollectionDefinition.Id + "\" already exists in database. Skip."), (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
			}
			else
			{
				UnitPerkCollectionDefinitions.Add(unitPerkCollectionDefinition.Id, unitPerkCollectionDefinition);
			}
		}
	}

	private void DeserializeUnitPerksTemplate()
	{
		UnitPerkTemplateDefinition = new UnitPerkTemplateDefinition((XContainer)(object)((XContainer)XDocument.Parse(unitPerkTemplateDefinition.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitPerkTemplateDefinition")));
	}

	private void DeserializeUnitSkinColors()
	{
		if (PlayableUnitSkinColorDefinitions != null)
		{
			return;
		}
		if ((Object)(object)unitSkinColorDefinitions == (Object)null)
		{
			TPDebug.LogError((object)"The document unitSkinColorDefinitions can't be null", (Object)(object)this);
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(unitSkinColorDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitSkinColorDefinitions"));
		if (val == null)
		{
			TPDebug.LogError((object)"The document unitSkinColorDefinitions must define UnitSkinColorDefinitions", (Object)(object)this);
			return;
		}
		PlayableUnitSkinColorDefinitions = new Dictionary<string, ColorSwapPaletteDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("ColorSwapPalette")))
		{
			ColorSwapPaletteDefinition colorSwapPaletteDefinition = new ColorSwapPaletteDefinition((XContainer)(object)item);
			PlayableUnitSkinColorDefinitions.Add(colorSwapPaletteDefinition.Id, colorSwapPaletteDefinition);
		}
	}

	private void DeserializeUnitEyesColors()
	{
		if (PlayableUnitEyesColorDefinitions != null)
		{
			return;
		}
		if ((Object)(object)unitEyesColorDefinitions == (Object)null)
		{
			CLoggerManager.Log((object)"The document unitEyesColorDefinitions can't be null", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(unitEyesColorDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitEyesColorDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"The document unitEyesColorDefinitions must define UnitEyesColorDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		PlayableUnitEyesColorDefinitions = new Dictionary<string, ColorSwapPaletteDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("ColorSwapPalette")))
		{
			ColorSwapPaletteDefinition colorSwapPaletteDefinition = new ColorSwapPaletteDefinition((XContainer)(object)item);
			PlayableUnitEyesColorDefinitions.Add(colorSwapPaletteDefinition.Id, colorSwapPaletteDefinition);
		}
	}

	private void DeserializeUnitTraits()
	{
		if (UnitTraitDefinitions != null)
		{
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(unitTraitDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitTraitDefinitions"));
		List<UnitTraitTierDefinition> list = new List<UnitTraitTierDefinition>();
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("UnitTraitTiers"))).Elements(XName.op_Implicit("TraitTier")))
		{
			list.Add(new UnitTraitTierDefinition((XContainer)(object)item));
		}
		UnitTraitTiersId = new Dictionary<int, string>();
		UnitBackgroundTraitTiersId = new Dictionary<int, string>();
		foreach (UnitTraitTierDefinition item2 in list)
		{
			Dictionary<int, string> dictionary = (item2.IsBackground ? UnitBackgroundTraitTiersId : UnitTraitTiersId);
			foreach (int cost in item2.Costs)
			{
				if (dictionary.ContainsKey(cost))
				{
					CLoggerManager.Log((object)$"Cost {cost} is present in multiple trait tiers, this is unexpected.", (LogType)0, (CLogLevel)2, true, "PlayableUnitDatabase", false);
				}
				else
				{
					dictionary.Add(cost, item2.Id);
				}
			}
		}
		UnitTraitDefinitions = new Dictionary<string, UnitTraitDefinition>();
		SecondaryTraitIds = new List<string>();
		foreach (XElement item3 in ((XContainer)val).Elements(XName.op_Implicit("UnitTraitDefinition")))
		{
			UnitTraitDefinition unitTraitDefinition = new UnitTraitDefinition((XContainer)(object)item3);
			UnitTraitDefinitions.Add(unitTraitDefinition.Id, unitTraitDefinition);
			if (!unitTraitDefinition.IsBackgroundTrait)
			{
				SecondaryTraitIds.Add(unitTraitDefinition.Id);
			}
		}
		SecondaryTraitIds.Sort(CompareTraitsByCost);
		UnitTraitGenerationDefinition = new UnitTraitGenerationDefinition((XContainer)(object)XDocument.Parse(unitTraitGenerationDefinition.text, (LoadOptions)2));
	}

	protected override void Awake()
	{
		base.Awake();
		if (((TPSingleton<PlayableUnitDatabase>)this)._IsValid || SecondaryTraitCost == null)
		{
			InitializeSecondaryTraitCost();
		}
	}

	private void InitializeSecondaryTraitCost()
	{
		SecondaryTraitCost = new List<int>();
		foreach (string secondaryTraitId in SecondaryTraitIds)
		{
			if (!UnitTraitDefinitions.ContainsKey(secondaryTraitId))
			{
				CLoggerManager.Log((object)("UnitTraitDefinitions doesn't contains this trait id : " + secondaryTraitId), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
				continue;
			}
			int cost = UnitTraitDefinitions[secondaryTraitId].Cost;
			if (!SecondaryTraitCost.Contains(cost))
			{
				SecondaryTraitCost.Add(cost);
			}
		}
	}
}
