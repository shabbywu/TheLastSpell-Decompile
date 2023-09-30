using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphAdditionalRewardRerollEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "AdditionalRewardReroll";

	public GlyphAdditionalRewardRerollEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
