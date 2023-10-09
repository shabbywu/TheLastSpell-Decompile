using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyReinforcedAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Reinforced;

	public Dictionary<UnitStatDefinition.E_Stat, Node> ModifiedStatsEveryXDay { get; private set; }

	public EnemyReinforcedAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		ModifiedStatsEveryXDay = new Dictionary<UnitStatDefinition.E_Stat, Node>();
		foreach (XElement item in obj.Elements(XName.op_Implicit("StatModifier")))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Id"));
			if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val.Value, out var result))
			{
				CLoggerManager.Log((object)("Id attribute could not be parsed into a stat : \"" + val.Value + "\"."), (Object)(object)TPSingleton<EnemyUnitDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "StaticLog", false);
				continue;
			}
			XAttribute val2 = item.Attribute(XName.op_Implicit("Value"));
			ModifiedStatsEveryXDay.Add(result, Parser.Parse(val2.Value, base.TokenVariables));
		}
	}
}
