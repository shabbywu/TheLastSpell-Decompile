using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Skill.SkillAction.BuildLocation;

public abstract class BuildLocationDefinition : Definition
{
	public static class Names
	{
		public const string AroundTheCasterName = "AroundTheCaster";
	}

	public abstract string Name { get; }

	public BuildLocationDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
