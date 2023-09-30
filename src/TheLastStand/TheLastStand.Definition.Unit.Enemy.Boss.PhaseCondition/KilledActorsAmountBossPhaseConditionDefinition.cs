using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;

public class KilledActorsAmountBossPhaseConditionDefinition : Definition, IBossPhaseConditionDefinition
{
	public string ActorId { get; private set; }

	public int Amount { get; private set; }

	public KilledActorsAmountBossPhaseConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ActorId"));
		if (val != null && val.Value != null)
		{
			ActorId = val.Value;
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Amount"));
		if (val2 != null && val2.Value != null)
		{
			if (int.TryParse(val2.Value, out var result))
			{
				Amount = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val2.Value + " into int"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
	}
}
