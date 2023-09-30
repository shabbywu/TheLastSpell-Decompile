using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class PropagationSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Propagation";
	}

	public int PropagationsCount { get; private set; }

	public override string Id => "Propagation";

	public PropagationSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("PropagationsCount"));
		PropagationsCount = int.Parse(val.Value);
	}
}
