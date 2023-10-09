using System;

namespace TheLastStand.Framework.ExpressionInterpreter;

public class NodeBinary : Node
{
	private readonly Node leftHandSide;

	private readonly Node rightHandSide;

	private readonly Func<double, double, double> callbackOperator;

	public NodeBinary(Node leftHandSide, Node rightHandSize, Func<double, double, double> callbackOperator)
	{
		this.leftHandSide = leftHandSide;
		rightHandSide = rightHandSize;
		this.callbackOperator = callbackOperator;
	}

	public override Node Clone()
	{
		return new NodeBinary(leftHandSide.Clone(), rightHandSide.Clone(), callbackOperator.Clone() as Func<double, double, double>);
	}

	public override object Eval()
	{
		object obj = leftHandSide.Eval();
		double arg = ((obj is int) ? ((double)(int)obj) : ((!(obj is float)) ? ((double)obj) : ((double)(float)obj)));
		object obj2 = rightHandSide.Eval();
		double arg2 = ((obj2 is int) ? ((double)(int)obj2) : ((!(obj2 is float)) ? ((double)obj2) : ((double)(float)obj2)));
		return callbackOperator(arg, arg2);
	}

	public override object Eval(InterpreterContext context)
	{
		object obj = leftHandSide.Eval(context);
		double arg = ((obj is int) ? ((double)(int)obj) : ((!(obj is float)) ? ((double)obj) : ((double)(float)obj)));
		object obj2 = rightHandSide.Eval(context);
		double arg2 = ((obj2 is int) ? ((double)(int)obj2) : ((!(obj2 is float)) ? ((double)obj2) : ((double)(float)obj2)));
		return callbackOperator(arg, arg2);
	}
}
