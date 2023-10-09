using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class FocusTileCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "FocusTile";
	}

	public int PosX { get; private set; }

	public int PosY { get; private set; }

	public FocusTileCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("X"));
		if (int.TryParse(val.Value, out var result))
		{
			PosX = result;
		}
		else
		{
			CLoggerManager.Log((object)("Unable to parse " + val.Value + " into int."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Y"));
		if (int.TryParse(val2.Value, out var result2))
		{
			PosY = result2;
		}
		else
		{
			CLoggerManager.Log((object)("Unable to parse " + val2.Value + " into int."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
	}
}
