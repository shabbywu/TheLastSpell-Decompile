using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPostcondition;

public class TargetsCountCondition : GoalConditionDefinition
{
	public const string Name = "TargetsCount";

	public int Max { get; private set; }

	public int Min { get; private set; }

	public TargetsCountCondition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Min"));
		if (val != null)
		{
			Min = int.Parse(val.Value);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Max"));
		if (val2 != null)
		{
			Max = int.Parse(val2.Value);
		}
	}
}
