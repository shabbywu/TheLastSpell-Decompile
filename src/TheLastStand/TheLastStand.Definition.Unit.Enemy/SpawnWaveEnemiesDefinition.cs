using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SpawnWaveEnemiesDefinition : TheLastStand.Framework.Serialization.Definition
{
	public BossWaveSettings BossWaveSettings { get; private set; }

	public Dictionary<string, int> EliteEnemyUnitTemplateDefinitions { get; private set; } = new Dictionary<string, int>();


	public bool EliteOverrideSpawnDefinition { get; private set; }

	public Dictionary<string, int> EnemyUnitTemplateDefinitions { get; private set; } = new Dictionary<string, int>();


	public List<Tuple<int, float>> EnemyTierDefinitions { get; private set; } = new List<Tuple<int, float>>();


	public List<int> AllTiers { get; private set; } = new List<int>();


	public List<SeerAdditionalPortraitSettings> SeerAdditionalPortraitsSettings { get; private set; } = new List<SeerAdditionalPortraitSettings>();


	public SpawnWaveEnemiesDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("EnemyUnitTemplateDefinitions"));
		if (val2 == null)
		{
			CLoggerManager.Log((object)"SpawnWaveDefinition has no EnemyUnitTemplateDefinitions!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("BossWaveSettings"));
		if (val3 != null)
		{
			BossWaveSettings = new BossWaveSettings(val3);
		}
		foreach (XElement item2 in ((XContainer)val2).Elements(XName.op_Implicit("EnemyUnitTemplateDefinition")))
		{
			XAttribute val4 = item2.Attribute(XName.op_Implicit("Id"));
			if (val4.IsNullOrEmpty())
			{
				CLoggerManager.Log((object)"EnemyUnitTemplateDefinition has no Id!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			XAttribute val5 = item2.Attribute(XName.op_Implicit("Weight"));
			if (val5.IsNullOrEmpty())
			{
				CLoggerManager.Log((object)"EnemyUnitTemplateDefinition has no Weight!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (!int.TryParse(val5.Value, out var result))
			{
				CLoggerManager.Log((object)"EnemyUnitTemplateDefinition has an invalid weight!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			EnemyUnitTemplateDefinitions.Add(val4.Value, result);
			int tier = EnemyUnitDatabase.EnemyUnitTemplateDefinitions[val4.Value].Tier;
			if (!AllTiers.Contains(tier))
			{
				AllTiers.Add(tier);
			}
		}
		foreach (XElement item3 in ((XContainer)val2).Elements(XName.op_Implicit("EnemyTier")))
		{
			XAttribute val6 = item3.Attribute(XName.op_Implicit("Value"));
			if (val6.IsNullOrEmpty())
			{
				CLoggerManager.Log((object)"EnemyTier has no Value!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			float item = 1f;
			XAttribute val7 = item3.Attribute(XName.op_Implicit("WeightMultiplier"));
			if (!val7.IsNullOrEmpty())
			{
				if (!float.TryParse(val7.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					CLoggerManager.Log((object)"EnemyTier has an invalid weight multiplier (setting it to 1)!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				item = result2;
			}
			if (!int.TryParse(val6.Value, out var result3))
			{
				CLoggerManager.Log((object)"EnemyUnitTemplateDefinition has an invalid tier (setting it to 1)!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			EnemyTierDefinitions.Add(new Tuple<int, float>(result3, item));
			if (!AllTiers.Contains(result3))
			{
				AllTiers.Add(result3);
			}
		}
		XElement val8 = ((XContainer)val2).Element(XName.op_Implicit("EliteEnemies"));
		if (val8 != null)
		{
			XAttribute val9 = val8.Attribute(XName.op_Implicit("OverrideSpawnDefinition"));
			if (!bool.TryParse(val9.Value, out var result4))
			{
				CLoggerManager.Log((object)("Could not parse attribute OverrideSpawnDefinition into a bool : '" + val9.Value + "'."), (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "SpawnWaveDatabase", false);
			}
			EliteOverrideSpawnDefinition = result4;
			EliteEnemyUnitTemplateDefinitions.Clear();
			foreach (XElement item4 in ((XContainer)val8).Elements(XName.op_Implicit("EliteEnemy")))
			{
				XAttribute val10 = item4.Attribute(XName.op_Implicit("Id"));
				XAttribute val11 = item4.Attribute(XName.op_Implicit("Nb"));
				if (!int.TryParse(val11.Value, out var result5))
				{
					CLoggerManager.Log((object)("Could not parse attribute Nb into an int : '" + val11.Value + "'."), (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "SpawnWaveDatabase", false);
				}
				if (EliteEnemyUnitTemplateDefinitions.ContainsKey(val10.Value))
				{
					CLoggerManager.Log((object)("Duplicate of : '" + val10.Value + "' in EliteEnemies definition. Adding them."), (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)2, (CLogLevel)2, true, "SpawnWaveDatabase", false);
					EliteEnemyUnitTemplateDefinitions[val10.Value] += result5;
				}
				else
				{
					EliteEnemyUnitTemplateDefinitions.Add(val10.Value, result5);
				}
			}
		}
		SeerAdditionalPortraitsSettings.Clear();
		XElement val12 = ((XContainer)val).Element(XName.op_Implicit("SeerAdditionalPortraitsSettings"));
		if (val12 == null)
		{
			return;
		}
		foreach (XElement item5 in ((XContainer)val12).Elements(XName.op_Implicit("SeerAdditionalPortraitSettings")))
		{
			SeerAdditionalPortraitsSettings.Add(new SeerAdditionalPortraitSettings(item5));
		}
	}

	public int GetMaximumEnemiesVariations()
	{
		int num = 0;
		for (int num2 = AllTiers.Count - 1; num2 >= 0; num2--)
		{
			num += EnemyUnitDatabase.EnemyUnitTemplatesByTierDefinitions[AllTiers[num2]].Count;
		}
		return num;
	}
}
