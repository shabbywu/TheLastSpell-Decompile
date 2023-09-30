using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class UnlockContextualSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "UnlockContextualSkill";
	}

	public string ContextualSkillId { get; private set; }

	public int OverallUses { get; private set; } = -1;


	public UnlockContextualSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ContextualSkillId"));
		ContextualSkillId = val.Value;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("OverallUses"));
		if (val2 != null)
		{
			if (int.TryParse(val2.Value, out var result))
			{
				OverallUses = result;
			}
			else
			{
				CLoggerManager.Log((object)"Found a OverallUses for UnlockContextualSkill but the int parsing failed. Safely assigning null.", (LogType)2, (CLogLevel)2, true, "UnlockContextualSkillEffectDefinition", false);
			}
		}
	}
}
