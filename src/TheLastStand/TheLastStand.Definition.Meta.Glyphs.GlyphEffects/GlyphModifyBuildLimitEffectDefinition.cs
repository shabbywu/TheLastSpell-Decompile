using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyBuildLimitEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "ModifyBuildLimit";

	public string BuildLimitId { get; private set; }

	public GlyphModifyBuildLimitEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("BuildLimitId"));
		GlyphDefinition.AssertIsTrue(val != null, "BuildLimitId attribute is missing in ModifyBuildLimit");
		BuildLimitId = StringExtensions.Replace(val.Value, ((Definition)this).TokenVariables);
	}
}
