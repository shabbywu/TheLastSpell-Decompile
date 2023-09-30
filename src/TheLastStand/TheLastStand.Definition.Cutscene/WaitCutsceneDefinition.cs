using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class WaitCutsceneDefinition : Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "Wait";
	}

	public float Duration { get; private set; }

	public WaitCutsceneDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (float.TryParse(val.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Duration = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse WaitCutsceneDefinition value " + val.Value + " as a valid float value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
