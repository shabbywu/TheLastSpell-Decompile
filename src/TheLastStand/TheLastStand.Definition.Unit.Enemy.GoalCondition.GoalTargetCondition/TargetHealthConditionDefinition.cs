using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class TargetHealthConditionDefinition : GoalConditionDefinition
{
	public const string Name = "TargetHealth";

	public float Max { get; private set; }

	public float Min { get; private set; }

	public TargetHealthConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Min"));
		if (val != null)
		{
			if (!float.TryParse(val.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				Debug.LogError((object)("Invalid Min (" + val.Value + ")"));
			}
			Min = result;
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Max"));
		if (val2 != null)
		{
			if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				Debug.LogError((object)("Invalid Max (" + val2.Value + ")"));
			}
			Max = result2;
		}
	}
}
