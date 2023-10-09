using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ImprovePassiveDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ImprovePassive";

	public string PassiveId { get; private set; }

	public Node Value { get; private set; }

	public ImprovePassiveDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("PassiveId"));
		if (val2.IsNullOrEmpty())
		{
			TPDebug.LogError((object)"ImprovePassiveDefinition must have a PassiveId", (Object)null);
			return;
		}
		PassiveId = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("UpgradedBonusValue"));
		if (val3.IsNullOrEmpty())
		{
			TPDebug.LogError((object)(base.Id + " UpgradeEffect must have an Attribute UpgradedBonusValue"), (Object)null);
		}
		else
		{
			Value = Parser.Parse(val3.Value);
		}
	}
}
