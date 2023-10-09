using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class GainGoldBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	public int GainGold { get; private set; }

	public GainGoldBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
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
				Debug.LogError((object)"A GainGold Building ActionEffect must have a valid GainGold (int)");
			}
			else
			{
				GainGold = result;
			}
		}
	}
}
