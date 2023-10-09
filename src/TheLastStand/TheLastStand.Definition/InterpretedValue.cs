using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition;

public abstract class InterpretedValue<T> : TheLastStand.Framework.Serialization.Definition
{
	private T tValue;

	protected Node tExpression;

	protected InterpretedValue(XContainer container, T defaultValue = default(T), Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
		DeserializeTValue(container, defaultValue);
	}

	public override void Deserialize(XContainer container)
	{
	}

	private void DeserializeTValue(XContainer container, T defaultValue)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			tValue = defaultValue;
		}
		else if (!TryDeserializeTValue(val, out tValue))
		{
			tExpression = Parser.Parse(val.Value, base.TokenVariables);
		}
	}

	public T GetValue()
	{
		if (!TryEvalExpression(out var expressionValue))
		{
			return tValue;
		}
		return expressionValue;
	}

	protected abstract bool TryDeserializeTValue(XElement xElement, out T newTValue);

	protected abstract bool TryEvalExpression(out T expressionValue);
}
