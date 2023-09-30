using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkDataCondition;

public class PerkDataConditionsDefinition : Definition
{
	public List<APerkDataConditionDefinition> ConditionDefinitions { get; private set; }

	public PerkDataConditionsDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		ConditionDefinitions = new List<APerkDataConditionDefinition>();
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		foreach (XElement item in ((XContainer)val).Elements())
		{
			switch (item.Name.LocalName)
			{
			case "IsTrue":
				ConditionDefinitions.Add(new IsTrueDataConditionDefinition((XContainer)(object)item, ((Definition)this).TokenVariables));
				break;
			case "IsFalse":
				ConditionDefinitions.Add(new IsFalseDataConditionDefinition((XContainer)(object)item, ((Definition)this).TokenVariables));
				break;
			default:
				CLoggerManager.Log((object)("Tried to Deserialize an unimplemented PerkDataCondition: " + item.Name.LocalName), (LogType)0, (CLogLevel)2, true, "PerkDataConditionsDefinition", false);
				break;
			}
		}
	}
}
