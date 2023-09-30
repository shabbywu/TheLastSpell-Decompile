using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Skill.SkillEffect;

public abstract class StatusEffectDefinition : AffectingUnitSkillEffectDefinition
{
	private InterpretedInt turnsCountInterpretedValue;

	private InterpretedFloat baseChanceInterpretedValue;

	public float BaseChance => baseChanceInterpretedValue.GetValue();

	public abstract Status.E_StatusType StatusType { get; }

	public int TurnsCount => turnsCountInterpretedValue.GetValue();

	public StatusEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		baseChanceInterpretedValue = new InterpretedFloat((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("BaseChance")), 1f, ((Definition)this).TokenVariables);
		turnsCountInterpretedValue = new InterpretedInt(((XContainer)val).Element(XName.op_Implicit("TurnsCount")), 1, ((Definition)this).TokenVariables);
	}
}
