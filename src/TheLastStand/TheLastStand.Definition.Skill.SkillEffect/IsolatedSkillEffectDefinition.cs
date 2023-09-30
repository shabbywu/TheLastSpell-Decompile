using System.Globalization;
using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class IsolatedSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Isolated";
	}

	public override string Id => "Isolated";

	public float DamageMultiplier { get; private set; }

	public IsolatedSkillEffectDefinition(XContainer container)
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
