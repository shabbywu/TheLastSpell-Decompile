using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.BonePile;

public class BonePileGeneratorsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<string, int> Buildings { get; } = new Dictionary<string, int>();


	public Dictionary<string, List<Tuple<int, int>>> BonePileEvolutionDefinitions { get; } = new Dictionary<string, List<Tuple<int, int>>>();


	public Dictionary<string, BonePileGeneratorDefinition> GeneratorsByZoneId { get; } = new Dictionary<string, BonePileGeneratorDefinition>();


	public Dictionary<string, BonePileCountProgressionDefinition> CountProgressionDefinitions { get; } = new Dictionary<string, BonePileCountProgressionDefinition>();


	public BonePileGeneratorsDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("BuildingIds"))).Elements(XName.op_Implicit("Building")))
		{
			XAttribute val2 = item.Attribute(XName.op_Implicit("Id"));
			XAttribute val3 = item.Attribute(XName.op_Implicit("MinPercentage"));
			if (!int.TryParse(val3.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse BonePile building " + val2.Value + " MinPercentage attribute value " + val3.Value + " to a valid int!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			Buildings.Add(val2.Value, result);
		}
		foreach (XElement item2 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("LevelEvolutions"))).Elements(XName.op_Implicit("LevelEvolution")))
		{
			XAttribute val4 = item2.Attribute(XName.op_Implicit("Id"));
			List<Tuple<int, int>> list = new List<Tuple<int, int>>();
			BonePileEvolutionDefinitions.Add(val4.Value, list);
			foreach (XElement item3 in ((XContainer)item2).Elements(XName.op_Implicit("Day")))
			{
				XAttribute val5 = item3.Attribute(XName.op_Implicit("Index"));
				if (!int.TryParse(val5.Value, out var result2))
				{
					CLoggerManager.Log((object)("Could not parse Day Index attribute value " + val5.Value + " to a valid int!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				XAttribute val6 = item3.Attribute(XName.op_Implicit("Level"));
				if (!int.TryParse(val6.Value, out var result3))
				{
					CLoggerManager.Log((object)("Could not parse Day Level attribute value " + val6.Value + " to a valid int!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				list.Add(new Tuple<int, int>(result2, result3));
			}
		}
		foreach (XElement item4 in ((XContainer)val).Elements(XName.op_Implicit("BonePileGeneratorDefinition")))
		{
			BonePileGeneratorDefinition bonePileGeneratorDefinition = new BonePileGeneratorDefinition((XContainer)(object)item4);
			GeneratorsByZoneId.Add(bonePileGeneratorDefinition.ZoneId, bonePileGeneratorDefinition);
		}
		foreach (XElement item5 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("BonePileCountProgressions"))).Elements(XName.op_Implicit("BonePileCountProgression")))
		{
			BonePileCountProgressionDefinition bonePileCountProgressionDefinition = new BonePileCountProgressionDefinition((XContainer)(object)item5);
			CountProgressionDefinitions.Add(bonePileCountProgressionDefinition.CityId, bonePileCountProgressionDefinition);
		}
		foreach (KeyValuePair<string, BonePileCountProgressionDefinition> countProgressionDefinition in CountProgressionDefinitions)
		{
			BonePileCountProgressionDefinition value = countProgressionDefinition.Value;
			if (value.HasTemplate)
			{
				if (!CountProgressionDefinitions.TryGetValue(value.TemplateCityId, out var value2))
				{
					CLoggerManager.Log((object)("No template definition with Id " + value.TemplateCityId + " was found for BonePileCountProgressionDefinition!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					value.DeserializeUsingTemplate(value2);
				}
			}
		}
	}
}
