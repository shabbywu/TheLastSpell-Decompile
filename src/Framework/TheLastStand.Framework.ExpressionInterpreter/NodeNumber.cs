namespace TheLastStand.Framework.ExpressionInterpreter;

public class NodeNumber : Node
{
	private readonly double number;

	public NodeNumber(double number)
	{
		this.number = number;
	}

	public override Node Clone()
	{
		return new NodeNumber(number);
	}

	public override object Eval()
	{
		return number;
	}

	public override object Eval(InterpreterContext context)
	{
		return Eval();
	}
}
