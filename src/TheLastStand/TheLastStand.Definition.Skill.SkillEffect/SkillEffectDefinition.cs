using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Skill.SkillEffect;

public abstract class SkillEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public abstract string Id { get; }

	public virtual bool DisplayCompendiumEntry => true;

	public virtual bool ShouldBeDisplayed => true;

	public SkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
