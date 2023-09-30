using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.PlayableUnitGeneration;

public class PlayableUnitGenerationDefinition : Definition
{
	public string ArchetypeId { get; private set; }

	public List<string> BackgroundTraitAvailableIds { get; private set; }

	public Dictionary<ItemSlotDefinition.E_ItemSlotId, EquipmentGenerationDefinition> EquipmentGenerationDefinitions { get; private set; }

	public Dictionary<UnitStatDefinition.E_Stat, StatGenerationDefinition> StatGenerationDefinitions { get; private set; }

	public int BaseGenerationLevel { get; set; }

	public PlayableUnitGenerationDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("ArchetypeId"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			TPDebug.LogError((object)"PlayableUnitGenerationDefinition must have an attribute ArchetypeId", (Object)null);
			return;
		}
		ArchetypeId = val2.Value;
		StatGenerationDefinitions = new Dictionary<UnitStatDefinition.E_Stat, StatGenerationDefinition>(UnitStatDefinition.SharedStatComparer);
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("StatGenerationDefinitions"))).Elements(XName.op_Implicit("StatGenerationDefinition")))
		{
			StatGenerationDefinition statGenerationDefinition = new StatGenerationDefinition((XContainer)(object)item, ArchetypeId);
			StatGenerationDefinitions.Add(statGenerationDefinition.Stat, statGenerationDefinition);
		}
		EquipmentGenerationDefinitions = new Dictionary<ItemSlotDefinition.E_ItemSlotId, EquipmentGenerationDefinition>();
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("EquipmentGenerationDefinitions"));
		foreach (XElement item2 in ((XContainer)val3).Elements(XName.op_Implicit("EquipmentGenerationDefinition")))
		{
			EquipmentGenerationDefinition equipmentGenerationDefinition = new EquipmentGenerationDefinition((XContainer)(object)item2);
			EquipmentGenerationDefinitions.Add(equipmentGenerationDefinition.Slot, equipmentGenerationDefinition);
		}
		if (int.TryParse(((XContainer)val3).Element(XName.op_Implicit("BaseGenerationLevel")).Value, out var result))
		{
			BaseGenerationLevel = result;
		}
		else
		{
			Debug.LogError((object)"Could not parse InitialGenerationLevel value, setting it to 0!");
			BaseGenerationLevel = 0;
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("UnitTraitDefinitions"));
		if (val4 == null)
		{
			TPDebug.LogError((object)"PlayableUnitGenerationDefinition must have an element UnitTraitDefinitions", (Object)null);
			return;
		}
		BackgroundTraitAvailableIds = new List<string>();
		foreach (XElement item3 in ((XContainer)val4).Elements(XName.op_Implicit("UnitTraitDefinition")))
		{
			XAttribute val5 = item3.Attribute(XName.op_Implicit("Id"));
			if (XDocumentExtensions.IsNullOrEmpty(val5))
			{
				TPDebug.LogError((object)"PlayableUnitGenerationDefinition must have an attribute Id", (Object)null);
			}
			else
			{
				BackgroundTraitAvailableIds.Add(val5.Value);
			}
		}
	}
}
