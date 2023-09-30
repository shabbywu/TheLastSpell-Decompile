using System.Globalization;
using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class OpportunisticSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Opportunistic";
	}

	public override string Id => "Opportunistic";

	public float DamageMultiplier { get; private set; }

	public OpportunisticSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		DamageMultiplier = float.Parse(((XContainer)val).Element(XName.op_Implicit("DamageMultiplier")).Value, NumberStyles.Float, CultureInfo.InvariantCulture);
	}
}
