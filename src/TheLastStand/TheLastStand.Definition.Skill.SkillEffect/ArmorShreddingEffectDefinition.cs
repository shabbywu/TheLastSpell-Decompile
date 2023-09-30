using System.Globalization;
using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ArmorShreddingEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ArmorShredding";
	}

	public override string Id => "ArmorShredding";

	public float BonusDamage { get; set; } = 1f;


	public ArmorShreddingEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val;
		if ((val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("BonusDamage"))) != null)
		{
			BonusDamage = float.Parse(val.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
		}
	}
}
