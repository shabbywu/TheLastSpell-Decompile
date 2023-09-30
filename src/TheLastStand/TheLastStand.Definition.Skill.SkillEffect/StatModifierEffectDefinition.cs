using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public abstract class StatModifierEffectDefinition : StatusEffectDefinition
{
	public abstract float ModifierValue { get; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	protected StatModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (Enum.TryParse<UnitStatDefinition.E_Stat>(((container is XElement) ? container : null).Element(XName.op_Implicit("Stat")).Attribute(XName.op_Implicit("Id")).Value, out var result))
		{
			Stat = result;
		}
		else
		{
			Debug.LogError((object)$"Unknown stat {result}");
		}
	}
}
