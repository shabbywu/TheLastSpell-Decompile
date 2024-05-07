using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item.ItemRestriction;

public class ItemRestrictionCategoryDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int BoundlessMinimumSelectedNb { get; private set; }

	public ItemDefinition.E_Category ItemCategory { get; private set; }

	public string ItemFamiliesListId { get; private set; }

	public int MinimumSelectedNb { get; private set; }

	public ItemRestrictionCategoryDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("ItemFamiliesListId"));
		if (val2.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"An ItemRestrictionCategoryDefinition doesn't have a list of items Id !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		ItemFamiliesListId = val2.Value;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("BoundlessMinimumSelectedNb"));
		if (val3 != null)
		{
			if (!int.TryParse(val3.Value, out var result))
			{
				CLoggerManager.Log((object)("The ItemRestrictionCategoryDefinition with list " + ItemFamiliesListId + " BoundlessMinimumSelectedNb " + HasAnInvalidInt(val3.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			BoundlessMinimumSelectedNb = result;
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("ItemCategory"));
		if (val4 != null)
		{
			if (!Enum.TryParse<ItemDefinition.E_Category>(val4.Value, out var result2))
			{
				CLoggerManager.Log((object)("ItemRestrictionCategoryDefinition with list " + ItemFamiliesListId + ", ItemCategory " + HasAnInvalid("E_Category", val4.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			ItemCategory = result2;
			if (ItemDatabase.ItemsListDefinitions.TryGetValue(ItemFamiliesListId, out var value))
			{
				foreach (KeyValuePair<string, int> item in value.ItemsWithOdd)
				{
					if (ItemDatabase.ItemsListDefinitions.TryGetValue(item.Key, out var value2))
					{
						ItemRestrictionFamilyDefinition itemRestrictionFamilyDefinition = new ItemRestrictionFamilyDefinition(value2.Id, ItemCategory);
						ItemDatabase.ItemRestrictionFamiliesDefinitions.Add(itemRestrictionFamilyDefinition.ItemsListId, itemRestrictionFamilyDefinition);
					}
				}
			}
			XElement val5 = ((XContainer)val).Element(XName.op_Implicit("MinimumSelectedNb"));
			if (val5 != null)
			{
				if (!int.TryParse(val5.Value, out var result3))
				{
					CLoggerManager.Log((object)("The ItemRestrictionCategoryDefinition with list " + ItemFamiliesListId + " MinimumSelectedNb " + HasAnInvalidInt(val5.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					MinimumSelectedNb = result3;
				}
			}
		}
		else
		{
			CLoggerManager.Log((object)("ItemRestrictionCategoryDefinition with list " + ItemFamiliesListId + " must have a Category"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
