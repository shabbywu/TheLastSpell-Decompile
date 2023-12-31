using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class ItemsListDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public Dictionary<string, int> ItemsWithOdd { get; } = new Dictionary<string, int>();


	public ItemsListDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"xItemCategoriesListDefinition must have a valid Id", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = val2.Value;
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Item")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("Odd"));
			if (val3.IsNullOrEmpty() || !int.TryParse(val3.Value, out var result))
			{
				CLoggerManager.Log((object)(Id + " Invalid odd!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			XAttribute val4 = item.Attribute(XName.op_Implicit("Id"));
			if (val4.IsNullOrEmpty())
			{
				CLoggerManager.Log((object)(Id + " Invalid category!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				ItemsWithOdd.Add(val4.Value, result);
			}
		}
	}
}
