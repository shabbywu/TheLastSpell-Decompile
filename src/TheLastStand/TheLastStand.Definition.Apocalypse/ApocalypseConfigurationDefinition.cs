using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Apocalypse;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse;

public class ApocalypseConfigurationDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<UnitStatDefinition.E_Stat, string> StatWithModifierTypes { get; } = new Dictionary<UnitStatDefinition.E_Stat, string>(UnitStatDefinition.SharedStatComparer);


	public List<ApocalypseUnlockCondition> ApocalypseUnlockConditions { get; } = new List<ApocalypseUnlockCondition>();


	public uint DamnedSoulsPercentagePerLevel { get; private set; }

	public ApocalypseConfigurationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in ((container is XElement) ? container : null).Elements())
		{
			if (item.Name.LocalName == "EnemyStatModifierConfiguration")
			{
				foreach (XElement item2 in ((XContainer)((XContainer)item).Element(XName.op_Implicit("StatModifiableValueType"))).Elements(XName.op_Implicit("StatWithModifierType")))
				{
					XAttribute val = item2.Attribute(XName.op_Implicit("Id"));
					XAttribute val2 = item2.Attribute(XName.op_Implicit("Type"));
					if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val.Value, out var result))
					{
						Debug.LogError((object)("StatWithModifierType " + val.Value + " " + HasAnInvalidStat(val.Value)));
					}
					StatWithModifierTypes.Add(result, val2.Value);
				}
			}
			else if (item.Name.LocalName == "UnlockCondition")
			{
				foreach (XElement item3 in ((XContainer)item).Elements())
				{
					string text = ((object)item3.Name).ToString();
					if (text != null && text == "RunCompletedInCity")
					{
						ApocalypseUnlockConditions.Add(new RunCompletedInCityCondition(new RunCompletedInCityConditionDefinition((XContainer)(object)item3)));
					}
				}
			}
			else if (item.Name.LocalName == "DamnedSoulsPercentagePerLevel")
			{
				if (!uint.TryParse(item.Value, out var result2))
				{
					CLoggerManager.Log((object)("Could not parse DamnedSoulsPercentagePerLevel value " + item.Value + " to a valid uint!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					DamnedSoulsPercentagePerLevel = result2;
				}
			}
		}
	}
}
