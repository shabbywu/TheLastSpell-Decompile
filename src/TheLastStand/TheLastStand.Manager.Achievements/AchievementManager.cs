using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Controller.TileMap;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Achievements;

namespace TheLastStand.Manager.Achievements;

public sealed class AchievementManager : Manager<AchievementManager>
{
	private AAchievementHandler achievementHandler;

	private AchievementUnlocker crit20EnemiesTurnAchievementUnlocker;

	private AchievementUnlocker stun50EnemiesNightAchievementUnlocker;

	private AchievementUnlocker perfectWinAchievementUnlocker;

	private HashSet<string> remainingBuildingsToBuild;

	private HashSet<string> remainingWeaponsToUnlock;

	private HashSet<Achievement> remainingAchievementsToUnlock;

	private bool isInitialized;

	private void Start()
	{
		InitIfNeeded();
	}

	public void CheckPlatinumAchievement()
	{
		HashSet<Achievement> hashSet = remainingAchievementsToUnlock;
		if (hashSet != null && hashSet.Count == 0)
		{
			UnlockAchievement(AchievementContainer.ACH_ALL_ACHIEVEMENTS);
		}
	}

	public void RefreshAchievements()
	{
		achievementHandler?.RefreshAchievements();
		CheckPlatinumAchievement();
	}

	public void SetAchievementProgression(Stat stat, int value, bool refreshAchievements = true)
	{
		achievementHandler?.SetAchievementProgression(stat, value, refreshAchievements);
		if (refreshAchievements)
		{
			CheckPlatinumAchievement();
		}
	}

	public void IncreaseAchievementProgression(Stat stat, int value, bool refreshAchievements = true)
	{
		achievementHandler?.IncreaseAchievementProgression(stat, value, refreshAchievements);
		if (refreshAchievements)
		{
			CheckPlatinumAchievement();
		}
	}

	public void UnlockAchievement(Achievement achievement, bool refreshIfAchieved = true)
	{
		achievementHandler?.UnlockAchievement(achievement, refreshIfAchieved);
		if (remainingAchievementsToUnlock != null && remainingAchievementsToUnlock.Remove(achievement) && refreshIfAchieved)
		{
			CheckPlatinumAchievement();
		}
	}

	private void InitIfNeeded()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			Init();
		}
	}

	private void Init()
	{
		achievementHandler = new SteamAchievementHandler();
		InitData();
	}

	public void InitData()
	{
		remainingBuildingsToBuild = new HashSet<string>();
		List<string> buildingsBuilt = TPSingleton<MetaConditionManager>.Instance.CampaignContext.BuildingsBuilt;
		foreach (KeyValuePair<string, BuildingDefinition> buildingDefinition in BuildingDatabase.BuildingDefinitions)
		{
			if (buildingDefinition.Value.ConstructionModuleDefinition.IsBuyable && !buildingsBuilt.Contains(buildingDefinition.Key))
			{
				remainingBuildingsToBuild.Add(buildingDefinition.Key);
			}
		}
		remainingWeaponsToUnlock = new HashSet<string>(AchievementContainer.UnlockAllWeapons);
		foreach (MetaUpgrade activatedUpgrade in TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades)
		{
			remainingWeaponsToUnlock.Remove(activatedUpgrade.MetaUpgradeDefinition.Id);
		}
		remainingAchievementsToUnlock = new HashSet<Achievement>(AchievementContainer.AllAchievements);
		if (achievementHandler != null)
		{
			foreach (Achievement allAchievement in AchievementContainer.AllAchievements)
			{
				if (achievementHandler.IsAchievementUnlocked(allAchievement))
				{
					remainingAchievementsToUnlock.Remove(allAchievement);
				}
			}
		}
		if (crit20EnemiesTurnAchievementUnlocker == null)
		{
			crit20EnemiesTurnAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_CRIT_20_ENEMIES_TURN, 20);
		}
		if (stun50EnemiesNightAchievementUnlocker == null)
		{
			stun50EnemiesNightAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_STUN_50_ENEMIES_NIGHT, 50);
		}
		if (perfectWinAchievementUnlocker == null)
		{
			perfectWinAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_PERFECT_WIN, 14);
		}
	}

	public void TriggerApplicationBackwardCompatibility(int saveVersion)
	{
		InitIfNeeded();
		if (saveVersion <= 11)
		{
			SetAchievementProgression(StatContainer.STAT_ENEMIES_KILLED_AMOUNT, (int)TPSingleton<MetaConditionManager>.Instance.CampaignContext.GetDouble(MetaConditionSpecificContext.E_ValueCategory.Kills), refreshAchievements: false);
			SetAchievementProgression(StatContainer.STAT_POTION_USES_AMOUNT, (int)TPSingleton<MetaConditionManager>.Instance.CampaignContext.GetDouble(MetaConditionSpecificContext.E_ValueCategory.PotionsUsed), refreshAchievements: false);
			SetAchievementProgression(StatContainer.STAT_NUMBER_OF_RUNS, (int)ApplicationManager.Application.RunsCompleted, refreshAchievements: false);
			SetAchievementProgression(StatContainer.STAT_COMPLETED_GLYPHS_AMOUNT, TPSingleton<GlyphManager>.Instance.MaxApoPassedByCityByGlyph.Count, refreshAchievements: false);
			List<string> buildingsBuilt = TPSingleton<MetaConditionManager>.Instance.CampaignContext.BuildingsBuilt;
			if (buildingsBuilt.Contains("Inn"))
			{
				UnlockAchievement(AchievementContainer.ACH_FIRST_INN, refreshIfAchieved: false);
			}
			if (buildingsBuilt.Contains("Seer"))
			{
				UnlockAchievement(AchievementContainer.ACH_FIRST_SEER, refreshIfAchieved: false);
			}
			bool flag = false;
			foreach (KeyValuePair<string, BuildingDefinition> buildingDefinition in BuildingDatabase.BuildingDefinitions)
			{
				if (buildingDefinition.Value.ConstructionModuleDefinition.IsBuyable && !buildingsBuilt.Contains(buildingDefinition.Key))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				UnlockAchievement(AchievementContainer.ACH_BUILT_ALL_BUILDINGS, refreshIfAchieved: false);
			}
			SetAchievementProgression(StatContainer.STAT_SCAVENGED_CORPSES_AND_RUINS_AMOUNT, (int)TPSingleton<MetaConditionManager>.Instance.CampaignContext.ScavengedBonePilesCount);
		}
		if (saveVersion <= 12)
		{
			if (TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Count > 0)
			{
				UnlockAchievement(AchievementContainer.ACH_FIRST_UNLOCK, refreshIfAchieved: false);
			}
			int num = 0;
			foreach (MetaUpgrade activatedUpgrade in TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades)
			{
				if (AchievementContainer.UnlockAllWeapons.Contains(activatedUpgrade.MetaUpgradeDefinition.Id))
				{
					num++;
				}
			}
			if (num == AchievementContainer.UnlockAllWeapons.Count)
			{
				UnlockAchievement(AchievementContainer.ACH_UNLOCK_ALL_WEAPONS, refreshIfAchieved: false);
			}
		}
		RefreshAchievements();
	}

	public void TriggerGameBackwardCompatibility()
	{
		if ((int)TPSingleton<MetaConditionManager>.Instance.RunContext.GetDouble(MetaConditionSpecificContext.E_ValueCategory.ManaSpent) >= 500)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_SPEND_500_MANA_RUN, refreshIfAchieved: false);
		}
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if (playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana) == 0f)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_OUT_OF_MANA, refreshIfAchieved: false);
			}
		}
		if (TPSingleton<ResourceManager>.Instance.MaxWorkers >= 12)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_HAVE_12_WORKERS, refreshIfAchieved: false);
		}
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count >= 6)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_HAVE_6_HEROES, refreshIfAchieved: false);
		}
		if (TPSingleton<DarkShopManager>.Instance.MetaShopView.GoddessView.CurrentEvolutionIndex > 0)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_SCHADEN_REVEAL, refreshIfAchieved: false);
		}
		if (TPSingleton<LightShopManager>.Instance.MetaShopView.GoddessView.CurrentEvolutionIndex > 0)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_FREUDE_REVEAL, refreshIfAchieved: false);
		}
		if (TPSingleton<GameManager>.Instance.Game.DayNumber >= 4 && ApocalypseManager.CurrentApocalypseIndex >= 1 && !TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled)
		{
			UnlockAchievement(AchievementContainer.ACH_NIGHT3_APO1);
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			int num = 0;
			foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
			{
				if (building.IsTrap && building.BattleModule.RemainingTrapCharges > 0)
				{
					num++;
				}
			}
			if (num >= 40)
			{
				UnlockAchievement(AchievementContainer.ACH_HAVE_40_TRAPS_NIGHT_BEGINNING);
			}
		}
		foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if (playableUnit2.LifetimeStats.MostUnitsKilledInOneBlow >= 12)
			{
				UnlockAchievement(AchievementContainer.ACH_HIT_12_ENEMIES_IN_AOE, refreshIfAchieved: false);
				break;
			}
		}
		if ((int)TPSingleton<MetaConditionManager>.Instance.RunContext.ItemsProducedCount >= 40)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_PRODUCE_40_BUILDING_ITEMS_RUN, refreshIfAchieved: false);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfId("Catapult") >= 10.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_10_CATAPULTS_RUN, refreshIfAchieved: false);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfId("StoneWallReinforced") >= 60.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_60_REINFORCED_STONE_WALLS_RUN, refreshIfAchieved: false);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfCategory(BuildingDefinition.E_BuildingCategory.Turret) >= 20.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_20_BALLISTAS_RUN, refreshIfAchieved: false);
		}
		RefreshAchievements();
	}

	public void HandleBuiltBuilding(string buildingId)
	{
		if (buildingId == "Inn")
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_FIRST_INN);
		}
		else if (buildingId == "Seer")
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_FIRST_SEER);
		}
		if (remainingBuildingsToBuild.Remove(buildingId) && remainingBuildingsToBuild.Count == 0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILT_ALL_BUILDINGS);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfId("Catapult") >= 10.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_10_CATAPULTS_RUN);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfId("StoneWallReinforced") >= 60.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_60_REINFORCED_STONE_WALLS_RUN);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfCategory(BuildingDefinition.E_BuildingCategory.Turret) >= 20.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_20_BALLISTAS_RUN);
		}
	}

	public void HandleGameOver(Game.E_GameOverCause gameOverCause)
	{
		if (gameOverCause != Game.E_GameOverCause.MagicSealsCompleted)
		{
			return;
		}
		Achievement? achievement = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id switch
		{
			"Felderland" => AchievementContainer.ACH_WIN_GILDENBERG, 
			"LakeBurg" => AchievementContainer.ACH_WIN_LAKEBURG, 
			"Glenwald" => AchievementContainer.ACH_WIN_GLENWALD, 
			"Elderlicht" => AchievementContainer.ACH_WIN_ELDERLICHT, 
			"Glintfein" => AchievementContainer.ACH_WIN_GLINTFEIN, 
			_ => null, 
		};
		if (achievement.HasValue)
		{
			UnlockAchievement(achievement.Value);
		}
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled)
		{
			return;
		}
		if (ApocalypseManager.CurrentApocalypseIndex >= 1)
		{
			UnlockAchievement(AchievementContainer.ACH_WIN_APO1);
		}
		if (ApocalypseManager.CurrentApocalypseIndex >= 3)
		{
			Achievement? achievement2 = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id switch
			{
				"Felderland" => AchievementContainer.ACH_WIN_GILDENBERG_APO3, 
				"LakeBurg" => AchievementContainer.ACH_WIN_LAKEBURG_APO3, 
				"Glenwald" => AchievementContainer.ACH_WIN_GLENWALD_APO3, 
				"Elderlicht" => AchievementContainer.ACH_WIN_ELDERLICHT_APO3, 
				"Glintfein" => AchievementContainer.ACH_WIN_GLINTFEIN_APO3, 
				_ => null, 
			};
			if (achievement2.HasValue)
			{
				UnlockAchievement(achievement2.Value);
			}
		}
		if (ApocalypseManager.CurrentApocalypseIndex >= 6)
		{
			UnlockAchievement(AchievementContainer.ACH_WIN_APO6);
		}
	}

	public void HandleMetaUpgrade(string upgradeId)
	{
		UnlockAchievement(AchievementContainer.ACH_FIRST_UNLOCK);
		if (remainingWeaponsToUnlock != null && remainingWeaponsToUnlock.Remove(upgradeId) && remainingWeaponsToUnlock.Count == 0)
		{
			UnlockAchievement(AchievementContainer.ACH_UNLOCK_ALL_WEAPONS);
		}
	}

	public void HandleNightEnd()
	{
		stun50EnemiesNightAchievementUnlocker.Reset();
		if (TPSingleton<GameManager>.Instance.Game.DayNumber == 3 && ApocalypseManager.CurrentApocalypseIndex >= 1 && !TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled)
		{
			UnlockAchievement(AchievementContainer.ACH_NIGHT3_APO1);
		}
		if (PanicManager.Panic.Level <= 1)
		{
			perfectWinAchievementUnlocker.IncreaseValue();
		}
	}

	public void HandleNightStart()
	{
		int num = 0;
		foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
		{
			if (building.IsTrap && building.BattleModule.RemainingTrapCharges > 0)
			{
				num++;
			}
		}
		if (num >= 40)
		{
			UnlockAchievement(AchievementContainer.ACH_HAVE_40_TRAPS_NIGHT_BEGINNING);
		}
		if (TPSingleton<GameManager>.Instance.Game.DayNumber == TPSingleton<WorldMapCityManager>.Instance.SelectedCity.MaxNightReached && TPSingleton<GameManager>.Instance.Game.DayNumber == TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount)
		{
			UnlockAchievement(AchievementContainer.ACH_REENCOUNTER_BOSS);
		}
	}

	public void HandleOnAttackDataComputed(AttackSkillActionExecutionTileData attackData)
	{
		if (attackData.IsCrit && attackData.Damageable is EnemyUnit)
		{
			crit20EnemiesTurnAchievementUnlocker.IncreaseValue();
		}
	}

	public void HandleRunLoad()
	{
		perfectWinAchievementUnlocker.SetValueLimit(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount);
	}

	public void HandleRunStart()
	{
		perfectWinAchievementUnlocker.Reset();
		perfectWinAchievementUnlocker.SetValueLimit(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount);
		SetAchievementProgression(StatContainer.STAT_NUMBER_OF_RUNS, (int)ApplicationManager.Application.RunsCompleted);
	}

	public void HandleSkillEnd(SkillActionExecutionController skillActionExecutionController)
	{
		if (skillActionExecutionController.SkillActionExecution.Caster is PlayableUnit || skillActionExecutionController.SkillActionExecution.Caster is BattleModule)
		{
			int num = 0;
			foreach (TheLastStand.Model.Unit.Unit allAttackedUnit in skillActionExecutionController.SkillActionExecution.AllAttackedUnits)
			{
				if (allAttackedUnit is EnemyUnit)
				{
					num++;
				}
			}
			if (num >= 12)
			{
				UnlockAchievement(AchievementContainer.ACH_HIT_12_ENEMIES_IN_AOE);
			}
		}
		if (!(skillActionExecutionController.SkillActionExecution.Caster is PlayableUnit playableUnit))
		{
			return;
		}
		foreach (TheLastStand.Model.Unit.Unit allAttackedUnit2 in skillActionExecutionController.SkillActionExecution.AllAttackedUnits)
		{
			if (allAttackedUnit2 is EnemyUnit enemyUnit && enemyUnit.IsDead && TileMapController.DistanceBetweenTiles(allAttackedUnit2.OriginTile, playableUnit.OriginTile) >= 14)
			{
				UnlockAchievement(AchievementContainer.ACH_KILL_14_TILES);
				break;
			}
		}
	}

	public void HandleStatusAdded(TheLastStand.Model.Unit.Unit unit, Status status)
	{
		if (unit is EnemyUnit && status is StunStatus)
		{
			stun50EnemiesNightAchievementUnlocker.IncreaseValue();
		}
	}

	public void HandleTurnStart()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			crit20EnemiesTurnAchievementUnlocker.Reset();
		}
	}

	public void Deserialize(SerializedAchievements serializedAchievements)
	{
		if (crit20EnemiesTurnAchievementUnlocker == null)
		{
			crit20EnemiesTurnAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_CRIT_20_ENEMIES_TURN, 20);
		}
		crit20EnemiesTurnAchievementUnlocker.Deserialize(serializedAchievements?.Crit20EnemiesTurnAchievementUnlocker);
		if (stun50EnemiesNightAchievementUnlocker == null)
		{
			stun50EnemiesNightAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_STUN_50_ENEMIES_NIGHT, 50);
		}
		stun50EnemiesNightAchievementUnlocker.Deserialize(serializedAchievements?.Stun50EnemiesNightAchievementUnlocker);
		if (perfectWinAchievementUnlocker == null)
		{
			perfectWinAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_PERFECT_WIN, 14);
		}
		perfectWinAchievementUnlocker.Deserialize(serializedAchievements?.PerfectWinAchievementUnlocker);
	}

	public SerializedAchievements Serialize()
	{
		return new SerializedAchievements
		{
			Crit20EnemiesTurnAchievementUnlocker = (crit20EnemiesTurnAchievementUnlocker.Serialize() as SerializedAchievementUnlocker),
			Stun50EnemiesNightAchievementUnlocker = (stun50EnemiesNightAchievementUnlocker.Serialize() as SerializedAchievementUnlocker),
			PerfectWinAchievementUnlocker = (perfectWinAchievementUnlocker.Serialize() as SerializedAchievementUnlocker)
		};
	}

	[DevConsoleCommand("AchievementUnlock")]
	public static void UnlockAchievementDebug([StringConverter(typeof(Achievement.StringToAchievementIdConverter))] string achievementId)
	{
		TPSingleton<AchievementManager>.Instance.achievementHandler?.UnlockAchievement(achievementId);
	}

	[DevConsoleCommand("AchievementUnlockAllButOne")]
	public static void UnlockAllAchievementButOneDebug()
	{
		foreach (Achievement allAchievement in AchievementContainer.AllAchievements)
		{
			if (allAchievement.SteamId != AchievementContainer.ACH_USE_50_SCROLLS.SteamId)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(allAchievement);
			}
		}
	}

	[DevConsoleCommand("AchievementReset")]
	public static void ResetAchievementDebug()
	{
		TPSingleton<AchievementManager>.Instance.achievementHandler?.ClearAchievements();
		((CLogger<AchievementManager>)TPSingleton<AchievementManager>.Instance).Log((object)"Achievements cleared !", (CLogLevel)0, true, false);
	}
}
