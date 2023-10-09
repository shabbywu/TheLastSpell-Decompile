using UnityEngine;

namespace TheLastStand.Framework.ExpressionInterpreter;

public class NodeMethodCall : Node
{
	private readonly string methodName;

	private readonly Node[] arguments;

	public NodeMethodCall(string methodeName, Node[] arguments)
	{
		methodName = methodeName;
		this.arguments = arguments;
	}

	public override Node Clone()
	{
		return new NodeMethodCall(methodName.Clone() as string, arguments.Clone() as Node[]);
	}

	public override object Eval()
	{
		Debug.LogError((object)"This node needs a context to be evaluated!");
		return 0;
	}

	public override object Eval(InterpreterContext context)
	{
		object[] array = new object[arguments.Length];
		for (int i = 0; i < arguments.Length; i++)
		{
			array[i] = arguments[i].Eval(context);
		}
		return context.CallMethod(methodName, array);
	}
}
