using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

public class InPlayableUnitRangConditionDefinition : SkillConditionDefinition
{
	public const string InPlayableUnitRangeName = "InPlayableUnitRange";

	public int MaxRange { get; private set; } = 1;


	public override string Name => "InPlayableUnitRange";

	public InPlayableUnitRangConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("MaxRange"));
		if (val != null && int.TryParse(val.Value, out var result))
		{
			MaxRange = result;
		}
		if (MaxRange <= 0)
		{
			MaxRange = 1;
		}
	}
}
