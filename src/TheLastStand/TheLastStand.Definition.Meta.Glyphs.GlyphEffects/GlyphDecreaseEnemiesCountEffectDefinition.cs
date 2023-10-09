using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphDecreaseEnemiesCountEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "DecreaseEnemiesCount";

	public int Percentage { get; private set; }

	public GlyphDecreaseEnemiesCountEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in DecreaseEnemiesCount.");
		XAttribute obj2 = ((XElement)obj).Attribute(XName.op_Implicit("Percentage"));
		GlyphDefinition.AssertIsTrue(obj2 != null, "Percentage attribute is missing in DecreaseEnemiesCount element.");
		string text = obj2.Value.Replace(base.TokenVariables);
		GlyphDefinition.AssertIsTrue(int.TryParse(text, out var result), "Could not parse Value attribute into an int in DecreaseEnemiesCount : " + text + ".");
		Percentage = result;
	}
}
