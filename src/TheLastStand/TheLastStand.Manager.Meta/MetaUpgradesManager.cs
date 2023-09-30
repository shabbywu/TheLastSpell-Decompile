using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Meta;
using TheLastStand.Database;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Meta;
using UnityEngine;

namespace TheLastStand.Manager.Meta;

public class MetaUpgradesManager : Manager<MetaUpgradesManager>, ISerializable, IDeserializable
{
	public enum E_MetaState
	{
		NA,
		Activated,
		Unlocked,
		Locked
	}

	[SerializeField]
	private List<string> metaActivatedByDefault = new List<string>();

	private MetaUpgradesEffects activatedUpgradesEffects;

	private MetaUpgradesEffects fulfilledUpgradesEffects;

	private MetaUpgradesEffects lockedUpgradesEffects;

	private MetaUpgradesEffects unlockedUpgradesEffects;

	private readonly List<MetaUpgrade> newUpgradesInApplication = new List<MetaUpgrade>();

	public static MetaUpgradesEffects ActivatedUpgradesEffects => TPSingleton<MetaUpgradesManager>.Instance.activatedUpgradesEffects;

	public static bool AnyAvailableMandatoryUpgrade => TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Any((MetaUpgrade o) => o.MetaUpgradeDefinition.MandatoryUnlock);

	public static MetaUpgradesEffects FulfilledUpgradesEffects => TPSingleton<MetaUpgradesManager>.Instance.fulfilledUpgradesEffects;

	public static bool HiddenUpgradeLastlyActivated { get; set; }

	public static MetaUpgradesEffects LockedUpgradesEffects => TPSingleton<MetaUpgradesManager>.Instance.lockedUpgradesEffects;

	public static MetaUpgradesEffects UnlockedUpgradesEffects => TPSingleton<MetaUpgradesManager>.Instance.unlockedUpgradesEffects;

	public List<MetaUpgrade> ActivatedUpgrades { get; } = new List<MetaUpgrade>();


	public List<MetaUpgrade> FulfilledUpgrades { get; } = new List<MetaUpgrade>();


	public List<MetaUpgrade> LockedUpgrades { get; } = new List<MetaUpgrade>();


	public List<MetaUpgrade> UnlockedUpgrades { get; } = new List<MetaUpgrade>();


	public static event Action<MetaUpgrade> MetaUpgradeActivated;

	public static void ActivateNewAvailableUpgradesInApplication()
	{
		foreach (MetaUpgrade item in TPSingleton<MetaUpgradesManager>.Instance.newUpgradesInApplication)
		{
			((CLogger<MetaUpgradesManager>)TPSingleton<MetaUpgradesManager>.Instance).Log((object)("Activating new upgrade in definition compared to save: " + item.MetaUpgradeDefinition.Id + " since its activation conditions are fulfilled already."), (CLogLevel)0, false, false);
			TPSingleton<MetaConditionManager>.Instance.RefreshUpgradeProgression(item);
			if (item.MetaUpgradeController.AreActivationConditionsFulfilled())
			{
				TPSingleton<MetaUpgradesManager>.Instance.ActivateUpgrade(item, shouldRefresh: false);
			}
		}
		TPSingleton<MetaUpgradesManager>.Instance.newUpgradesInApplication.Clear();
	}

	public static bool IsThisAffixUnlockedByDefault(string id)
	{
		return MetaDatabase.DefaultEnabledIdsByMetaEffectType[typeof(UnlockAffixesMetaEffectDefinition)].Contains(id);
	}

	public static bool IsThisBuildingUnlockedByDefault(string id)
	{
		return MetaDatabase.DefaultEnabledIdsByMetaEffectType[typeof(UnlockBuildingMetaEffectDefinition)].Contains(id);
	}

	public static bool IsThisBuildingUpgradeUnlockedByDefault(string id)
	{
		return MetaDatabase.DefaultEnabledIdsByMetaEffectType[typeof(UnlockBuildingUpgradeMetaEffectDefinition)].Contains(id);
	}

	public static bool IsThisCityUnlockedByDefault(string id)
	{
		return MetaDatabase.DefaultEnabledIdsByMetaEffectType[typeof(UnlockCitiesMetaEffectDefinition)].Contains(id);
	}

	public static bool IsThisEnemyUnlockedByDefault(string id)
	{
		return MetaDatabase.DefaultEnabledIdsByMetaEffectType[typeof(NewEnemyMetaEffectDefinition)].Contains(id);
	}

	public static bool IsThisItemUnlockedByDefault(string id)
	{
		return MetaDatabase.DefaultEnabledIdsByMetaEffectType[typeof(UnlockItemsMetaEffectDefinition)].Contains(id);
	}

	public static void OnGameStateChange(Game.E_State state, Game.E_State previousState)
	{
		if (state == Game.E_State.GameOver)
		{
			ResetAllUpgradesConditionsOnRunEnd();
		}
	}

	[DevConsoleCommand("RefreshFulfilledUpgrades")]
	public static void RefreshFulfilledUpgrades()
	{
		List<MetaUpgrade> list = TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.FindAll((MetaUpgrade u) => u.MetaUpgradeController.AreActivationConditionsFulfilled());
		((CLogger<MetaUpgradesManager>)TPSingleton<MetaUpgradesManager>.Instance).Log((object)string.Format("Fulfilling {0} upgrades from the Unlocked list : {1}.", list.Count, string.Join(",", list.Select((MetaUpgrade o) => o.MetaUpgradeDefinition.Id))), (CLogLevel)0, false, false);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			TPSingleton<MetaUpgradesManager>.Instance.FulfillUpgrade(list[num]);
		}
	}

	public static void ResetAllUpgradesConditionsOnRunEnd()
	{
		((CLogger<MetaUpgradesManager>)TPSingleton<MetaUpgradesManager>.Instance).Log((object)"Reseting all meta conditions on run end.", (CLogLevel)1, false, false);
		TPSingleton<MetaConditionManager>.Instance.RenewRunContext();
	}

	public static E_MetaState TryGetUpgradeState(string upgradeId, out MetaUpgrade upgrade)
	{
		upgrade = TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Find((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == upgradeId);
		if (upgrade != null)
		{
			return E_MetaState.Activated;
		}
		upgrade = TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Find((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == upgradeId);
		if (upgrade != null)
		{
			return E_MetaState.Unlocked;
		}
		upgrade = TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.Find((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == upgradeId);
		if (upgrade != null)
		{
			return E_MetaState.Locked;
		}
		upgrade = null;
		return E_MetaState.NA;
	}

	public static void UnlockAvailableUpgrades()
	{
		List<MetaUpgrade> list = TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.FindAll((MetaUpgrade u) => u.MetaUpgradeController.AreUnlockConditionsFulfilled());
		((CLogger<MetaUpgradesManager>)TPSingleton<MetaUpgradesManager>.Instance).Log((object)$"Unlocked {list.Count} available upgrades from the Locked list.", (CLogLevel)0, false, false);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			TPSingleton<MetaUpgradesManager>.Instance.UnlockUpgrade(list[num]);
		}
	}

	public int ComputeDistanceMaxFromCenterModifiers()
	{
		int num = 0;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<WavesParametersMetaEffectDefinition>(out var effects, E_MetaState.Activated))
		{
			WavesParametersMetaEffectDefinition[] array = effects;
			foreach (WavesParametersMetaEffectDefinition wavesParametersMetaEffectDefinition in array)
			{
				num += wavesParametersMetaEffectDefinition.DistanceMaxFromCenterModifier;
			}
		}
		return num;
	}

	public int ComputeStartTraitTotalPointsModifiers()
	{
		int num = 0;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<TraitsParametersMetaEffectDefinition>(out var effects, E_MetaState.Activated))
		{
			TraitsParametersMetaEffectDefinition[] array = effects;
			foreach (TraitsParametersMetaEffectDefinition traitsParametersMetaEffectDefinition in array)
			{
				num += traitsParametersMetaEffectDefinition.StartTraitTotalPointsModifier;
			}
		}
		return num;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		activatedUpgradesEffects = new MetaUpgradeEffectsController().UpgradesEffects;
		unlockedUpgradesEffects = new MetaUpgradeEffectsController().UpgradesEffects;
		lockedUpgradesEffects = new MetaUpgradeEffectsController().UpgradesEffects;
		fulfilledUpgradesEffects = new MetaUpgradeEffectsController().UpgradesEffects;
		if (container == null)
		{
			InitializeMetaUpgrades();
		}
		else
		{
			LoadMetaUpgrades(container as SerializedMetaUpgrades);
		}
	}

	public MetaUpgradeController GetController(string definitionId)
	{
		object obj = ActivatedUpgrades.Find((MetaUpgrade o) => o.MetaUpgradeDefinition.Id == definitionId)?.MetaUpgradeController;
		if (obj == null)
		{
			obj = UnlockedUpgrades.Find((MetaUpgrade o) => o.MetaUpgradeDefinition.Id == definitionId)?.MetaUpgradeController;
			if (obj == null)
			{
				MetaUpgrade? metaUpgrade = LockedUpgrades.Find((MetaUpgrade o) => o.MetaUpgradeDefinition.Id == definitionId);
				if (metaUpgrade == null)
				{
					return null;
				}
				obj = metaUpgrade.MetaUpgradeController;
			}
		}
		return (MetaUpgradeController)obj;
	}

	public string[] GetLockedAffixesIds()
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockAffixesMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			UnlockAffixesMetaEffectDefinition[] array = effects;
			foreach (UnlockAffixesMetaEffectDefinition unlockAffixesMetaEffectDefinition in array)
			{
				list.AddRange(unlockAffixesMetaEffectDefinition.AffixesToUnlock);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockAffixesMetaEffectDefinition>(out var effects2, E_MetaState.Unlocked))
		{
			UnlockAffixesMetaEffectDefinition[] array = effects2;
			foreach (UnlockAffixesMetaEffectDefinition unlockAffixesMetaEffectDefinition2 in array)
			{
				list.AddRange(unlockAffixesMetaEffectDefinition2.AffixesToUnlock);
			}
		}
		return list.ToArray();
	}

	public string[] GetLockedBuildingActionsIds()
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockBuildingActionMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			UnlockBuildingActionMetaEffectDefinition[] array = effects;
			foreach (UnlockBuildingActionMetaEffectDefinition unlockBuildingActionMetaEffectDefinition in array)
			{
				list.Add(unlockBuildingActionMetaEffectDefinition.BuildingActionId);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockBuildingActionMetaEffectDefinition>(out var effects2, E_MetaState.Unlocked))
		{
			UnlockBuildingActionMetaEffectDefinition[] array = effects2;
			foreach (UnlockBuildingActionMetaEffectDefinition unlockBuildingActionMetaEffectDefinition2 in array)
			{
				list.Add(unlockBuildingActionMetaEffectDefinition2.BuildingActionId);
			}
		}
		return list.ToArray();
	}

	public string[] GetLockedBuildingsIds()
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockBuildingMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			UnlockBuildingMetaEffectDefinition[] array = effects;
			foreach (UnlockBuildingMetaEffectDefinition unlockBuildingMetaEffectDefinition in array)
			{
				list.Add(unlockBuildingMetaEffectDefinition.BuildingId);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockBuildingMetaEffectDefinition>(out var effects2, E_MetaState.Unlocked))
		{
			UnlockBuildingMetaEffectDefinition[] array = effects2;
			foreach (UnlockBuildingMetaEffectDefinition unlockBuildingMetaEffectDefinition2 in array)
			{
				list.Add(unlockBuildingMetaEffectDefinition2.BuildingId);
			}
		}
		return list.ToArray();
	}

	public string[] GetLockedBuildingUpgradesIds()
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockBuildingUpgradeMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			UnlockBuildingUpgradeMetaEffectDefinition[] array = effects;
			foreach (UnlockBuildingUpgradeMetaEffectDefinition unlockBuildingUpgradeMetaEffectDefinition in array)
			{
				list.Add(unlockBuildingUpgradeMetaEffectDefinition.UpgradeId);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockBuildingUpgradeMetaEffectDefinition>(out var effects2, E_MetaState.Unlocked))
		{
			UnlockBuildingUpgradeMetaEffectDefinition[] array = effects2;
			foreach (UnlockBuildingUpgradeMetaEffectDefinition unlockBuildingUpgradeMetaEffectDefinition2 in array)
			{
				list.Add(unlockBuildingUpgradeMetaEffectDefinition2.UpgradeId);
			}
		}
		return list.ToArray();
	}

	public string[] GetLockedCitiesIds()
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockCitiesMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			UnlockCitiesMetaEffectDefinition[] array = effects;
			foreach (UnlockCitiesMetaEffectDefinition unlockCitiesMetaEffectDefinition in array)
			{
				list.AddRange(unlockCitiesMetaEffectDefinition.CitiesToUnlock);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockCitiesMetaEffectDefinition>(out var effects2, E_MetaState.Unlocked))
		{
			UnlockCitiesMetaEffectDefinition[] array = effects2;
			foreach (UnlockCitiesMetaEffectDefinition unlockCitiesMetaEffectDefinition2 in array)
			{
				list.AddRange(unlockCitiesMetaEffectDefinition2.CitiesToUnlock);
			}
		}
		return list.ToArray();
	}

	public string[] GetLockedGlyphIds()
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockGlyphsMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			UnlockGlyphsMetaEffectDefinition[] array = effects;
			foreach (UnlockGlyphsMetaEffectDefinition unlockGlyphsMetaEffectDefinition in array)
			{
				list.AddRange(unlockGlyphsMetaEffectDefinition.GlyphIds);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockGlyphsMetaEffectDefinition>(out var effects2, E_MetaState.Unlocked))
		{
			UnlockGlyphsMetaEffectDefinition[] array = effects2;
			foreach (UnlockGlyphsMetaEffectDefinition unlockGlyphsMetaEffectDefinition2 in array)
			{
				list.AddRange(unlockGlyphsMetaEffectDefinition2.GlyphIds);
			}
		}
		return list.ToArray();
	}

	public string[] GetLockedItemsIds()
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockItemsMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			UnlockItemsMetaEffectDefinition[] array = effects;
			foreach (UnlockItemsMetaEffectDefinition unlockItemsMetaEffectDefinition in array)
			{
				list.AddRange(unlockItemsMetaEffectDefinition.ItemsToUnlock);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockItemsMetaEffectDefinition>(out var effects2, E_MetaState.Unlocked))
		{
			UnlockItemsMetaEffectDefinition[] array = effects2;
			foreach (UnlockItemsMetaEffectDefinition unlockItemsMetaEffectDefinition2 in array)
			{
				list.AddRange(unlockItemsMetaEffectDefinition2.ItemsToUnlock);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<LockItemsMetaEffectDefinition>(out var effects3, E_MetaState.Activated))
		{
			LockItemsMetaEffectDefinition[] array2 = effects3;
			foreach (LockItemsMetaEffectDefinition lockItemsMetaEffectDefinition in array2)
			{
				list.AddRange(lockItemsMetaEffectDefinition.ItemsToLock);
			}
		}
		return list.ToArray();
	}

	public HashSet<int> GetLockedPerkCollectionSlots()
	{
		HashSet<int> hashSet = new HashSet<int>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockPerkCollectionSlotsMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			UnlockPerkCollectionSlotsMetaEffectDefinition[] array = effects;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (int item in array[i].PerkCollectionSlotsToUnlock)
				{
					hashSet.Add(item);
				}
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockPerkCollectionSlotsMetaEffectDefinition>(out effects, E_MetaState.Unlocked))
		{
			UnlockPerkCollectionSlotsMetaEffectDefinition[] array = effects;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (int item2 in array[i].PerkCollectionSlotsToUnlock)
				{
					hashSet.Add(item2);
				}
			}
		}
		return hashSet;
	}

	public string[] GetLockedTraitsIds()
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockTraitsMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			UnlockTraitsMetaEffectDefinition[] array = effects;
			foreach (UnlockTraitsMetaEffectDefinition unlockTraitsMetaEffectDefinition in array)
			{
				list.AddRange(unlockTraitsMetaEffectDefinition.TraitsToUnlock);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockTraitsMetaEffectDefinition>(out var effects2, E_MetaState.Unlocked))
		{
			UnlockTraitsMetaEffectDefinition[] array = effects2;
			foreach (UnlockTraitsMetaEffectDefinition unlockTraitsMetaEffectDefinition2 in array)
			{
				list.AddRange(unlockTraitsMetaEffectDefinition2.TraitsToUnlock);
			}
		}
		return list.ToArray();
	}

	public string[] GetLockedWavesIds()
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockWavesMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			UnlockWavesMetaEffectDefinition[] array = effects;
			foreach (UnlockWavesMetaEffectDefinition unlockWavesMetaEffectDefinition in array)
			{
				list.AddRange(unlockWavesMetaEffectDefinition.WavesToUnlock);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockWavesMetaEffectDefinition>(out var effects2, E_MetaState.Unlocked))
		{
			UnlockWavesMetaEffectDefinition[] array = effects2;
			foreach (UnlockWavesMetaEffectDefinition unlockWavesMetaEffectDefinition2 in array)
			{
				list.AddRange(unlockWavesMetaEffectDefinition2.WavesToUnlock);
			}
		}
		return list.ToArray();
	}

	public List<string> GetUnavailableEnemiesIds()
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<NewEnemyMetaEffectDefinition>(out var effects, E_MetaState.Locked))
		{
			NewEnemyMetaEffectDefinition[] array = effects;
			foreach (NewEnemyMetaEffectDefinition newEnemyMetaEffectDefinition in array)
			{
				list.Add(newEnemyMetaEffectDefinition.EnemyId);
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<NewEnemyMetaEffectDefinition>(out var effects2, E_MetaState.Unlocked))
		{
			NewEnemyMetaEffectDefinition[] array = effects2;
			foreach (NewEnemyMetaEffectDefinition newEnemyMetaEffectDefinition2 in array)
			{
				list.Add(newEnemyMetaEffectDefinition2.EnemyId);
			}
		}
		return list;
	}

	public Vector2Int ComputeUnitTraitPointsBoundariesModifiers()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = Vector2Int.zero;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<TraitsParametersMetaEffectDefinition>(out var effects, E_MetaState.Activated))
		{
			TraitsParametersMetaEffectDefinition[] array = effects;
			foreach (TraitsParametersMetaEffectDefinition traitsParametersMetaEffectDefinition in array)
			{
				val += traitsParametersMetaEffectDefinition.UnitTraitPointsBoundariesModifiers;
			}
		}
		return val;
	}

	public ISerializedData Serialize()
	{
		return (ISerializedData)(object)new SerializedMetaUpgrades
		{
			ActivatedUpgrades = ActivatedUpgrades.Select((MetaUpgrade o) => o.Serialize() as SerializedMetaUpgrade).ToList(),
			FullfilledUpgrades = FulfilledUpgrades.Select((MetaUpgrade o) => o.Serialize() as SerializedMetaUpgrade).ToList(),
			UnlockedUpgrades = UnlockedUpgrades.Select((MetaUpgrade o) => o.Serialize() as SerializedMetaUpgrade).ToList(),
			LockedUpgrades = LockedUpgrades.Select((MetaUpgrade o) => o.Serialize() as SerializedMetaUpgrade).ToList()
		};
	}

	protected override void Awake()
	{
		base.Awake();
		MetaConditionManager.OnConditionsRefreshed += OnMetaConditionsRefreshed;
	}

	protected override void OnDestroy()
	{
		((CLogger<MetaUpgradesManager>)this).OnDestroy();
		MetaConditionManager.OnConditionsRefreshed -= OnMetaConditionsRefreshed;
	}

	public void ActivateUpgrade(MetaUpgrade upgrade, bool shouldRefresh = true)
	{
		ActivateUpgrade(upgrade.MetaUpgradeDefinition.Id, shouldRefresh);
	}

	private void ActivateUpgrade(string upgradeId, bool shouldRefresh = true)
	{
		MetaUpgrade metaUpgrade = TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Find((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == upgradeId);
		if (metaUpgrade != null)
		{
			RemoveOrAddThisMetaUpgradeFromFulfilled(metaUpgrade);
			RemoveOrAddThisMetaUpgradeFromActivated(metaUpgrade, remove: false);
		}
		else if (ListExtensions.TryFind<MetaUpgrade>(TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades, (Predicate<MetaUpgrade>)((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == upgradeId), ref metaUpgrade))
		{
			RemoveOrAddThisMetaUpgradeFromUnlocked(metaUpgrade);
			RemoveOrAddThisMetaUpgradeFromActivated(metaUpgrade, remove: false);
		}
		else if (ListExtensions.TryFind<MetaUpgrade>(TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades, (Predicate<MetaUpgrade>)((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == upgradeId), ref metaUpgrade))
		{
			RemoveOrAddThisMetaUpgradeFromLocked(metaUpgrade);
			RemoveOrAddThisMetaUpgradeFromActivated(metaUpgrade, remove: false);
		}
		if (shouldRefresh)
		{
			MetaUpgradesManager.MetaUpgradeActivated?.Invoke(metaUpgrade);
		}
		TPSingleton<AchievementManager>.Instance.HandleMetaUpgrade(upgradeId);
	}

	private void FulfillUpgrade(MetaUpgrade upgrade)
	{
		FulfillUpgrade(upgrade.MetaUpgradeDefinition.Id);
	}

	private void FulfillUpgrade(string upgradeId)
	{
		MetaUpgrade upgrade = TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Find((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == upgradeId);
		RemoveOrAddThisMetaUpgradeFromUnlocked(upgrade);
		RemoveOrAddThisMetaUpgradeFromFulfilled(upgrade, remove: false);
	}

	private void InitializeMetaUpgrades()
	{
		ActivatedUpgrades.Clear();
		FulfilledUpgrades.Clear();
		UnlockedUpgrades.Clear();
		LockedUpgrades.Clear();
		((CLogger<MetaUpgradesManager>)this).Log((object)$"Initializing {MetaDatabase.MetaUpgradesDefinitions.Count} meta upgrades", (CLogLevel)1, false, false);
		foreach (MetaUpgradeDefinition value in MetaDatabase.MetaUpgradesDefinitions.Values)
		{
			MetaUpgradeController metaUpgradeController = new MetaUpgradeController(value);
			RemoveOrAddThisMetaUpgradeFromLocked(metaUpgradeController.MetaUpgrade, remove: false);
		}
		for (int i = 0; i < metaActivatedByDefault.Count; i++)
		{
			ActivateUpgrade(metaActivatedByDefault[i], shouldRefresh: false);
		}
	}

	private void LoadMetaUpgrades(SerializedMetaUpgrades metaUpgradesElement)
	{
		ActivatedUpgrades.Clear();
		FulfilledUpgrades.Clear();
		UnlockedUpgrades.Clear();
		LockedUpgrades.Clear();
		foreach (SerializedMetaUpgrade activatedUpgrade in metaUpgradesElement.ActivatedUpgrades)
		{
			if (MetaDatabase.MetaUpgradesDefinitions.ContainsKey(activatedUpgrade.Id))
			{
				MetaUpgradeController metaUpgradeController = new MetaUpgradeController(MetaDatabase.MetaUpgradesDefinitions[activatedUpgrade.Id]);
				metaUpgradeController.MetaUpgrade.InvestedSouls = activatedUpgrade.InvestedSouls;
				RemoveOrAddThisMetaUpgradeFromActivated(metaUpgradeController.MetaUpgrade, remove: false);
			}
		}
		foreach (SerializedMetaUpgrade fullfilledUpgrade in metaUpgradesElement.FullfilledUpgrades)
		{
			if (MetaDatabase.MetaUpgradesDefinitions.ContainsKey(fullfilledUpgrade.Id))
			{
				MetaUpgradeController metaUpgradeController = new MetaUpgradeController(MetaDatabase.MetaUpgradesDefinitions[fullfilledUpgrade.Id]);
				metaUpgradeController.MetaUpgrade.InvestedSouls = fullfilledUpgrade.InvestedSouls;
				RemoveOrAddThisMetaUpgradeFromFulfilled(metaUpgradeController.MetaUpgrade, remove: false);
			}
		}
		foreach (SerializedMetaUpgrade unlockedUpgrade in metaUpgradesElement.UnlockedUpgrades)
		{
			if (MetaDatabase.MetaUpgradesDefinitions.ContainsKey(unlockedUpgrade.Id))
			{
				MetaUpgradeController metaUpgradeController = new MetaUpgradeController(MetaDatabase.MetaUpgradesDefinitions[unlockedUpgrade.Id]);
				metaUpgradeController.MetaUpgrade.InvestedSouls = unlockedUpgrade.InvestedSouls;
				RemoveOrAddThisMetaUpgradeFromUnlocked(metaUpgradeController.MetaUpgrade, remove: false);
			}
		}
		foreach (SerializedMetaUpgrade lockedUpgrade in metaUpgradesElement.LockedUpgrades)
		{
			if (MetaDatabase.MetaUpgradesDefinitions.ContainsKey(lockedUpgrade.Id))
			{
				MetaUpgradeController metaUpgradeController = new MetaUpgradeController(MetaDatabase.MetaUpgradesDefinitions[lockedUpgrade.Id]);
				metaUpgradeController.MetaUpgrade.InvestedSouls = lockedUpgrade.InvestedSouls;
				RemoveOrAddThisMetaUpgradeFromLocked(metaUpgradeController.MetaUpgrade, remove: false);
			}
		}
		foreach (MetaUpgradeDefinition upgradeDefinition in MetaDatabase.MetaUpgradesDefinitions.Values)
		{
			if (LockedUpgrades.FindAll((MetaUpgrade o) => o.MetaUpgradeDefinition.Id == upgradeDefinition.Id).Count == 0 && UnlockedUpgrades.FindAll((MetaUpgrade o) => o.MetaUpgradeDefinition.Id == upgradeDefinition.Id).Count == 0 && FulfilledUpgrades.FindAll((MetaUpgrade o) => o.MetaUpgradeDefinition.Id == upgradeDefinition.Id).Count == 0 && ActivatedUpgrades.FindAll((MetaUpgrade o) => o.MetaUpgradeDefinition.Id == upgradeDefinition.Id).Count == 0)
			{
				MetaUpgradeController metaUpgradeController2 = new MetaUpgradeController(upgradeDefinition);
				RemoveOrAddThisMetaUpgradeFromLocked(metaUpgradeController2.MetaUpgrade, remove: false);
				((CLogger<MetaUpgradesManager>)TPSingleton<MetaUpgradesManager>.Instance).Log((object)("Adding new meta upgrade that was not in save file: " + upgradeDefinition.Id + "."), (CLogLevel)1, false, false);
				newUpgradesInApplication.Add(metaUpgradeController2.MetaUpgrade);
			}
		}
		for (int i = 0; i < metaActivatedByDefault.Count; i++)
		{
			ActivateUpgrade(metaActivatedByDefault[i], shouldRefresh: false);
		}
	}

	private void OnMetaConditionsRefreshed()
	{
		UnlockAvailableUpgrades();
		RefreshFulfilledUpgrades();
	}

	private void UnlockUpgrade(MetaUpgrade upgrade)
	{
		UnlockUpgrade(upgrade.MetaUpgradeDefinition.Id);
	}

	private void UnlockUpgrade(string upgradeId)
	{
		MetaUpgrade upgrade = TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.Find((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == upgradeId);
		RemoveOrAddThisMetaUpgradeFromLocked(upgrade);
		RemoveOrAddThisMetaUpgradeFromUnlocked(upgrade, remove: false);
	}

	private void RemoveOrAddThisMetaUpgradeFromActivated(MetaUpgrade upgrade, bool remove = true)
	{
		if (remove)
		{
			ActivatedUpgrades.Remove(upgrade);
			ActivatedUpgradesEffects.MetaUpgradeEffectsController.RemoveUpgradesEffects(upgrade);
			return;
		}
		InsertIntoMetaUpgradeList(upgrade, ActivatedUpgrades);
		ActivatedUpgradesEffects.MetaUpgradeEffectsController.AddUpgradeEffects(upgrade);
		if (((StateMachine)ApplicationManager.Application).State.GetName() != "Game")
		{
			return;
		}
		for (int num = upgrade.MetaUpgradeDefinition.UpgradeEffectDefinitions.Count - 1; num >= 0; num--)
		{
			if (upgrade.MetaUpgradeDefinition.UpgradeEffectDefinitions[num] is UnitAttributeModifierMetaEffectDefinition unitAttributeModifierDefinition)
			{
				foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
				{
					playableUnit.PlayableUnitStatsController.OnStatModifierUpgradeUnlocked(unitAttributeModifierDefinition);
				}
			}
		}
	}

	private void RemoveOrAddThisMetaUpgradeFromFulfilled(MetaUpgrade upgrade, bool remove = true)
	{
		if (remove)
		{
			FulfilledUpgrades.Remove(upgrade);
			FulfilledUpgradesEffects.MetaUpgradeEffectsController.RemoveUpgradesEffects(upgrade);
		}
		else
		{
			InsertIntoMetaUpgradeList(upgrade, FulfilledUpgrades);
			FulfilledUpgradesEffects.MetaUpgradeEffectsController.AddUpgradeEffects(upgrade);
		}
	}

	private void RemoveOrAddThisMetaUpgradeFromLocked(MetaUpgrade upgrade, bool remove = true)
	{
		if (remove)
		{
			LockedUpgrades.Remove(upgrade);
			LockedUpgradesEffects.MetaUpgradeEffectsController.RemoveUpgradesEffects(upgrade);
		}
		else
		{
			InsertIntoMetaUpgradeList(upgrade, LockedUpgrades);
			LockedUpgradesEffects.MetaUpgradeEffectsController.AddUpgradeEffects(upgrade);
		}
	}

	private void RemoveOrAddThisMetaUpgradeFromUnlocked(MetaUpgrade upgrade, bool remove = true)
	{
		if (remove)
		{
			UnlockedUpgrades.Remove(upgrade);
			UnlockedUpgradesEffects.MetaUpgradeEffectsController.RemoveUpgradesEffects(upgrade);
		}
		else
		{
			InsertIntoMetaUpgradeList(upgrade, UnlockedUpgrades);
			UnlockedUpgradesEffects.MetaUpgradeEffectsController.AddUpgradeEffects(upgrade);
		}
	}

	private void InsertIntoMetaUpgradeList(MetaUpgrade upgrade, List<MetaUpgrade> upgradeList)
	{
		int num = upgradeList.Count;
		while (num > 0 && upgradeList[num - 1].MetaUpgradeDefinition.DeserializationIndex > upgrade.MetaUpgradeDefinition.DeserializationIndex)
		{
			num--;
		}
		upgradeList.Insert(num, upgrade);
	}
}
