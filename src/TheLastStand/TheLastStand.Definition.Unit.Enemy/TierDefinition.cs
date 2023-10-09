using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class TierDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int Index { get; private set; }

	public float LifetimeStatsWeight { get; private set; }

	public TierDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Index"));
		if (int.TryParse(val.Value, out var result))
		{
			Index = result;
		}
		else
		{
			CLoggerManager.Log((object)("TierDefinition: Could not parse index " + val.Value + " to an int."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XElement val2 = obj.Element(XName.op_Implicit("LifetimeStatsWeight"));
		if (float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			LifetimeStatsWeight = result2;
		}
		else
		{
			CLoggerManager.Log((object)("TierDefinition: Could not parse LifetimeStatsWeight " + val2.Value + " to a float."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
