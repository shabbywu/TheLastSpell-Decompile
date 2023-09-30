using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphIncreaseLevelupRerollsEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "IncreaseLevelupRerolls";

	public Node RerollsBonusExpression { get; private set; }

	public GlyphIncreaseLevelupRerollsEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphIncreaseStartingResourcesEffectDefinition.");
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Value"));
		GlyphDefinition.AssertIsTrue(val != null, "Value attribute is missing in IncreaseLevelupRerolls.");
		RerollsBonusExpression = Parser.Parse(val.Value, ((Definition)this).TokenVariables);
	}
}
