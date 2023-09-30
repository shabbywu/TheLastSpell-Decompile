using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphIncreaseBuildingHealthEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "IncreaseBuildingHealth";

	public string IdList { get; private set; }

	public GlyphIncreaseBuildingHealthEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		base.Deserialize(container);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("IdList"));
		IdList = StringExtensions.Replace(val.Value, ((Definition)this).TokenVariables);
	}
}
