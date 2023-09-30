using System;
using System.Collections.Generic;
using System.Linq;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization;

namespace TheLastStand.Model;

public struct KillReportData
{
	public float BaseExperience { get; }

	public EnemyUnitTemplateDefinition EnemyUnitTemplateDefinition { get; }

	public Dictionary<int, int> KillAmountPerKillerId { get; }

	public string SpecificAssetsId => EnemyUnitTemplateDefinition.SpecificAssetsId;

	public bool HideInNightReport => EnemyUnitTemplateDefinition.HideInNightReport;

	public string Id => EnemyUnitTemplateDefinition.Id;

	public int KillAmount => KillAmountPerKillerId.Sum((KeyValuePair<int, int> x) => x.Value);

	public string SpecificId
	{
		get
		{
			if (!(EnemyUnitTemplateDefinition is EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition))
			{
				return Id;
			}
			return eliteEnemyUnitTemplateDefinition.EliteId;
		}
	}

	public float TotalBaseExperience => BaseExperience * (float)KillAmount;

	public float TotalExperienceToShare => EnemyUnitController.ComputeExperienceGain(TotalBaseExperience);

	public KillReportData(EnemyUnit enemyUnit, IEntity killer)
	{
		BaseExperience = enemyUnit.UnitStatsController.UnitStats.Stats[UnitStatDefinition.E_Stat.ExperienceGain].FinalClamped;
		EnemyUnitTemplateDefinition = enemyUnit.EnemyUnitTemplateDefinition;
		KillAmountPerKillerId = new Dictionary<int, int>();
		KillAmountPerKillerId.Add(killer?.RandomId ?? (-1), 1);
	}

	public KillReportData(SerializedKillReportData serializedData, EnemyUnitTemplateDefinition enemyUnitTemplateDefinition)
	{
		BaseExperience = serializedData.BaseExperience;
		EnemyUnitTemplateDefinition = enemyUnitTemplateDefinition;
		KillAmountPerKillerId = new Dictionary<int, int>();
		int i = 0;
		for (int count = serializedData.KillAmountPerKillerId.Count; i < count; i++)
		{
			SerializedIntPair serializedIntPair = serializedData.KillAmountPerKillerId[i];
			KillAmountPerKillerId.Add(serializedIntPair.UnitId, serializedIntPair.Value);
		}
	}

	public float GetTotalExperienceForEntity(IEntity entity)
	{
		return EnemyUnitController.ComputeExperienceGain(BaseExperience * (float)DictionaryExtensions.GetValueOrDefault<int, int>(KillAmountPerKillerId, entity.RandomId) * PlayableUnitDatabase.KillerBonusExperienceFactor);
	}

	public int GetKillAmountForEntity(IEntity entity)
	{
		return DictionaryExtensions.GetValueOrDefault<int, int>(KillAmountPerKillerId, entity.RandomId);
	}

	public void AddKillForEntity(IEntity killer, int amount = 1)
	{
		DictionaryExtensions.AddValueOrCreateKey<int, int>(KillAmountPerKillerId, killer?.RandomId ?? (-1), amount, (Func<int, int, int>)((int a, int b) => a + b));
	}

	public SerializedKillReportData Serialize()
	{
		List<SerializedIntPair> list = new List<SerializedIntPair>();
		foreach (KeyValuePair<int, int> item2 in KillAmountPerKillerId)
		{
			SerializedIntPair serializedIntPair = default(SerializedIntPair);
			serializedIntPair.UnitId = item2.Key;
			serializedIntPair.Value = item2.Value;
			SerializedIntPair item = serializedIntPair;
			list.Add(item);
		}
		SerializedKillReportData result = default(SerializedKillReportData);
		result.BaseExperience = BaseExperience;
		result.EnemyUnitSpecificId = SpecificId;
		result.KillAmountPerKillerId = list;
		return result;
	}
}
