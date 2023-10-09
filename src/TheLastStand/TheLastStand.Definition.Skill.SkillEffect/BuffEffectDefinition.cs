using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class BuffEffectDefinition : StatModifierEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Buff";
	}

	private InterpretedFloat bonusInterpretedValue;

	public float Bonus => bonusInterpretedValue.GetValue();

	public override string Id => "Buff";

	public override float ModifierValue => Bonus;

	public override Status.E_StatusType StatusType => Status.E_StatusType.Buff;

	public BuffEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		bonusInterpretedValue = new InterpretedFloat((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("Bonus")), 0f, base.TokenVariables);
	}
}
