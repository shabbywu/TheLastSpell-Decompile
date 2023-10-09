using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Perk.PerkDataCondition;

public class IsTrueDataConditionDefinition : APerkDataConditionDefinition
{
	public static class Constants
	{
		public const string Id = "IsTrue";
	}

	public Node ValueExpression { get; private set; }

	public IsTrueDataConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val.Value, base.TokenVariables);
	}
}
