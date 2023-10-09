using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Framework.Serialization;

public abstract class Definition : ILegacyDeserializable
{
	public Dictionary<string, string> TokenVariables { get; protected set; }

	public Definition(XContainer container, Dictionary<string, string> tokenVariables = null)
	{
		TokenVariables = tokenVariables;
		Deserialize(container);
	}

	public abstract void Deserialize(XContainer container);

	protected void DeserializeTokenVariables(XElement xTokenVariables)
	{
		TokenVariables = new Dictionary<string, string>();
		if (xTokenVariables == null)
		{
			return;
		}
		foreach (XElement item in ((XContainer)xTokenVariables).Elements(XName.op_Implicit("TokenVariable")))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Key"));
			XAttribute val2 = item.Attribute(XName.op_Implicit("Value"));
			TokenVariables.Add(val.Value, val2.Value);
		}
	}

	public string HasAnInvalid(string type, string invalidValue)
	{
		return "has an invalid " + type + " value ! (Invalid " + type + " : " + invalidValue + ")";
	}

	public string HasAnInvalidFloat(string invalidValue)
	{
		return HasAnInvalid("float", invalidValue);
	}

	public string HasAnInvalidStat(string invalidValue)
	{
		return HasAnInvalid("Stat", invalidValue);
	}

	public string HasAnInvalidInt(string invalidValue)
	{
		return HasAnInvalid("int", invalidValue);
	}

	public string OfTheItem(string itemId, int level)
	{
		return $"of the item {itemId}(Level : {level})";
	}
}
