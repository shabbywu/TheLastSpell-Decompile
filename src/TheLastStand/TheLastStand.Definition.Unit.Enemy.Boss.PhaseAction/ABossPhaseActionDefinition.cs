using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public abstract class ABossPhaseActionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Node DelayExpression { get; private set; }

	protected ABossPhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Delay"));
		if (!string.IsNullOrEmpty((val != null) ? val.Value : null))
		{
			DelayExpression = Parser.Parse(val.Value);
		}
	}
}
