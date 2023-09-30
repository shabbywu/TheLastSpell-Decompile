using System;
using System.Xml.Linq;
using TheLastStand.Definition.TileMap;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class FlagTagConditionDefinition : GoalConditionDefinition
{
	public const string Name = "FlagTag";

	public TileFlagDefinition.E_TileFlagTag FlagTag { get; private set; }

	public FlagTagConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!Enum.TryParse<TileFlagDefinition.E_TileFlagTag>(((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("FlagTag")).Value, out var result))
		{
			Debug.LogError((object)"Invalid FlagTag");
		}
		FlagTag = result;
	}
}
