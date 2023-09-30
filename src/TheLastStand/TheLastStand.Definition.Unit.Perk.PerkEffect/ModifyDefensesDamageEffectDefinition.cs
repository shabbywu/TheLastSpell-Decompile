using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ModifyDefensesDamageEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ModifyDefensesDamage";
	}

	public Node PercentageExpression { get; private set; }

	public Node RangeExpression { get; private set; }

	public ModifyDefensesDamageEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Percentage"));
		PercentageExpression = Parser.Parse(val.Value, ((Definition)this).TokenVariables);
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Range"));
		RangeExpression = Parser.Parse(val2.Value, ((Definition)this).TokenVariables);
	}
}
