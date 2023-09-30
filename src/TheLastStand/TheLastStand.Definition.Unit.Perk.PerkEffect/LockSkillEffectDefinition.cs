using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class LockSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "LockSkill";
	}

	public string SkillId { get; private set; }

	public LockSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("SkillId"));
		SkillId = val.Value;
	}
}
