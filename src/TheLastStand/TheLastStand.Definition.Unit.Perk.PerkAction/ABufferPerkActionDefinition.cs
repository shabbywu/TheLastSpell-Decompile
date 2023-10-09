using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public abstract class ABufferPerkActionDefinition : APerkActionDefinition
{
	public BufferModuleDefinition.BufferIndex BufferIndex { get; private set; }

	public abstract string Id { get; }

	public Node ValueExpression { get; private set; }

	public ABufferPerkActionDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val.Value, base.TokenVariables);
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("BufferIndex"));
		if (!string.IsNullOrEmpty((val2 != null) ? val2.Value : null))
		{
			if (Enum.TryParse<BufferModuleDefinition.BufferIndex>(val2.Value, out var result))
			{
				BufferIndex = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val2.Value + " into enum BufferIndex."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
	}
}
