using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphBonusUnitsEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "BonusUnits";

	public bool IncreaseUnitsLimit { get; private set; } = true;


	public List<UnitGenerationDefinition> UnitGenerationDefinitions { get; private set; }

	public GlyphBonusUnitsEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		GlyphDefinition.AssertIsTrue(obj != null, "BonusUnits received null in Deserialize.");
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("IncreaseUnitsLimit"));
		if (val != null)
		{
			GlyphDefinition.AssertIsTrue(bool.TryParse(val.Value, out var result), "Could not parse IncreaseUnitsLimit into a bool in BonusUnits.");
			IncreaseUnitsLimit = result;
		}
		UnitGenerationDefinitions = new List<UnitGenerationDefinition>();
		foreach (XElement item in obj.Elements(XName.op_Implicit("Unit")))
		{
			UnitGenerationDefinitions.Add(new UnitGenerationDefinition((XContainer)(object)item));
		}
	}
}
