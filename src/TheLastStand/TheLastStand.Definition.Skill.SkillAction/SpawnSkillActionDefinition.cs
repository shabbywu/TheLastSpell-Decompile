using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillAction;

public class SpawnSkillActionDefinition : SkillActionDefinition
{
	public const string Name = "Spawn";

	public List<string> BuildingIdsToDestroy = new List<string>();

	public float Delay { get; private set; }

	public List<EnemySpawnData> EnemiesByAmount { get; } = new List<EnemySpawnData>();


	public List<EnemySpawnData> EnemiesByWeight { get; } = new List<EnemySpawnData>();


	public List<EnemySpawnData> RandomEnemies { get; } = new List<EnemySpawnData>();


	public bool IsByAmount { get; private set; }

	public Node RandomEnemiesAmount { get; private set; }

	public SpawnSkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("Spawn"));
		XAttribute val2 = val.Attribute(XName.op_Implicit("Delay"));
		if (val2 != null)
		{
			if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log((object)("Could not parse SpawnSkillAction Delay attribute value " + val2.Value + " to a valid float!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				Delay = result;
			}
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("ByWeight"));
		if (val3 != null)
		{
			foreach (XElement item in ((XContainer)val3).Elements(XName.op_Implicit("Enemy")))
			{
				EnemiesByWeight.Add(DeserializeEnemyElement(item));
			}
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("ByAmount"));
		if (val4 != null)
		{
			foreach (XElement item2 in ((XContainer)val4).Elements(XName.op_Implicit("Enemy")))
			{
				EnemiesByAmount.Add(DeserializeEnemyElement(item2));
			}
			XElement val5 = ((XContainer)val4).Element(XName.op_Implicit("RandomEnemies"));
			if (val5 != null)
			{
				XAttribute val6 = val5.Attribute(XName.op_Implicit("Amount"));
				if (val6 != null)
				{
					RandomEnemiesAmount = Parser.Parse(val6.Value, (Dictionary<string, string>)null);
				}
				foreach (XElement item3 in ((XContainer)val5).Elements(XName.op_Implicit("Enemy")))
				{
					RandomEnemies.Add(DeserializeEnemyElement(item3));
				}
			}
			if (RandomEnemiesAmount == null)
			{
				Node val8 = (RandomEnemiesAmount = Parser.Parse("0", (Dictionary<string, string>)null));
			}
		}
		IsByAmount = EnemiesByAmount.Count > 0 || RandomEnemies.Count > 0;
		XElement val9 = ((XContainer)val).Element(XName.op_Implicit("BuildingDestructionRule"));
		if (val9 == null)
		{
			return;
		}
		foreach (XElement item4 in ((XContainer)val9).Elements(XName.op_Implicit("BuildingsList")))
		{
			XAttribute val10 = item4.Attribute(XName.op_Implicit("Id"));
			if (val10 == null)
			{
				CLoggerManager.Log((object)"SpawnSkillAction has an empty Id of a BuildingDestructionRule/BuildingsList !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (!GenericDatabase.IdsListDefinitions.TryGetValue(val10.Value, out var value))
			{
				CLoggerManager.Log((object)("Trying to get Ids List " + val10.Value + " but it does not exist."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			foreach (string id in value.Ids)
			{
				if (!BuildingIdsToDestroy.Contains(id))
				{
					BuildingIdsToDestroy.Add(id);
				}
			}
		}
	}

	private EnemySpawnData DeserializeEnemyElement(XElement enemyElement)
	{
		XAttribute val = enemyElement.Attribute(XName.op_Implicit("Id"));
		int result = -1;
		TileFlagDefinition.E_TileFlagTag result2 = TileFlagDefinition.E_TileFlagTag.None;
		string bossPhaseActorId = null;
		XAttribute val2 = enemyElement.Attribute(XName.op_Implicit("Weight"));
		if (val2 != null && !int.TryParse(val2.Value, out result))
		{
			CLoggerManager.Log((object)$"Could not parse Weight \"{val2.Value}\" of {((object)this).GetType().Name} to a valid int! (Enemy to spawn Id: {val})", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val3 = enemyElement.Attribute(XName.op_Implicit("Amount"));
		Node amount = ((val3 == null) ? Parser.Parse("1", (Dictionary<string, string>)null) : Parser.Parse(val3.Value, (Dictionary<string, string>)null));
		XAttribute val4 = enemyElement.Attribute(XName.op_Implicit("FlagTag"));
		if (val4 != null && !Enum.TryParse<TileFlagDefinition.E_TileFlagTag>(val4.Value, out result2))
		{
			CLoggerManager.Log((object)$"Could not parse FlagTag \"{val4.Value}\" of {((object)this).GetType().Name} to a valid TileFlag! (Enemy to spawn Id: {val})", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val5 = enemyElement.Attribute(XName.op_Implicit("BossActorId"));
		if (val5 != null)
		{
			bossPhaseActorId = val5.Value;
		}
		bool result3 = false;
		XAttribute val6 = enemyElement.Attribute(XName.op_Implicit("IsGuardian"));
		if (val6 != null && !bool.TryParse(val6.Value, out result3))
		{
			CLoggerManager.Log((object)("Unable to parse " + val6.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		return new EnemySpawnData(val.Value, result, amount, result2, new UnitCreationSettings(bossPhaseActorId, castSpawnSkill: true, playSpawnAnim: true, playSpawnCutscene: true, waitSpawnAnim: false, -1, null, result3));
	}
}
