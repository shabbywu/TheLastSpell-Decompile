using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SubAreaDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int Distance { get; private set; }

	public int Height { get; private set; }

	public int Weight { get; private set; }

	public int Width { get; private set; }

	public SubAreaDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Weight"));
		if (val2 != null)
		{
			if (!int.TryParse(val2.Value, out var result))
			{
				CLoggerManager.Log((object)"SubAreas' Weight should be of type integer !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			Weight = result;
		}
		else
		{
			Weight = 1;
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Distance"));
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Width"));
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("Height"));
		if (!int.TryParse(val3.Value, out var result2))
		{
			CLoggerManager.Log((object)"SubAreas' Distance should be of type integer !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		if (!int.TryParse(val4.Value, out var result3))
		{
			CLoggerManager.Log((object)"SubAreas' Width should be of type integer !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		if (!int.TryParse(val5.Value, out var result4))
		{
			CLoggerManager.Log((object)"SubAreas' Height should be of type integer !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Distance = result2;
		Width = result3;
		Height = result4;
	}
}
