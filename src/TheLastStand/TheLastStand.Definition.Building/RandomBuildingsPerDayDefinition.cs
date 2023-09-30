using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Building;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class RandomBuildingsPerDayDefinition : Definition
{
	public string Id { get; private set; }

	public Dictionary<int, Dictionary<RandomBuildingsDirectionsDefinition, int>> RandomBuildingsPerDayDefinitions { get; } = new Dictionary<int, Dictionary<RandomBuildingsDirectionsDefinition, int>>();


	public RandomBuildingsPerDayDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		foreach (XElement item in obj.Elements(XName.op_Implicit("RandomBuildingsDirectionsDefinitions")))
		{
			XAttribute val2 = item.Attribute(XName.op_Implicit("DayNumber"));
			if (XDocumentExtensions.IsNullOrEmpty(val2))
			{
				CLoggerManager.Log((object)"RandomBuildingsDirectionsPerDayDefinition must have a DayNumber attribute!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (!int.TryParse(val2.Value, out var result))
			{
				CLoggerManager.Log((object)"DayNumber must be a valid int value!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			Dictionary<RandomBuildingsDirectionsDefinition, int> dictionary = new Dictionary<RandomBuildingsDirectionsDefinition, int>();
			foreach (XElement item2 in ((XContainer)item).Elements(XName.op_Implicit("RandomBuildingsDirectionsDefinition")))
			{
				XAttribute val3 = item2.Attribute(XName.op_Implicit("Id"));
				RandomBuildingsDirectionsDefinition key = BuildingDatabase.RandomBuildingsDirectionsDefinitions[val3.Value];
				XAttribute val4 = item2.Attribute(XName.op_Implicit("Weight"));
				int value = 1;
				if (val4 != null)
				{
					if (!int.TryParse(val4.Value, out var result2))
					{
						CLoggerManager.Log((object)("Could not parse RandomBuildingsDirectionsDefinition weight " + val4.Value + " to a valid int! Setting it to 100."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
					value = result2;
				}
				dictionary.Add(key, value);
			}
			RandomBuildingsPerDayDefinitions.Add(result, dictionary);
		}
	}
}
