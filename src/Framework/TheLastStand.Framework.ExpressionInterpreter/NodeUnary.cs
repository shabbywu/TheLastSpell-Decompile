using System;

namespace TheLastStand.Framework.ExpressionInterpreter;

public class NodeUnary : Node
{
	private readonly Node rightHandSide;

	private readonly Func<double, double> callbackOperator;

	public NodeUnary(Node rightHandSize, Func<double, double> callbackOperator)
	{
		rightHandSide = rightHandSize;
		this.callbackOperator = callbackOperator;
	}

	public override Node Clone()
	{
		return new NodeUnary(rightHandSide.Clone(), callbackOperator.Clone() as Func<double, double>);
	}

	public override object Eval()
	{
		return callbackOperator((double)rightHandSide.Eval());
	}

	public override object Eval(InterpreterContext context)
	{
		return callbackOperator((double)rightHandSide.Eval(context));
	}
}
