using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class UpgradeModuleDefinition : BuildingModuleDefinition
{
	public List<BuildingUpgradeDefinition> BuildingUpgradeDefinitions { get; private set; }

	public string UpgradeOf { get; set; }

	public UpgradeModuleDefinition(BuildingDefinition buildingDefinition, XContainer upgradeDefinition)
		: base(buildingDefinition, upgradeDefinition)
	{
	}

	public List<string> GetPreviousUpgrades()
	{
		List<string> list = new List<string>();
		BuildingDefinition buildingDefinition = BuildingDefinition;
		while (buildingDefinition.UpgradeModuleDefinition.UpgradeOf != null)
		{
			list.Add(buildingDefinition.UpgradeModuleDefinition.UpgradeOf);
			buildingDefinition = BuildingDatabase.BuildingDefinitions[buildingDefinition.UpgradeModuleDefinition.UpgradeOf];
		}
		return list;
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("UpgradeOf"));
		if (val2 != null)
		{
			if (XDocumentExtensions.IsNullOrEmpty(val2))
			{
				Debug.LogError((object)("Building " + BuildingDefinition.Id + " has an invalid UpgradeOf !"));
				return;
			}
			UpgradeOf = val2.Value;
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("BuildingUpgradeDefinitions"));
		if (val3 == null)
		{
			return;
		}
		BuildingUpgradeDefinitions = new List<BuildingUpgradeDefinition>();
		foreach (XElement item in ((XContainer)val3).Elements(XName.op_Implicit("BuildingUpgradeDefinition")))
		{
			XAttribute val4 = item.Attribute(XName.op_Implicit("Id"));
			if (XDocumentExtensions.IsNullOrEmpty(val4))
			{
				Debug.LogError((object)("BuildingDefinition " + BuildingDefinition.Id + " BuildingUpgradeDefinition must have an attribute Id"));
			}
			if (BuildingDatabase.BuildingUpgradeDefinitions.TryGetValue(val4.Value, out var value))
			{
				BuildingUpgradeDefinitions.Add(value);
				continue;
			}
			Debug.LogError((object)("BuildingUpgradeDefinition " + val4.Value + " not found"));
			break;
		}
	}
}
