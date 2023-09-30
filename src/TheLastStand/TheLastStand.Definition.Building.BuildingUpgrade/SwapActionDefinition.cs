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
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			TPDebug.LogError((object)"SwapActionDefinition must have an OldActionId", (Object)null);
			return;
		}
		OldActionId = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("NewActionId"));
		if (XDocumentExtensions.IsNullOrEmpty(val3))
		{
			TPDebug.LogError((object)"SwapkActionDefinition must have an NewActionId", (Object)null);
		}
		else
		{
			NewActionId = val3.Value;
		}
	}
}
