using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building;

public class ShopEvolutionDefinition : Definition
{
	public List<Tuple<int, int>> LevelsPerDay { get; private set; }

	public ShopEvolutionDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		LevelsPerDay = new List<Tuple<int, int>>();
		foreach (XElement item in obj.Elements(XName.op_Implicit("Day")))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Index"));
			XAttribute val2 = item.Attribute(XName.op_Implicit("Level"));
			LevelsPerDay.Add(new Tuple<int, int>(int.Parse(val.Value), int.Parse(val2.Value)));
		}
	}
}
