using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.PlayableUnitGeneration;

public class EquipmentGenerationDefinition : Definition
{
	public class ItemGenerationData
	{
		public string ItemId { get; }

		public string ItemLevelModifiersList { get; }

		public string ItemRaritiesList { get; }

		public string ItemsList { get; }

		public ItemGenerationData(string itemId, string itemLevelModifiersList, string itemRaritiesList, string itemsList)
		{
			ItemId = itemId;
			ItemLevelModifiersList = itemLevelModifiersList;
			ItemRaritiesList = itemRaritiesList;
			ItemsList = itemsList;
		}
	}

	public List<Tuple<int, ItemGenerationData>> ItemsPerWeight { get; private set; } = new List<Tuple<int, ItemGenerationData>>();


	public ItemSlotDefinition.E_ItemSlotId Slot { get; private set; }

	public int TotalWeight { get; private set; }

	public EquipmentGenerationDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Slot"));
		if (!Enum.TryParse<ItemSlotDefinition.E_ItemSlotId>(val.Value, out var result))
		{
			CLoggerManager.Log((object)("An EquipmentGenerationDefinition has an invalid Id " + val.Value + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		Slot = result;
		foreach (XElement item2 in obj.Elements(XName.op_Implicit("Items")))
		{
			int num = int.Parse(item2.Attribute(XName.op_Implicit("Weight")).Value);
			TotalWeight += num;
			XElement obj2 = ((XContainer)item2).Element(XName.op_Implicit("ItemsList"));
			XAttribute val2 = ((obj2 != null) ? obj2.Attribute(XName.op_Implicit("Id")) : null);
			if (val2 != null)
			{
				XAttribute val3 = item2.Attribute(XName.op_Implicit("Id"));
				XElement obj3 = ((XContainer)item2).Element(XName.op_Implicit("ItemLevelModifiersList"));
				XAttribute val4 = ((obj3 != null) ? obj3.Attribute(XName.op_Implicit("Id")) : null);
				XElement obj4 = ((XContainer)item2).Element(XName.op_Implicit("ItemRaritiesList"));
				XAttribute val5 = ((obj4 != null) ? obj4.Attribute(XName.op_Implicit("Id")) : null);
				ItemGenerationData item = new ItemGenerationData(((val3 != null) ? val3.Value : null) ?? string.Empty, (val4 != null) ? val4.Value : null, (val5 != null) ? val5.Value : null, (val2 != null) ? val2.Value : null);
				ItemsPerWeight.Add(new Tuple<int, ItemGenerationData>(num, item));
			}
		}
	}
}
