using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public abstract class GlyphIntValueBasedEffectDefinition : GlyphEffectDefinition
{
	public int Value { get; private set; }

	protected GlyphIntValueBasedEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		string text = val.Value.Replace(base.TokenVariables);
		if (!int.TryParse(text, out var result))
		{
			CLoggerManager.Log((object)(GetType().FullName + " Unable to parse " + text + " (" + val.Value + ") into an int"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		Value = result;
	}
}
