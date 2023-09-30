using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class SwapContextualSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "SwapContextualSkill";
	}

	public string ContextualSkillIdToLock { get; private set; }

	public string ContextualSkillIdToUnlock { get; private set; }

	public int OverallUses { get; private set; } = -1;


	public SwapContextualSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ContextualSkillIdToLock"));
		ContextualSkillIdToLock = val.Value;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("ContextualSkillIdToUnlock"));
		ContextualSkillIdToUnlock = val2.Value;
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("OverallUses"));
		if (val3 != null)
		{
			if (int.TryParse(val3.Value, out var result))
			{
				OverallUses = result;
			}
			else
			{
				CLoggerManager.Log((object)"Found a OverallUses for SwapContextualSkill but the int parsing failed. Safely assigning null.", (LogType)2, (CLogLevel)2, true, "SwapContextualSkillEffectDefinition", false);
			}
		}
	}
}
