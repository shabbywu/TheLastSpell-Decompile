using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class EnemiesStatModifierApocalyseEffectDefinition : ApocalypseEffectDefinition
{
	public List<string> AffectedEnemies { get; private set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public int Value { get; private set; }

	public EnemiesStatModifierApocalyseEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Stat"));
		XAttribute val3 = val.Attribute(XName.op_Implicit("Value"));
		if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val2.Value, out var result))
		{
			CLoggerManager.Log((object)("An Apocalypse EnemyStatModifier Effect's " + ((Definition)this).HasAnInvalidStat(val2.Value) + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		if (!int.TryParse(val3.Value, out var result2))
		{
			CLoggerManager.Log((object)("An Apocalypse EnemyStatModifier Effect's " + ((Definition)this).HasAnInvalidInt(val3.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		AffectedEnemies = new List<string>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("TriggeredList")))
		{
			foreach (string id in GenericDatabase.IdsListDefinitions[item.Value].Ids)
			{
				AddAffectedEnemy(id);
			}
		}
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("TriggeredId")))
		{
			AddAffectedEnemy(item2.Value);
		}
		Stat = result;
		Value = result2;
	}

	private void AddAffectedEnemy(string id)
	{
		if (!AffectedEnemies.Contains(id))
		{
			AffectedEnemies.Add(id);
		}
	}
}
