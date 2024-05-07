using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class TransformBuildingDefinition : BuildingPassiveEffectDefinition
{
	public List<string> BuildingIds { get; private set; }

	public bool Instantaneous { get; private set; }

	public bool PlayDestructionSmoke { get; private set; }

	public TransformBuildingDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		BuildingIds = new List<string>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("BuildingId")))
		{
			BuildingIds.Add(item.Value);
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("BuildingListId"));
		if (val2 != null)
		{
			if (GenericDatabase.IdsListDefinitions.TryGetValue(val2.Value, out var value))
			{
				BuildingIds.AddRange(value.Ids);
			}
			else
			{
				CLoggerManager.Log((object)("Could not find a building id list with id: " + val2.Value), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Instantaneous"));
		if (val3 != null)
		{
			if (!bool.TryParse(val3.Value, out var result))
			{
				CLoggerManager.Log((object)"Could not parse TransformBuildingDefinition Instantaneous value to a valid bool.", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				Instantaneous = false;
			}
			else
			{
				Instantaneous = result;
			}
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("PlayDestructionSmoke"));
		if (val4 != null)
		{
			if (!bool.TryParse(val4.Value, out var result2))
			{
				CLoggerManager.Log((object)"Could not parse TransformBuildingDefinition PlayDestructionSmoke value to a valid bool.", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				PlayDestructionSmoke = false;
			}
			else
			{
				PlayDestructionSmoke = result2;
			}
		}
	}

	public string GetRandomBuildingId()
	{
		int randomRange = RandomManager.GetRandomRange(this, 0, BuildingIds.Count);
		return BuildingIds[randomRange];
	}
}
