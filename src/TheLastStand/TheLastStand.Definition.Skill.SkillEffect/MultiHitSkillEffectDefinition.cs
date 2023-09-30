using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class MultiHitSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "MultiHit";
	}

	public int HitsCount { get; private set; }

	public override string Id => "MultiHit";

	public MultiHitSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("HitsCount"));
		HitsCount = int.Parse(val.Value);
	}
}
