using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class UnlockActionDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "UnlockAction";

	public string NewActionId { get; private set; }

	public UnlockActionDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute val = ((XElement)((xContainer is XElement) ? xContainer : null)).Attribute(XName.op_Implicit("NewActionId"));
		if (XDocumentExtensions.IsNullOrEmpty(val))
		{
			TPDebug.LogError((object)"UnlockActionDefinition must have an NewActionId", (Object)null);
		}
		else
		{
			NewActionId = val.Value;
		}
	}
}
