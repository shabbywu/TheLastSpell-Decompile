using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class UnitPerkTemplateDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<int, int> RequiredPerksCountPerTier { get; private set; }

	public Dictionary<int, UnitPerkCollectionSetDefinition> UnitPerkCollectionSetDefinitions { get; private set; }

	public int TierCount { get; private set; }

	public UnitPerkTemplateDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement unitPerkTiersElement = obj.Element(XName.op_Implicit("UnitPerkTiers"));
		DeserializeUnitPerkTiers(unitPerkTiersElement);
		XElement unitPerkCollectionSetDefinitionsElement = obj.Element(XName.op_Implicit("UnitPerkCollectionSetDefinitions"));
		DeserializeUnitPerkCollectionSetDefinitions(unitPerkCollectionSetDefinitionsElement);
		TierCount = 0;
		foreach (KeyValuePair<int, int> item in RequiredPerksCountPerTier)
		{
			if (item.Key > TierCount)
			{
				TierCount = item.Key;
			}
		}
	}

	private void DeserializeUnitPerkTiers(XElement unitPerkTiersElement)
	{
		RequiredPerksCountPerTier = new Dictionary<int, int>();
		foreach (XElement item in ((XContainer)unitPerkTiersElement).Elements(XName.op_Implicit("UnitPerkTier")))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Index"));
			if (!int.TryParse(val.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse Index attribute into an int : \"" + val.Value + "\". Skip."), (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
				continue;
			}
			XElement val2 = ((XContainer)item).Element(XName.op_Implicit("RequiredPerksCount"));
			if (!int.TryParse(val2.Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse RequiredPerksCount element into an int : \"" + val2.Value + "\". Skip."), (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
			}
			else if (RequiredPerksCountPerTier.ContainsKey(result))
			{
				CLoggerManager.Log((object)$"Tried to add the same tier index ({result}) several times in UnitPerkTiers element. Skip.", (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
			}
			else
			{
				RequiredPerksCountPerTier.Add(result, result2);
			}
		}
	}

	private void DeserializeUnitPerkCollectionSetDefinitions(XElement unitPerkCollectionSetDefinitionsElement)
	{
		UnitPerkCollectionSetDefinitions = new Dictionary<int, UnitPerkCollectionSetDefinition>();
		foreach (XElement item in ((XContainer)unitPerkCollectionSetDefinitionsElement).Elements(XName.op_Implicit("UnitPerkCollectionSetDefinition")))
		{
			UnitPerkCollectionSetDefinition unitPerkCollectionSetDefinition = new UnitPerkCollectionSetDefinition((XContainer)(object)item);
			if (UnitPerkCollectionSetDefinitions.ContainsKey(unitPerkCollectionSetDefinition.Index))
			{
				CLoggerManager.Log((object)$"Tried to add the same tier index ({unitPerkCollectionSetDefinition.Index}) several times in UnitPerkCollectionSetDefinitions element. Skip.", (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
			}
			else
			{
				UnitPerkCollectionSetDefinitions[unitPerkCollectionSetDefinition.Index] = unitPerkCollectionSetDefinition;
			}
		}
	}
}
