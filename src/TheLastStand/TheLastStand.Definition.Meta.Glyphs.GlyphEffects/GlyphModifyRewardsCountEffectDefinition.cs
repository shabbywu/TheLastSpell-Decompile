using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyRewardsCountEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "ModifyRewardsCount";

	public int NightRewardsModifier { get; private set; }

	public int ProdRewardsModifier { get; private set; }

	public GlyphModifyRewardsCountEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in ModifyRewardsCount");
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("NightRewardsModifier"));
		if (val != null)
		{
			GlyphDefinition.AssertIsTrue(int.TryParse(StringExtensions.Replace(val.Value, ((Definition)this).TokenVariables), out var result), "Could not parse NightRewardsModifier into an int in ModifyRewardsCount");
			NightRewardsModifier = result;
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("ProdRewardsModifier"));
		if (val2 != null)
		{
			GlyphDefinition.AssertIsTrue(int.TryParse(StringExtensions.Replace(val2.Value, ((Definition)this).TokenVariables), out var result2), "Could not parse ProdRewardsModifier into an int in ModifyRewardsCount");
			ProdRewardsModifier = result2;
		}
	}
}
