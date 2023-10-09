using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class ComboDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<int, int> Multipliers { get; private set; } = new Dictionary<int, int>();


	public int EnemyAttacksReceivedForOnePenalty { get; private set; } = 1;


	public ComboDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in container.Elements(XName.op_Implicit("Multiplier")))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Step"));
			if (val.IsNullOrEmpty())
			{
				Debug.LogError((object)"The Multiplier must have Step!");
				continue;
			}
			if (!int.TryParse(val.Value, out var result))
			{
				Debug.LogError((object)"The Step must be a valid integer!");
				continue;
			}
			XElement val2 = ((XContainer)item).Element(XName.op_Implicit("KillsNeeded"));
			int result2;
			if (val2.IsNullOrEmpty())
			{
				Debug.LogError((object)"The Multiplier must have KillsNeeded!");
			}
			else if (!int.TryParse(val2.Value, out result2))
			{
				Debug.LogError((object)"The killsNeeded must be a valid integer!");
			}
			else
			{
				Multipliers.Add(result, result2);
			}
		}
		if (!Multipliers.ContainsKey(1))
		{
			Multipliers.Add(1, 0);
		}
		Multipliers = Multipliers.OrderBy((KeyValuePair<int, int> x) => x.Key).ToDictionary((KeyValuePair<int, int> x) => x.Key, (KeyValuePair<int, int> x) => x.Value);
		XElement val3 = container.Element(XName.op_Implicit("EnemyAttacksReceivedForOnePenalty"));
		int result3;
		if (val3.IsNullOrEmpty())
		{
			Debug.LogError((object)"The Combo Definition must have EnemyAttacksReceivedForOnePenalty!");
		}
		else if (!int.TryParse(val3.Value, out result3) && result3 > 0)
		{
			Debug.LogError((object)"The enemyAttacksReceivedForOnePenalty must be a valid integer and higher than 0!");
		}
		else
		{
			EnemyAttacksReceivedForOnePenalty = result3;
		}
	}
}
