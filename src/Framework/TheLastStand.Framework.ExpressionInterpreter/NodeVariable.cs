using UnityEngine;

namespace TheLastStand.Framework.ExpressionInterpreter;

public class NodeVariable : Node
{
	private readonly string variableName;

	public NodeVariable(string variableName)
	{
		this.variableName = variableName;
	}

	public override Node Clone()
	{
		return new NodeVariable(variableName.Clone() as string);
	}

	public override object Eval()
	{
		Debug.LogError((object)"This node needs a context to be evaluated!");
		return 0;
	}

	public override object Eval(InterpreterContext context)
	{
		return context.ResolveVariable(variableName);
	}
}
