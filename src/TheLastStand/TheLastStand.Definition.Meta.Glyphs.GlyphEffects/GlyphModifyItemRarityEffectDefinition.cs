using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyItemRarityEffectDefinition : GlyphEffectDefinition
{
	public class ItemRarityModifier
	{
		public int MinRarityIndex { get; set; } = -1;

	}

	public const string Name = "ModifyItemRarity";

	public const string MinRarityName = "MinRarity";

	public string ItemTag { get; private set; }

	public int MinRarityIndex { get; private set; }

	public GlyphModifyItemRarityEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphModifyItemRarityEffectDefinition.");
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Tag"));
		GlyphDefinition.AssertIsTrue(val != null, "Tag attribute is missing in ModifyItemRarity");
		ItemTag = ((val != null) ? val.Value : null);
		foreach (XElement item in obj.Elements(XName.op_Implicit("MinRarity")))
		{
			GlyphDefinition.AssertIsTrue(int.TryParse(item.Value, out var result), "Could not parse MinRarity into an int in ModifyItemRarity : " + item.Value);
			MinRarityIndex = result;
		}
	}
}
