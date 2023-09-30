using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyAuraAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Aura;

	public bool IncludeSelf { get; private set; }

	public Node Range { get; private set; }

	public Dictionary<UnitStatDefinition.E_Stat, Node> StatModifiers { get; private set; }

	public int TurnsCount { get; private set; }

	public EnemyAuraAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Range"));
		Range = Parser.Parse(val2.Value, ((Definition)this).TokenVariables);
		IncludeSelf = ((XContainer)val).Element(XName.op_Implicit("IncludeSelf")) != null;
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("StatModifiers"));
		XAttribute val3 = obj.Attribute(XName.op_Implicit("TurnsCount"));
		TurnsCount = 1;
		if (val3 != null)
		{
			if (!int.TryParse(val3.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse TurnsCount attribute into an int in elite aura affix : \"" + val3.Value + "\"."), (Object)(object)TPSingleton<EnemyUnitDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "EnemyUnitDatabase", false);
			}
			else
			{
				TurnsCount = result;
			}
		}
		StatModifiers = new Dictionary<UnitStatDefinition.E_Stat, Node>();
		foreach (XElement item in ((XContainer)obj).Elements(XName.op_Implicit("StatModifier")))
		{
			XAttribute val4 = item.Attribute(XName.op_Implicit("Id"));
			if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val4.Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse Id attribute into a stat in elite aura affix : \"" + val4.Value + "\"."), (Object)(object)TPSingleton<EnemyUnitDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "EnemyUnitDatabase", false);
				continue;
			}
			XAttribute val5 = item.Attribute(XName.op_Implicit("Value"));
			StatModifiers.Add(result2, Parser.Parse(val5.Value, ((Definition)this).TokenVariables));
		}
	}
}
