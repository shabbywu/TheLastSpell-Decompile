using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyPlayableUnitsStatsEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "ModifyPlayableUnitsStats";

	public Dictionary<UnitStatDefinition.E_Stat, Node> StatsToModify { get; private set; }

	public GlyphModifyPlayableUnitsStatsEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in GlyphIncreaseStartingResourcesEffectDefinition.");
		StatsToModify = new Dictionary<UnitStatDefinition.E_Stat, Node>();
		foreach (XElement item in obj.Elements(XName.op_Implicit("StatToModify")))
		{
			GlyphDefinition.AssertIsTrue(Enum.TryParse<UnitStatDefinition.E_Stat>(item.Attribute(XName.op_Implicit("Stat")).Value, out var result), "Could not parse Stat attribute in ModifyPlayableUnitsStats or the attribute is missing.");
			XAttribute val = item.Attribute(XName.op_Implicit("Value"));
			GlyphDefinition.AssertIsTrue(val != null, "The Value attribute in ModifyPlayableUnitsStats is missing.");
			StatsToModify.Add(result, Parser.Parse(val.Value, ((Definition)this).TokenVariables));
		}
	}
}
