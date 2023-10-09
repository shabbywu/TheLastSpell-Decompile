using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class UnitPerkCollectionSetDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int Index { get; private set; }

	public HashSet<Tuple<UnitPerkCollectionDefinition, int>> CollectionsPerWeight { get; private set; }

	public UnitPerkCollectionSetDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		if (int.TryParse(((XElement)obj).Attribute(XName.op_Implicit("Index")).Value, out var result))
		{
			Index = result - 1;
		}
		else
		{
			CLoggerManager.Log((object)"Index attribute could not be parsed as an int UnitPerkCollectionSetDefinition", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
		}
		CollectionsPerWeight = new HashSet<Tuple<UnitPerkCollectionDefinition, int>>();
		foreach (XElement item in obj.Elements(XName.op_Implicit("UnitPerkCollectionDefinition")))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Id"));
			if (!PlayableUnitDatabase.UnitPerkCollectionDefinitions.TryGetValue(val.Value, out var value))
			{
				CLoggerManager.Log((object)("Could not find the perk collection \"" + val.Value + "\" in the database. Skip."), (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
				continue;
			}
			XAttribute val2 = item.Attribute(XName.op_Implicit("Weight"));
			if (!int.TryParse(val2.Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse the Weight element into an int : \"" + val2.Value + "\". Skip."), (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (LogType)0, (CLogLevel)2, true, "PlayableUnitManager", false);
			}
			else
			{
				CollectionsPerWeight.Add(new Tuple<UnitPerkCollectionDefinition, int>(value, result2));
			}
		}
	}
}
