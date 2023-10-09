using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Trait;

public class UnitTraitTierDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string DefaultTierId = "Default";
	}

	public HashSet<int> Costs { get; private set; }

	public string Id { get; private set; }

	public bool IsBackground { get; private set; }

	public UnitTraitTierDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("IsBackground"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result))
			{
				IsBackground = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse IsBackground into a bool for UnitTraitTierDefinition \"" + Id + "\"."), (LogType)0, (CLogLevel)2, true, "UnitTraitTierDefinition", false);
			}
		}
		Costs = new HashSet<int>();
		foreach (XElement item in obj.Elements(XName.op_Implicit("Cost")))
		{
			if (int.TryParse(item.Attribute(XName.op_Implicit("Value")).Value, out var result2))
			{
				Costs.Add(result2);
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse Cost Value attribute into an int in UnitTraitTierDefinition \"" + Id + "\"."), (LogType)0, (CLogLevel)2, true, "UnitTraitTierDefinition", false);
			}
		}
	}
}
