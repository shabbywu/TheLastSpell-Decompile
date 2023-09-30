using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.CastFx;

public class SoundEffectDefinition : Definition
{
	public Node Delay { get; private set; }

	public string FolderPath { get; private set; } = string.Empty;


	public string Path { get; private set; } = string.Empty;


	public Dictionary<string, int> RandomPaths { get; } = new Dictionary<string, int>();


	public SoundEffectDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		XElement val = container.Element(XName.op_Implicit("FolderPath"));
		XElement val2 = container.Element(XName.op_Implicit("Path"));
		XElement val3 = container.Element(XName.op_Implicit("RandomPaths"));
		if (val == null && val2 == null && val3 == null)
		{
			CLoggerManager.Log((object)"The SoundEffect must have a FolderPath, a Path or RandomPaths", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		if (val != null)
		{
			FolderPath = val.Value;
		}
		else if (val2 != null)
		{
			Path = val2.Value;
		}
		else
		{
			foreach (XElement item in ((XContainer)val3).Elements(XName.op_Implicit("RandomPath")))
			{
				XAttribute val4 = item.Attribute(XName.op_Implicit("Path"));
				if (RandomPaths.ContainsKey(val4.Value))
				{
					CLoggerManager.Log((object)("Random paths already contains path " + val4.Value + "!"), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				if (!int.TryParse(item.Attribute(XName.op_Implicit("Weight")).Value, out var result))
				{
					CLoggerManager.Log((object)"Random path's weight should be of type float!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				RandomPaths.Add(val4.Value, result);
			}
		}
		XElement val5 = container.Element(XName.op_Implicit("Delay"));
		Delay = (Node)((val5 != null) ? ((object)Parser.Parse(val5.Value, (Dictionary<string, string>)null)) : ((object)new NodeNumber(0.0)));
	}
}
