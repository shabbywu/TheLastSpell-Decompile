using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.Building;
using TheLastStand.Controller.Meta;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Controller.Unit;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Database.WorldMap;
using TheLastStand.Definition;
using TheLastStand.Definition.BonePile;
using TheLastStand.Definition.Brazier;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Dev;
using TheLastStand.Framework;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.EventSystem;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Helpers;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Trap;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Events;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Meta;
using TheLastStand.Model.ProductionReport;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.WorldMap;
using TheLastStand.ScriptableObjects;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Building;
using TheLastStand.View;
using TheLastStand.View.Building;
using TheLastStand.View.Building.Construction;
using TheLastStand.View.Building.UI;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.Shop;
using TheLastStand.View.TileMap;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Manager.Building;

public sealed class BuildingManager : Manager<BuildingManager>
{
	public static class Consts
	{
		public const string Buildings = "Buildings";

		public const string Building = "Building";

		public const string BuildingHitsSoundAssetPrefix = "Sounds/SFX/BuildingHits/";

		public const string BuildingConstructionSoundAssetPrefix = "Sounds/SFX/BuildingConstruction/";

		public const string BuildingsTextAssetLevelEditorPathFormat = "TextAssets/Cities/Level Editor/{0}/{0}{1}_Buildings";

		public const string BuildingsTextAssetPathFormat = "TextAssets/Cities/{0}/{0}{1}_Buildings";
	}

	[SerializeField]
	private Transform buildingsTransform;

	[SerializeField]
	private Transform buildingsHudsTransform;

	[SerializeField]
	private ShopView shopView;

	[SerializeField]
	private ProductionReportPanel productionReportPanel;

	[SerializeField]
	private BuildingView buildingViewPrefab;

	[SerializeField]
	private BuildingView magicCircleViewPrefab;

	[SerializeField]
	private BuildingView watchTowerViewPrefab;

	[SerializeField]
	private BuildingTooltip buildingInfoPanel;

	[SerializeField]
	private BuildingActionTooltip buildingActionTooltip;

	[SerializeField]
	private BuildingConstructionTooltip buildingConstructionTooltip;

	[SerializeField]
	private ConstructionModeTooltip buildingRepairTooltip;

	[SerializeField]
	private BuildingSkillTooltip buildingSkillTooltip;

	[SerializeField]
	private BuildingUpgradeTooltip buildingUpgradeTooltip;

	[SerializeField]
	private AudioClip[] constructionSimpleAudioClips;

	[SerializeField]
	private AudioClip[] constructionAnimationAudioClips;

	[SerializeField]
	private AudioClip[] destructionAudioClips;

	[SerializeField]
	private AudioClip repairAudioClip;

	[SerializeField]
	private float waitBeforeGuardiansSpawnDuration = 0.5f;

	[SerializeField]
	private float guardianSpawnAnimDuration = 1f;

	[SerializeField]
	[Tooltip("Every building within this distance will be destroyed entirely. Buildings further away will take lower damages.")]
	private int magicCircleExplosionForce = 5;

	[SerializeField]
	private Vector2 randomProductionTriggerDelay = Vector2.up;

	[SerializeField]
	private float randomBuildingsAppearanceMaxDelay = 2f;

	[SerializeField]
	private float randomBuildingsDisappearanceMaxDelay = 2f;

	[SerializeField]
	private PooledAudioSourceData buildingPooledAudioSourceData;

	private MagicCircle magicCircle;

	private BuildingAction selectedBuildingAction;

	private TheLastStand.Model.Skill.Skill previewedSkill;

	private Tile previousTile;

	private TheLastStand.Model.Skill.Skill selectedSkill;

	[SerializeField]
	private bool debugForceUseBuildingActionAllPhases;

	public static TheLastStand.Model.Skill.Skill PreviewedSkill
	{
		get
		{
			return TPSingleton<BuildingManager>.Instance.previewedSkill;
		}
		set
		{
			if (TPSingleton<BuildingManager>.Instance.previewedSkill != null)
			{
				TPSingleton<BuildingManager>.Instance.previewedSkill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
			}
			TPSingleton<BuildingManager>.Instance.previewedSkill = value;
		}
	}

	public static Transform BuildingsHudsTransform => TPSingleton<BuildingManager>.Instance.buildingsHudsTransform;

	public static BuildingConstructionTooltip BuildingConstructionTooltip => TPSingleton<BuildingManager>.Instance.buildingConstructionTooltip;

	public static ConstructionModeTooltip BuildingRepairTooltip => TPSingleton<BuildingManager>.Instance.buildingRepairTooltip;

	public static BuildingActionTooltip BuildingActionTooltip => TPSingleton<BuildingManager>.Instance.buildingActionTooltip;

	public static BuildingTooltip BuildingInfoPanel => TPSingleton<BuildingManager>.Instance.buildingInfoPanel;

	public static BuildingSkillTooltip BuildingSkillTooltip => TPSingleton<BuildingManager>.Instance.buildingSkillTooltip;

	public static BuildingUpgradeTooltip BuildingUpgradeTooltip => TPSingleton<BuildingManager>.Instance.buildingUpgradeTooltip;

	public static AudioClip[] DestructionAudioClips => TPSingleton<BuildingManager>.Instance.destructionAudioClips;

	public static MagicCircle MagicCircle
	{
		get
		{
			if (TPSingleton<BuildingManager>.Instance.magicCircle == null)
			{
				foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
				{
					if (building is MagicCircle magicCircle)
					{
						TPSingleton<BuildingManager>.Instance.magicCircle = magicCircle;
						break;
					}
				}
			}
			return TPSingleton<BuildingManager>.Instance.magicCircle;
		}
	}

	public static int MagicCircleExplosionForce => TPSingleton<BuildingManager>.Instance.magicCircleExplosionForce;

	public static PooledAudioSourceData BuildingPooledAudioSourceData => TPSingleton<BuildingManager>.Instance.buildingPooledAudioSourceData;

	public static AudioClip RepairAudioClip => TPSingleton<BuildingManager>.Instance.repairAudioClip;

	public static BuildingAction SelectedBuildingAction
	{
		get
		{
			return TPSingleton<BuildingManager>.Instance.selectedBuildingAction;
		}
		set
		{
			if (value == TPSingleton<BuildingManager>.Instance.selectedBuildingAction)
			{
				return;
			}
			TPSingleton<BuildingManager>.Instance.selectedBuildingAction = value;
			if (TPSingleton<BuildingManager>.Instance.selectedBuildingAction != null)
			{
				return;
			}
			foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				playableUnit.UnitView.UnitHUD.AttackEstimationDisplay.Hide();
			}
			GameController.SetState(Game.E_State.Management);
		}
	}

	public static TheLastStand.Model.Skill.Skill SelectedSkill
	{
		get
		{
			return TPSingleton<BuildingManager>.Instance.selectedSkill;
		}
		set
		{
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			TheLastStand.Model.Skill.Skill skill = TPSingleton<BuildingManager>.Instance.selectedSkill;
			TPSingleton<BuildingManager>.Instance.selectedSkill = value;
			if (TPSingleton<BuildingManager>.Instance.selectedSkill == skill)
			{
				return;
			}
			skill?.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
			if (TPSingleton<BuildingManager>.Instance.selectedSkill != null)
			{
				GameController.SetState(Game.E_State.BuildingPreparingSkill);
				TPSingleton<BuildingManager>.Instance.selectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(TileObjectSelectionManager.SelectedBuilding.BattleModule);
				if (TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.SkillActionDefinition is AttackSkillActionDefinition)
				{
					SkillManager.AttackInfoPanel.SetSkill(TPSingleton<BuildingManager>.Instance.selectedSkill, TileObjectSelectionManager.SelectedBuilding.BattleModule);
					SkillManager.GenericActionInfoPanel.Hide();
					TheLastStand.Model.Unit.Unit unit = TPSingleton<GameManager>.Instance.Game.Cursor.Tile?.Unit;
					if (unit == TileObjectSelectionManager.SelectedUnit)
					{
						int count = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern.Count;
						Vector2Int origin = TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
						if (count > ((Vector2Int)(ref origin)).x)
						{
							List<List<char>> pattern = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern;
							origin = TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
							int count2 = pattern[((Vector2Int)(ref origin)).x].Count;
							origin = TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
							if (count2 > ((Vector2Int)(ref origin)).y)
							{
								List<List<char>> pattern2 = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern;
								origin = TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
								List<char> list = pattern2[((Vector2Int)(ref origin)).x];
								origin = TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
								if (list[((Vector2Int)(ref origin)).y] != 'X')
								{
									goto IL_02aa;
								}
							}
						}
					}
					SkillManager.AttackInfoPanel.TargetTile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
					SkillManager.AttackInfoPanel.TargetUnit = unit;
					SkillManager.AttackInfoPanel.RefreshAttackData();
					SkillManager.AttackInfoPanel.Display();
					if (EnemyUnitManager.IsAnyEnemyTooltipDisplayed())
					{
						EnemyUnitManager.GetDisplayedEnemyTootlip().Refresh();
					}
				}
				else if (TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.SkillActionDefinition is GenericSkillActionDefinition)
				{
					SkillManager.GenericActionInfoPanel.SetSkill(TPSingleton<BuildingManager>.Instance.selectedSkill, TileObjectSelectionManager.SelectedUnit);
					SkillManager.AttackInfoPanel.Hide();
					SkillManager.GenericActionInfoPanel.Display();
				}
				else
				{
					SkillManager.AttackInfoPanel.Hide();
					SkillManager.GenericActionInfoPanel.Hide();
				}
			}
			goto IL_02aa;
			IL_02aa:
			if (TPSingleton<BuildingManager>.Instance.selectedSkill == null)
			{
				SkillManager.AttackInfoPanel.Hide();
				SkillManager.GenericActionInfoPanel.Hide();
				if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingPreparingSkill)
				{
					GameController.SetState(Game.E_State.Management);
				}
				GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.DeselectSkill();
			}
		}
	}

	public int AccessShopBuildingCount { get; private set; }

	public List<TheLastStand.Model.Building.Building> Buildings { get; private set; }

	public Dictionary<Tile, BuildingToRestore> BuildingsToRestore { get; } = new Dictionary<Tile, BuildingToRestore>();


	public Dictionary<Tile, BuildingDefinition> RandomsBuildings { get; } = new Dictionary<Tile, BuildingDefinition>();


	public Dictionary<string, int> BonePileGenerationCounter { get; } = new Dictionary<string, int>();


	public Dictionary<string, int> BonePileGenerationLimits { get; } = new Dictionary<string, int>();


	public List<Tile> TilesLockedByRandomBuildings { get; } = new List<Tile>();


	public List<TheLastStand.Model.Building.Building> Braziers { get; } = new List<TheLastStand.Model.Building.Building>();


	public List<BattleModule> BuildingsDeathRattling { get; } = new List<BattleModule>();


	public BuildingUpgradeLevel GlobalItemProductionUpgradeLevel { get; private set; }

	public ProductionReport ProductionReport { get; private set; }

	public Vector2 RandomProductionTriggerDelay => randomProductionTriggerDelay;

	public Shop Shop { get; private set; }

	public List<TheLastStand.Model.Building.Building> WaitBuildingGauges { get; } = new List<TheLastStand.Model.Building.Building>();


	public WaitUntil WaitUntilDeathRattlingBuildingsAreDone { get; } = new WaitUntil((Func<bool>)(() => TPSingleton<BuildingManager>.Instance.BuildingsDeathRattling.Count == 0));


	public static bool DebugUseForceBuildingActionsAllPhases => TPSingleton<BuildingManager>.Instance.debugForceUseBuildingActionAllPhases;

	public static void ClearBuildings()
	{
		TPSingleton<BuildingManager>.Instance.Buildings.Clear();
		TPSingleton<BuildingManager>.Instance.magicCircle = null;
	}

	public static int ComputeBuildingCost(ConstructionModuleDefinition constructionModuleDefinition, bool useDefault = false)
	{
		bool flag = constructionModuleDefinition.NativeGoldCost > 0;
		int num = (flag ? constructionModuleDefinition.NativeGoldCost : constructionModuleDefinition.NativeMaterialsCost);
		if (useDefault)
		{
			return num;
		}
		int num2 = ResourceManager.ComputeExtraPercentageForCost(flag ? ResourceManager.E_PriceModifierType.ProductionBuildings : ResourceManager.E_PriceModifierType.DefensiveBuildings);
		if (!MetaUpgradesManager.IsThisBuildingUnlockedByDefault(constructionModuleDefinition.BuildingDefinition.Id) && MetaUpgradeEffectsController.TryGetEffectsOfType<BuildingModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int i = 0; i < effects.Length; i++)
			{
				if (effects[i].BuildingId == constructionModuleDefinition.BuildingDefinition.Id)
				{
					num2 -= effects[i].GoldCostReduction;
				}
			}
		}
		int num3 = Mathf.RoundToInt((float)(num * num2) / 100f);
		return num + num3;
	}

	public static float ComputeBuildingTotalHealth(DamageableModuleDefinition damageableModuleDefinition, bool useDefaultValues = false)
	{
		float nativeHealthTotal = damageableModuleDefinition.NativeHealthTotal;
		if (useDefaultValues)
		{
			return nativeHealthTotal;
		}
		string id = damageableModuleDefinition.BuildingDefinition.Id;
		int num = 0;
		if (!MetaUpgradesManager.IsThisBuildingUnlockedByDefault(id) && MetaUpgradeEffectsController.TryGetEffectsOfType<BuildingModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int i = 0; i < effects.Length; i++)
			{
				if (effects[i].BuildingId == id)
				{
					num += effects[i].HealthBonus;
				}
			}
		}
		int glyphHealthTotalPercentageModifier = damageableModuleDefinition.GlyphHealthTotalPercentageModifier;
		float num2 = Mathf.Round(nativeHealthTotal * ((float)(num + glyphHealthTotalPercentageModifier) / 100f));
		return nativeHealthTotal + num2;
	}

	public static TheLastStand.Model.Building.Building CreateBuilding(BuildingDefinition buildingDefinition, Tile tile, bool updateView = true, bool playSound = true, bool instantly = true, bool triggerEvent = true, string bossPhaseActorId = null, bool recomputeReachableTiles = true, bool isGeneratingBonePile = false)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)string.Format("Creating building {0}{1} at position {2}", buildingDefinition.Id, instantly ? " instantly" : string.Empty, tile.Position), (CLogLevel)1, false, false);
		List<Tile> occupiedTiles = tile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		for (int num = occupiedTiles.Count - 1; num >= 0; num--)
		{
			Vector2Int relativeBuildingTilePosition = BlueprintModule.GetRelativeBuildingTilePosition(occupiedTiles[num], tile, buildingDefinition.BlueprintModuleDefinition);
			if (buildingDefinition.BlueprintModuleDefinition.Tiles[((Vector2Int)(ref relativeBuildingTilePosition)).y][((Vector2Int)(ref relativeBuildingTilePosition)).x].HasFlag(Tile.E_UnitAccess.Hero))
			{
				occupiedTiles.RemoveAt(num);
			}
		}
		TileMapManager.FreeTilesFromPlayableUnits(occupiedTiles, (Tile t) => !isGeneratingBonePile || (!TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.ContainsKey(t) && !TPSingleton<BuildingManager>.Instance.WillTileBeUsedForRandomBuilding(t)));
		BuildingView buildingView = CreateBuildingView(buildingDefinition);
		TheLastStand.Model.Building.Building building = new BuildingController(buildingDefinition, buildingView, tile).Building;
		InitBuilding(building, tile, updateView, playSound, instantly, recomputeReachableTiles);
		building.BossPhaseActorId = bossPhaseActorId;
		if (triggerEvent && ((StateMachine)ApplicationManager.Application).State.GetName() != "LevelEditor")
		{
			TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnBuildingConstructed);
		}
		return building;
	}

	public static IEnumerator DestroyAll(int radius, float maxDistance = -1f)
	{
		List<TheLastStand.Model.Building.Building> buildings = TPSingleton<BuildingManager>.Instance.Buildings.Where((TheLastStand.Model.Building.Building x) => !(x is MagicCircle) && !x.BlueprintModule.IsIndestructible).ToList();
		int index = buildings.Count - 1;
		while (index >= 0)
		{
			TheLastStand.Model.Building.Building building = buildings[index];
			Vector2Int val = building.OriginTile.Position - MagicCircle.OriginTile.Position;
			float num = ((Vector2Int)(ref val)).sqrMagnitude;
			if (!(num > maxDistance * maxDistance) || maxDistance == -1f)
			{
				building.DamageableModule.DamageableController.LoseHealth(building.DamageableModule.HealthTotal - num + (float)(radius * radius), null, refreshHud: false);
				if (building.DamageableModule.Health <= 0f)
				{
					building.BuildingView.PlayDieAnim();
					yield return SharedYields.WaitForEndOfFrame;
				}
			}
			int num2 = index - 1;
			index = num2;
		}
	}

	public static void DestroyBuilding(Tile tile, bool updateView = true, bool addDeadBuilding = false, bool triggerEvent = true, bool triggerOnDeathEvent = true, bool recomputeReachableTiles = true, bool playDeathSound = false)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		TheLastStand.Model.Building.Building building = tile.Building;
		if (building == null)
		{
			return;
		}
		((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)$"Destroying building {building.UniqueIdentifier} at position {tile.Position}", (CLogLevel)1, false, false);
		foreach (Tile occupiedTile in building.BlueprintModule.OccupiedTiles)
		{
			if (updateView)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ClearBuilding(occupiedTile);
			}
			if (addDeadBuilding)
			{
				occupiedTile.TileController.AddDeadBuilding(building.BuildingDefinition);
			}
		}
		building.BlueprintModule.BlueprintModuleController.FreeOccupiedTiles();
		TPSingleton<ConstructionManager>.Instance.DecrementBuildingCount(building.BuildingDefinition);
		EventManager.TriggerEvent((Event)(object)new BuildingDestroyedEvent(building));
		if (triggerEvent && ((StateMachine)ApplicationManager.Application).State.GetName() != "LevelEditor")
		{
			TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnBuildingDestroyed);
		}
		((Component)building.BuildingView).gameObject.SetActive(false);
		TPSingleton<BuildingManager>.Instance.Buildings.Remove(building);
		if (building.IsBrazier)
		{
			TPSingleton<BuildingManager>.Instance.Braziers.Remove(building);
			BrazierModule brazierModule = building.BrazierModule;
			if (brazierModule != null && brazierModule.IsExtinguishing)
			{
				building.BrazierModule.IsExtinguishing = false;
			}
		}
		if (building.IsBossPhaseActor)
		{
			TPSingleton<BossManager>.Instance.HandleBossPhaseActorDeath(building);
		}
		if (TileObjectSelectionManager.SelectedBuilding == building)
		{
			TileObjectSelectionManager.SelectBuilding(null);
		}
		if (building.BuildingDefinition.Id == "Shop" || building.BuildingDefinition.Id == "Inn")
		{
			if (building.BuildingDefinition.Id == "Shop")
			{
				TPSingleton<BuildingManager>.Instance.AccessShopBuildingCount--;
				List<BuildingUpgradeEffect> list = building.UpgradeModule?.GetActivatedBuildingUpgradeEffects();
				if (list != null && list.Count > 0)
				{
					int num = 0;
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i] is ImproveSellRatio improveSellRatio)
						{
							num += improveSellRatio.ImproveSellRatioDefinition.Value;
						}
					}
					TPSingleton<BuildingManager>.Instance.Shop.SellRatioLevel -= num;
				}
			}
			if (TPSingleton<ToDoListView>.Exist())
			{
				TPSingleton<ToDoListView>.Instance.RefreshGoldNotification();
			}
		}
		building.UpgradeModule?.UpgradeModuleController.OnDeath();
		building.PassivesModule?.PassivesModuleController.OnDeath(triggerOnDeathEvent);
		if (((StateMachine)ApplicationManager.Application).State.GetName() != "LevelEditor")
		{
			PlayableUnitManagementView.OnBuildingDestroyed();
			if (recomputeReachableTiles)
			{
				TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles = true;
				if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill && TileObjectSelectionManager.HasPlayableUnitSelected && TPSingleton<GameManager>.Instance.Game.Cursor?.Tile != null && !TPSingleton<NightTurnsManager>.Instance.IsEndingNight)
				{
					TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
					TPSingleton<PlayableUnitManager>.Instance.RefreshUnitMovePath(TileObjectSelectionManager.SelectedPlayableUnit);
				}
				GameView.BottomScreenPanel.BottomLeftPanel.CancelMovementPanel.Refresh();
			}
			TPSingleton<ToDoListView>.Instance.RefreshWorkersNotification();
		}
		ConstructionModuleDefinition constructionModuleDefinition = building.BuildingDefinition.ConstructionModuleDefinition;
		if (constructionModuleDefinition != null && constructionModuleDefinition.DestructionAnimationType == BuildingDefinition.E_ConstructionAnimationType.Animated)
		{
			float num2 = Random.Range(TPSingleton<TileMapView>.Instance.DestructionAnimationRandomDelay.x, TPSingleton<TileMapView>.Instance.DestructionAnimationRandomDelay.y);
			foreach (Tile occupiedTile2 in building.BlueprintModule.OccupiedTiles)
			{
				TileMapView.SpawnDestructionAnimation(building, occupiedTile2, Mathf.Max(0f, num2));
			}
		}
		if (playDeathSound && (building.ConstructionModule == null || building.ConstructionModule.ConstructionModuleDefinition.PlayDestructionSound))
		{
			building.BuildingView.PlayDeathSound();
		}
	}

	public static void DisplayBuildingsHudsIfNeeded()
	{
		int i = 0;
		for (int count = TPSingleton<BuildingManager>.Instance.Buildings.Count; i < count; i++)
		{
			TPSingleton<BuildingManager>.Instance.Buildings[i].BuildingView.BuildingHUD.DisplayHealthIfNeeded();
		}
	}

	public static void EndTurn()
	{
		for (int num = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num >= 0; num--)
		{
			TPSingleton<BuildingManager>.Instance.Buildings[num].BuildingController.EndTurn();
		}
	}

	public bool GenerateBonePiles()
	{
		bool flag = false;
		BonePileGenerationCounter.Clear();
		RefreshBonePilesGenerationLimits();
		foreach (KeyValuePair<Tile, Dictionary<string, int>> bonePilesPercentage in TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages)
		{
			flag |= bonePilesPercentage.Key.TileController.TryGenerateBonePile(bonePilesPercentage.Value);
		}
		TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.Clear();
		return flag;
	}

	public void RefreshBonePilesGenerationLimits()
	{
		string id = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
		if (!BonePileDatabase.BonePileGeneratorsDefinition.CountProgressionDefinitions.TryGetValue(id, out var value))
		{
			return;
		}
		BonePileGenerationLimits.Clear();
		int dayNumber = TPSingleton<GameManager>.Instance.Game.DayNumber;
		foreach (KeyValuePair<string, BonePileCountProgressionDefinition.ProgressionData> bonePileProgression in value.BonePileProgressions)
		{
			int num = Mathf.CeilToInt((float)Mathf.Max(dayNumber - 1 - bonePileProgression.Value.Delay, 0) / (float)bonePileProgression.Value.IncreaseEveryXDays) * bonePileProgression.Value.IncreaseValue;
			int num2 = bonePileProgression.Value.BaseValue + num;
			if (bonePileProgression.Value.Limit > -1)
			{
				num2 = Mathf.Min(num2, bonePileProgression.Value.Limit);
			}
			BonePileGenerationLimits.Add(bonePileProgression.Key, num2);
		}
		foreach (KeyValuePair<string, int> building in BonePileDatabase.BonePileGeneratorsDefinition.Buildings)
		{
			if (!BonePileGenerationLimits.ContainsKey(building.Key))
			{
				BonePileGenerationLimits.Add(building.Key, -1);
			}
		}
		string text = $"Bone Piles limits (day {dayNumber}):";
		foreach (KeyValuePair<string, int> bonePileGenerationLimit in BonePileGenerationLimits)
		{
			text = text + "\n- " + bonePileGenerationLimit.Key + ": " + ((bonePileGenerationLimit.Value == -1) ? "-1 (Unlimited)" : bonePileGenerationLimit.Value.ToString());
		}
		((CLogger<BuildingManager>)this).Log((object)text, (CLogLevel)0, false, false);
	}

	public static List<TheLastStand.Model.Building.Building> GetBuildingsById(string buildingId)
	{
		List<TheLastStand.Model.Building.Building> list = new List<TheLastStand.Model.Building.Building>();
		for (int num = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num >= 0; num--)
		{
			if (TPSingleton<BuildingManager>.Instance.Buildings[num].BuildingDefinition.Id == buildingId)
			{
				list.Add(TPSingleton<BuildingManager>.Instance.Buildings[num]);
			}
		}
		return list;
	}

	public static bool HasInn()
	{
		foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
		{
			if (building.BuildingDefinition.Id == "Inn")
			{
				return true;
			}
		}
		return false;
	}

	public static void OnBuildingActionHovered(BuildingAction buildingAction, bool hover)
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingAction && buildingAction != null && !buildingAction.IsExecutionInstant)
		{
			int i = 0;
			for (int count = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i < count; i++)
			{
				TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitView.ToggleSkillTargeting(hover && buildingAction.BuildingActionController.CanExecuteActionOnTile(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].OriginTile));
			}
		}
	}

	public static void OnGameStateChange(Game.E_State state, Game.E_State previousState)
	{
		switch (previousState)
		{
		case Game.E_State.Construction:
			DisplayBuildingsHudsIfNeeded();
			HideTrapsHandledDefensesHudIfNeeded();
			BuildingConstructionTooltip.Hide();
			break;
		case Game.E_State.BuildingPreparingAction:
			if (state != Game.E_State.BuildingPreparingAction && SelectedBuildingAction != null)
			{
				((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(TPSingleton<BuildingManager>.Instance.WaitFinishAction(instant: true));
			}
			break;
		case Game.E_State.BuildingPreparingSkill:
			if (state != Game.E_State.BuildingExecutingSkill && state != Game.E_State.BuildingPreparingSkill)
			{
				SelectedSkill = null;
			}
			break;
		case Game.E_State.BuildingExecutingSkill:
			TileObjectSelectionManager.SelectedBuilding?.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: true);
			break;
		case Game.E_State.MetaShops:
			if (state == Game.E_State.Management)
			{
				TPSingleton<ShopManager>.Instance.RefreshItemsCountPerCategory();
			}
			break;
		}
		switch (state)
		{
		case Game.E_State.Management:
			GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.RefreshSkills();
			break;
		case Game.E_State.CharacterSheet:
		case Game.E_State.Recruitment:
		case Game.E_State.Shopping:
		case Game.E_State.BuildingUpgrade:
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
		case Game.E_State.HowToPlay:
			BuildingConstructionTooltip.Hide();
			break;
		case Game.E_State.Construction:
			DisplayBuildingsHudsIfNeeded();
			DisplayTrapsChargesIfNeeded();
			break;
		case Game.E_State.BuildingPreparingAction:
		{
			BuildingAction buildingAction = SelectedBuildingAction;
			if (buildingAction != null && !buildingAction.IsExecutionInstant)
			{
				int i = 0;
				for (int count = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i < count; i++)
				{
					TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitView.ToggleSkillTargeting(SelectedBuildingAction.BuildingActionController.CanExecuteActionOnTile(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].OriginTile));
				}
			}
			break;
		}
		}
		GameView.BottomScreenPanel.BuildingManagementPanel.OnGameStateChange(state);
	}

	private static void HideTrapsHandledDefensesHudIfNeeded()
	{
		for (int i = 0; i < TPSingleton<TrapManager>.Instance.Traps.Count; i++)
		{
			if (TPSingleton<GameManager>.Instance.Game.Cursor.Tile == null || TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Building == null || TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Building != TPSingleton<TrapManager>.Instance.Traps[i])
			{
				TPSingleton<TrapManager>.Instance.Traps[i].BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: false);
			}
		}
	}

	private static void DisplayTrapsChargesIfNeeded()
	{
		foreach (TheLastStand.Model.Building.Building trap in TPSingleton<TrapManager>.Instance.Traps)
		{
			if (!trap.BattleModule.IsTrapFullyCharged())
			{
				trap.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: true);
			}
		}
	}

	public static void OnTurnStart()
	{
		DisplayBuildingsHudsIfNeeded();
	}

	[DevConsoleCommand(Name = "RefreshShop")]
	public static void RefreshShop()
	{
		TPSingleton<BuildingManager>.Instance.Shop.ShopController.ClearItems();
		foreach (BuildingPassive buildingPassife in TPSingleton<BuildingManager>.Instance.Buildings.Find((TheLastStand.Model.Building.Building o) => o.Id == "Shop").PassivesModule.BuildingPassives)
		{
			foreach (BuildingPassiveEffect passiveEffect in buildingPassife.PassiveEffects)
			{
				if (passiveEffect is GenerateNewItemsRoster)
				{
					passiveEffect.BuildingPassiveEffectController.Apply();
					return;
				}
			}
		}
	}

	public static TheLastStand.Model.Building.Building ReplaceBuilding(Tile destinationTile, TheLastStand.Model.Building.Building previousBuilding, BuildingDefinition buildingToPlace, bool ignoreBuilding = false, bool instantly = true, bool carryUpgrades = true, string bossPhaseActorId = null)
	{
		bool flag = TileObjectSelectionManager.HasBuildingSelected && TileObjectSelectionManager.SelectedBuilding == previousBuilding;
		TheLastStand.Model.Building.Building building = CreateBuilding(buildingToPlace, destinationTile, updateView: true, playSound: true, instantly, triggerEvent: true, bossPhaseActorId, recomputeReachableTiles: false);
		if (carryUpgrades && building.UpgradeModule?.BuildingUpgrades != null)
		{
			foreach (BuildingUpgrade buildingUpgrade in building.UpgradeModule.BuildingUpgrades)
			{
				foreach (string linkedUpgradeId2 in buildingUpgrade.BuildingUpgradeDefinition.LinkedUpgradesIds)
				{
					if (previousBuilding.UpgradeModule.BuildingUpgrades.Any((BuildingUpgrade x) => x.BuildingUpgradeDefinition.Id == linkedUpgradeId2 && x.IsUnlocked))
					{
						buildingUpgrade.BuildingUpgradeController.UnlockUpgrade(freeUpgrade: true, playFx: false);
					}
				}
			}
		}
		if (carryUpgrades && building.UpgradeModule?.BuildingGlobalUpgrades != null)
		{
			foreach (BuildingGlobalUpgrade buildingGlobalUpgrade in building.UpgradeModule.BuildingGlobalUpgrades)
			{
				foreach (string linkedUpgradeId in buildingGlobalUpgrade.BuildingUpgradeDefinition.LinkedUpgradesIds)
				{
					if (previousBuilding.UpgradeModule.BuildingGlobalUpgrades.Any((BuildingGlobalUpgrade x) => x.BuildingUpgradeDefinition.Id == linkedUpgradeId && x.IsUnlocked))
					{
						buildingGlobalUpgrade.BuildingUpgradeController.UnlockUpgrade(freeUpgrade: true, playFx: false);
					}
				}
			}
		}
		BuildingUpgradeTooltip.Hide();
		if (flag)
		{
			TileObjectSelectionManager.SelectBuilding(building, focusCameraOnBuilding: true);
		}
		else
		{
			PlayableUnitManagementView.OnBuildingDestroyed();
		}
		TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles = true;
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill && TileObjectSelectionManager.HasPlayableUnitSelected && TPSingleton<GameManager>.Instance.Game.Cursor?.Tile != null)
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
			TPSingleton<PlayableUnitManager>.Instance.RefreshUnitMovePath(TileObjectSelectionManager.SelectedPlayableUnit);
		}
		GameView.BottomScreenPanel.BottomLeftPanel.CancelMovementPanel.Refresh();
		return building;
	}

	public void DestroyLightFogSpawners()
	{
		List<Tile> list = new List<Tile>(TileMapManager.GetTilesWithFlag(TileFlagDefinition.E_TileFlagTag.FogSpawner));
		List<Tile> list2 = new List<Tile>();
		foreach (Tile item in list)
		{
			TheLastStand.Model.Building.Building building = item.Building;
			if (building != null && building.IsLightFogSpawner)
			{
				list2.Add(item);
			}
		}
		TileMapManager.ClearBuildingOnTiles(list2);
	}

	private string GetLightFogSpawnerId()
	{
		string result = string.Empty;
		foreach (Tuple<int, string> buildingToSpawnId in TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.LightFogSpawnersGenerationDefinition.BuildingToSpawnIds)
		{
			if (TPSingleton<GameManager>.Instance.Game.DayNumber >= buildingToSpawnId.Item1)
			{
				result = buildingToSpawnId.Item2;
				continue;
			}
			break;
		}
		return result;
	}

	public void GenerateLightFogSpawners(int count)
	{
		if (count > 0)
		{
			List<string> allowingBuildingsIds = GenericDatabase.IdsListDefinitions["AllowingLightFogSpawners"].Ids;
			List<Tile> list = new List<Tile>(TileMapManager.GetTilesWithFlag(TileFlagDefinition.E_TileFlagTag.FogSpawner)).Where((Tile tile) => !tile.HasFog && (tile.Building == null || allowingBuildingsIds.Contains(tile.Building.Id)) && !WillTileBeUsedForRandomBuilding(tile)).ToList();
			if (list.Count < count)
			{
				((CLogger<BuildingManager>)this).LogWarning((object)$"There are not enough candidate tiles to spawn the desired count of Fog Spawners : {list.Count} < {count}", (CLogLevel)1, true, false);
			}
			list = RandomManager.Shuffle(this, list).ToList();
			string lightFogSpawnerId = GetLightFogSpawnerId();
			BuildingDefinition buildingDefinition = BuildingDatabase.BuildingDefinitions[lightFogSpawnerId];
			((CLogger<BuildingManager>)this).Log((object)$"Spawning {Mathf.Min(count, list.Count)} Light Fog Spawners with id \"{lightFogSpawnerId}\".", (CLogLevel)1, false, false);
			for (int i = 0; i < list.Count && i < count; i++)
			{
				TileMapManager.ClearBuildingOnTiles(list[i].GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition));
				CreateBuilding(buildingDefinition, list[i], updateView: true, playSound: false, instantly: false);
			}
		}
	}

	public void ExtinguishBraziers()
	{
		for (int num = Braziers.Count - 1; num >= 0; num--)
		{
			if (Braziers[num].BrazierModule != null)
			{
				Braziers[num].BrazierModule.BrazierModuleController.LoseBrazierPoints(Braziers[num].BrazierModule.BrazierPoints, triggerEvent: true);
			}
		}
	}

	public IEnumerator LitBraziers()
	{
		BrazierDefinition brazierDefinition = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.BrazierDefinition;
		if (brazierDefinition != null)
		{
			return LitBraziers(brazierDefinition.GetBraziersAmountToSpawn(), brazierDefinition.GetBrazierToSpawn());
		}
		return null;
	}

	public IEnumerator LitBraziers(int count, BuildingDefinition brazierDefinition, string bossPhaseActorId = null, bool prioritizeWaveSides = true)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		if (count <= 0 || Braziers == null || Braziers.Count == 0)
		{
			return null;
		}
		List<TheLastStand.Model.Building.Building> list = Braziers.Where((TheLastStand.Model.Building.Building brazier) => !brazier.OriginTile.HasFog && brazier.IsUnlitBrazier).ToList();
		if (list.Count < count)
		{
			((CLogger<BuildingManager>)this).LogWarning((object)$"There are not enough unlit braziers to generate the desired amount of braziers : {list.Count} < {count}", (CLogLevel)1, true, false);
		}
		list = RandomManager.Shuffle(TPSingleton<SpawnWaveManager>.Instance, list).ToList();
		if (prioritizeWaveSides)
		{
			PrioritizeWaveSides(list);
		}
		((CLogger<BuildingManager>)this).Log((object)$"Generating {Mathf.Min(count, list.Count)} braziers with id \"{brazierDefinition.Id}\".", (CLogLevel)1, false, false);
		List<Vector2> list2 = new List<Vector2>();
		for (int i = 0; i < list.Count && i < count; i++)
		{
			list2.Add(TileMapView.GetTileCenter(list[i].OriginTile));
		}
		KMeansClustersInfo kMeansClustersInfo = null;
		int num = 1;
		while (kMeansClustersInfo == null || !SkillCasterAttackGroups.MaxDistancesAreInBound(kMeansClustersInfo))
		{
			kMeansClustersInfo = KMeans.GetBestClusters(list2, num++, 10, 50, null, SkillCasterAttackGroups.MaxDistancesAreInBound);
		}
		return LightBraziersCoroutine(list, kMeansClustersInfo, brazierDefinition, bossPhaseActorId);
	}

	private void PrioritizeWaveSides(List<TheLastStand.Model.Building.Building> candidateBraziers)
	{
		Tile centerTile = TileMapController.GetCenterTile();
		int num = ListExtensions.Split<TheLastStand.Model.Building.Building>(candidateBraziers, (Func<TheLastStand.Model.Building.Building, bool>)delegate(TheLastStand.Model.Building.Building brazier)
		{
			foreach (SpawnPointInfo item in SpawnWaveManager.CurrentSpawnWave.SpawnPointsInfo)
			{
				if (!brazier.OriginTile.DirectionToCenter.IsOppositeTo(item.Direction))
				{
					return true;
				}
			}
			return false;
		}, 0, -1);
		ListExtensions.Split<TheLastStand.Model.Building.Building>(candidateBraziers, (Func<TheLastStand.Model.Building.Building, bool>)delegate(TheLastStand.Model.Building.Building brazier)
		{
			SpawnDirectionsDefinition.E_Direction brazierDirection = TileMapController.GetDirectionBetweenTiles(centerTile, brazier.OriginTile).ToSpawnDirection();
			return SpawnWaveManager.CurrentSpawnWave.SpawnPointsInfo.Any((SpawnPointInfo x) => x.Direction == brazierDirection);
		}, 0, num);
	}

	private IEnumerator LightBraziersCoroutine(List<TheLastStand.Model.Building.Building> candidateBraziers, KMeansClustersInfo clusters, BuildingDefinition brazierDefinition, string bossPhaseActorId = null)
	{
		int clusterIndex = 0;
		while (clusterIndex < clusters.means.Length)
		{
			ACameraView.MoveTo(Vector2.op_Implicit(clusters.means[clusterIndex]), CameraView.AnimationMoveSpeed, (Ease)0);
			yield return SharedYields.WaitForSeconds(CameraView.AnimationMoveSpeed);
			yield return SharedYields.WaitForSeconds(waitBeforeGuardiansSpawnDuration);
			for (int i = 0; i < clusters.clusterIdByData.Length; i++)
			{
				if (clusters.clusterIdByData[i] == clusterIndex)
				{
					ReplaceBuilding(candidateBraziers[i].OriginTile, candidateBraziers[i], brazierDefinition, ignoreBuilding: true, instantly: false, carryUpgrades: false, bossPhaseActorId);
				}
			}
			yield return SharedYields.WaitForSeconds(guardianSpawnAnimDuration);
			int num = clusterIndex + 1;
			clusterIndex = num;
		}
	}

	public IEnumerator ClearRandomBuildings(bool instantly = false)
	{
		if (!TryGetRandomBuildingsDefinitionForSelectedCity(out var randomBuildingsPerDayDefinitions) || !randomBuildingsPerDayDefinitions.RandomBuildingsPerDayDefinitions.TryGetValue(TPSingleton<GameManager>.Instance.Game.DayNumber, out var _))
		{
			yield break;
		}
		List<Tuple<KeyValuePair<Tile, BuildingDefinition>, float>> randomBuildingDelays = new List<Tuple<KeyValuePair<Tile, BuildingDefinition>, float>>(RandomsBuildings.Count);
		foreach (KeyValuePair<Tile, BuildingDefinition> randomsBuilding in RandomsBuildings)
		{
			randomBuildingDelays.Add(new Tuple<KeyValuePair<Tile, BuildingDefinition>, float>(randomsBuilding, Random.Range(0f, instantly ? 0f : randomBuildingsDisappearanceMaxDelay)));
		}
		randomBuildingDelays.Sort((Tuple<KeyValuePair<Tile, BuildingDefinition>, float> tuple, Tuple<KeyValuePair<Tile, BuildingDefinition>, float> tuple1) => (tuple.Item2 > tuple1.Item2) ? 1 : ((tuple.Item2 != tuple1.Item2) ? (-1) : 0));
		float time = 0f;
		int buildingIndex = 0;
		while (buildingIndex < randomBuildingDelays.Count)
		{
			for (time += Time.deltaTime; buildingIndex < randomBuildingDelays.Count && randomBuildingDelays[buildingIndex].Item2 < time; buildingIndex++)
			{
				DestroyBuilding(randomBuildingDelays[buildingIndex].Item1.Key, updateView: true, addDeadBuilding: false, triggerEvent: true, triggerOnDeathEvent: true, recomputeReachableTiles: true, playDeathSound: true);
				List<Tile> occupiedTiles = randomBuildingDelays[buildingIndex].Item1.Key.GetOccupiedTiles(randomBuildingDelays[buildingIndex].Item1.Value.BlueprintModuleDefinition);
				for (int num = occupiedTiles.Count - 1; num >= 0; num--)
				{
					RestoreBuildingIfNeeded(occupiedTiles[num]);
				}
			}
			yield return null;
		}
		RandomsBuildings.Clear();
		TilesLockedByRandomBuildings.Clear();
	}

	public void ComputeRandomBuildingsPositions(string debugDirectionsId = "", int debugRotationsCount = -1)
	{
		if (!TryGetRandomBuildingsDefinitionForSelectedCity(out var randomBuildingsPerDayDefinitions))
		{
			return;
		}
		int dayNumber = TPSingleton<GameManager>.Instance.Game.DayNumber;
		RandomBuildingsDirectionsDefinition value = null;
		if (string.IsNullOrEmpty(debugDirectionsId))
		{
			if (!randomBuildingsPerDayDefinitions.RandomBuildingsPerDayDefinitions.TryGetValue(dayNumber, out var value2))
			{
				return;
			}
			value = DictionaryHelpers.GetRandomItemFromWeights(value2, this);
		}
		else if (!BuildingDatabase.RandomBuildingsDirectionsDefinitions.TryGetValue(debugDirectionsId, out value))
		{
			((CLogger<BuildingManager>)this).LogError((object)("Unknown RandomBuildingsDirectionsDefinition of Id " + debugDirectionsId), (CLogLevel)1, true, true);
			return;
		}
		((CLogger<BuildingManager>)this).Log((object)string.Format("Using {0} with Id {1} for DayNumber {2}.", "RandomBuildingsDirectionsDefinition", value.Id, dayNumber), (CLogLevel)0, false, false);
		Dictionary<SpawnDirectionsDefinition.E_Direction, RandomBuildingsGenerationDefinition> dictionary = new Dictionary<SpawnDirectionsDefinition.E_Direction, RandomBuildingsGenerationDefinition>();
		switch ((debugRotationsCount != -1) ? debugRotationsCount : ((!value.DisableRotation) ? RandomManager.GetRandomRange(this, 0, 4) : 0))
		{
		case 0:
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Top, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Top]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Right, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Right]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Bottom, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Bottom]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Left, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Left]);
			break;
		case 1:
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Top, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Left]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Right, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Top]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Bottom, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Right]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Left, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Bottom]);
			break;
		case 2:
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Top, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Bottom]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Right, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Left]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Bottom, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Top]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Left, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Right]);
			break;
		case 3:
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Top, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Right]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Right, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Bottom]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Bottom, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Left]);
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Left, value.GenerationDefinitionByDirection[SpawnDirectionsDefinition.E_Direction.Top]);
			break;
		}
		List<string> ids = GenericDatabase.IdsListDefinitions["AllowingRandomBuildings"].Ids;
		foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, RandomBuildingsGenerationDefinition> generationDefinitionPerDirection in dictionary)
		{
			foreach (RandomBuildingsGenerationDefinition.BuildingInfo item in generationDefinitionPerDirection.Value.BuildingsInfo)
			{
				IEnumerable<Tile> enumerable = from o in TileMapManager.GetTilesWithFlag(item.TileFlag)
					where o.DirectionToCenter == generationDefinitionPerDirection.Key
					select o;
				List<Tile> list = RandomManager.Shuffle(this, enumerable).ToList();
				int num = 0;
				foreach (Tile item2 in list)
				{
					BuildingDefinition buildingDefinition = BuildingDatabase.BuildingDefinitions[item.Id];
					List<Tile> occupiedTiles = item2.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
					bool flag = true;
					foreach (Tile item3 in occupiedTiles)
					{
						if ((item3.Building != null && !ids.Contains(item3.Building.Id)) || WillTileBeUsedForRandomBuilding(item3))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						RandomsBuildings.Add(item2, buildingDefinition);
						TilesLockedByRandomBuildings.AddRange(occupiedTiles);
						num++;
						if (num == item.Count)
						{
							break;
						}
					}
				}
			}
		}
	}

	public void GenerateRandomBuildings()
	{
		if (!TryGetRandomBuildingsDefinitionForSelectedCity(out var randomBuildingsPerDayDefinitions) || !randomBuildingsPerDayDefinitions.RandomBuildingsPerDayDefinitions.TryGetValue(TPSingleton<GameManager>.Instance.Game.DayNumber, out var _))
		{
			return;
		}
		foreach (KeyValuePair<Tile, BuildingDefinition> randomsBuilding in RandomsBuildings)
		{
			List<Tile> occupiedTiles = randomsBuilding.Key.GetOccupiedTiles(randomsBuilding.Value.BlueprintModuleDefinition);
			for (int num = occupiedTiles.Count - 1; num >= 0; num--)
			{
				Tile tile = occupiedTiles[num];
				TheLastStand.Model.Building.Building building = tile.Building;
				if (building != null && building.IsTrap && !BuildingsToRestore.ContainsKey(tile))
				{
					BuildingToRestore value2 = new BuildingToRestore(tile, tile.Building.Id, tile.Building.BattleModule.RemainingTrapCharges);
					BuildingsToRestore.Add(tile, value2);
				}
			}
			CreateBuilding(randomsBuilding.Value, randomsBuilding.Key, updateView: true, playSound: false);
		}
	}

	public IEnumerator GenerateRandomBuildingsCoroutine()
	{
		if (!TryGetRandomBuildingsDefinitionForSelectedCity(out var randomBuildingsPerDayDefinitions) || !randomBuildingsPerDayDefinitions.RandomBuildingsPerDayDefinitions.TryGetValue(TPSingleton<GameManager>.Instance.Game.DayNumber, out var _))
		{
			yield break;
		}
		List<Tuple<KeyValuePair<Tile, BuildingDefinition>, float>> randomBuildingDelays = new List<Tuple<KeyValuePair<Tile, BuildingDefinition>, float>>(RandomsBuildings.Count);
		foreach (KeyValuePair<Tile, BuildingDefinition> randomsBuilding in RandomsBuildings)
		{
			randomBuildingDelays.Add(new Tuple<KeyValuePair<Tile, BuildingDefinition>, float>(randomsBuilding, Random.Range(0f, randomBuildingsAppearanceMaxDelay)));
		}
		randomBuildingDelays.Sort((Tuple<KeyValuePair<Tile, BuildingDefinition>, float> tuple, Tuple<KeyValuePair<Tile, BuildingDefinition>, float> tuple1) => (tuple.Item2 > tuple1.Item2) ? 1 : ((tuple.Item2 != tuple1.Item2) ? (-1) : 0));
		float time = 0f;
		int buildingIndex = 0;
		while (buildingIndex < randomBuildingDelays.Count)
		{
			for (time += Time.deltaTime; buildingIndex < randomBuildingDelays.Count && randomBuildingDelays[buildingIndex].Item2 < time; buildingIndex++)
			{
				List<Tile> occupiedTiles = randomBuildingDelays[buildingIndex].Item1.Key.GetOccupiedTiles(randomBuildingDelays[buildingIndex].Item1.Value.BlueprintModuleDefinition);
				for (int num = occupiedTiles.Count - 1; num >= 0; num--)
				{
					Tile tile = occupiedTiles[num];
					TheLastStand.Model.Building.Building building = tile.Building;
					if (building != null && building.IsTrap && !BuildingsToRestore.ContainsKey(tile))
					{
						BuildingToRestore value2 = new BuildingToRestore(tile, tile.Building.Id, tile.Building.BattleModule.RemainingTrapCharges);
						BuildingsToRestore.Add(tile, value2);
					}
				}
				CreateBuilding(randomBuildingDelays[buildingIndex].Item1.Value, randomBuildingDelays[buildingIndex].Item1.Key, updateView: true, playSound: true, instantly: false);
			}
			yield return null;
		}
	}

	public bool WillTileBeUsedForRandomBuilding(Tile tile)
	{
		if (TilesLockedByRandomBuildings.Contains(tile))
		{
			return true;
		}
		return false;
	}

	private bool TryGetRandomBuildingsDefinitionForSelectedCity(out RandomBuildingsPerDayDefinition randomBuildingsPerDayDefinitions)
	{
		randomBuildingsPerDayDefinitions = null;
		string randomBuildingsPerDayDefinitionId = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.RandomBuildingsPerDayDefinitionId;
		if (!string.IsNullOrEmpty(randomBuildingsPerDayDefinitionId))
		{
			return BuildingDatabase.RandomBuildingsPerDayDefinitions.TryGetValue(randomBuildingsPerDayDefinitionId, out randomBuildingsPerDayDefinitions);
		}
		return false;
	}

	public void BuyNewMage()
	{
		MagicCircle.MageCount++;
		MagicCircle.BuildingController.DamageableModuleController.GainHealth(MagicCircle.MageLife);
		MagicCircle.MagicCircleView.Dirty = true;
	}

	public void CastSelectedSkill()
	{
		((Conversation<ICompensableCommand, ICompensableCommand>)(object)PlayableUnitManager.UnitsConversation).Clear();
		GameView.BottomScreenPanel.BottomLeftPanel.CancelMovementPanel.Refresh();
		GameController.SetState(Game.E_State.BuildingExecutingSkill);
		if (TileObjectSelectionManager.SelectedBuilding.Id == "Catapult")
		{
			TrophyManager.AppendValueToTrophiesConditions<CatapultUsedTrophyConditionController>(new object[1] { 1 });
		}
		SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.ExecuteSkill();
		((MonoBehaviour)this).StartCoroutine(WaitForSkillExecution(TileObjectSelectionManager.SelectedBuilding));
		if (InputManager.JoystickConfig.HUDNavigation.DeselectBuildingSkillAfterExecution)
		{
			GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
		}
	}

	public List<TheLastStand.Model.Building.Building> GetAvailableBuildingsWithActions(Predicate<BuildingAction> filter = null)
	{
		List<TheLastStand.Model.Building.Building> list = new List<TheLastStand.Model.Building.Building>();
		int i = 0;
		for (int count = TPSingleton<BuildingManager>.Instance.Buildings.Count; i < count; i++)
		{
			TheLastStand.Model.Building.Building building = TPSingleton<BuildingManager>.Instance.Buildings[i];
			if (building.ProductionModule?.BuildingActions == null)
			{
				continue;
			}
			if (filter != null)
			{
				if (building.ProductionModule.BuildingActions.Exists(filter))
				{
					list.Add(building);
				}
			}
			else
			{
				list.Add(building);
			}
		}
		return list;
	}

	public void PlayBonePileConstructionSound()
	{
		if (BonePileGenerationCounter != null && BonePileGenerationCounter.Count > 0)
		{
			PlayConstructionSound(BuildingDatabase.BuildingDefinitions[BonePileGenerationCounter.First().Key]);
		}
	}

	public void RefreshBuildingsProductionPanels()
	{
		int i = 0;
		for (int count = Buildings.Count; i < count; i++)
		{
			TheLastStand.Model.Building.Building building = Buildings[i];
			if (building.ProductionModule?.BuildingGaugeEffect != null)
			{
				(building as MagicCircle)?.MagicCircleView.MagicCircleHUD?.ProductionPanelMagicCircle.AnimateUnitsIncrement();
			}
		}
	}

	public void ResetShopRerollIndex()
	{
		Shop.ShopRerollIndex = 0;
		Shop.ShopView.RefreshRerollButton();
	}

	public void RestoreBuildingIfNeeded(Tile tile)
	{
		if (!BuildingsToRestore.TryGetValue(tile, out var value))
		{
			return;
		}
		if (BuildingDatabase.BuildingDefinitions.TryGetValue(value.BuildingId, out var value2))
		{
			TheLastStand.Model.Building.Building building = CreateBuilding(value2, tile, updateView: true, playSound: false);
			if (building.BattleModule != null)
			{
				building.BattleModule.RemainingTrapCharges = value.TrapUses;
			}
		}
		BuildingsToRestore.Remove(tile);
	}

	public void BackwardCompatibilityAfterDeserialize(SerializedGameState saveGame, int saveVersion)
	{
		if (saveVersion == 19)
		{
			for (int num = Buildings.Count - 1; num >= 0; num--)
			{
				TheLastStand.Model.Building.Building building = Buildings[num];
				TheLastStand.Model.Building.Building building2 = building;
				building2.BossPhaseActorId = building.Id switch
				{
					"BrazierBoss_Lit1" => "BrazierA_P1", 
					"BrazierBoss_Lit2" => "BrazierA_P2", 
					"BrazierBoss_Lit3" => "BrazierA_P3", 
					_ => building.BossPhaseActorId, 
				};
			}
		}
	}

	public void StartTurn()
	{
		for (int num = Buildings.Count - 1; num >= 0; num--)
		{
			Buildings[num].BuildingController.StartTurn();
		}
	}

	public IEnumerator TriggerBuildingPassiveCoroutine()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			StartTurn();
			EffectManager.DisplayEffects();
			yield return (object)new WaitWhile((Func<bool>)(() => WaitBuildingGauges.Count > 0));
		}
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver)
		{
			GameManager.Save();
			GameController.SetState(Game.E_State.Management);
			TPSingleton<ToDoListView>.Instance.SwitchRaycastTargetState(state: true);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (((StateMachine)ApplicationManager.Application).State.GetName() != "LevelEditor")
		{
			MetaUpgradesManager.MetaUpgradeActivated += OnMetaUpgradeActivated;
		}
	}

	protected override void OnDestroy()
	{
		((CLogger<BuildingManager>)this).OnDestroy();
		MetaUpgradesManager.MetaUpgradeActivated -= OnMetaUpgradeActivated;
	}

	private static TheLastStand.Model.Building.Building CreateBuilding(SerializedBuilding buildingElement, int saveVersion)
	{
		if (!BuildingDatabase.BuildingDefinitions.TryGetValue(buildingElement.Id, out var value))
		{
			throw new MissingAssetException<BuildingDatabase>(buildingElement.Id);
		}
		Tile tile = TileMapManager.GetTile(buildingElement.Position.X, buildingElement.Position.Y);
		BuildingView buildingView = CreateBuildingView(value);
		TheLastStand.Model.Building.Building building = new BuildingController(buildingElement, value, buildingView, tile, saveVersion).Building;
		InitBuilding(building, tile, updateView: true, playSound: false, instantly: true, recomputeReachableTiles: false, onLoad: true);
		return building;
	}

	private static BuildingView CreateBuildingView(BuildingDefinition buildingDefinition)
	{
		BuildingView buildingView = ((buildingDefinition.Id == "MagicCircle") ? Object.Instantiate<BuildingView>(TPSingleton<BuildingManager>.Instance.magicCircleViewPrefab) : ((!buildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Watchtower)) ? ObjectPooler.GetPooledComponent<BuildingView>("BuildingViews", TPSingleton<BuildingManager>.Instance.buildingViewPrefab, (Transform)null, false) : ObjectPooler.GetPooledComponent<BuildingView>("WatchtowerViews", TPSingleton<BuildingManager>.Instance.watchTowerViewPrefab, (Transform)null, false)));
		buildingView.Init();
		((Component)buildingView).transform.SetParent(TPSingleton<BuildingManager>.Instance.buildingsTransform, true);
		((Object)((Component)buildingView).gameObject).name = buildingDefinition.Id;
		if (buildingDefinition.BattleModuleDefinition != null)
		{
			buildingView.InitHandledDefensesHud(buildingDefinition);
		}
		return buildingView;
	}

	private static void InitBuilding(TheLastStand.Model.Building.Building building, Tile tile, bool updateView = true, bool playSound = true, bool instantly = false, bool recomputeReachableTiles = true, bool onLoad = false)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)string.Format("Initializing building {0}{1} at position {2}", building.BuildingDefinition.Id, instantly ? " instantly" : string.Empty, tile.Position), (CLogLevel)1, false, false);
		TPSingleton<BuildingManager>.Instance.Buildings.Add(building);
		if (building.IsBrazier)
		{
			TPSingleton<BuildingManager>.Instance.Braziers.Add(building);
		}
		building.BuildingView.BuildingController = building.BuildingController;
		building.BuildingView.InitVisuals();
		if (tile.Building != null && !building.BlueprintModule.IsIndestructible)
		{
			float num = building.DamageableModule.HealthTotal * ((!tile.Building.BlueprintModule.IsIndestructible) ? (tile.Building.DamageableModule.Health / tile.Building.DamageableModule.HealthTotal) : 1f);
			building.BuildingController.DamageableModuleController.SetHealth((int)num);
		}
		for (int i = 0; i < building.BlueprintModule.OccupiedTiles.Count; i++)
		{
			DestroyBuilding(building.BlueprintModule.OccupiedTiles[i], updateView, addDeadBuilding: false, triggerEvent: true, triggerOnDeathEvent: true, recomputeReachableTiles);
			building.BlueprintModule.OccupiedTiles[i].CurrentUnitAccess = building.BlueprintModule.ConvertBuildingTileToUnitAccess(building.BlueprintModule.OccupiedTiles[i]);
			building.BlueprintModule.OccupiedTiles[i].TileController.SetBuilding(building);
			if (building.BuildingDefinition.ConstructionModuleDefinition.OccupationVolumeType != BuildingDefinition.E_OccupationVolumeType.Adjacent)
			{
				continue;
			}
			for (int j = -1; j < 2; j++)
			{
				for (int k = -1; k < 2; k++)
				{
					TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
					Vector2Int position = building.BlueprintModule.OccupiedTiles[i].Position;
					int x = ((Vector2Int)(ref position)).x + j;
					position = building.BlueprintModule.OccupiedTiles[i].Position;
					Tile tile2 = tileMap.GetTile(x, ((Vector2Int)(ref position)).y + k);
					if (tile2 != null && !building.BlueprintModule.OccupiedTiles.Contains(tile2))
					{
						tile2.TileController.SetOccupiedByBuildingVolume(isInBuildingVolume: true);
					}
				}
			}
		}
		if (updateView)
		{
			string suffix = ((!building.IsTrap) ? string.Empty : ((building.BattleModule.RemainingTrapCharges == 0) ? "_Disabled" : string.Empty));
			if (instantly)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuildingInstantly(building, tile, suffix);
			}
			else
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuilding(building, tile, suffix);
			}
		}
		if (playSound && (building.ConstructionModule == null || building.ConstructionModule.ConstructionModuleDefinition.PlayConstructionSound))
		{
			PlayConstructionSound(building.BuildingDefinition);
		}
		TPSingleton<ConstructionManager>.Instance.IncrementBuildingCount(building.BuildingDefinition);
		EventManager.TriggerEvent((Event)(object)new BuildingConstructedEvent(building.Id));
		if (TPSingleton<PlayableUnitManager>.Exist())
		{
			TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles = true;
		}
		if (building.BuildingDefinition.Id == "Shop" || building.BuildingDefinition.Id == "Inn")
		{
			if (building.BuildingDefinition.Id == "Shop")
			{
				TPSingleton<BuildingManager>.Instance.AccessShopBuildingCount++;
			}
			if (TPSingleton<ToDoListView>.Exist())
			{
				TPSingleton<ToDoListView>.Instance.RefreshGoldNotification();
			}
		}
		building.PassivesModule?.PassivesModuleController.ApplyPassiveEffect(E_EffectTime.Permanent, force: false, onLoad);
	}

	private static void PlayConstructionSound(BuildingDefinition buildingDefinition)
	{
		AudioClip[] array = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/BuildingConstruction/" + buildingDefinition.Id, true);
		if (array != null && array.Length != 0)
		{
			SoundManager.PlayAudioClip(array, BuildingPooledAudioSourceData);
		}
		else
		{
			SoundManager.PlayAudioClip((buildingDefinition.ConstructionModuleDefinition.ConstructionAnimationType == BuildingDefinition.E_ConstructionAnimationType.Animated) ? TPSingleton<BuildingManager>.Instance.constructionAnimationAudioClips : TPSingleton<BuildingManager>.Instance.constructionSimpleAudioClips, BuildingPooledAudioSourceData);
		}
	}

	private void CastSelectedAction()
	{
		if (selectedBuildingAction.ProductionBuilding.BuildingParent.BlueprintModule.IsIndestructible || !selectedBuildingAction.ProductionBuilding.BuildingParent.DamageableModule.IsDead)
		{
			GameController.SetState(Game.E_State.BuildingExecutingAction);
			selectedBuildingAction.BuildingActionController.ExecuteActionEffects();
			((MonoBehaviour)this).StartCoroutine(WaitFinishAction());
			int i = 0;
			for (int count = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i < count; i++)
			{
				TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitView.ToggleSkillTargeting(show: false);
			}
			if (InputManager.JoystickConfig.HUDNavigation.DeselectBuildingSkillAfterExecution)
			{
				GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
			}
		}
	}

	private void GenerateInitialBuildings()
	{
		bool flag = ((StateMachine)ApplicationManager.Application).State.GetName() == "LevelEditor";
		string levelLayoutId = (flag ? LevelEditorManager.CityToLoadId : TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.LevelLayoutBuildingsId);
		string format = ((flag || GameManager.LoadLevelEditorCityAssets) ? "TextAssets/Cities/Level Editor/{0}/{0}{1}_Buildings" : "TextAssets/Cities/{0}/{0}{1}_Buildings");
		string arg = string.Empty;
		UpgradeCityMetaEffectDefinition[] effects;
		if (flag)
		{
			arg = ((LevelEditorManager.CityToLoadLevel > 0) ? LevelEditorManager.CityToLoadLevel.ToString() : string.Empty);
		}
		else if (MetaUpgradeEffectsController.TryGetEffectsOfType<UpgradeCityMetaEffectDefinition>(out effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			arg = effects.LastOrDefault((UpgradeCityMetaEffectDefinition o) => o.CityId == levelLayoutId)?.Level.ToString();
		}
		TextAsset val = ResourcePooler.LoadOnce<TextAsset>(string.Format(format, levelLayoutId, arg), false);
		((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)("Loading Level Buildings layout using Id <b>" + levelLayoutId + "</b> from path <b>" + string.Format(format, levelLayoutId, arg) + "</b> " + (GameManager.LoadLevelEditorCityAssets ? " (in Level but using Level Editor path)" : string.Empty) + "."), (CLogLevel)2, true, false);
		((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)("Level Tilemap Buildings loading " + (((Object)(object)val != (Object)null) ? "<color=green>succeeded</color>" : "<color=red>failed</color>") + "."), (CLogLevel)2, true, false);
		foreach (XElement item in ((XContainer)((XContainer)XDocument.Parse(val.text, (LoadOptions)2)).Element(XName.op_Implicit("Buildings"))).Elements(XName.op_Implicit("Building")))
		{
			XAttribute val2 = item.Attribute(XName.op_Implicit("Id"));
			XAttribute val3 = item.Attribute(XName.op_Implicit("X"));
			XAttribute val4 = item.Attribute(XName.op_Implicit("Y"));
			if (!BuildingDatabase.BuildingDefinitions.TryGetValue(val2.Value, out var value))
			{
				((CLogger<BuildingManager>)this).LogWarning((object)("Building Id " + val2.Value + " not found in Database, skipping it."), (CLogLevel)2, true, false);
				continue;
			}
			if (!int.TryParse(val3.Value, out var result))
			{
				((CLogger<BuildingManager>)this).LogError((object)("Could not parse " + val2.Value + " X value in text asset " + ((Object)val).name + ", skipping this building."), (CLogLevel)2, true, true);
				continue;
			}
			if (!int.TryParse(val4.Value, out var result2))
			{
				((CLogger<BuildingManager>)this).LogError((object)("Could not parse " + val2.Value + " Y value in text asset " + ((Object)val).name + ", skipping this building."), (CLogLevel)2, true, true);
				continue;
			}
			TheLastStand.Model.Building.Building building = CreateBuilding(value, TileMapManager.GetTile(result, result2), updateView: true, playSound: false, instantly: true, triggerEvent: false);
			if (((StateMachine)ApplicationManager.Application).State.GetName() != "LevelEditor")
			{
				XElement val5 = ((XContainer)item).Element(XName.op_Implicit("Health"));
				if (val5 != null)
				{
					if (!float.TryParse(val5.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
					{
						((CLogger<BuildingManager>)this).LogError((object)("Could not parse " + val5.Value + " in text asset " + ((Object)val).name + " to valid float value, skipping this building."), (CLogLevel)2, true, true);
						continue;
					}
					building.DamageableModule?.DamageableModuleController.SetHealth(result3);
				}
				XElement val6 = ((XContainer)item).Element(XName.op_Implicit("UpgradesLevels"));
				if (val6 == null)
				{
					continue;
				}
				foreach (XElement buildingUpgradeLevelElement in ((XContainer)val6).Elements())
				{
					BuildingUpgrade buildingUpgrade = building.UpgradeModule.BuildingUpgrades.FirstOrDefault((BuildingUpgrade o) => o.BuildingUpgradeDefinition.Id == buildingUpgradeLevelElement.Name.LocalName);
					if (buildingUpgrade != null)
					{
						int num = int.Parse(buildingUpgradeLevelElement.Value);
						for (int i = 0; i < num; i++)
						{
							buildingUpgrade.BuildingUpgradeController.UnlockUpgrade(freeUpgrade: true, playFx: false);
						}
					}
				}
			}
			else
			{
				TPSingleton<BuildingsSettingsManager>.Instance.OnBuildingPlaced(building, (XContainer)(object)item);
			}
		}
		if (!flag)
		{
			TPSingleton<BuildingManager>.Instance.ComputeRandomBuildingsPositions();
			TPSingleton<BuildingManager>.Instance.GenerateRandomBuildings();
			int modifiedLightFogSpawnersCount = ApocalypseManager.CurrentApocalypse.GetModifiedLightFogSpawnersCount(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.LightFogSpawnersGenerationDefinition.InitialCount);
			GenerateLightFogSpawners(modifiedLightFogSpawnersCount);
		}
	}

	private void OnMetaUpgradeActivated(MetaUpgrade metaUpgrade)
	{
		if (metaUpgrade.MetaUpgradeDefinition.UpgradeEffectDefinitions.Any((MetaEffectDefinition x) => x is BuildingModifierMetaEffectDefinition))
		{
			RefreshBuildingsHealth();
		}
	}

	private void RefreshBuildingsHealth()
	{
		if (Buildings == null)
		{
			return;
		}
		for (int num = Buildings.Count - 1; num >= 0; num--)
		{
			TheLastStand.Model.Building.Building building = Buildings[num];
			if (building != MagicCircle && BuildingDatabase.BuildingDefinitions.TryGetValue(building.Id, out var value) && value.DamageableModuleDefinition != null && building.DamageableModule.HealthTotal != value.DamageableModuleDefinition.HealthTotal)
			{
				building.BuildingController.DamageableModuleController.UpdateHealth(value.DamageableModuleDefinition.HealthTotal);
			}
		}
	}

	private void Update()
	{
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		if (!TPSingleton<ConstructionView>.Exist())
		{
			return;
		}
		if (InputManager.GetButtonDown(52))
		{
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Recruitment && RecruitmentController.CanOpenRecruitmentPanel())
			{
				RecruitmentController.OpenRecruitmentPanel();
			}
			else if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Recruitment)
			{
				RecruitmentController.CloseRecruitmentPanel();
			}
		}
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingPreparingAction)
		{
			if (SelectedBuildingAction.IsExecutionInstant)
			{
				SelectedBuildingAction.BuildingActionController.SetTarget(SelectedBuildingAction.ProductionBuilding.BuildingParent.OriginTile);
				CastSelectedAction();
			}
			else if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				int i = 0;
				for (int count = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i < count; i++)
				{
					TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitView.ToggleSkillTargeting(show: false);
				}
				((MonoBehaviour)this).StartCoroutine(WaitFinishAction(instant: true));
			}
			else
			{
				if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
				{
					if (tile != null && tile.Unit != null)
					{
						UnitView unitView = tile.Unit.UnitView;
						if (SelectedBuildingAction.BuildingActionController.CanExecuteActionOnTile(tile))
						{
							unitView.OnSkillTargetHover(hover: true);
						}
						unitView.UnitHUD.AttackEstimationDisplay.DisplayBuildingAction(SelectedBuildingAction, tile.Unit);
					}
					if (previousTile != null && previousTile.Unit != null)
					{
						UnitView unitView2 = previousTile.Unit.UnitView;
						unitView2.OnSkillTargetHover(hover: false);
						unitView2.UnitHUD.AttackEstimationDisplay.Hide();
					}
				}
				if (GameView.TopScreenPanel.UnitPortraitsPanel.TargettedPortraitHasChanged)
				{
					UnitPortraitView portraitIsHovered = GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered();
					UnitPortraitView previousPortraitWasHovered = GameView.TopScreenPanel.UnitPortraitsPanel.GetPreviousPortraitWasHovered();
					if (GameView.TopScreenPanel.UnitPortraitsPanel.CursorIsHoverPortrait && SelectedBuildingAction.BuildingActionController.CanExecuteActionOnTile(portraitIsHovered.PlayableUnit.OriginTile))
					{
						portraitIsHovered.PlayableUnit.PlayableUnitView.OnSkillTargetHover(hover: true);
					}
					if ((Object)(object)portraitIsHovered != (Object)null)
					{
						portraitIsHovered.PlayableUnit.UnitView.UnitHUD.AttackEstimationDisplay.DisplayBuildingAction(SelectedBuildingAction, portraitIsHovered.PlayableUnit);
					}
					if ((Object)(object)previousPortraitWasHovered != (Object)null)
					{
						previousPortraitWasHovered.PlayableUnit.PlayableUnitView.OnSkillTargetHover(hover: false);
						previousPortraitWasHovered.PlayableUnit.UnitView.UnitHUD.AttackEstimationDisplay.Hide();
					}
				}
				if (InputManager.GetButtonDown(22) && GameView.TopScreenPanel.UnitPortraitsPanel.CursorIsHoverPortrait)
				{
					tile = GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered().PlayableUnit.OriginTile;
					if (SelectedBuildingAction.BuildingActionController.CanExecuteActionOnTile(tile))
					{
						SelectedBuildingAction.BuildingActionController.SetTarget(tile);
						CastSelectedAction();
					}
					tile = null;
				}
				if (InputManager.GetButtonDown(24) && tile != null && SelectedBuildingAction.BuildingActionController.CanExecuteActionOnTile(tile))
				{
					SelectedBuildingAction.BuildingActionController.SetTarget(tile);
					tile.Unit.UnitView.OnSkillTargetHover(hover: false);
					CastSelectedAction();
				}
			}
		}
		else if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingPreparingSkill)
		{
			if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
			{
				SkillManager.SkillInfoPanel.RefreshOnTileChanged();
				Tile tile2 = TPSingleton<GameManager>.Instance.Game.Cursor.PreviousTile;
				TheLastStand.Model.Unit.Unit unit = tile?.Unit;
				if (SelectedSkill.SkillDefinition.SkillActionDefinition is AttackSkillActionDefinition)
				{
					if (unit == TileObjectSelectionManager.SelectedUnit)
					{
						int count2 = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern.Count;
						Vector2Int origin = TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
						if (count2 > ((Vector2Int)(ref origin)).x)
						{
							List<List<char>> pattern = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern;
							origin = TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
							int count3 = pattern[((Vector2Int)(ref origin)).x].Count;
							origin = TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
							if (count3 > ((Vector2Int)(ref origin)).y)
							{
								List<List<char>> pattern2 = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern;
								origin = TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
								List<char> list = pattern2[((Vector2Int)(ref origin)).x];
								origin = TPSingleton<BuildingManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
								if (list[((Vector2Int)(ref origin)).y] != 'X')
								{
									SkillManager.AttackInfoPanel.TargetTile = tile;
									SkillManager.AttackInfoPanel.TargetUnit = null;
									if (SkillManager.AttackInfoPanel.Displayed)
									{
										SkillManager.AttackInfoPanel.Hide();
									}
									goto IL_0602;
								}
							}
						}
					}
					if (!SkillManager.AttackInfoPanel.Displayed)
					{
						SkillManager.AttackInfoPanel.Display();
					}
					SkillManager.AttackInfoPanel.TargetTile = tile;
					SkillManager.AttackInfoPanel.TargetUnit = unit;
					SkillManager.AttackInfoPanel.RefreshAttackData();
				}
				else if (SelectedSkill?.SkillDefinition.ValidTargets != null)
				{
					if (SelectedSkill.SkillDefinition.ValidTargets.AnyUnits)
					{
						if (tile != null)
						{
							unit?.UnitView.OnSkillTargetHover(hover: true);
						}
						if (tile2 != null && tile2.Unit != null)
						{
							unit.UnitView.OnSkillTargetHover(hover: false);
						}
					}
					else
					{
						if (tile != null && tile.Building != null && SelectedSkill.SkillDefinition.ValidTargets.Buildings.ContainsKey(tile.Building.Id))
						{
							tile.Building.BuildingView.OnSkillTargetHover(hover: true);
						}
						if (tile2 != null && tile2.Building != null)
						{
							tile2.Building.BuildingView.OnSkillTargetHover(hover: false);
						}
					}
				}
			}
			goto IL_0602;
		}
		goto IL_0757;
		IL_0602:
		if (InputManager.GetButtonDown(0))
		{
			SelectedSkill = null;
			PlayableUnitManager.SelectNextUnit();
		}
		else if (InputManager.GetButtonDown(11))
		{
			SelectedSkill = null;
			PlayableUnitManager.SelectPreviousUnit();
		}
		else
		{
			int unitIndexHotkeyPressed = TPSingleton<PlayableUnitManager>.Instance.GetUnitIndexHotkeyPressed();
			if (unitIndexHotkeyPressed != -1)
			{
				SelectedSkill = null;
				TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed], CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(((Component)TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed].UnitView).transform.position));
			}
			if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				if (SelectedSkill.SkillAction.HasEffect("MultiHit") && SelectedSkill.SkillAction.SkillActionExecution.TargetTiles.Count > 0)
				{
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.RemoveLastTarget();
				}
				else
				{
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
					SelectedSkill = null;
					if (tile != null && !tile.HasFog && tile.Unit is EnemyUnit enemyUnit && enemyUnit.State != TheLastStand.Model.Unit.Unit.E_State.Dead)
					{
						TPSingleton<EnemyUnitManager>.Instance.DisplayOneEnemyReachableTiles(enemyUnit);
					}
				}
				SoundManager.PlayAudioClip(SkillManager.AudioSource, SkillManager.SkillCancelAudioClip);
			}
		}
		goto IL_0757;
		IL_0757:
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction)
		{
			if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged && tile?.Building != null && tile.Unit == null && (tile.Building.IsHandledDefense || (tile.Building.IsTrap && (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || !TPSingleton<PlayableUnitManager>.Instance.IsSelectedPlayableUnitInRange(tile)))))
			{
				tile.Building.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: true);
			}
			if (TileObjectSelectionManager.SelectedBuilding != null && (TileObjectSelectionManager.SelectedBuilding.BlueprintModule.IsIndestructible || !TileObjectSelectionManager.SelectedBuilding.DamageableModule.IsDead))
			{
				if (InputManager.GetButtonDown(83))
				{
					GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.SelectNextBuildingCapacity(next: false);
				}
				else if (InputManager.GetButtonDown(82))
				{
					GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.SelectNextBuildingCapacity(next: true);
				}
				else if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
				{
					GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
				}
				else if (InputManager.GetButtonDown(22))
				{
					GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.Submit();
				}
			}
		}
		if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged && previousTile?.Building != null && previousTile.Building != TileObjectSelectionManager.SelectedBuilding && !(PlayableUnitManager.SelectedSkill?.SkillAction is ResupplySkillAction) && (previousTile.Building.IsHandledDefense || (previousTile.Building.IsTrap && (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction || previousTile.Building.BattleModule.IsTrapFullyCharged()))))
		{
			previousTile.Building.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: false);
		}
		if (DebugManager.DebugMode && Input.GetKeyDown((KeyCode)286))
		{
			DebugReplenishUsesPerTurn();
		}
		previousTile = tile;
	}

	private IEnumerator WaitFinishAction(bool instant = false)
	{
		if (!instant && SelectedBuildingAction.BuildingActionDefinition.CastFxDefinition != null)
		{
			float num = SelectedBuildingAction.BuildingActionDefinition.CastFxDefinition.CastTotalDuration.EvalToFloat((InterpreterContext)(object)SelectedBuildingAction.CastFx.CastFXInterpreterContext);
			yield return SharedYields.WaitForSeconds(num);
		}
		else
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		GameView.BottomScreenPanel.BuildingManagementPanel.Refresh();
		EffectManager.DisplayEffects();
		SelectedBuildingAction = null;
		GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
	}

	private IEnumerator WaitForSkillExecution(TheLastStand.Model.Building.Building building)
	{
		if (building == null)
		{
			yield break;
		}
		while (building.BattleModule.IsExecutingSkill)
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		SelectedSkill?.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
		SelectedSkill = null;
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingExecutingSkill)
		{
			GameController.SetState(Game.E_State.Management);
			if (InputManager.IsLastControllerJoystick)
			{
				GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.JoystickSkillBar.SelectCurrentOrPreviousSkill();
				SkillManager.RefreshSelectedSkillValidityOnTile(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
			}
		}
	}

	[DevConsoleCommand]
	public static void MagicCircleMageCount(int newMageCount)
	{
		if (newMageCount == MagicCircle.MageCount)
		{
			return;
		}
		int mageCount = MagicCircle.MageCount;
		MagicCircle.MageCount = Mathf.Clamp(newMageCount, 0, MagicCircle.MageSlots);
		if (newMageCount > MagicCircle.MageSlots)
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogWarning((object)$"Magic Circle just has {MagicCircle.MageCount} slots!", (CLogLevel)1, true, false);
		}
		int num = MagicCircle.MageCount - mageCount;
		for (int i = 0; i < Mathf.Abs(num); i++)
		{
			if (num < 0)
			{
				MagicCircle.BuildingController.DamageableModuleController.LoseHealth(MagicCircle.MageLife);
			}
			else
			{
				MagicCircle.BuildingController.DamageableModuleController.GainHealth(MagicCircle.MageLife);
			}
		}
		MagicCircle.MagicCircleView.Dirty = true;
	}

	[DevConsoleCommand]
	public static void MagicCircleMageSlotCount(int newMageSlotCount)
	{
		if (newMageSlotCount != MagicCircle.MageSlots)
		{
			MagicCircle.MageSlots = Mathf.Clamp(newMageSlotCount, 1, MagicCircle.MagicCircleDefinition.MageSlotMax);
			if (newMageSlotCount > MagicCircle.MagicCircleDefinition.MageSlotMax)
			{
				((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogWarning((object)$"Maximum Magic Circle slots set up in BuildingDefinitions is {MagicCircle.MagicCircleDefinition.MageSlotMax}", (CLogLevel)1, true, false);
			}
			else if (newMageSlotCount < 1)
			{
				((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogWarning((object)"Magic Circle cannot have less than 1 slot.", (CLogLevel)1, true, false);
			}
			MagicCircleMageCount(Mathf.Min(MagicCircle.MageSlots, MagicCircle.MageCount));
			MagicCircle.MagicCircleView.Dirty = true;
		}
	}

	[DevConsoleCommand("MagicCircleToggleIndestructibility")]
	public static void DebugToggleMagicCircleIndestructibility(bool isIndestructible = true)
	{
		TPSingleton<BuildingManager>.Instance.magicCircle.DebugIsIndesctructible = isIndestructible;
	}

	[DevConsoleCommand("ReplenishUsesPerTurn")]
	public static void DebugReplenishUsesPerTurn()
	{
		if (TileObjectSelectionManager.SelectedBuilding != null)
		{
			TileObjectSelectionManager.SelectedBuilding.BuildingController.BattleModuleController?.RefillSkillsOverallUses();
			TileObjectSelectionManager.SelectedBuilding.BuildingController.ProductionModuleController?.RefreshActionsUsesPerTurn();
		}
		GameView.BottomScreenPanel.BuildingManagementPanel.Refresh();
	}

	[DevConsoleCommand("SetBarricadeOnAllTilesOutOfCity")]
	public static void DebugSetBarricadeOnAllTilesOutOfCity()
	{
		((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(SetBuildingOnAllTilesOutOfCityCoroutine("Barricade"));
	}

	[DevConsoleCommand("SetTrapOnAllTilesOutOfCity")]
	public static void DebugSetTrapOnAllTilesOutOfCity()
	{
		((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(SetBuildingOnAllTilesOutOfCityCoroutine("DamageTrap"));
	}

	[DevConsoleCommand("SetWarpGatesOnAllTilesOutOfCity")]
	public static void DebugSetWarpGatesOnAllTilesOutOfCity()
	{
		((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(SetBuildingOnAllTilesOutOfCityCoroutine("Teleporter"));
	}

	[DevConsoleCommand("SetBuildingOnAllTilesOutOfCity")]
	public static void SetBuildingOnAllTilesOutOfCity([StringConverter(typeof(TheLastStand.Model.Building.Building.StringToOneTileBuildingIdConverter))] string buildingId)
	{
		((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(SetBuildingOnAllTilesOutOfCityCoroutine(buildingId));
	}

	private static IEnumerator SetBuildingOnAllTilesOutOfCityCoroutine(string buildingId)
	{
		for (int i = 0; i < TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
		{
			for (int j = 0; j < TPSingleton<TileMapManager>.Instance.TileMap.Height; j++)
			{
				Tile tile = TileMapManager.GetTile(i, j);
				if (tile.GroundDefinition.GroundCategory != GroundDefinition.E_GroundCategory.City && tile.Building == null && tile.GroundDefinition.IsCrossable && !tile.HasFog)
				{
					CreateBuilding(BuildingDatabase.BuildingDefinitions[buildingId], tile);
				}
				if (j % TPSingleton<TileMapManager>.Instance.TileMap.Height == 0)
				{
					yield return null;
				}
			}
		}
	}

	[DevConsoleCommand(Name = "ForceUseBuildingActionAllPhases")]
	private static void DebugForceUseBuildingActionAllPhases(bool forceUseBuildingActionAllPhases = true)
	{
		TPSingleton<BuildingManager>.Instance.debugForceUseBuildingActionAllPhases = forceUseBuildingActionAllPhases;
		GameView.BottomScreenPanel.BuildingManagementPanel.Refresh();
	}

	[DevConsoleCommand(Name = "RandomBuildingsClear")]
	private static void DebugClearRandomBuildings()
	{
		((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(TPSingleton<BuildingManager>.Instance.ClearRandomBuildings());
	}

	[DevConsoleCommand(Name = "RandomBuildingsReroll")]
	private static void DebugRerollRandomBuildings(bool instantly = false)
	{
		DebugRerollRandomBuildings(string.Empty, -1, instantly);
	}

	[DevConsoleCommand(Name = "RandomBuildingsReroll")]
	private static void DebugRerollRandomBuildings([StringConverter(typeof(StringToRandomBuildingsDirectionsDefinitionIdConverter))] string directionId = "", bool instantly = false)
	{
		DebugRerollRandomBuildings(directionId, -1, instantly);
	}

	[DevConsoleCommand(Name = "RandomBuildingsReroll")]
	private static void DebugRerollRandomBuildings(int rotationsCount = -1, bool instantly = false)
	{
		DebugRerollRandomBuildings(string.Empty, rotationsCount, instantly);
	}

	[DevConsoleCommand(Name = "RandomBuildingsReroll")]
	private static void DebugRerollRandomBuildings([StringConverter(typeof(StringToRandomBuildingsDirectionsDefinitionIdConverter))] string directionId = "", int rotationsCount = -1, bool instantly = false)
	{
		((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(TPSingleton<BuildingManager>.Instance.DebugRerollRandomBuildingsCoroutine(directionId, rotationsCount, instantly));
	}

	private IEnumerator DebugRerollRandomBuildingsCoroutine(string directionId, int rotationsCount, bool instantly)
	{
		yield return ClearRandomBuildings(instantly);
		ComputeRandomBuildingsPositions(directionId, rotationsCount % 4);
		if (instantly)
		{
			TPSingleton<BuildingManager>.Instance.GenerateRandomBuildings();
		}
		else
		{
			yield return GenerateRandomBuildingsCoroutine();
		}
	}

	[DevConsoleCommand(Name = "BonePileSimulateLimitProgression")]
	private static void SimulateBonePileLimitProgression([StringConverter(typeof(StringToCityIdConverter))] string cityId)
	{
		if (!CityDatabase.CityDefinitions.TryGetValue(cityId, out var value))
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)("No city with Id " + cityId + " was found in CityDatabase."), (CLogLevel)1, true, true);
			return;
		}
		if (!BonePileDatabase.BonePileGeneratorsDefinition.CountProgressionDefinitions.TryGetValue(cityId, out var value2))
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)("No BonePile limit progression is defined for cityId " + cityId + "."), (CLogLevel)0, true, false);
			return;
		}
		for (int i = 0; i < value.VictoryDaysCount; i++)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (KeyValuePair<string, BonePileCountProgressionDefinition.ProgressionData> bonePileProgression in value2.BonePileProgressions)
			{
				int num = Mathf.CeilToInt((float)Mathf.Max(i - bonePileProgression.Value.Delay, 0) / (float)bonePileProgression.Value.IncreaseEveryXDays) * bonePileProgression.Value.IncreaseValue;
				int num2 = bonePileProgression.Value.BaseValue + num;
				if (bonePileProgression.Value.Limit > -1)
				{
					num2 = Mathf.Min(num2, bonePileProgression.Value.Limit);
				}
				dictionary.Add(bonePileProgression.Key, num2);
			}
			foreach (KeyValuePair<string, int> building in BonePileDatabase.BonePileGeneratorsDefinition.Buildings)
			{
				if (!dictionary.ContainsKey(building.Key))
				{
					dictionary.Add(building.Key, -1);
				}
			}
			string text = $"{cityId} Day {i + 1}";
			foreach (KeyValuePair<string, int> item in dictionary)
			{
				text = text + "\n- " + item.Key + " generation limit: " + ((item.Value == -1) ? "-1 (Unlimited)" : item.Value.ToString());
			}
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)text, (CLogLevel)0, true, false);
		}
	}

	private void Initialize()
	{
		if (((StateMachine)ApplicationManager.Application).State.GetName() != "LevelEditor")
		{
			Shop = new ShopController(shopView).Shop;
			ProductionReport = new ProductionReportController(productionReportPanel).ProductionReport;
			GlobalItemProductionUpgradeLevel = new BuildingUpgradeLevel();
		}
		GenerateInitialBuildings();
	}

	public void Deserialize(ISerializedData container, int saveVersion = -1)
	{
		Buildings = new List<TheLastStand.Model.Building.Building>();
		if (container is SerializedBuildings serializedBuildings)
		{
			Shop = new ShopController(serializedBuildings.Shop, shopView).Shop;
			Shop.Deserialize((ISerializedData)(object)serializedBuildings.Shop);
			ProductionReport = new ProductionReportController(serializedBuildings.ProductionReport, productionReportPanel).ProductionReport;
			GlobalItemProductionUpgradeLevel = new BuildingUpgradeLevel(serializedBuildings.BuildingUpgradeLevel);
			foreach (SerializedBuilding building in serializedBuildings.Buildings)
			{
				try
				{
					CreateBuilding(building, saveVersion);
				}
				catch (MissingAssetException<BuildingDatabase> arg)
				{
					((CLogger<BuildingManager>)this).LogError((object)$"{arg}\nThis asset will be skipped.", (CLogLevel)1, true, true);
				}
			}
			foreach (SerializedBuildingToRestore item in serializedBuildings.BuildingsToRestore)
			{
				try
				{
					BuildingToRestore buildingToRestore = new BuildingToRestore((ISerializedData)(object)item, saveVersion);
					BuildingsToRestore.Add(buildingToRestore.Tile, buildingToRestore);
				}
				catch (Exception arg2)
				{
					((CLogger<BuildingManager>)this).LogError((object)$"{arg2}\nThis building to restore will be skipped.", (CLogLevel)1, true, true);
				}
			}
			foreach (SerializedRandomBuilding randomBuilding in serializedBuildings.RandomBuildings)
			{
				try
				{
					Tile tile = TileMapManager.GetTile(randomBuilding.TilePosition.X, randomBuilding.TilePosition.Y);
					BuildingDefinition buildingDefinition = BuildingDatabase.BuildingDefinitions[randomBuilding.BuildingId];
					RandomsBuildings.Add(tile, buildingDefinition);
					TilesLockedByRandomBuildings.AddRange(tile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition));
				}
				catch (Exception arg3)
				{
					((CLogger<BuildingManager>)this).LogError((object)$"{arg3}\nThis random building will be skipped.", (CLogLevel)1, true, true);
				}
			}
			TPSingleton<ConstructionManager>.Instance.Deserialize((ISerializedData)(object)serializedBuildings.SerializedConstruction, saveVersion);
		}
		else
		{
			Initialize();
		}
		magicCircle = null;
	}

	public ISerializedData Serialize()
	{
		SerializedBuildings serializedBuildings = new SerializedBuildings
		{
			Shop = (Shop.Serialize() as SerializedShop),
			ProductionReport = (ProductionReport.Serialize() as SerializedProductionReport),
			BuildingUpgradeLevel = (GlobalItemProductionUpgradeLevel.Serialize() as SerializedBuildingUpgradeLevel),
			SerializedConstruction = (TPSingleton<ConstructionManager>.Instance.Serialize() as SerializedConstruction),
			BuildingsToRestore = BuildingsToRestore.Select((KeyValuePair<Tile, BuildingToRestore> o) => (SerializedBuildingToRestore)(object)o.Value.Serialize()).ToList(),
			RandomBuildings = RandomsBuildings.Select((KeyValuePair<Tile, BuildingDefinition> o) => new SerializedRandomBuilding
			{
				TilePosition = new SerializableVector2Int(o.Key.Position),
				BuildingId = o.Value.Id
			}).ToList()
		};
		for (int num = Buildings.Count - 1; num >= 0; num--)
		{
			serializedBuildings.Buildings.Add(Buildings[num].Serialize() as SerializedBuilding);
		}
		return (ISerializedData)(object)serializedBuildings;
	}
}
