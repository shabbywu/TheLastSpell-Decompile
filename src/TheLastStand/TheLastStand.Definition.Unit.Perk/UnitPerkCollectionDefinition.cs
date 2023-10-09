using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class UnitPerkCollectionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public bool MultipleAllowed { get; private set; }

	public Dictionary<int, List<Tuple<PerkDefinition, int>>> PerksFromTier { get; private set; }

	public UnitPerkCollectionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("MultipleAllowed"));
		if (bool.TryParse(val3.Value, out var result))
		{
			MultipleAllowed = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse MultipleAllowed attribute into an int in Perk Collection \"" + Id + "\" : \"" + val3.Value + "\"."), (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
		}
		PerksFromTier = new Dictionary<int, List<Tuple<PerkDefinition, int>>>();
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("UnitPerkTierDefinition")))
		{
			XAttribute val4 = item2.Attribute(XName.op_Implicit("Tier"));
			if (!int.TryParse(val4.Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse Tier attribute into an int in Perk Collection \"" + Id + "\" : \"" + val4.Value + "\". Skip."), (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
				continue;
			}
			if (PerksFromTier.ContainsKey(result2))
			{
				CLoggerManager.Log((object)$"Tier \"{result2}\" already exists in Perk Collection \"{Id}\". Skip.", (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
				continue;
			}
			List<Tuple<PerkDefinition, int>> list = new List<Tuple<PerkDefinition, int>>();
			foreach (XElement item3 in ((XContainer)item2).Elements(XName.op_Implicit("UnitPerkDefinition")))
			{
				XAttribute val5 = item3.Attribute(XName.op_Implicit("Id"));
				if (!PlayableUnitDatabase.PerkDefinitions.TryGetValue(val5.Value, out var value))
				{
					CLoggerManager.Log((object)("Perk Id \"" + val5.Value + "\" in Perk Collection \"" + Id + "\" doesn't exist in the database. Skip."), (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
					continue;
				}
				int item = 1;
				XAttribute val6 = item3.Attribute(XName.op_Implicit("Weight"));
				if (val6 != null)
				{
					if (!int.TryParse(val6.Value, out var result3))
					{
						CLoggerManager.Log((object)("Weight attribute couldn't be parsed into an int for perk \"" + val5.Value + "\" in Perk Collection \"" + Id + "\". Set it to 1."), (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
					}
					else
					{
						item = result3;
					}
				}
				list.Add(new Tuple<PerkDefinition, int>(value, item));
			}
			PerksFromTier.Add(result2, list);
		}
	}
}
