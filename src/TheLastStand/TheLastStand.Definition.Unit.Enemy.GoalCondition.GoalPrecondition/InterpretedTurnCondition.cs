using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class InterpretedTurnCondition : GoalConditionDefinition
{
	public const string Name = "InterpretedTurn";

	public Node Expression { get; private set; }

	public InterpretedTurnCondition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		Expression = Parser.Parse(val.Value);
	}
}
