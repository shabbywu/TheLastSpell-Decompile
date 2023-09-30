using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class TargetIdConditionDefinition : GoalConditionDefinition
{
	public const string Name = "TargetId";

	public bool Exclude { get; private set; }

	public string[] TargetIds { get; private set; }

	public TargetIdConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Exclude"));
		if (val2 != null)
		{
			Exclude = bool.Parse(val2.Value);
		}
		List<string> list = new List<string>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("TargetsListId")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("Value"));
			foreach (string id in GenericDatabase.IdsListDefinitions[val3.Value].Ids)
			{
				if (!list.Contains(id))
				{
					list.Add(id);
				}
			}
		}
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("TargetId")))
		{
			XAttribute val4 = item2.Attribute(XName.op_Implicit("Value"));
			if (!list.Contains(val4.Value))
			{
				list.Add(val4.Value);
			}
		}
		TargetIds = list.ToArray();
	}
}
