using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ResetBufferEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ResetBuffer";
	}

	public int? ResetValue;

	public ResetBufferEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("ResetValue"));
		if (val != null)
		{
			if (int.TryParse(val.Value, out var result))
			{
				ResetValue = result;
			}
			else
			{
				CLoggerManager.Log((object)"Found a ResetValue for ResetBuffer but the int parsing failed. Safely assigning null.", (LogType)2, (CLogLevel)2, true, "ResetBufferEffectDefinition", false);
			}
		}
	}
}
