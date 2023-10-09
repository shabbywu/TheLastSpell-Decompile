using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;

public class SurviveTurnsBossPhaseConditionDefinition : TheLastStand.Framework.Serialization.Definition, IBossPhaseConditionDefinition
{
	public Node Expression { get; private set; }

	public SurviveTurnsBossPhaseConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		Expression = Parser.Parse(val.Value);
	}
}
