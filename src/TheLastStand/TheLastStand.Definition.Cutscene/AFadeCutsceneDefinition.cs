using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public abstract class AFadeCutsceneDefinition : Definition, ICutsceneDefinition
{
	public Color Color { get; private set; } = Color.white;


	public float Duration { get; private set; }

	public bool WaitDuration { get; private set; } = true;


	protected AFadeCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
	//IL_0006: Unknown result type (might be due to invalid IL or missing references)


	public override void Deserialize(XContainer container)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Color"));
		Color color = default(Color);
		if (ColorUtility.TryParseHtmlString(val2.Value, ref color))
		{
			Color = color;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse " + ((object)this).GetType().Name + " value " + val2.Value + " as a valid color."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val3 = val.Attribute(XName.op_Implicit("Duration"));
		if (float.TryParse(val3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Duration = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse " + ((object)this).GetType().Name + " value " + val3.Value + " as a valid float value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("WaitDuration"));
		if (val4 != null)
		{
			if (bool.TryParse(val4.Value, out var result2))
			{
				WaitDuration = result2;
				return;
			}
			CLoggerManager.Log((object)("Could not parse " + ((object)this).GetType().Name + " waitDuration value " + val4.Value + " as a valid bool value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			WaitDuration = true;
		}
	}
}
