using UnityEngine;

namespace TheLastStand.Framework.ExpressionInterpreter;

public class DefaultInterpreterContext : InterpreterContext
{
	public DefaultInterpreterContext(object targetObject)
		: base(targetObject)
	{
	}

	public float Clamp(double a, double b, double c)
	{
		return Mathf.Clamp((float)a, (float)b, (float)c);
	}

	public float Exp(double p)
	{
		return Mathf.Exp((float)p);
	}

	public float Floor(double a)
	{
		return Mathf.Floor((float)a);
	}

	public float Ceil(double a)
	{
		return Mathf.Ceil((float)a);
	}

	public float Round(double a)
	{
		return Mathf.Round((float)a);
	}

	public float Log(double f)
	{
		return Mathf.Log((float)f);
	}

	public float Max(double a, double b)
	{
		return Mathf.Max((float)a, (float)b);
	}

	public float Min(double a, double b)
	{
		return Mathf.Min((float)a, (float)b);
	}

	public float Pow(double f, double p)
	{
		return Mathf.Pow((float)f, (float)p);
	}

	public float Sqrt(double f)
	{
		return Mathf.Sqrt((float)f);
	}

	public float BooleanChoice(bool a, double b, double c)
	{
		if (!a)
		{
			return (float)c;
		}
		return (float)b;
	}
}
