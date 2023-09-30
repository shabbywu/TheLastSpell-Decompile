using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition;

public abstract class GoalConditionDefinition : Definition
{
	public GoalConditionDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}
}
