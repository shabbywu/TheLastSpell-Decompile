using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Skill;

public class MaxTargetHealthLeftConditionDefinition : SkillConditionDefinition
{
	public const string MaxTargetHealthLeftName = "MaxTargetHealthLeft";

	public Node HealthThreshold { get; private set; }

	public override string Name => "MaxTargetHealthLeft";

	public MaxTargetHealthLeftConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		HealthThreshold = Parser.Parse(val.Value);
	}
}
