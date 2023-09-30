using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class PassivesModuleDefinition : BuildingModuleDefinition
{
	private List<BuildingPassiveDefinition> buildingPassiveDefinitions;

	public List<BuildingPassiveDefinition> BuildingPassiveDefinitions
	{
		get
		{
			if (!TPSingleton<GlyphManager>.Exist())
			{
				return buildingPassiveDefinitions;
			}
			return TPSingleton<GlyphManager>.Instance.GetModifiedBuildingPassives(BuildingDefinition.Id, buildingPassiveDefinitions);
		}
	}

	public bool HasOnDeathEffect { get; private set; }

	public PassivesModuleDefinition(BuildingDefinition buildingDefinition, XContainer passivesDefinition)
		: base(buildingDefinition, passivesDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		buildingPassiveDefinitions = new List<BuildingPassiveDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("BuildingPassiveDefinition")))
		{
			XAttribute val2 = item.Attribute(XName.op_Implicit("Id"));
			if (!BuildingDatabase.BuildingPassiveDefinitions.TryGetValue(val2.Value, out var value))
			{
				CLoggerManager.Log((object)("Could not find building passive with the id (" + val2.Value + ")."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
			else
			{
				buildingPassiveDefinitions.Add(value);
			}
		}
		HasOnDeathEffect = buildingPassiveDefinitions.Any((BuildingPassiveDefinition x) => x.HasOnDeathEffect);
	}
}
