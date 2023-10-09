using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class FocusMagicCircleCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "FocusMagicCircle";
	}

	public bool ZoomIn { get; private set; } = true;


	public FocusMagicCircleCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("ZoomIn"));
		if (val != null)
		{
			if (bool.TryParse(val.Value, out var result))
			{
				ZoomIn = result;
				return;
			}
			CLoggerManager.Log((object)("Could not parse FocusMagicCircleCutsceneDefinition ZoomIn value " + val.Value + " as a valid bool value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			ZoomIn = true;
		}
		else
		{
			ZoomIn = true;
		}
	}
}
