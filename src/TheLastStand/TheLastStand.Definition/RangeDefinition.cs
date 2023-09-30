using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class RangeDefinition : Definition
{
	public string Id { get; private set; }

	public Vector2Int Range { get; private set; }

	public RangeDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		int num = 0;
		int num2 = -1;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Min"));
		if (val2 != null)
		{
			if (int.TryParse(val2.Value, out var result))
			{
				num = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse Min attribute into an int in RangeDefinition : " + val2.Value), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("Max"));
		if (val3 != null)
		{
			if (int.TryParse(val3.Value, out var result2))
			{
				num2 = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse Max attribute into an int in RangeDefinition : " + val3.Value), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		Range = new Vector2Int(num, num2);
	}
}
