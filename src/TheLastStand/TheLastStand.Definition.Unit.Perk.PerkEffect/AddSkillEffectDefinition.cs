using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class AddSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "AddSkillEffect";
	}

	public Dictionary<string, List<SkillEffectDefinition>> SkillEffectDefinitions;

	public PerkDataConditionsDefinition PerkDataConditionsDefinition { get; private set; }

	public AddSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		SkillEffectDefinitions = new Dictionary<string, List<SkillEffectDefinition>>();
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("SkillEffects"))).Elements())
		{
			AddEffect(SkillActionDefinition.DeserializeSkillEffect(item));
		}
		PerkDataConditionsDefinition = new PerkDataConditionsDefinition((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("Conditions")), ((Definition)this).TokenVariables);
	}

	private void AddEffect(SkillEffectDefinition skillEffectDefinition)
	{
		if (!SkillEffectDefinitions.TryGetValue(skillEffectDefinition.Id, out var value))
		{
			value = new List<SkillEffectDefinition>();
			SkillEffectDefinitions.Add(skillEffectDefinition.Id, value);
		}
		value.Add(skillEffectDefinition);
	}
}
