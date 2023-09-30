using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Skill;

public abstract class SkillConditionDefinition : Definition
{
	public virtual string Name { get; }

	public SkillConditionDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}
}
