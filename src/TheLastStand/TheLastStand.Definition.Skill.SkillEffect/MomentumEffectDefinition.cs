using System.Globalization;
using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class MomentumEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const float MomentumCap = 4f;

		public const string Id = "Momentum";
	}

	public override string Id => "Momentum";

	public float DamageBonusPerTile { get; private set; }

	public MomentumEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		DamageBonusPerTile = float.Parse(((XContainer)val).Element(XName.op_Implicit("DamageBonusPerTile")).Value, NumberStyles.Float, CultureInfo.InvariantCulture);
	}
}
