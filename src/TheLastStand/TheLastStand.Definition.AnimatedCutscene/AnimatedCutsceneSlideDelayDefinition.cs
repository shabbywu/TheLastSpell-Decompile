using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideDelayDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "Delay";
	}

	public float Delay { get; private set; }

	public bool Unskippable { get; private set; }

	public AnimatedCutsceneSlideDelayDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Duration"));
		if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log((object)("Unable to parse " + val2.Value + " to a valid float value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Delay = result;
		Unskippable = ((XContainer)val).Element(XName.op_Implicit("Unskippable")) != null;
	}
}
