using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Definition.Skill.SkillEffect;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyRevengeAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Revenge;

	public StatusEffectDefinition StatusEffectDefinition { get; private set; }

	public EnemyRevengeAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = ((container is XElement) ? container : null).Elements().First();
		StatusEffectDefinition = val.Name.LocalName switch
		{
			"Debuff" => new DebuffEffectDefinition((XContainer)(object)val, base.TokenVariables), 
			"Stun" => new StunEffectDefinition((XContainer)(object)val, base.TokenVariables), 
			"Poison" => new PoisonEffectDefinition((XContainer)(object)val, base.TokenVariables), 
			_ => null, 
		};
	}
}
