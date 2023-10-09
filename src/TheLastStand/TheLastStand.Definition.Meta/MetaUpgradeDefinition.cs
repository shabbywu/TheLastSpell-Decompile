using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class MetaUpgradeDefinition : TheLastStand.Framework.Serialization.Definition
{
	[Flags]
	public enum E_MetaUpgradeCategory
	{
		None = 0,
		Misc = 1,
		City = 2,
		Building = 4,
		Weapon = 8,
		Equipment = 0x10,
		Glyph = 0x20,
		Hero = 0x40,
		All = 0x7F
	}

	[Flags]
	public enum E_MetaUpgradeFilter
	{
		None = 0,
		Acquired = 1,
		Locked = 2,
		NotAcquiredYet = 4,
		New = 8
	}

	public class ConditionsGroup
	{
		public int GroupIndex;

		public bool CheckOnce;

		public List<MetaConditionDefinition> Conditions = new List<MetaConditionDefinition>();
	}

	public E_MetaUpgradeCategory Category { get; private set; }

	public bool DamnedSoulsShop => Price != 0;

	public int DeserializationIndex { get; private set; }

	public bool MandatoryUnlock { get; private set; }

	public bool Hidden { get; private set; }

	public string IconName { get; private set; } = string.Empty;


	public string Id { get; private set; }

	public uint Price { get; private set; }

	public List<ConditionsGroup> ActivationConditionsDefinitions { get; } = new List<ConditionsGroup>();


	public List<ConditionsGroup> UnlockConditionsDefinitions { get; } = new List<ConditionsGroup>();


	public List<MetaEffectDefinition> UpgradeEffectDefinitions { get; } = new List<MetaEffectDefinition>();


	public List<string> BuildingActionsToShow { get; private set; } = new List<string>();


	public List<string> BuildingsToShow { get; private set; } = new List<string>();


	public List<string> BuildingUpgradesToShow { get; private set; } = new List<string>();


	public List<string> GlyphsToShow { get; private set; } = new List<string>();


	public List<string> ItemsToShow { get; private set; } = new List<string>();


	public MetaUpgradeDefinition(XContainer container, int deserializationIndex)
		: base(container)
	{
		DeserializationIndex = deserializationIndex;
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		Id = val.Attribute(XName.op_Implicit("Id")).Value;
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("IconName"));
		IconName = ((obj != null) ? obj.Value : null) ?? Id;
		Hidden = ((XContainer)val).Element(XName.op_Implicit("Hidden")) != null;
		XAttribute obj2 = val.Attribute(XName.op_Implicit("Price"));
		Price = uint.Parse(((obj2 != null) ? obj2.Value : null) ?? Price.ToString());
		MandatoryUnlock = ((XContainer)val).Element(XName.op_Implicit("MandatoryUnlock")) != null;
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("UnlockConditions"));
		if (val2 != null)
		{
			DeserializeConditions(val2, UnlockConditionsDefinitions);
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("ActivationConditions"));
		if (val3 != null)
		{
			DeserializeConditions(val3, ActivationConditionsDefinitions);
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("UpgradeEffects"));
		if (val4 != null)
		{
			foreach (XElement item in ((XContainer)val4).Elements())
			{
				switch (item.Name.LocalName)
				{
				case "AdditionalInitMages":
					UpgradeEffectDefinitions.Add(new AdditionalInitMagesMetaEffectDefinition((XContainer)(object)item));
					break;
				case "AdditionalRerollReward":
					UpgradeEffectDefinitions.Add(new AdditionalRerollRewardMetaEffectDefinition((XContainer)(object)item));
					break;
				case "BuildingModifier":
					UpgradeEffectDefinitions.Add(new BuildingModifierMetaEffectDefinition((XContainer)(object)item));
					break;
				case "CreateItemModifier":
					UpgradeEffectDefinitions.Add(new CreateItemModifierMetaEffectDefinition((XContainer)(object)item));
					break;
				case "FogModifier":
					UpgradeEffectDefinitions.Add(new FogModifierMetaEffectDefinition((XContainer)(object)item));
					break;
				case "InitResourcesBonus":
					UpgradeEffectDefinitions.Add(new InitResourcesBonusMetaEffectDefinition((XContainer)(object)item));
					break;
				case "ItemLevelProbabilityModifier":
					UpgradeEffectDefinitions.Add(new ItemLevelProbabilityMetaEffectDefinition((XContainer)(object)item));
					break;
				case "ItemRaritiesModifier":
					UpgradeEffectDefinitions.Add(new ItemRaritiesMetaEffectDefinition((XContainer)(object)item));
					break;
				case "LockItems":
					UpgradeEffectDefinitions.Add(new LockItemsMetaEffectDefinition((XContainer)(object)item));
					break;
				case "NewEnemy":
					UpgradeEffectDefinitions.Add(new NewEnemyMetaEffectDefinition((XContainer)(object)item));
					break;
				case "TraitsParameters":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new TraitsParametersMetaEffectDefinition((XContainer)(object)item));
					break;
				case "PlayableUnitAttributeModifier":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnitAttributeModifierMetaEffectDefinition((XContainer)(object)item));
					break;
				case "UnlockAffixes":
					UpgradeEffectDefinitions.Add(new UnlockAffixesMetaEffectDefinition((XContainer)(object)item));
					break;
				case "UnlockBuildingAction":
				{
					Category |= E_MetaUpgradeCategory.Building;
					UnlockBuildingActionMetaEffectDefinition unlockBuildingActionMetaEffectDefinition = new UnlockBuildingActionMetaEffectDefinition((XContainer)(object)item);
					BuildingActionsToShow.Add(unlockBuildingActionMetaEffectDefinition.BuildingActionId);
					UpgradeEffectDefinitions.Add(unlockBuildingActionMetaEffectDefinition);
					break;
				}
				case "UnlockBuilding":
				{
					Category |= E_MetaUpgradeCategory.Building;
					UnlockBuildingMetaEffectDefinition unlockBuildingMetaEffectDefinition = new UnlockBuildingMetaEffectDefinition((XContainer)(object)item);
					BuildingsToShow.Add(unlockBuildingMetaEffectDefinition.BuildingId);
					UpgradeEffectDefinitions.Add(unlockBuildingMetaEffectDefinition);
					break;
				}
				case "UnlockBuildingUpgrade":
				{
					Category |= E_MetaUpgradeCategory.Building;
					UnlockBuildingUpgradeMetaEffectDefinition unlockBuildingUpgradeMetaEffectDefinition = new UnlockBuildingUpgradeMetaEffectDefinition((XContainer)(object)item);
					BuildingUpgradesToShow.Add(unlockBuildingUpgradeMetaEffectDefinition.UpgradeId);
					UpgradeEffectDefinitions.Add(unlockBuildingUpgradeMetaEffectDefinition);
					break;
				}
				case "UnlockEquipmentGeneration":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockEquipmentGenerationMetaEffectDefinition((XContainer)(object)item));
					break;
				case "UnlockItems":
				{
					UnlockItemsMetaEffectDefinition unlockItemsMetaEffectDefinition = new UnlockItemsMetaEffectDefinition((XContainer)(object)item);
					foreach (string item2 in unlockItemsMetaEffectDefinition.ItemsToUnlock)
					{
						if (ItemDatabase.ItemDefinitions.TryGetValue(item2, out var value))
						{
							if (value.IsWeapon)
							{
								Category |= E_MetaUpgradeCategory.Weapon;
							}
							else
							{
								Category |= E_MetaUpgradeCategory.Equipment;
							}
						}
					}
					ItemsToShow.AddRange(unlockItemsMetaEffectDefinition.ItemsToUnlock);
					UpgradeEffectDefinitions.Add(unlockItemsMetaEffectDefinition);
					break;
				}
				case "UnlockCities":
					Category |= E_MetaUpgradeCategory.City;
					UpgradeEffectDefinitions.Add(new UnlockCitiesMetaEffectDefinition((XContainer)(object)item));
					break;
				case "UnlockGlyphs":
				{
					Category |= E_MetaUpgradeCategory.Glyph;
					UnlockGlyphsMetaEffectDefinition unlockGlyphsMetaEffectDefinition = new UnlockGlyphsMetaEffectDefinition((XContainer)(object)item);
					GlyphsToShow.AddRange(unlockGlyphsMetaEffectDefinition.GlyphIds);
					UpgradeEffectDefinitions.Add(unlockGlyphsMetaEffectDefinition);
					break;
				}
				case "UnlockPerkCollectionSlots":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockPerkCollectionSlotsMetaEffectDefinition((XContainer)(object)item));
					break;
				case "UnlockRerollReward":
					UpgradeEffectDefinitions.Add(new UnlockRerollRewardMetaEffectDefinition((XContainer)(object)item));
					break;
				case "UnlockShopReroll":
					UpgradeEffectDefinitions.Add(new UnlockShopRerollMetaEffectDefinition((XContainer)(object)item));
					break;
				case "UnlockTraits":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockTraitsMetaEffectDefinition((XContainer)(object)item));
					break;
				case "UnlockWaves":
					UpgradeEffectDefinitions.Add(new UnlockWavesMetaEffectDefinition((XContainer)(object)item));
					break;
				case "UpgradeCity":
					UpgradeEffectDefinitions.Add(new UpgradeCityMetaEffectDefinition((XContainer)(object)item));
					break;
				case "WavesParameters":
					UpgradeEffectDefinitions.Add(new WavesParametersMetaEffectDefinition((XContainer)(object)item));
					break;
				default:
					CLoggerManager.Log((object)("MetaUpgrade effect " + item.Name.LocalName + " is not handled to be parsed as a valid definition!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					break;
				}
			}
			XElement val5 = ((XContainer)val).Element(XName.op_Implicit("ForceDisplayTooltips"));
			if (val5 != null)
			{
				foreach (XElement item3 in ((XContainer)val5).Elements())
				{
					XAttribute val6 = item3.Attribute(XName.op_Implicit("Id"));
					switch (item3.Name.LocalName)
					{
					case "ItemTooltip":
						ItemsToShow.Add(val6.Value);
						break;
					case "GlyphTooltip":
						GlyphsToShow.Add(val6.Value);
						break;
					case "BuildingTooltip":
						BuildingsToShow.Add(val6.Value);
						break;
					case "BuildingActionTooltip":
						BuildingActionsToShow.Add(val6.Value);
						break;
					case "BuildingUpgradeTooltip":
						BuildingUpgradesToShow.Add(val6.Value);
						break;
					}
				}
			}
			XElement val7 = ((XContainer)val).Element(XName.op_Implicit("ForceHideTooltips"));
			if (val7 != null)
			{
				foreach (XElement item4 in ((XContainer)val7).Elements())
				{
					XAttribute val8 = item4.Attribute(XName.op_Implicit("Id"));
					switch (item4.Name.LocalName)
					{
					case "ItemTooltip":
						ItemsToShow.Remove(val8.Value);
						break;
					case "GlyphTooltip":
						GlyphsToShow.Remove(val8.Value);
						break;
					case "BuildingTooltip":
						BuildingsToShow.Remove(val8.Value);
						break;
					case "BuildingActionTooltip":
						BuildingActionsToShow.Remove(val8.Value);
						break;
					case "BuildingUpgradeTooltip":
						BuildingUpgradesToShow.Remove(val8.Value);
						break;
					}
				}
			}
			XElement val9 = ((XContainer)val).Element(XName.op_Implicit("Categories"));
			if (val9 != null)
			{
				XAttribute val10 = val9.Attribute(XName.op_Implicit("OverrideAutomaticCategories"));
				bool result = default(bool);
				if (val10 != null && bool.TryParse(val10.Value, out result) && result)
				{
					Category = E_MetaUpgradeCategory.None;
				}
				foreach (XElement item5 in ((XContainer)val9).Elements(XName.op_Implicit("Category")))
				{
					XAttribute val11 = item5.Attribute(XName.op_Implicit("Value"));
					if (Enum.TryParse<E_MetaUpgradeCategory>(val11.Value, out var result2))
					{
						Category |= result2;
					}
					else
					{
						CLoggerManager.Log((object)("Could not parse Category attribute into a meta upgrade category in meta upgrade " + Id + " : " + val11.Value), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
					}
				}
			}
			if (Category == E_MetaUpgradeCategory.None)
			{
				Category = E_MetaUpgradeCategory.Misc;
			}
			if (Category == E_MetaUpgradeCategory.Misc)
			{
				CLoggerManager.Log((object)("Meta upgrade doesn't have a category except Misc. This shouldn't happen ! (" + Id + ")."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		else
		{
			CLoggerManager.Log((object)("MetaUpgrade " + Id + " doesn't have UpgradeEffects element!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}

	public override string ToString()
	{
		string log = "<b>#--- " + Id + (Hidden ? "(Hidden)" : "") + " ---#</b>\n";
		log += "Unlock Conditions Groups :\n";
		for (int i = 0; i < UnlockConditionsDefinitions.Count; i++)
		{
			log += $"Group {i + 1}:\n";
			UnlockConditionsDefinitions[i].Conditions.ForEach(delegate(MetaConditionDefinition o)
			{
				log += $"- {o}\n";
			});
		}
		log += "Activation Conditions Groups :\n";
		for (int j = 0; j < ActivationConditionsDefinitions.Count; j++)
		{
			log += $"Group {j + 1}:\n";
			ActivationConditionsDefinitions[j].Conditions.ForEach(delegate(MetaConditionDefinition o)
			{
				log += $"- {o}\n";
			});
		}
		log += "Effects :\n";
		UpgradeEffectDefinitions.ForEach(delegate(MetaEffectDefinition o)
		{
			log += $"- {o}\n";
		});
		return log;
	}

	private void DeserializeConditions(XElement conditionsElement, List<ConditionsGroup> conditionsDefinitions)
	{
		int num = 0;
		int num2 = 0;
		foreach (XElement item in ((XContainer)conditionsElement).Elements(XName.op_Implicit("ConditionsGroup")))
		{
			bool flag = ((XContainer)item).Element(XName.op_Implicit("Hidden")) != null;
			if (!flag)
			{
				num2++;
			}
			bool checkOnce = ((XContainer)item).Element(XName.op_Implicit("CheckOnce")) != null;
			ConditionsGroup conditionsGroup = new ConditionsGroup
			{
				CheckOnce = checkOnce,
				GroupIndex = num
			};
			foreach (XElement item2 in ((XContainer)item).Elements())
			{
				if (!(item2.Name.LocalName == "Hidden") && !(item2.Name.LocalName == "CheckOnce"))
				{
					try
					{
						conditionsGroup.Conditions.Add(new MetaConditionDefinition((XContainer)(object)item2, flag, num));
					}
					catch (Exception arg)
					{
						CLoggerManager.Log((object)$"Caught and skipped invalid or obsolete condition definition in MetaUpgrade {Id}:\n{arg}", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
				}
			}
			conditionsDefinitions.Add(conditionsGroup);
			num++;
		}
		if (num2 >= 2)
		{
			CLoggerManager.Log((object)("More than one unlock/activation conditions groups are shown in MetaUpgrade " + Id + ". At most ONE must be visible."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
