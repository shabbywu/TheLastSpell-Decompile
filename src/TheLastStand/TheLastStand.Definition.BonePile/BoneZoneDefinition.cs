using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.BonePile;

public class BoneZoneDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public int MinHavenDistance { get; private set; } = -1;


	public int MaxMagicCircleDistance { get; private set; } = -1;


	public BoneZoneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("MinHavenDistance"));
		if (val3 != null)
		{
			if (!int.TryParse(val3.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse MinHavenDistance element value " + val3.Value + " as a valid int! (BoneZone Id=" + Id + ")"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			MinHavenDistance = result;
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("MaxMagicCircleDistance"));
		if (val4 != null)
		{
			if (!int.TryParse(val4.Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse MaxMagicCircleDistance element value " + val4.Value + " as a valid int! (BoneZone Id=" + Id + ")"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				MaxMagicCircleDistance = result2;
			}
		}
	}
}
