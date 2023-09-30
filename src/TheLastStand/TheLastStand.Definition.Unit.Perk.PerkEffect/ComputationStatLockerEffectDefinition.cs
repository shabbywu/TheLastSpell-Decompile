using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model.Skill;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ComputationStatLockerEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ComputationStatLocker";
	}

	public TheLastStand.Model.Skill.Skill.E_ComputationStat ComputationStat { get; private set; }

	public ComputationStatLockerEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("ComputationStat"));
		if (Enum.TryParse<TheLastStand.Model.Skill.Skill.E_ComputationStat>(val.Value, out var result))
		{
			ComputationStat = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse ComputationStat attribute into an E_ComputationStat : " + val.Value + "."), (LogType)0, (CLogLevel)2, true, "ComputationStatLockerEffectDefinition", false);
		}
	}
}
