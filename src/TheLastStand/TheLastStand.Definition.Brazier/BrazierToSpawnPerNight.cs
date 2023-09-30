using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building;
using UnityEngine;

namespace TheLastStand.Definition.Brazier;

public class BrazierToSpawnPerNight : NightIndexedItem
{
	private static class Constants
	{
		public const string BrazierToSpawnElement = "BrazierToSpawn";

		public const string BuildingIdAttribute = "BuildingId";
	}

	public BuildingDefinition BrazierDefinition;

	public override void Init(int nightIndex, XElement xElement)
	{
		base.Init(nightIndex, xElement);
		XAttribute val = xElement.Attribute(XName.op_Implicit("BuildingId"));
		if (!BuildingDatabase.BuildingDefinitions.TryGetValue(val.Value, out var value))
		{
			CLoggerManager.Log((object)("BuildingId attribute could not be found in the buildings database (" + val.Value + "). Skipped."), (LogType)0, (CLogLevel)2, true, "BrazierDefinition", false);
		}
		else
		{
			BrazierDefinition = value;
		}
	}
}
