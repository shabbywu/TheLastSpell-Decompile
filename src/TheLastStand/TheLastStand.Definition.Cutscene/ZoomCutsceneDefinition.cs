using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class ZoomCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "Zoom";
	}

	public bool In { get; private set; }

	public bool Instant { get; private set; }

	public ZoomCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("In"));
		if (bool.TryParse(val.Value, out var result))
		{
			In = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse ZoomCutsceneDefinition In Attribute value " + val.Value + " as a valid bool value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Instant"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result2))
			{
				Instant = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse ZoomCutsceneDefinition Instant Attribute value " + val2.Value + " as a valid bool value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}
}
