using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;

public class SurviveTurnsBossPhaseConditionDefinition : Definition, IBossPhaseConditionDefinition
{
	public Node Expression { get; private set; }

	public SurviveTurnsBossPhaseConditionDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		Expression = Parser.Parse(val.Value, (Dictionary<string, string>)null);
	}
}
