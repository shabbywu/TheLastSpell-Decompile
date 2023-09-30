using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ContagionEffectDefinition : StatusEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Contagion";
	}

	public int Count { get; private set; } = 2;


	public override string Id => "Contagion";

	public override Status.E_StatusType StatusType => Status.E_StatusType.Contagion;

	public ContagionEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		AffectedUnits = E_SkillUnitAffect.IgnoreCaster;
		base.Deserialize(container);
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("Count"));
		if (val != null)
		{
			if (!int.TryParse(val.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse Contagion Count element value " + val.Value + " to a valid int value! Setting it to 2."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				Count = 2;
			}
			else
			{
				Count = result;
			}
		}
		else
		{
			Count = 2;
		}
	}
}
