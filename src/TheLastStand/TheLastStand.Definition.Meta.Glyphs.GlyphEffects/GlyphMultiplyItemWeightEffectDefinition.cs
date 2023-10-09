using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphMultiplyItemWeightEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "MultiplyItemWeight";

	public string ItemId { get; private set; }

	public string ItemListId { get; private set; }

	public float WeightMultiplier { get; private set; }

	public bool IsCumulative { get; private set; } = true;


	public GlyphMultiplyItemWeightEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphIncreaseStartingResourcesEffectDefinition.");
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ItemListId"));
		GlyphDefinition.AssertIsTrue(val != null, "ItemListId attribute is missing in MultiplyItemWeight.");
		ItemListId = val.Value.Replace(base.TokenVariables);
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("ItemId"));
		GlyphDefinition.AssertIsTrue(val2 != null, "ItemId attribute is missing in MultiplyItemWeight.");
		ItemId = val2.Value.Replace(base.TokenVariables);
		XAttribute obj2 = ((XElement)obj).Attribute(XName.op_Implicit("WeightMultiplier"));
		GlyphDefinition.AssertIsTrue(obj2 != null, "WeightMultiplier attribute is missing in MultiplyItemWeight.");
		string text = obj2.Value.Replace(base.TokenVariables);
		GlyphDefinition.AssertIsTrue(float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result), "WeightMultiplier could not be parsed into a float in MultiplyItemWeight : " + text);
		WeightMultiplier = result;
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("IsCumulative"));
		if (val3 != null)
		{
			GlyphDefinition.AssertIsTrue(bool.TryParse(val3.Value, out var result2), "IsCumulative could not be parsed into a bool in MultiplyItemWeight : " + val3.Value);
			IsCumulative = result2;
		}
	}
}
