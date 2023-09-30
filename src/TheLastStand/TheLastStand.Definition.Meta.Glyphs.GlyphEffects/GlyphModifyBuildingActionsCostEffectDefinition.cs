using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphModifyBuildingActionsCostEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "ModifyBuildingActionsCost";

	public Dictionary<string, int> BuildingActionCostModifiers { get; private set; }

	public int ModifiersDailyLimit { get; private set; }

	public GlyphModifyBuildingActionsCostEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		GlyphDefinition.AssertIsTrue(obj != null, "Received null element in ModifyBuildingActionsCost");
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ModifiersDailyLimit"));
		GlyphDefinition.AssertIsTrue(val != null, "ModifiersDailyLimit is missing in ModifyBuildingActionsCost");
		GlyphDefinition.AssertIsTrue(int.TryParse(StringExtensions.Replace(val.Value, ((Definition)this).TokenVariables), out var result), "Could not parse ModifiersDailyLimit into an int in ModifyBuildingActionsCost : " + StringExtensions.Replace(val.Value, ((Definition)this).TokenVariables));
		ModifiersDailyLimit = result;
		BuildingActionCostModifiers = new Dictionary<string, int>();
		foreach (XElement item in obj.Elements(XName.op_Implicit("BuildingActionCostModifier")))
		{
			XAttribute val2 = item.Attribute(XName.op_Implicit("BuildingActionId"));
			GlyphDefinition.AssertIsTrue(val2 != null, "BuildingActionId attribute is missing in BuildingActionCostModifier in ModifyBuildingActionsCost");
			XAttribute val3 = item.Attribute(XName.op_Implicit("CostModifier"));
			GlyphDefinition.AssertIsTrue(val3 != null, "CostModifier attribute is missing in BuildingActionCostModifier in ModifyBuildingActionsCost");
			GlyphDefinition.AssertIsTrue(int.TryParse(StringExtensions.Replace(val3.Value, ((Definition)this).TokenVariables), out var result2), "CostModifier could not be parsed into an int in ModifyBuildingActionsCost : " + StringExtensions.Replace(val3.Value, ((Definition)this).TokenVariables));
			BuildingActionCostModifiers.Add(StringExtensions.Replace(val2.Value, ((Definition)this).TokenVariables), result2);
		}
	}
}
