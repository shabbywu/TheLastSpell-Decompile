using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyRarityProbabilityTreeEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "ModifyRarityProbabilityTree";

	public string TreeId { get; private set; }

	public Dictionary<int, int> ProbabilityModifiers { get; private set; }

	public GlyphModifyRarityProbabilityTreeEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in ModifyRarityProbabilityTree.");
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("TreeId"));
		GlyphDefinition.AssertIsTrue(val != null, "TreeId attribute is missing in ModifyRarityProbabilityTree.");
		TreeId = val.Value;
		ProbabilityModifiers = new Dictionary<int, int>();
		foreach (XElement item in obj.Elements(XName.op_Implicit("Probability")))
		{
			XAttribute obj2 = item.Attribute(XName.op_Implicit("Weight"));
			GlyphDefinition.AssertIsTrue(obj2 != null, "Weight attribute is missing in Probability element in ModifyRarityProbabilityTree.");
			GlyphDefinition.AssertIsTrue(int.TryParse(obj2.Value.Replace(base.TokenVariables), out var result), "Could not parse Weight attribute into an int in Probability element in ModifyRarityProbabilityTree.");
			GlyphDefinition.AssertIsTrue(int.TryParse(item.Value.Replace(base.TokenVariables), out var result2), "Could not parse Probability into an int in ModifyRarityProbabilityTree.");
			ProbabilityModifiers.AddValueOrCreateKey(result2, result, (int a, int b) => a + b);
		}
	}
}
