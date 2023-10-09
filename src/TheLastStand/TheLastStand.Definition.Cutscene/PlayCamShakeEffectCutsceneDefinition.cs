using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class PlayCamShakeEffectCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlayCamShakeEffect";

		public const string DataAnimationCurvePath = "AnimationCurves/";
	}

	public DataAnimationCurve DataAnimationCurve { get; private set; }

	public float DelayBetweenEachShake { get; private set; }

	public float Duration { get; private set; }

	public float DurationOfEachShake { get; private set; }

	public float IntensityMultiplier { get; private set; }

	public bool WaitCamShake { get; private set; }

	public PlayCamShakeEffectCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("DataAnimationCurve"));
		DataAnimationCurve = ResourcePooler.LoadOnce<DataAnimationCurve>(Path.Combine("AnimationCurves/", val.Value), failSilently: false);
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("DelayBetweenEachShake"));
		if (float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			DelayBetweenEachShake = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse PlayCamShakeEffectCutsceneDefinition value " + val2.Value + " as a valid float value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("Duration"));
		if (float.TryParse(val3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			Duration = result2;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse PlayCamShakeEffectCutsceneDefinition value " + val3.Value + " as a valid float value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val4 = ((XElement)obj).Attribute(XName.op_Implicit("DurationOfEachShake"));
		if (float.TryParse(val4.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
		{
			DurationOfEachShake = result3;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse PlayCamShakeEffectCutsceneDefinition value " + val4.Value + " as a valid float value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val5 = ((XElement)obj).Attribute(XName.op_Implicit("IntensityMultiplier"));
		if (float.TryParse(val5.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
		{
			IntensityMultiplier = result4;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse PlayCamShakeEffectCutsceneDefinition value " + val5.Value + " as a valid float value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val6 = ((XElement)obj).Attribute(XName.op_Implicit("WaitCamShake"));
		if (val6 != null)
		{
			if (bool.TryParse(val6.Value, out var result5))
			{
				WaitCamShake = result5;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse PlayCamShakeEffectCutsceneDefinition value " + val6.Value + " as a valid bool value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}
}
