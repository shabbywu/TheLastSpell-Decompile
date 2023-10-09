using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ImproveGaugeEffectDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ImproveGaugeEffect";

	public int Value { get; private set; }

	public ImproveGaugeEffectDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute val = ((XElement)((xContainer is XElement) ? xContainer : null)).Attribute(XName.op_Implicit("UpgradedBonusValue"));
		int result;
		if (val.IsNullOrEmpty())
		{
			TPDebug.LogError((object)(base.Id + " UpgradeEffect must have an Attribute UpgradedBonusValue"), (Object)null);
		}
		else if (!int.TryParse(val.Value, out result))
		{
			TPDebug.LogError((object)(base.Id + " UpgradeEffect must have a valid Attribute UpgradedBonusValue (int)"), (Object)null);
		}
		else
		{
			Value = result;
		}
	}
}
