using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class DebuffEffectDefinition : StatModifierEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Debuff";
	}

	private InterpretedFloat malusInterpretedValue;

	public override string Id => "Debuff";

	public float Malus => malusInterpretedValue.GetValue();

	public override float ModifierValue => Malus;

	public override Status.E_StatusType StatusType => Status.E_StatusType.Debuff;

	public DebuffEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		AffectedUnits = E_SkillUnitAffect.IgnoreCaster;
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		malusInterpretedValue = new InterpretedFloat((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("Malus")), 0f, ((Definition)this).TokenVariables);
	}
}
