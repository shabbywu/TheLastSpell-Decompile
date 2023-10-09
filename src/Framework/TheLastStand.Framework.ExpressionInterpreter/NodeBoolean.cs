using System;

namespace TheLastStand.Framework.ExpressionInterpreter;

public class NodeBoolean : Node
{
	private readonly Node leftValue;

	private readonly Node rightValue;

	private readonly Func<double, double, bool> callbackOperator;

	public NodeBoolean(Node leftValue, Node rightHandSize, Func<double, double, bool> callbackOperator)
	{
		this.leftValue = leftValue;
		rightValue = rightHandSize;
		this.callbackOperator = callbackOperator;
	}

	public override Node Clone()
	{
		return new NodeBoolean(leftValue.Clone(), rightValue.Clone(), callbackOperator.Clone() as Func<double, double, bool>);
	}

	public override object Eval()
	{
		var (arg, arg2) = GetValues(leftValue.Eval(), rightValue.Eval());
		return callbackOperator(arg, arg2);
	}

	public override object Eval(InterpreterContext context)
	{
		var (arg, arg2) = GetValues(leftValue.Eval(context), rightValue.Eval(context));
		return callbackOperator(arg, arg2);
	}

	private (double, double) GetValues(object leftObject, object rightObject)
	{
		double num = 0.0;
		num = ((leftObject is int) ? ((double)(int)leftObject) : ((!(leftObject is float)) ? ((double)leftObject) : ((double)(float)leftObject)));
		double num2 = 0.0;
		num2 = ((rightObject is int) ? ((double)(int)rightObject) : ((!(rightObject is float)) ? ((double)rightObject) : ((double)(float)rightObject)));
		return (num, num2);
	}
}
