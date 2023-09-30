using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public abstract class InstantiateEffectDisplayDefinition : APerkActionDefinition
{
	public Node ValueExpression { get; private set; }

	public InstantiateEffectDisplayDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val.Value, ((Definition)this).TokenVariables);
	}
}
