using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Apocalypse.LightFogSpawner;

public class LightFogSpawnersGenerationDefinition : Definition
{
	public int InitialCount { get; private set; }

	public int ScalingCount { get; private set; }

	public int Period { get; private set; }

	public List<Tuple<int, string>> BuildingToSpawnIds { get; private set; }

	public LightFogSpawnersGenerationDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			InitialCount = 0;
			ScalingCount = 0;
			Period = 1;
			return;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("InitialCount"));
		XAttribute val3 = val2.Attribute(XName.op_Implicit("Value"));
		int.TryParse(val3.Value, out var result);
		InitialCount = result;
		val2 = ((XContainer)val).Element(XName.op_Implicit("ScalingCount"));
		val3 = val2.Attribute(XName.op_Implicit("Value"));
		int.TryParse(val3.Value, out var result2);
		ScalingCount = result2;
		val3 = val2.Attribute(XName.op_Implicit("Period"));
		if (!int.TryParse(val3.Value, out var result3))
		{
			result3 = 1;
		}
		Period = result3;
		BuildingToSpawnIds = new List<Tuple<int, string>>
		{
			new Tuple<int, string>(0, "LightFogSpawner_Alive")
		};
		val2 = ((XContainer)val).Element(XName.op_Implicit("LightFogSpawnersToSpawn"));
		if (val2 == null)
		{
			return;
		}
		foreach (XElement item in ((XContainer)val2).Elements(XName.op_Implicit("LightFogSpawnerToSpawn")))
		{
			val3 = item.Attribute(XName.op_Implicit("DayIndex"));
			int.TryParse(val3.Value, out var result4);
			val3 = item.Attribute(XName.op_Implicit("BuildingId"));
			if (result4 == 0 && BuildingToSpawnIds.Count > 0)
			{
				BuildingToSpawnIds.RemoveAt(0);
			}
			BuildingToSpawnIds.Add(new Tuple<int, string>(result4, val3.Value));
		}
	}
}
