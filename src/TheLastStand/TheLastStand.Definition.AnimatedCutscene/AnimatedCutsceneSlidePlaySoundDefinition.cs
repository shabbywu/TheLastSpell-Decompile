using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlidePlaySoundDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "PlaySound";
	}

	public string ClipAssetName { get; private set; }

	public float Delay { get; private set; }

	public AnimatedCutsceneSlidePlaySoundDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ClipAssetName"));
		ClipAssetName = val.Value;
		XElement val2 = obj.Element(XName.op_Implicit("Delay"));
		if (val2 != null)
		{
			if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log((object)("Unable to parse " + val2.Value + " to a valid float value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				Delay = result;
			}
		}
	}
}
