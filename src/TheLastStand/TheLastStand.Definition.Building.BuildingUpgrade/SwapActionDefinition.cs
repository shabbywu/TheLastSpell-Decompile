using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class SwapActionDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "SwapAction";

	public string OldActionId { get; private set; }

	public string NewActionId { get; private set; }

	public SwapActionDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("OldActionId"));
		if (val2.IsNullOrEmpty())
		{
			TPDebug.LogError((object)"SwapActionDefinition must have an OldActionId", (Object)null);
			return;
		}
		OldActionId = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("NewActionId"));
		if (val3.IsNullOrEmpty())
		{
			TPDebug.LogError((object)"SwapkActionDefinition must have an NewActionId", (Object)null);
		}
		else
		{
			NewActionId = val3.Value;
		}
	}
}
