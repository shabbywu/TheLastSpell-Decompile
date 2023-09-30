using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class GaugeModuleDefinition : BufferModuleDefinition
{
	public new static class Constants
	{
		public const string Id = "GaugeModule";
	}

	public UnitStatDefinition.E_Stat GaugeStat { get; private set; }

	[NotNull]
	public Node GaugeValue { get; private set; }

	public GaugeModuleDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("GaugeValue"));
		GaugeValue = Parser.Parse(val2.Value, ((Definition)this).TokenVariables);
		XAttribute val3 = val.Attribute(XName.op_Implicit("GaugeStat"));
		if (Enum.TryParse<UnitStatDefinition.E_Stat>(val3.Value, out var result))
		{
			GaugeStat = result;
		}
		else
		{
			CLoggerManager.Log((object)$"Unable to parse {val3.Value} into a valid E_Stat, line {((IXmlLineInfo)val).LineNumber}", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
	}
}
