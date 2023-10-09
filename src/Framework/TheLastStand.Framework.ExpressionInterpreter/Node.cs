using UnityEngine;

namespace TheLastStand.Framework.ExpressionInterpreter;

public abstract class Node
{
	public abstract Node Clone();

	public abstract object Eval();

	public abstract object Eval(InterpreterContext context);

	public virtual object Eval(object contextObject)
	{
		return Eval(new InterpreterContext(contextObject));
	}

	public float EvalToFloat()
	{
		object obj = Eval();
		if (obj is double num)
		{
			return (float)num;
		}
		if (obj is float)
		{
			return (float)obj;
		}
		if (obj is int num2)
		{
			return num2;
		}
		Debug.LogError((object)"Impossible to convert the expression evaluation into float");
		return -1f;
	}

	public float EvalToFloat(InterpreterContext context)
	{
		object obj = Eval(context);
		if (obj is double num)
		{
			return (float)num;
		}
		if (obj is float)
		{
			return (float)obj;
		}
		if (obj is int num2)
		{
			return num2;
		}
		Debug.LogError((object)"Impossible to convert the expression evaluation into float");
		return -1f;
	}

	public float EvalToFloat(object contextObject)
	{
		object obj = Eval(contextObject);
		if (obj is double num)
		{
			return (float)num;
		}
		if (obj is float)
		{
			return (float)obj;
		}
		if (obj is int num2)
		{
			return num2;
		}
		Debug.LogError((object)"Impossible to convert the expression evaluation into float");
		return -1f;
	}

	public int EvalToInt()
	{
		object obj = Eval();
		if (obj is double num)
		{
			return (int)num;
		}
		if (obj is float num2)
		{
			return (int)num2;
		}
		if (obj is int)
		{
			return (int)obj;
		}
		Debug.LogError((object)"Impossible to convert the expression evaluation into int");
		return -1;
	}

	public int EvalToInt(InterpreterContext context)
	{
		object obj = Eval(context);
		if (obj is double num)
		{
			return (int)num;
		}
		if (obj is float num2)
		{
			return (int)num2;
		}
		if (obj is int)
		{
			return (int)obj;
		}
		Debug.LogError((object)"Impossible to convert the expression evaluation into int");
		return -1;
	}

	public int EvalToInt(object contextObject)
	{
		object obj = Eval(contextObject);
		if (obj is double num)
		{
			return (int)num;
		}
		if (obj is float num2)
		{
			return (int)num2;
		}
		if (obj is int)
		{
			return (int)obj;
		}
		Debug.LogError((object)"Impossible to convert the expression evaluation into int");
		return -1;
	}

	public bool EvalToBool()
	{
		object obj = Eval();
		if (obj is bool)
		{
			return (bool)obj;
		}
		Debug.LogError((object)"Impossible to convert the expression evaluation into bool");
		return false;
	}

	public bool EvalToBool(InterpreterContext context)
	{
		object obj = Eval(context);
		if (obj is bool)
		{
			return (bool)obj;
		}
		Debug.LogError((object)"Impossible to convert the expression evaluation into bool");
		return false;
	}

	public bool EvalToBool(object contextObject)
	{
		object obj = Eval(contextObject);
		if (obj is bool)
		{
			return (bool)obj;
		}
		Debug.LogError((object)"Impossible to convert the expression evaluation into bool");
		return false;
	}
}
