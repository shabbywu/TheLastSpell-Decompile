using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyCostsEffectDefinition : GlyphIntValueBasedEffectDefinition
{
	public const string Name = "ModifyCosts";

	public ResourceManager.E_PriceModifierType Type { get; private set; }

	public GlyphModifyCostsEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute obj = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Type"));
		GlyphDefinition.AssertIsTrue(obj != null, "Type attribute is missing in ModifyCosts");
		GlyphDefinition.AssertIsTrue(Enum.TryParse<ResourceManager.E_PriceModifierType>(StringExtensions.Replace(obj.Value, ((Definition)this).TokenVariables), out var result), "Could not parse Type attribute into an E_PriceModifierType in ModifyCosts");
		Type = result;
	}
}
