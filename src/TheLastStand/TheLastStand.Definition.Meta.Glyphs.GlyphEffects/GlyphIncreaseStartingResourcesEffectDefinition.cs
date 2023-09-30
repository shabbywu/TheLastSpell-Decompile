using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphIncreaseStartingResourcesEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "IncreaseStartingResources";

	public Node GoldBonusExpression { get; private set; }

	public Node MaterialsBonusExpression { get; private set; }

	public GlyphIncreaseStartingResourcesEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphIncreaseStartingResourcesEffectDefinition.");
		XElement val = obj.Element(XName.op_Implicit("GoldBonus"));
		GoldBonusExpression = ((val != null) ? Parser.Parse(val.Value, ((Definition)this).TokenVariables) : null);
		XElement val2 = obj.Element(XName.op_Implicit("MaterialsBonus"));
		MaterialsBonusExpression = ((val2 != null) ? Parser.Parse(val2.Value, ((Definition)this).TokenVariables) : null);
	}
}
