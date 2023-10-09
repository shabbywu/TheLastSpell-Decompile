using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class ItemSlotDefinition : TheLastStand.Framework.Serialization.Definition
{
	[Flags]
	public enum E_ItemSlotId
	{
		None = 0,
		RightHand = 1,
		LeftHand = 2,
		Body = 4,
		Foot = 8,
		Head = 0x10,
		Trinket = 0x20,
		Usables = 0x40,
		Inventory = 0x80,
		Shop = 0x100,
		RewardItem = 0x200,
		WeaponSlot = 3,
		ArmorSlot = 0x3C,
		EquipmentSlot = 0x7F
	}

	public static class Constants
	{
		public static class Ids
		{
			public const string NoSlot = "NoSlot";

			public const string Body = "Body";

			public const string Foot = "Foot";

			public const string Free = "Free";

			public const string Head = "Head";

			public const string LeftHand = "LeftHand";

			public const string RightHand = "RightHand";

			public const string Trinket = "Trinket";

			public const string Inventory = "Inventory";

			public const string Shop = "Shop";

			public const string RewardItem = "RewardItem";
		}
	}

	public E_ItemSlotId Id { get; private set; }

	public ItemDefinition.E_Category Categories { get; private set; }

	public List<ItemDefinition.E_Hands> Hands { get; private set; } = new List<ItemDefinition.E_Hands>();


	public ItemSlotDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			Debug.LogError((object)"The ItemSlotDefinition has no Id!");
			return;
		}
		if (!Enum.TryParse<E_ItemSlotId>(val2.Value, out var result))
		{
			Debug.LogError((object)("An ItemSlotDefinition has an invalid Id " + val2.Value + "!"));
		}
		Id = result;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Categories"));
		if (val3.IsNullOrEmpty())
		{
			Debug.LogError((object)$"The ItemSlotDefinition {Id} must have a Categories element!");
			return;
		}
		foreach (XElement item in ((XContainer)val3).Elements(XName.op_Implicit("Category")))
		{
			if (!Enum.TryParse<ItemDefinition.E_Category>(item.Value, out var result2))
			{
				Debug.LogError((object)$"The ItemSlotDefinition {Id} has an invalid category {item.Value}!");
			}
			else if (Categories.HasFlag(result2))
			{
				Debug.LogError((object)$"The ItemSlotDefinition {Id} already has the category {result2}!");
			}
			else
			{
				Categories |= result2;
			}
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Hands"));
		if (val4.IsNullOrEmpty())
		{
			Hands.Add(ItemDefinition.E_Hands.None);
			return;
		}
		foreach (XElement item2 in ((XContainer)val4).Elements(XName.op_Implicit("Hand")))
		{
			if (!Enum.TryParse<ItemDefinition.E_Hands>(item2.Value, out var result3))
			{
				Debug.LogError((object)$"The ItemSlotDefinition {Id} has an invalid hand {item2.Value}!");
			}
			else if (Hands.Contains(result3))
			{
				Debug.LogError((object)$"The ItemSlotDefinition {Id} already has the hand {result3}!");
			}
			else
			{
				Hands.Add(result3);
			}
		}
	}
}
