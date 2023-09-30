using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ImproveSellRatioDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ImproveSellRatio";

	public int Value { get; private set; } = 1;


	public ImproveSellRatioDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute val = ((XElement)((xContainer is XElement) ? xContainer : null)).Attribute(XName.op_Implicit("UpgradedBonusValue"));
		if (!XDocumentExtensions.IsNullOrEmpty(val))
		{
			if (!int.TryParse(val.Value, out var result))
			{
				TPDebug.LogError((object)(base.Id + " UpgradeEffect must have a valid Attribute UpgradedBonusValue (int)"), (Object)null);
			}
			else
			{
				Value = result;
			}
		}
	}
}
