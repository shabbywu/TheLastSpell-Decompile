using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class AffixDefinition : Definition
{
	public class LeveledAffixDefinition : Definition
	{
		public AffixDefinition AffixDefinition { get; private set; }

		public int Level { get; private set; }

		public Dictionary<UnitStatDefinition.E_Stat, float> StatModifiers { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, float>(UnitStatDefinition.SharedStatComparer);


		public LeveledAffixDefinition(AffixDefinition affixDefinition, XContainer container)
			: base(container, (Dictionary<string, string>)null)
		{
			AffixDefinition = affixDefinition;
		}

		public override void Deserialize(XContainer container)
		{
			XElement val = (XElement)(object)((container is XElement) ? container : null);
			XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
			if (XDocumentExtensions.IsNullOrEmpty(val2))
			{
				Debug.LogError((object)"The Level has no Id!");
				return;
			}
			if (!int.TryParse(val2.Value, out var result) || result < 1 || result > 10)
			{
				Debug.LogError((object)("The Level (" + val2.Value + ") is invalid!"));
				return;
			}
			Level = result;
			foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Modifier")))
			{
				if (XDocumentExtensions.IsNullOrEmpty(item))
				{
					Debug.LogError((object)"The Modifier is empty!");
					continue;
				}
				XAttribute val3 = item.Attribute(XName.op_Implicit("Stat"));
				if (XDocumentExtensions.IsNullOrEmpty(val3))
				{
					Debug.LogError((object)"The Modifier has no Stat!");
				}
				else
				{
					StatModifiers.Add((UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), val3.Value), float.Parse(item.Value, NumberStyles.Float, CultureInfo.InvariantCulture));
				}
			}
		}
	}

	public bool Droppable { get; private set; } = true;


	public Dictionary<UnitStatDefinition.E_Stat, float> EpicStatModifiers { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, float>(UnitStatDefinition.SharedStatComparer);


	public string Id { get; private set; }

	public Dictionary<ItemDefinition.E_Category, float> ItemCategoriesWithWeight { get; private set; } = new Dictionary<ItemDefinition.E_Category, float>(ItemDefinition.SharedCategoryComparer);


	public Dictionary<int, LeveledAffixDefinition> LevelDefinitions { get; private set; } = new Dictionary<int, LeveledAffixDefinition>();


	public int LevelMax { get; private set; }

	public int LevelMin { get; private set; }

	public int MaxOccurrences { get; private set; } = -1;


	public float TotalCategoryWeight { get; private set; }

	public AffixDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			Debug.LogError((object)"The AffixDefinition has no Id!");
			return;
		}
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("MaxOccurrences"));
		if (val3 != null)
		{
			if (int.TryParse(val3.Value, out var result))
			{
				MaxOccurrences = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse MaxOccurrences attribute into an int : " + val3.Value + "."), (LogType)0, (CLogLevel)2, true, "AffixDefinition", false);
			}
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("Droppable"));
		if (val4 != null)
		{
			if (!bool.TryParse(val4.Value, out var result2))
			{
				Debug.LogError((object)("AffixDefinition " + Id + " has an invalid Droppable!"));
				return;
			}
			Droppable = result2;
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("ItemLevel"));
		if (val5 == null)
		{
			Debug.LogError((object)("The AffixDefinition " + Id + " has no ItemLevel!"));
			return;
		}
		XAttribute val6 = val5.Attribute(XName.op_Implicit("Min"));
		if (val6 == null)
		{
			Debug.LogError((object)("The AffixDefinition " + Id + " has no ItemLevel Min!"));
			return;
		}
		if (!int.TryParse(val6.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3) || result3 < 0)
		{
			Debug.LogError((object)("The AffixDefinition " + Id + " has an invalid ItemLevel Min " + val6.Value + "!"));
			return;
		}
		LevelMin = result3;
		XAttribute val7 = val5.Attribute(XName.op_Implicit("Max"));
		if (val7 == null)
		{
			Debug.LogError((object)("The AffixDefinition " + Id + " has no ItemLevel Max!"));
			return;
		}
		if (!int.TryParse(val7.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result4) || result4 < 0)
		{
			Debug.LogError((object)("The AffixDefinition " + Id + " has an invalid ItemLevel Max " + val7.Value + "!"));
			return;
		}
		LevelMax = result4;
		XElement val8 = ((XContainer)val).Element(XName.op_Implicit("ItemCategories"));
		if (val8 == null)
		{
			Debug.LogError((object)("The AffixDefinition " + Id + " has no ItemCategories!"));
			return;
		}
		foreach (XElement item in ((XContainer)val8).Elements(XName.op_Implicit("ItemCategory")))
		{
			if (XDocumentExtensions.IsNullOrEmpty(item))
			{
				Debug.LogError((object)("AffixDefinition " + Id + "'s ItemCategory is empty!"));
				continue;
			}
			if (!Enum.TryParse<ItemDefinition.E_Category>(item.Value, out var result5))
			{
				Debug.LogError((object)("AffixDefinition " + Id + "'s ItemCategory " + ((Definition)this).HasAnInvalid("E_Category", item.Value)));
				continue;
			}
			if (ItemCategoriesWithWeight.ContainsKey(result5))
			{
				Debug.LogError((object)$"The affix {Id} already contains an  ItemCategory {result5}!");
				continue;
			}
			XAttribute val9 = item.Attribute(XName.op_Implicit("Weight"));
			if (XDocumentExtensions.IsNullOrEmpty(val9))
			{
				TPDebug.Log((object)$"The affix {Id} must have a Weight to its ItemCategory {result5}", (Object)null);
				return;
			}
			if (!float.TryParse(val9.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result6))
			{
				TPDebug.Log((object)$"The affix {Id} must have a valid Weight (float) to its ItemCategory {result5}", (Object)null);
				return;
			}
			ItemCategoriesWithWeight.Add(result5, result6);
			TotalCategoryWeight += result6;
		}
		XElement val10 = ((XContainer)val).Element(XName.op_Implicit("Levels"));
		if (val10 == null)
		{
			Debug.LogError((object)("The AffixDefinition " + Id + " has no Levels!"));
			return;
		}
		foreach (XElement item2 in ((XContainer)val10).Elements(XName.op_Implicit("Level")))
		{
			LeveledAffixDefinition leveledAffixDefinition = new LeveledAffixDefinition(this, (XContainer)(object)item2);
			LevelDefinitions.Add(leveledAffixDefinition.Level, leveledAffixDefinition);
		}
		XElement val11 = ((XContainer)val).Element(XName.op_Implicit("EpicBonus"));
		if (XDocumentExtensions.IsNullOrEmpty(val11))
		{
			Debug.LogError((object)("The AffixDefinition " + Id + " has no EpicBonus!"));
			return;
		}
		foreach (XElement item3 in ((XContainer)val11).Elements(XName.op_Implicit("Modifier")))
		{
			if (XDocumentExtensions.IsNullOrEmpty(item3))
			{
				Debug.LogError((object)"The Modifier is empty!");
				continue;
			}
			XAttribute val12 = item3.Attribute(XName.op_Implicit("Stat"));
			if (XDocumentExtensions.IsNullOrEmpty(val12))
			{
				Debug.LogError((object)"The Modifier has no Stat!");
			}
			else
			{
				EpicStatModifiers.Add((UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), val12.Value), float.Parse(item3.Value, NumberStyles.Float, CultureInfo.InvariantCulture));
			}
		}
	}
}
