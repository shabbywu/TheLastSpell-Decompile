using System;
using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Unit;
using TheLastStand.View.Unit;

namespace TheLastStand.Model.Unit.Enemy;

public class EliteEnemyUnit : EnemyUnit
{
	public class StringToEliteEnemyUnitTemplateIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.Keys);
	}

	public class StringToEliteAffixIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(EnemyUnitDatabase.EnemyAffixDefinitions.Keys);
	}

	public override string Description
	{
		get
		{
			string result = default(string);
			if (Localizer.TryGet("EliteEnemyDescription_" + EliteEnemyUnitTemplateDefinition.EliteId, ref result))
			{
				return result;
			}
			return Localizer.Get("EnemyDescription_" + EliteEnemyUnitTemplateDefinition.Id);
		}
	}

	public EliteEnemyUnitController EliteEnemyUnitController => base.UnitController as EliteEnemyUnitController;

	public EliteEnemyUnitStatsController EliteEnemyUnitStatsController => base.UnitStatsController as EliteEnemyUnitStatsController;

	public EliteEnemyUnitTemplateDefinition EliteEnemyUnitTemplateDefinition { get; private set; }

	public override string Name
	{
		get
		{
			string result = default(string);
			if (Localizer.TryGet("EliteEnemyName_" + EliteEnemyUnitTemplateDefinition.EliteId, ref result))
			{
				return result;
			}
			return Localizer.Get("EnemyName_" + EliteEnemyUnitTemplateDefinition.Id);
		}
	}

	public override string SpecificId => EliteEnemyUnitTemplateDefinition.EliteId;

	public EliteEnemyUnit(EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition, EliteEnemyUnitController unitController, UnitView unitView, UnitCreationSettings unitCreationSettings, EnemyAffixDefinition enemyAffixDefinition = null)
		: base(eliteEnemyUnitTemplateDefinition, unitController, unitView, unitCreationSettings)
	{
		EliteEnemyUnitTemplateDefinition = eliteEnemyUnitTemplateDefinition;
		if (enemyAffixDefinition == null)
		{
			PickRandomAffix();
		}
		else
		{
			base.Affixes.Insert(0, CreateAffix(enemyAffixDefinition));
		}
	}

	public EliteEnemyUnit(EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition, SerializedEliteEnemyUnit serializedEliteEnemyUnit, EliteEnemyUnitController unitController, UnitView unitView, UnitCreationSettings unitCreationSettings, int saveVersion)
		: base(eliteEnemyUnitTemplateDefinition, unitController, unitView, unitCreationSettings)
	{
		EliteEnemyUnitTemplateDefinition = eliteEnemyUnitTemplateDefinition;
		Deserialize((ISerializedData)(object)serializedEliteEnemyUnit, saveVersion);
	}

	private void PickRandomAffix()
	{
		if (!EnemyUnitDatabase.EliteToAffixDefinitions.TryGetValue(SpecificId, out var value))
		{
			return;
		}
		float totalWeight = 0f;
		value.ForEach(delegate(EnemyAffixDefinition potentialAffix)
		{
			totalWeight += potentialAffix.Weight;
		});
		float randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0f, totalWeight);
		for (int i = 0; i < value.Count; i++)
		{
			totalWeight -= value[i].Weight;
			if (totalWeight <= randomRange)
			{
				base.Affixes.Insert(0, CreateAffix(value[i]));
				return;
			}
		}
		if (value.Count > 0)
		{
			((CLogger<EnemyUnitManager>)TPSingleton<EnemyUnitManager>.Instance).LogWarning((object)"Something went wrong with the affix picking algorithm, ask Matthieu H to fix this.", (CLogLevel)1, true, false);
			base.Affixes.Insert(0, CreateAffix(value[0]));
		}
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedEliteEnemyUnit serializedEliteEnemyUnit = container as SerializedEliteEnemyUnit;
		EnemyAffixDefinition affixDefinition = default(EnemyAffixDefinition);
		if (EnemyUnitDatabase.EliteToAffixDefinitions.TryGetValue(SpecificId, out var value) && ListExtensions.TryFind<EnemyAffixDefinition>(value, (Predicate<EnemyAffixDefinition>)((EnemyAffixDefinition x) => x.Id == serializedEliteEnemyUnit.AffixId), ref affixDefinition))
		{
			base.Affixes.Insert(0, CreateAffix(affixDefinition));
		}
		else
		{
			PickRandomAffix();
		}
		ADeserialize((ISerializedData)(object)serializedEliteEnemyUnit, saveVersion);
	}

	public override void DeserializeAfterInit(ISerializedData container, int saveVersion)
	{
		DeserializeStats((ISerializedData)(object)(container as SerializedEliteEnemyUnit)?.EliteEnemyUnitStats, saveVersion);
	}

	public override void DeserializeStats(ISerializedData serializedUnitStats, int saveVersion)
	{
		SerializedEliteEnemyUnitStats serializedUnitStats2 = serializedUnitStats as SerializedEliteEnemyUnitStats;
		base.UnitStatsController = new EliteEnemyUnitStatsController(serializedUnitStats2, this, saveVersion);
	}

	public override ISerializedData Serialize()
	{
		return (ISerializedData)(object)new SerializedEliteEnemyUnit(SerializeUnit())
		{
			Id = EliteEnemyUnitTemplateDefinition.EliteId,
			BossPhaseActorId = base.BossPhaseActorId,
			OverrideVariantId = base.CurrentVariantIndex,
			LinkedBuilding = LinkedBuilding?.RandomId,
			IsGuardian = base.IsGuardian,
			IgnoreFromEnemyUnitsCount = base.IgnoreFromEnemyUnitsCount,
			LastHourInFog = base.LastHourInFog,
			LastHourInAnyFog = base.LastHourInAnyFog,
			SerializedBehavior = new SerializedBehavior(this),
			AffixId = base.Affixes[0].EnemyAffixDefinition.Id,
			EliteEnemyUnitStats = (EliteEnemyUnitStatsController.EliteEnemyUnitStats.Serialize() as SerializedEliteEnemyUnitStats)
		};
	}
}
