using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ImproveLevelDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ImproveLevel";

	public int LevelsCount { get; private set; } = 1;


	public ImproveLevelDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute val = ((XElement)((xContainer is XElement) ? xContainer : null)).Attribute(XName.op_Implicit("UpgradedBonusValue"));
		if (!val.IsNullOrEmpty())
		{
			if (!int.TryParse(val.Value, out var result))
			{
				TPDebug.LogError((object)(base.Id + " UpgradeEffect must have a valid Attribute UpgradedBonusValue (int)"), (Object)null);
			}
			else
			{
				LevelsCount = result;
			}
		}
	}
}
