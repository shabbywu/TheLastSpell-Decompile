using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;

public class BossPhaseTurnConditionDefinition : TheLastStand.Framework.Serialization.Definition, IBossPhaseConditionDefinition
{
	public Node Expression { get; private set; }

	public BossPhaseTurnConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		Expression = Parser.Parse(val.Value);
	}
}
