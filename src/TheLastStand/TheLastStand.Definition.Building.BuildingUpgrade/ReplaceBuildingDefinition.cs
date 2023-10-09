using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ReplaceBuildingDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ReplaceBuilding";

	public string NewBuildingId { get; private set; }

	public ReplaceBuildingDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute val = ((XElement)((xContainer is XElement) ? xContainer : null)).Attribute(XName.op_Implicit("NewBuildingId"));
		if (val.IsNullOrEmpty())
		{
			TPDebug.LogError((object)"ReplaceBuildingDefinition must have a NewBuildingId", (Object)null);
		}
		else
		{
			NewBuildingId = val.Value;
		}
	}
}
