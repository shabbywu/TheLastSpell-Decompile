using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Trophy;

public class DefaultTrophyDefinition : Definition
{
	public string Id { get; protected set; }

	public bool IgnoreGem { get; private set; } = true;


	public Dictionary<int, Node> MultiplierPerDay { get; protected set; } = new Dictionary<int, Node>();


	public string BackgroundPath { get; private set; }

	public DefaultTrophyDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		XElement val2 = obj.Element(XName.op_Implicit("BackgroundPath"));
		BackgroundPath = val2.Value;
		foreach (XElement item in obj.Elements(XName.op_Implicit("DamnedSoulsMultiplier")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("NightTarget"));
			if (val3 != null)
			{
				if (!int.TryParse(val3.Value, out var result))
				{
					TPDebug.LogError((object)"Attribute DayTarget should be an integer !", (Object)null);
				}
				if (MultiplierPerDay.ContainsKey(result))
				{
					MultiplierPerDay[result] = Parser.Parse(item.Value, (Dictionary<string, string>)null);
				}
				else
				{
					MultiplierPerDay.Add(result, Parser.Parse(item.Value, (Dictionary<string, string>)null));
				}
			}
		}
	}
}
