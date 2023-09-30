using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ResupplySkillsSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ResupplySkills";
	}

	public int Amount { get; private set; }

	public override string Id => "ResupplySkills";

	public ResupplySkillsSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("Amount"));
		if (val != null)
		{
			if (!int.TryParse(val.Value, out var result))
			{
				CLoggerManager.Log((object)"An Amount of a SkillEffect ResupplySkills isn't a valid integer !", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				Amount = result;
			}
		}
		else
		{
			CLoggerManager.Log((object)"A SkillEffect ResupplySkills needs an Element Amount !", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
