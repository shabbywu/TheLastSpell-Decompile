using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class GainMaterialsBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	public int GainMaterials { get; private set; }

	public GainMaterialsBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		if (!val.IsNullOrEmpty())
		{
			if (!int.TryParse(val.Value, out var result))
			{
				Debug.LogError((object)"A GainMaterials Building ActionEffect must have a valid GainMaterials (int)");
			}
			else
			{
				GainMaterials = result;
			}
		}
	}
}
