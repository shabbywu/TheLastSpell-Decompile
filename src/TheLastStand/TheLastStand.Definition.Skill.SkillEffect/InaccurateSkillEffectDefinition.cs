using System.Globalization;
using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class InaccurateSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Inaccurate";
	}

	public override string Id => "Inaccurate";

	public float Malus { get; private set; }

	public InaccurateSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		Malus = float.Parse(((XContainer)val).Element(XName.op_Implicit("Malus")).Value, NumberStyles.Float, CultureInfo.InvariantCulture);
	}
}
