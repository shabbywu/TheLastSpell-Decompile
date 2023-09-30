using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using DG.Tweening;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class InvertImageCutsceneDefinition : Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "InvertImage";
	}

	public float Duration { get; private set; }

	public Ease Easing { get; private set; }

	public InvertImageCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Duration"));
		if (float.TryParse(val.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Duration = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse InvertImageCutsceneDefinition value " + val.Value + " as a valid float value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Easing"));
		if (Enum.TryParse<Ease>(val2.Value, out Ease result2))
		{
			Easing = result2;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse InvertImageCutsceneDefinition value " + val2.Value + " as a valid Ease enum."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
