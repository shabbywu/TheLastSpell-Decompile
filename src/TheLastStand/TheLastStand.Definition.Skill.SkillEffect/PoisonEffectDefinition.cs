using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class PoisonEffectDefinition : StatusEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Poison";
	}

	private InterpretedFloat damagePerTurnInterpretedFloat;

	public float DamagePerTurn => damagePerTurnInterpretedFloat.GetValue();

	public override string Id => "Poison";

	public override Status.E_StatusType StatusType => Status.E_StatusType.Poison;

	public PoisonEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		AffectedUnits = E_SkillUnitAffect.IgnoreCaster;
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		damagePerTurnInterpretedFloat = new InterpretedFloat((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("DamagePerTurn")), 0f, base.TokenVariables);
	}
}
