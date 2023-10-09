using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphSetFogCapEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "SetFogCap";

	public string IndexName { get; private set; }

	public GlyphSetFogCapEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Index"));
		IndexName = val.Value.Replace(base.TokenVariables);
	}
}
