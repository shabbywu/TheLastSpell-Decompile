using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class RandomBuildingsGenerationDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string AllowingRandomBuildingsIdsList = "AllowingRandomBuildings";
	}

	public struct BuildingInfo
	{
		public string Id;

		public TileFlagDefinition.E_TileFlagTag TileFlag;

		public int Count;
	}

	public string Id { get; private set; }

	public List<BuildingInfo> BuildingsInfo { get; } = new List<BuildingInfo>();


	public RandomBuildingsGenerationDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		foreach (XElement item2 in obj.Elements(XName.op_Implicit("Building")))
		{
			XAttribute val2 = item2.Attribute(XName.op_Implicit("Id"));
			XAttribute val3 = item2.Attribute(XName.op_Implicit("TileFlag"));
			if (!Enum.TryParse<TileFlagDefinition.E_TileFlagTag>(val3.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse TileFlag attribute value " + val3.Value + " of Building " + val2.Value + " in RandomBuildingsGenerationDefinition " + Id + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			XAttribute val4 = item2.Attribute(XName.op_Implicit("Count"));
			if (!int.TryParse(val4.Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse Count attribute value " + val4.Value + " of Building " + val2.Value + " in RandomBuildingsGenerationDefinition " + Id + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			BuildingInfo buildingInfo = default(BuildingInfo);
			buildingInfo.Id = val2.Value;
			buildingInfo.TileFlag = result;
			buildingInfo.Count = result2;
			BuildingInfo item = buildingInfo;
			BuildingsInfo.Add(item);
		}
	}
}
