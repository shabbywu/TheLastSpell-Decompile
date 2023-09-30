using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class BufferModuleDefinition : APerkModuleDefinition
{
	public enum BufferIndex
	{
		Buffer,
		Buffer2,
		Buffer3
	}

	public static class Constants
	{
		public const string Id = "BufferModule";
	}

	public int DefaultBufferValue { get; private set; }

	public BufferModuleDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("DefaultBufferValue"));
		if (val != null)
		{
			if (int.TryParse(val.Value, out var result))
			{
				DefaultBufferValue = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse DefaultBufferValue attribute into an int : " + val.Value + "."), (LogType)0, (CLogLevel)2, true, "BufferModuleDefinition", false);
			}
		}
	}
}
