using System;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Brazier;

public class GuardiansGroupsToSpawnPerNight : NightIndexedItem
{
	public GuardiansGroupsToSpawn GuardiansGroupsToSpawn;

	public override void Init(int nightIndex, XElement xElement)
	{
		base.Init(nightIndex, xElement);
		GuardiansGroupsToSpawn = new GuardiansGroupsToSpawn();
		foreach (XElement item2 in ((XContainer)xElement).Elements(XName.op_Implicit("GuardiansGroupToSpawn")))
		{
			XAttribute val = item2.Attribute(XName.op_Implicit("Id"));
			if (!BraziersDefinition.GuardiansGroups.TryGetValue(val.Value, out var value))
			{
				CLoggerManager.Log((object)("Guardians group " + val.Value + " could not be found in the database."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			XAttribute val2 = item2.Attribute(XName.op_Implicit("Weight"));
			int item = ((val2 == null) ? 1 : int.Parse(val2.Value));
			GuardiansGroupsToSpawn.Add(new Tuple<BraziersDefinition.GuardiansGroup, int>(value, item));
		}
	}
}
