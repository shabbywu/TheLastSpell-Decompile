using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class ToggleHUDCutsceneDefinition : Definition, ICutsceneDefinition
{
	public class Constants
	{
		public const string Id = "ToggleHUD";
	}

	public bool Display { get; private set; }

	public bool OnlyIfVictoryTriggered { get; private set; }

	public ToggleHUDCutsceneDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute obj2 = ((XElement)obj).Attribute(XName.op_Implicit("Display"));
		if (bool.TryParse((obj2 != null) ? obj2.Value : null, out var result))
		{
			Display = result;
		}
		else
		{
			CLoggerManager.Log((object)"Missing or Incorrect Display Attribute in ToggleHUD Element.", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("OnlyIfVictoryTriggered"));
		if (!string.IsNullOrEmpty((val != null) ? val.Value : null))
		{
			if (bool.TryParse(val.Value, out var result2))
			{
				OnlyIfVictoryTriggered = result2;
			}
			else
			{
				CLoggerManager.Log((object)("ToggleHUD OnlyIfVictoryTriggered: could not parse " + val.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
	}
}
