using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class IncreasePlaysPerTurnDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "IncreasePlaysPerTurn";

	public int Value { get; set; }

	public IncreasePlaysPerTurnDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute val = ((XElement)((xContainer is XElement) ? xContainer : null)).Attribute(XName.op_Implicit("Value"));
		if (!val.IsNullOrEmpty())
		{
			if (!int.TryParse(val.Value, out var result))
			{
				TPDebug.LogError((object)(base.Id + " UpgradeEffect must have a valid Attribute Value (int)"), (Object)null);
			}
			else
			{
				Value = result;
			}
		}
	}
}
