using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Perk.PerkDataCondition;

public class IsFalseDataConditionDefinition : APerkDataConditionDefinition
{
	public static class Constants
	{
		public const string Id = "IsFalse";
	}

	public Node ValueExpression { get; private set; }

	public IsFalseDataConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val.Value, ((Definition)this).TokenVariables);
	}
}
