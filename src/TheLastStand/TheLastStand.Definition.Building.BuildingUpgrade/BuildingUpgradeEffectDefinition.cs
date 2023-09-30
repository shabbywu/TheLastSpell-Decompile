using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class BuildingUpgradeEffectDefinition : Definition
{
	public string Id { get; private set; }

	public BuildingUpgradeEffectDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XAttribute val = ((XElement)((xContainer is XElement) ? xContainer : null)).Attribute(XName.op_Implicit("Id"));
		if (XDocumentExtensions.IsNullOrEmpty(val))
		{
			CLoggerManager.Log((object)"BuildingUpgradeEffectDefinition must have an Id", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			Id = val.Value;
		}
	}
}
