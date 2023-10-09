using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class TargetInRangeConditionDefinition : GoalConditionDefinition
{
	public const string Name = "TargetInRange";

	public Node MaxExpression { get; private set; }

	public Node MinExpression { get; private set; }

	public TargetInRangeConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public int EvalToInt(Node node, InterpreterContext interpreterContext = null)
	{
		if (interpreterContext == null)
		{
			return node?.EvalToInt() ?? 0;
		}
		return node?.EvalToInt(interpreterContext) ?? 0;
	}

	public int GetMinEvalToInt(InterpreterContext interpreterContext = null)
	{
		return EvalToInt(MinExpression, interpreterContext);
	}

	public int GetMaxEvalToInt(InterpreterContext interpreterContext = null)
	{
		return EvalToInt(MaxExpression, interpreterContext);
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Min"));
		if (val != null)
		{
			MinExpression = Parser.Parse(val.Value);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Max"));
		if (val2 != null)
		{
			MaxExpression = Parser.Parse(val2.Value);
		}
	}
}
