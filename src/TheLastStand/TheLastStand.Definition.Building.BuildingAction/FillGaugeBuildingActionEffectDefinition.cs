using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class FillGaugeBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	public int Amount { get; private set; }

	public FillGaugeBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement val = ((xContainer is XElement) ? xContainer : null).Element(XName.op_Implicit("Amount"));
		int result;
		if (val.IsNullOrEmpty())
		{
			Debug.LogError((object)"A FillGauge Building ActionEffect must have an Amount element");
		}
		else if (!int.TryParse(val.Value, out result))
		{
			Debug.LogError((object)"A FillGauge Building ActionEffect must have a valid Amount (int)");
		}
		else
		{
			Amount = result;
		}
	}
}
