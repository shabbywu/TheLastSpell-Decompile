using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class PlaySoundCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlaySound";
	}

	public string AudioClipPath { get; private set; }

	public float Delay { get; private set; }

	public PlaySoundCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("Path"));
		AudioClipPath = val.Value;
		XElement val2 = obj.Element(XName.op_Implicit("Delay"));
		if (val2 != null)
		{
			if (float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				Delay = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse " + val2.Value + " as a valid float value in PlaySoundCutsceneDefinition!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}
}
