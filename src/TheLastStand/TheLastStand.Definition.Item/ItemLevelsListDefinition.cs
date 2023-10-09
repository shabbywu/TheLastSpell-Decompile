using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class ItemLevelsListDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; set; }

	public Dictionary<int, int> ItemLevelsWithOdd { get; set; } = new Dictionary<int, int>();


	public ItemLevelsListDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			Debug.LogError((object)"xItemLevelsListDefinition must have a valid Id");
			return;
		}
		Id = val2.Value;
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("ItemLevel")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("Odd"));
			if (val3.IsNullOrEmpty() || !int.TryParse(val3.Value, out var result))
			{
				Debug.LogError((object)(Id + " Invalid odd!"));
				continue;
			}
			XAttribute val4 = item.Attribute(XName.op_Implicit("Id"));
			if (val4.IsNullOrEmpty() || !int.TryParse(val4.Value, out var result2))
			{
				Debug.LogError((object)(Id + " Invalid level!"));
			}
			else
			{
				ItemLevelsWithOdd.Add(result2, result);
			}
		}
	}
}
