using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class ItemDefinition : TheLastStand.Framework.Serialization.Definition
{
	[Flags]
	public enum E_Category
	{
		None = 0,
		MeleeWeapon = 1,
		RangeWeapon = 2,
		MagicWeapon = 4,
		Shield = 8,
		ClothBodyArmor = 0x10,
		LightBodyArmor = 0x20,
		MediumBodyArmor = 0x40,
		HeavyBodyArmor = 0x80,
		ClothHelm = 0x100,
		LightHelm = 0x200,
		MediumHelm = 0x400,
		HeavyHelm = 0x800,
		ClothBoots = 0x1000,
		LightBoots = 0x2000,
		MediumBoots = 0x4000,
		HeavyBoots = 0x8000,
		Trinket = 0x10000,
		Utility = 0x20000,
		Potion = 0x40000,
		Scroll = 0x80000,
		Usable = 0xC0000,
		Weapon = 7,
		Helm = 0xF00,
		Boots = 0xF000,
		BodyArmor = 0xF0,
		Armor = 0xFFF0,
		Equipment = 0x1FFF8,
		OffHand = 0x20008,
		All = 0xFFFFF
	}

	public enum E_Hands
	{
		None,
		OneHand,
		TwoHands,
		OffHand
	}

	public enum E_Rarity
	{
		None,
		Common,
		Magic,
		Rare,
		Epic
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CategoryComparer : IEqualityComparer<E_Category>
	{
		public bool Equals(E_Category x, E_Category y)
		{
			return x == y;
		}

		public int GetHashCode(E_Category obj)
		{
			return (int)obj;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct RarityComparer : IEqualityComparer<E_Rarity>
	{
		public bool Equals(E_Rarity x, E_Rarity y)
		{
			return x == y;
		}

		public int GetHashCode(E_Rarity obj)
		{
			return (int)obj;
		}
	}

	public static readonly CategoryComparer SharedCategoryComparer;

	public static readonly RarityComparer SharedRarityComparer;

	private string artId = string.Empty;

	private List<int> definedLevels;

	public string ArtId
	{
		get
		{
			if (!(artId != string.Empty))
			{
				return Id;
			}
			return artId;
		}
	}

	public Dictionary<int, Vector2> BaseDamageByLevel { get; } = new Dictionary<int, Vector2>();


	public string BaseName => Localizer.Get("ItemName_" + Id);

	public Dictionary<int, float> BasePriceByLevel { get; } = new Dictionary<int, float>();


	public Dictionary<int, Dictionary<UnitStatDefinition.E_Stat, float>> BaseStatBonusesByLevel { get; } = new Dictionary<int, Dictionary<UnitStatDefinition.E_Stat, float>>();


	public Dictionary<string, BodyPartDefinition> BodyPartsDefinitions { get; private set; }

	public E_Category Category { get; private set; }

	public string CategoryName => Category.GetLocalizedName();

	public E_Hands Hands { get; private set; }

	public string HandsName => Localizer.Get(string.Format("{0}{1}", "HandsName_", Hands));

	public string Id { get; private set; }

	public bool IsWeapon
	{
		get
		{
			if (Category != E_Category.MagicWeapon && Category != E_Category.MeleeWeapon)
			{
				return Category == E_Category.RangeWeapon;
			}
			return true;
		}
	}

	public bool IsHandItem => Hands != E_Hands.None;

	public Dictionary<int, Tuple<UnitStatDefinition.E_Stat, float>> MainStatBonusByLevel { get; } = new Dictionary<int, Tuple<UnitStatDefinition.E_Stat, float>>();


	public Vector2Int Resistance { get; private set; }

	public Dictionary<int, Dictionary<string, int>> SkillsByLevel { get; } = new Dictionary<int, Dictionary<string, int>>();


	public ItemDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"An item hasn't an Id !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = val2.Value;
		artId = Id;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Category"));
		if (val3 != null)
		{
			if (Enum.TryParse<E_Category>(val3.Value, out var result))
			{
				Category = result;
				XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Tags"));
				if (val4 != null)
				{
					foreach (XElement item in ((XContainer)val4).Elements(XName.op_Implicit("Tag")))
					{
						string value = item.Value;
						if (ItemDatabase.ItemsByTag.ContainsKey(value))
						{
							ItemDatabase.ItemsByTag[value].Add(Id);
							continue;
						}
						ItemDatabase.ItemsByTag.Add(value, new List<string> { Id });
					}
				}
				XElement val5 = ((XContainer)val).Element(XName.op_Implicit("Hands"));
				if (val5 != null)
				{
					if (!Enum.TryParse<E_Hands>(val5.Value, out var result2))
					{
						CLoggerManager.Log((object)("Item " + Id + "'s Hands " + HasAnInvalid("E_Hands", val5.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
						return;
					}
					Hands = result2;
				}
				XElement val6 = ((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.Resistance.ToString()));
				if (val6 != null)
				{
					Resistance = val6.ParseMinMax();
				}
				XElement obj = ((XContainer)val).Element(XName.op_Implicit("LevelVariations"));
				Vector2 value2 = Vector2.zero;
				float value3 = -1f;
				Dictionary<UnitStatDefinition.E_Stat, float> value4 = null;
				Tuple<UnitStatDefinition.E_Stat, float> value5 = null;
				Dictionary<string, int> value6 = null;
				definedLevels = new List<int>();
				{
					Vector2 val11 = default(Vector2);
					foreach (XElement item2 in ((XContainer)obj).Elements(XName.op_Implicit("Level")))
					{
						XAttribute val7 = item2.Attribute(XName.op_Implicit("Id"));
						if (!int.TryParse(val7.Value, out var result3))
						{
							CLoggerManager.Log((object)("Item " + Id + "'s Level " + HasAnInvalidInt(val7.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
							continue;
						}
						definedLevels.Add(result3);
						XElement val8 = ((XContainer)item2).Element(XName.op_Implicit("BaseDamage"));
						if (val8 != null)
						{
							XAttribute val9 = val8.Attribute(XName.op_Implicit("Min"));
							if (val9.IsNullOrEmpty())
							{
								CLoggerManager.Log((object)("The BaseDamage " + OfTheItem(Id, result3) + " hasn't a Min !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								break;
							}
							if (!int.TryParse(val9.Value, out var result4))
							{
								CLoggerManager.Log((object)$"Item {Id}(Level : {result3})'s BaseDamage Min {HasAnInvalidInt(val9.Value)}", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								break;
							}
							XAttribute val10 = val8.Attribute(XName.op_Implicit("Max"));
							if (val10.IsNullOrEmpty())
							{
								CLoggerManager.Log((object)("The BaseDamage " + OfTheItem(Id, result3) + " hasn't a Max !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								break;
							}
							if (!int.TryParse(val10.Value, out var result5))
							{
								CLoggerManager.Log((object)$"Item {Id}(Level : {result3})'s BaseDamage Max {HasAnInvalidInt(val10.Value)}", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								break;
							}
							((Vector2)(ref val11))._002Ector((float)result4, (float)result5);
							value2 = val11;
							BaseDamageByLevel.Add(result3, val11);
						}
						else
						{
							BaseDamageByLevel.Add(result3, value2);
						}
						XElement val12 = ((XContainer)item2).Element(XName.op_Implicit("BasePrice"));
						if (val12 != null)
						{
							if (!float.TryParse(val12.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result6))
							{
								CLoggerManager.Log((object)$"Item {Id}(Level : {result3})'s Price {HasAnInvalidFloat(val12.Value)}", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								break;
							}
							value3 = result6;
							BasePriceByLevel.Add(result3, result6);
						}
						else
						{
							BasePriceByLevel.Add(result3, value3);
						}
						BaseStatBonusesByLevel.Add(result3, new Dictionary<UnitStatDefinition.E_Stat, float>());
						XElement val13 = ((XContainer)item2).Element(XName.op_Implicit("BaseStatBonuses"));
						if (val13 != null)
						{
							foreach (XElement item3 in ((XContainer)val13).Elements(XName.op_Implicit("BaseStatBonus")))
							{
								XAttribute val14 = item3.Attribute(XName.op_Implicit("Stat"));
								UnitStatDefinition.E_Stat result7;
								float result8;
								if (val14.IsNullOrEmpty())
								{
									CLoggerManager.Log((object)("A BaseStatBonus " + OfTheItem(Id, result3) + " hasn't a Stat!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								}
								else if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val14.Value, out result7))
								{
									CLoggerManager.Log((object)("A BaseStatBonus " + OfTheItem(Id, result3) + " " + HasAnInvalidStat(val14.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								}
								else if (item3.IsNullOrEmpty())
								{
									CLoggerManager.Log((object)("A BaseStatBonus (" + result7.ToString() + ") " + OfTheItem(Id, result3) + " is empty !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								}
								else if (!float.TryParse(item3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result8))
								{
									CLoggerManager.Log((object)("A BaseStatBonus (" + result7.ToString() + ") " + OfTheItem(Id, result3) + " " + HasAnInvalidFloat(item3.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								}
								else
								{
									BaseStatBonusesByLevel[result3].Add(result7, result8);
								}
							}
							value4 = BaseStatBonusesByLevel[result3];
						}
						else
						{
							BaseStatBonusesByLevel[result3] = value4;
						}
						XElement val15 = ((XContainer)item2).Element(XName.op_Implicit("MainStatBonus"));
						if (!val15.IsNullOrEmpty())
						{
							XAttribute val16 = val15.Attribute(XName.op_Implicit("Stat"));
							if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val16.Value, out var result9))
							{
								CLoggerManager.Log((object)("The MainStatBonus " + OfTheItem(Id, result3) + " " + HasAnInvalidStat(val16.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
							}
							if (!float.TryParse(val15.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result10))
							{
								CLoggerManager.Log((object)("The MainStatBonus " + OfTheItem(Id, result3) + " " + HasAnInvalidFloat(val15.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								break;
							}
							MainStatBonusByLevel.Add(result3, new Tuple<UnitStatDefinition.E_Stat, float>(result9, result10));
							value5 = MainStatBonusByLevel[result3];
						}
						else
						{
							MainStatBonusByLevel.Add(result3, value5);
						}
						SkillsByLevel.Add(result3, null);
						XElement val17 = ((XContainer)item2).Element(XName.op_Implicit("Skills"));
						if (val17 != null)
						{
							SkillsByLevel[result3] = new Dictionary<string, int>();
							foreach (XElement item4 in ((XContainer)val17).Elements())
							{
								if (item4.IsNullOrEmpty())
								{
									CLoggerManager.Log((object)("A skill " + OfTheItem(Id, result3) + " is Empty !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
									continue;
								}
								int result11 = -1;
								XAttribute val18 = item4.Attribute(XName.op_Implicit("OverallUsesCount"));
								if (val18 != null && !int.TryParse(val18.Value, out result11))
								{
									CLoggerManager.Log((object)("The skill " + item4.Value + " " + OfTheItem(Id, result3) + " " + HasAnInvalidInt(val18.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
								}
								else
								{
									SkillsByLevel[result3].Add(item4.Value, result11);
								}
							}
							value6 = SkillsByLevel[result3];
						}
						else
						{
							SkillsByLevel[result3] = value6;
						}
					}
					return;
				}
			}
			CLoggerManager.Log((object)("Item " + Id + "'s Category " + HasAnInvalid("E_Category", val3.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			CLoggerManager.Log((object)("Item " + Id + " must have a Category"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}

	public void DeserializeArtRelatedDatas(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("ArtId"));
		if (!val.IsNullOrEmpty())
		{
			artId = val.Value;
		}
		else
		{
			artId = Id;
		}
		XElement val2 = obj.Element(XName.op_Implicit("BodyParts"));
		if (val2 == null)
		{
			return;
		}
		BodyPartsDefinitions = new Dictionary<string, BodyPartDefinition>();
		foreach (XElement item in ((XContainer)val2).Elements(XName.op_Implicit("BodyPartDefinition")))
		{
			BodyPartDefinition bodyPartDefinition = new BodyPartDefinition((XContainer)(object)item);
			BodyPartsDefinitions.Add(bodyPartDefinition.Id, bodyPartDefinition);
		}
	}

	public int GetHigherExistingLevelFromInitValue(int level)
	{
		while (level > -1)
		{
			if (definedLevels.Contains(level))
			{
				return level;
			}
			level--;
		}
		return -1;
	}

	public int GetLowerExistingLevelFromInitValue(int level)
	{
		while (level < 999)
		{
			if (definedLevels.Contains(level))
			{
				return level;
			}
			level++;
		}
		return -1;
	}

	public bool HasTag(string tag)
	{
		if (ItemDatabase.ItemsByTag.TryGetValue(tag, out var value))
		{
			return value.Contains(Id);
		}
		return false;
	}
}
