using System;
using System.Collections;
using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.Building;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Skill;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Building.UI;
using TheLastStand.View.Cursor;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace TheLastStand.View.Building;

public class BuildingView : MonoBehaviour, IDamageableView, ITileObjectView
{
	public static class Constants
	{
		public static class Effects
		{
			public const string AttackFeedbackPrefabResourcePath = "Prefab/Displayable Effect/Attack Feedback";

			public const string ExtinguishBrazierFeedbackPrefabResourcePath = "Prefab/Displayable Effect/Extinguish Brazier Feedback";

			public const string HealFeedbackPrefabResourcePath = "Prefab/Displayable Effect/Heal Feedback";

			public const string DamageFlamePoolName = "BuildingDamageFlame";

			public const string DestructionSmokePoolName = "DestructionSmoke";

			public const string DamageParticlesPoolNameFormat = "Damaged Particles {0}";
		}

		public static class LocalizationPrefixes
		{
			public const string BuildingNamePrefix = "BuildingName_";

			public const string BuildingDescriptionPrefix = "BuildingDescription_";
		}

		public static class Sprites
		{
			public const string BuildingPortraitImagePrefix = "View/Sprites/UI/Buildings/Portraits/64px/BuildingsPortraits_";

			public const string BuildingSkillGoldCostIconSpritePath = "View/Sprites/UI/Buildings/ProductionRewards/Small/WorldUI_Gauges_Prod_Reward_Gold";

			public const string BuildingSkillMaterialsCostIconSpritePath = "View/Sprites/UI/Buildings/ProductionRewards/Small/WorldUI_Gauges_Prod_Reward_Material";

			public const string DestroyedBuildingPath = "View\\Sprites\\Buildings\\Destroyed\\{0}\\TLS_Buildings_{0}Remains";
		}

		public static class BuildingSkills
		{
			public const string Destroy = "Destroy";

			public const string Repair = "Repair";

			public const string Upgrade1 = "Upgrade1";

			public const string Upgrade2 = "Upgrade2";

			public const string Upgrade3 = "Upgrade3";

			public const string Upgrade4 = "Upgrade4";
		}

		public static class Sound
		{
			public const string DeathSoundPathFormat = "Sounds/SFX/Buildings/Death/{0}";
		}
	}

	[SerializeField]
	protected BuildingHUD hudPrefab;

	[SerializeField]
	private HandledDefensesHUD handledDefenseshudPrefab;

	[SerializeField]
	protected Transform hudFollowTarget;

	[SerializeField]
	private GameObject flamesLayer;

	[SerializeField]
	private DamagedBuildingFlameView[] flameViews;

	[SerializeField]
	private ParticleSystem smokeSystem;

	[SerializeField]
	private GameObject damagedParticles;

	[SerializeField]
	private Transform skillTargetingMarkAnchor;

	[SerializeField]
	private Vector3 skillTargetingOffset = Vector3.zero;

	private Dictionary<GameObject, DamagedBuildingFlameView> flamesPool = new Dictionary<GameObject, DamagedBuildingFlameView>();

	private BuildingController buildingController;

	private Coroutine displaySkillEffectsCoroutine;

	private Coroutine dieAnimCoroutine;

	private bool canFinalizedDeath;

	private HealFeedback healFeedback;

	private SkillTargetingMark skillTargetingMark;

	private bool skillTargetingMarkOffsetHasBeenSet;

	private bool hovered;

	private bool selected;

	private TheLastStand.Model.Skill.Skill skillBeingDisplayed;

	private bool initialized;

	public Action OnFinalInitFrame;

	public AttackFeedback AttackFeedback { get; private set; }

	public ExtinguishBrazierFeedback ExtinguishBrazierFeedback { get; private set; }

	public BuildingController BuildingController
	{
		get
		{
			return buildingController;
		}
		set
		{
			buildingController = value;
			BuildingHUD.Building = buildingController.Building;
			if ((Object)(object)HandledDefensesHUD != (Object)null)
			{
				HandledDefensesHUD.Building = buildingController.Building;
			}
		}
	}

	public BuildingHUD BuildingHUD { get; protected set; }

	public IDamageable Damageable => Building.DamageableModule;

	public IDamageableHUD DamageableHUD => BuildingHUD;

	public GainArmorFeedback GainArmorFeedback => null;

	public GameObject GameObject => ((Component)this).gameObject;

	public HandledDefensesHUD HandledDefensesHUD { get; private set; }

	public HealFeedback HealFeedback
	{
		get
		{
			if ((Object)(object)healFeedback == (Object)null)
			{
				healFeedback = Object.Instantiate<HealFeedback>(ResourcePooler.LoadOnce<HealFeedback>("Prefab/Displayable Effect/Heal Feedback", failSilently: false));
				healFeedback.Init(this);
			}
			return healFeedback;
		}
	}

	public Transform HudFollowTarget => hudFollowTarget;

	public bool Hovered
	{
		get
		{
			return hovered;
		}
		set
		{
			bool flag = value || (TileObjectSelectionManager.HasEnemyUnitSelected && TileObjectSelectionManager.SelectedEnemyUnit.LinkedBuilding == Building);
			if (hovered == flag)
			{
				return;
			}
			hovered = flag;
			if (!TPSingleton<TileObjectSelectionManager>.Exist())
			{
				return;
			}
			if (!TileObjectSelectionManager.HasBuildingSelected)
			{
				if (flag && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill)
				{
					DisplaySkillRangeIfNeeded(displayHandledSkill: true);
				}
				else
				{
					HideSkillRangeIfNeeded();
				}
			}
			RefreshCursorFeedback();
		}
	}

	public bool Selected
	{
		get
		{
			return selected;
		}
		set
		{
			if (selected == value)
			{
				return;
			}
			selected = value;
			if (!TPSingleton<TileObjectSelectionManager>.Exist())
			{
				return;
			}
			if (!value)
			{
				HideSkillRangeIfNeeded();
			}
			else
			{
				if (skillBeingDisplayed != null)
				{
					HideSkillRangeIfNeeded();
				}
				DisplaySkillRangeIfNeeded(displayHandledSkill: false);
			}
			RefreshCursorFeedback();
		}
	}

	public bool HoveredOrSelected
	{
		get
		{
			if (!Hovered)
			{
				return Selected;
			}
			return true;
		}
	}

	public bool PlaceholderView { get; set; }

	public List<IDisplayableEffect> SkillEffectDisplays { get; private set; } = new List<IDisplayableEffect>();


	public WaitUntil WaitUntilDeathCanBeFinalized { get; private set; }

	private TheLastStand.Model.Building.Building Building => buildingController.Building;

	public static Sprite GetPortraitSprite(string buildingDefinitionId)
	{
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Buildings/Portraits/Icons_Buildings_" + buildingDefinitionId, failSilently: false);
	}

	public static bool TryGetDiffuseTileBase(string buildingDefinitionId, string suffix, int x, int y, out TileBase tileBase)
	{
		tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}{4}{5}", "View/Tiles/Buildings/Diffuse", buildingDefinitionId, suffix, buildingDefinitionId, x, y), failSilently: true);
		if ((Object)(object)tileBase == (Object)null && buildingDefinitionId != "MagicCircle")
		{
			tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/Placeholder/Placeholder", failSilently: true);
			return false;
		}
		return true;
	}

	public static TileBase GetGhostTileBase(string buildingDefinitionId, string suffix, int x, int y)
	{
		TileBase tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}{4}{5}{6}", "View/Tiles/Buildings/Ghost", buildingDefinitionId, suffix, buildingDefinitionId, "Ghost", x, y), failSilently: true);
		if ((Object)(object)tileBase == (Object)null)
		{
			TryGetDiffuseTileBase(buildingDefinitionId, suffix, x, y, out tileBase);
		}
		return tileBase;
	}

	public void AddSkillEffectDisplay(IDisplayableEffect displayableEffect)
	{
		if (displayableEffect is EffectDisplay effectDisplay)
		{
			int num = ((Object)((Component)effectDisplay).gameObject).name.IndexOf('-');
			string text = ((num < 1) ? TPHelpers.RemoveFirstOccurrence(((Object)((Component)effectDisplay).gameObject).name, "(Clone)") : ((Object)((Component)effectDisplay).gameObject).name.Remove(num - 1));
			((Object)effectDisplay).name = text + " - " + ((Object)this).name;
			effectDisplay.FollowElement.ChangeTarget(DamageableHUD.Transform);
		}
		SkillEffectDisplays.Add(displayableEffect);
	}

	private void DisplayLinkedEnemiesHover(bool show)
	{
		if (Building.PassivesModule?.BuildingPassives == null)
		{
			return;
		}
		foreach (EnemyUnit linkedEnemy in Building.PassivesModule.GetLinkedEnemies())
		{
			linkedEnemy.EnemyUnitView.Hovered = show;
		}
	}

	private TheLastStand.Model.Skill.Skill GetSkillToDisplayOnHover(bool fallbackOnHandledSkill = true)
	{
		if (Building.BattleModule == null)
		{
			return null;
		}
		TheLastStand.Model.Skill.Skill result = null;
		if (Building.BattleModule.Goals != null && Building.BattleModule.Goals[0].GoalController.CheckPreconditionGroups())
		{
			result = Building.BattleModule.Goals[0].Skill;
		}
		else if (fallbackOnHandledSkill && Building.IsHandledDefense && Building.BattleModule.Skills != null)
		{
			result = Building.BattleModule.Skills[0];
		}
		return result;
	}

	public Coroutine DisplaySkillEffects(float delay)
	{
		if (displaySkillEffectsCoroutine != null)
		{
			return null;
		}
		displaySkillEffectsCoroutine = ((MonoBehaviour)this).StartCoroutine(DisplaySkillEffectsCoroutine(delay));
		return displaySkillEffectsCoroutine;
	}

	public void DisplaySkillRangeIfNeeded(bool displayHandledSkill)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (skillBeingDisplayed != null)
		{
			return;
		}
		skillBeingDisplayed = GetSkillToDisplayOnHover(displayHandledSkill);
		if (skillBeingDisplayed == null)
		{
			return;
		}
		SkillActionExecution skillActionExecution = skillBeingDisplayed.SkillAction.SkillActionExecution;
		SkillDefinition skillDefinition = skillBeingDisplayed.SkillDefinition;
		skillActionExecution.SkillExecutionController.Reset();
		skillActionExecution.Caster = Building.BattleModule;
		if (skillDefinition.Range == Vector2Int.zero && skillDefinition.TotalAreaOfEffectTilesCount > 1)
		{
			skillActionExecution.SkillExecutionView.DisplayAreaOfEffect(Building.OriginTile, hit: false, clearAreaOfEffectTiles: false);
		}
		else if (skillDefinition.Range != Vector2Int.zero)
		{
			if (skillActionExecution.CastFx != null)
			{
				skillActionExecution.CastFx.SourceTile = skillActionExecution.SkillSourceTile;
			}
			skillActionExecution.SkillSourceTileObject = Building;
			skillActionExecution.SkillExecutionController.PrepareSkill(Building.BattleModule);
			skillActionExecution.SkillExecutionController.DisplayInRangeTiles();
			SkillManager.SkillEffectFeedback.Display();
		}
	}

	public void HideSkillRangeIfNeeded()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (skillBeingDisplayed != null)
		{
			SkillActionExecution skillActionExecution = skillBeingDisplayed.SkillAction.SkillActionExecution;
			SkillDefinition skillDefinition = skillBeingDisplayed.SkillDefinition;
			if (skillDefinition.Range == Vector2Int.zero && skillDefinition.TotalAreaOfEffectTilesCount > 1)
			{
				skillActionExecution.SkillExecutionView.ResetAffectedDamageablesHud();
				TileMapView.ClearTiles(TileMapView.AreaOfEffectTilemap);
			}
			else if (skillDefinition.Range != Vector2Int.zero)
			{
				skillActionExecution.SkillExecutionController.Reset();
			}
			skillBeingDisplayed = null;
		}
	}

	public void ToggleSkillTargeting(bool display)
	{
		if (display)
		{
			Tile tile = (((Object)(object)GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered() != (Object)null) ? GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered().PlayableUnit.OriginTile : TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
			OnSkillTargetHover(tile == Building.OriginTile);
			UnityEvent onShow = skillTargetingMark.OnShow;
			if (onShow != null)
			{
				onShow.Invoke();
			}
		}
		else if ((Object)(object)skillTargetingMark != (Object)null)
		{
			UnityEvent onHide = skillTargetingMark.OnHide;
			if (onHide != null)
			{
				onHide.Invoke();
			}
			((Component)skillTargetingMark).gameObject.SetActive(false);
			skillTargetingMark = null;
		}
	}

	public Sprite GetBuildingCostIconSprite()
	{
		return ResourcePooler.LoadOnce<Sprite>(Building.ConstructionModule.CostsMaterials ? "View/Sprites/UI/Buildings/ProductionRewards/Small/WorldUI_Gauges_Prod_Reward_Material" : "View/Sprites/UI/Buildings/ProductionRewards/Small/WorldUI_Gauges_Prod_Reward_Gold", failSilently: false);
	}

	public string GetBuildingSkillCostString(string buildingSkillId)
	{
		switch (buildingSkillId)
		{
		case "Destroy":
			return string.Empty;
		case "Repair":
			return $"{Building.ConstructionModule.RepairCost}";
		default:
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)("Unknown building skill id " + buildingSkillId), (CLogLevel)1, true, true);
			return string.Empty;
		}
	}

	public Sprite GetPortraitSprite()
	{
		string resourcePath = "View/Sprites/UI/Buildings/Portraits/64px/BuildingsPortraits_" + Building.BuildingDefinition.Id;
		if (Building.IsTrap)
		{
			BattleModule battleModule = Building.BattleModule;
			if (battleModule != null && battleModule.RemainingTrapCharges == 0)
			{
				resourcePath = "View/Sprites/UI/Buildings/Portraits/64px/BuildingsPortraits_" + Building.BuildingDefinition.Id + "_Disabled";
				goto IL_00a7;
			}
		}
		if (Building is MagicCircle)
		{
			resourcePath = "View/Sprites/UI/Buildings/Portraits/64px/BuildingsPortraits_" + Building.BuildingDefinition.Id + "_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
		}
		goto IL_00a7;
		IL_00a7:
		return ResourcePooler.LoadOnce<Sprite>(resourcePath, failSilently: false) ?? ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Buildings/Portraits/64px/BuildingsPortraits_" + Building.BuildingDefinition.Id, failSilently: false);
	}

	public void Init()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		if (initialized)
		{
			return;
		}
		InitHud();
		canFinalizedDeath = false;
		if (WaitUntilDeathCanBeFinalized == null)
		{
			WaitUntilDeathCanBeFinalized = new WaitUntil((Func<bool>)(() => canFinalizedDeath));
		}
		((MonoBehaviour)this).StartCoroutine(FinishInitAfterAFrame());
		initialized = true;
	}

	public virtual void InitVisuals()
	{
		AttackFeedback = Object.Instantiate<AttackFeedback>(ResourcePooler.LoadOnce<AttackFeedback>("Prefab/Displayable Effect/Attack Feedback", failSilently: false));
		AttackFeedback.Init(this);
		if (Building.BrazierModule != null)
		{
			ExtinguishBrazierFeedback = Object.Instantiate<ExtinguishBrazierFeedback>(ResourcePooler.LoadOnce<ExtinguishBrazierFeedback>("Prefab/Displayable Effect/Extinguish Brazier Feedback", failSilently: false));
			ExtinguishBrazierFeedback.Init(this);
			BuildingHUD.DisplayBrazierIfNeeded();
		}
		else if ((Object)(object)ExtinguishBrazierFeedback != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)ExtinguishBrazierFeedback).gameObject);
			ExtinguishBrazierFeedback = null;
		}
		SetSkillTargetingMarkOffset();
		ToggleBuildingFlamesOnDamagedThreshold();
	}

	public void OnSkillTargetHover(bool hover)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)skillTargetingMark == (Object)null)
		{
			skillTargetingMark = ObjectPooler.GetPooledComponent<SkillTargetingMark>("SkillTargetingMarkSprite", SkillManager.SkillTargetingMarkSpritePrefab, (Transform)null, dontSetParent: false);
			Transform transform = ((Component)skillTargetingMark).transform;
			transform.SetParent(skillTargetingMarkAnchor);
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
		}
		if (!((Component)skillTargetingMark).gameObject.activeInHierarchy)
		{
			((Component)skillTargetingMark).gameObject.SetActive(true);
		}
		skillTargetingMark.SetHoverAnimatorState(hover);
	}

	public void PlaceBuildingTilesAfterConstructionAnimation(Tile baseTile, int animationSpritesCount, int animationFrameRate, string suffix = "")
	{
		((MonoBehaviour)this).StartCoroutine(PlaceBuildingTilesAfterConstructionAnimationCoroutine(baseTile, animationSpritesCount, animationFrameRate, suffix));
	}

	public virtual void PlayTakeDamageAnim()
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(Building.BuildingDefinition.DamageableModuleDefinition.DamagedParticlesId))
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)(Building.Id + " damaged particles has not been set in the definition, using default particles."), (CLogLevel)1, false, false);
		}
		GameObject pooledGameObject = ObjectPooler.GetPooledGameObject(string.IsNullOrEmpty(Building.BuildingDefinition.DamageableModuleDefinition.DamagedParticlesId) ? Building.Id : Building.BuildingDefinition.DamageableModuleDefinition.DamagedParticlesId, string.IsNullOrEmpty(Building.BuildingDefinition.DamageableModuleDefinition.DamagedParticlesId) ? damagedParticles : null);
		if ((Object)(object)pooledGameObject != (Object)null)
		{
			pooledGameObject.transform.position = Vector2.op_Implicit(TileMapView.GetTileCenter(Building.OriginTile));
		}
		else
		{
			Building.LogError("Unable to instantiate or get a pooled DamagedParticles for this building. PoolName is : " + (string.IsNullOrEmpty(Building.BuildingDefinition.DamageableModuleDefinition.DamagedParticlesId) ? Building.Id : Building.BuildingDefinition.DamageableModuleDefinition.DamagedParticlesId), (CLogLevel)1);
		}
	}

	public virtual void PlayDieAnim()
	{
		if (dieAnimCoroutine == null)
		{
			if (Building.ConstructionModule == null || Building.BuildingDefinition.ConstructionModuleDefinition.PlayDestructionSound)
			{
				PlayDeathSound();
			}
			dieAnimCoroutine = ((MonoBehaviour)this).StartCoroutine(PlayDieAnimCoroutine());
		}
	}

	public void RefreshBuildingDamagedAppearance()
	{
		TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuildingInstantly(Building, Building.OriginTile);
		ToggleBuildingFlamesOnDamagedThreshold();
	}

	[ContextMenu("Refresh Health")]
	public void RefreshHealth()
	{
		BuildingHUD.RefreshHealth();
	}

	public void RefreshCursorFeedback()
	{
		BuildingHUD.DisplayBrazierIfNeeded();
		CursorView.DisplayBuildingShape(Building, Hovered);
		TPSingleton<TileMapView>.Instance.DisplayBuildingSelectionFeedback(Building, Selected);
		TPSingleton<TileMapView>.Instance.DisplayBuildingOutline(Building, Hovered && !Selected, hover: true);
		TPSingleton<TileMapView>.Instance.DisplayBuildingOutline(Building, Selected, hover: false);
		DisplayLinkedEnemiesHover(HoveredOrSelected);
	}

	public void RefreshHudPositionInstantly()
	{
		BuildingHUD.RefreshPositionInstantly();
		if ((Object)(object)HandledDefensesHUD != (Object)null)
		{
			HandledDefensesHUD.RefreshPositionInstantly();
		}
	}

	public virtual void SetColor(Color color)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Building.BlueprintModule.OccupiedTiles.Count; i++)
		{
			TileMapView.BuildingTilemap.SetColor(new Vector3Int(Building.BlueprintModule.OccupiedTiles[i].X, Building.BlueprintModule.OccupiedTiles[i].Y, 0), color);
			TileMapView.BuildingFrontTilemap.SetColor(new Vector3Int(Building.BlueprintModule.OccupiedTiles[i].X, Building.BlueprintModule.OccupiedTiles[i].Y, 0), color);
		}
	}

	public void ToggleBuildingFlamesOnDamagedThreshold()
	{
		if (!(this is MagicCircleView) && !Building.BlueprintModule.IsIndestructible && Building.DamageableModule.IsUnderDamagedThreshold)
		{
			flamesLayer.SetActive(true);
			DisplayBuildingFlamesOnLayer();
			return;
		}
		flamesLayer.SetActive(false);
		while (flamesLayer.transform.childCount > 0)
		{
			GameObject gameObject = ((Component)flamesLayer.transform.GetChild(0)).gameObject;
			gameObject.SetActive(false);
			gameObject.transform.SetParent(((Component)SingletonBehaviour<ObjectPooler>.Instance).transform.parent);
		}
	}

	public void InitHandledDefensesHud(BuildingDefinition buildingDefinition)
	{
		if ((buildingDefinition.BlueprintModuleDefinition.Category & BuildingDefinition.E_BuildingCategory.Trap) != 0 || (buildingDefinition.BlueprintModuleDefinition.Category & BuildingDefinition.E_BuildingCategory.HandledDefense) != 0)
		{
			HandledDefensesHUD = ObjectPooler.GetPooledComponent<HandledDefensesHUD>("HandledDefensesHUDs", handledDefenseshudPrefab, BuildingManager.BuildingsHudsTransform, dontSetParent: false);
		}
	}

	protected virtual void InitHud()
	{
		BuildingHUD = ObjectPooler.GetPooledComponent<BuildingHUD>("BuildingHUDs", hudPrefab, BuildingManager.BuildingsHudsTransform, dontSetParent: false);
	}

	protected virtual void OnDisable()
	{
		initialized = false;
		Hovered = false;
		Selected = false;
		if ((Object)(object)BuildingHUD != (Object)null)
		{
			((Component)BuildingHUD).gameObject.SetActive(false);
			BuildingHUD = null;
		}
		ToggleSkillTargeting(display: false);
	}

	protected virtual void OnEnable()
	{
		Init();
	}

	private void DisplayBuildingFlamesOnLayer()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		Queue<Vector2> queue = new Queue<Vector2>(Building.BuildingDefinition.DamageableModuleDefinition.FlamesPositions.ShuffleAndReturn());
		for (int i = 0; i < Building.BuildingDefinition.DamageableModuleDefinition.FlameCount; i++)
		{
			Vector2 val = queue.Dequeue();
			DamagedBuildingFlameView damagedBuildingFlameView = flameViews.PickRandom();
			GameObject pooledGameObject = ObjectPooler.GetPooledGameObject("BuildingDamageFlame", ((Component)damagedBuildingFlameView).gameObject);
			if (!flamesPool.TryGetValue(pooledGameObject, out var value))
			{
				flamesPool[pooledGameObject] = pooledGameObject.GetComponent<DamagedBuildingFlameView>();
				value = flamesPool[pooledGameObject];
			}
			pooledGameObject.transform.SetParent(flamesLayer.transform);
			pooledGameObject.transform.localPosition = Vector2.op_Implicit(val);
			value.SpriteRenderer.flipX = Convert.ToBoolean(i % 2);
			Animator animator = value.Animator;
			AnimatorStateInfo currentAnimatorStateInfo = value.Animator.GetCurrentAnimatorStateInfo(0);
			animator.Play(((AnimatorStateInfo)(ref currentAnimatorStateInfo)).fullPathHash, -1, Random.value);
		}
	}

	private IEnumerator DisplaySkillEffectsCoroutine(float delay)
	{
		if (SkillEffectDisplays.Count != 0)
		{
			if (delay > 0f)
			{
				yield return SharedYields.WaitForSeconds(delay);
			}
			while (SkillEffectDisplays.Count > 0)
			{
				IDisplayableEffect displayableEffect = SkillEffectDisplays[0];
				SkillEffectDisplays.Remove(displayableEffect);
				yield return displayableEffect.Display();
			}
			displaySkillEffectsCoroutine = null;
		}
	}

	private IEnumerator FinishInitAfterAFrame()
	{
		yield return null;
		RefreshHudPositionInstantly();
		OnFinalInitFrame?.Invoke();
		OnFinalInitFrame = null;
	}

	private IEnumerator PlaceBuildingTilesAfterConstructionAnimationCoroutine(Tile baseTile, int animationSpritesCount, int animationFrameRate, string suffix = "")
	{
		float framesStep = 1f / (float)animationFrameRate;
		int i = 0;
		while (i < animationSpritesCount)
		{
			yield return SharedYields.WaitForSeconds(framesStep);
			int num = i + 1;
			i = num;
		}
		TPSingleton<TileMapView>.Instance.DisplayBuildingInstantly(buildingController.Building, baseTile, suffix);
	}

	public void PlayDeathSound()
	{
		AudioClip[] array = ResourcePooler.LoadAllOnce<AudioClip>($"Sounds/SFX/Buildings/Death/{Building.BuildingDefinition.Id}", failSilently: true);
		if (array != null && array.Length != 0)
		{
			SoundManager.PlayAudioClip(array, BuildingManager.BuildingPooledAudioSourceData);
		}
		else
		{
			SoundManager.PlayAudioClip(BuildingManager.DestructionAudioClips, BuildingManager.BuildingPooledAudioSourceData);
		}
	}

	public IEnumerator PlayDestructionSmokeCoroutine()
	{
		foreach (Tile occupiedTile in Building.BlueprintModule.OccupiedTiles)
		{
			GameObject pooledGameObject = ObjectPooler.GetPooledGameObject("DestructionSmoke", ((Component)smokeSystem).gameObject, ((Component)occupiedTile.TileView).transform);
			pooledGameObject.SetActive(true);
			pooledGameObject.transform.SetParent(((Component)occupiedTile.TileView).transform);
			pooledGameObject.transform.localPosition = Vector3.zero;
			yield return SharedYields.WaitForEndOfFrame;
		}
	}

	public IEnumerator PlayDieAnimCoroutine()
	{
		if (Building.BuildingDefinition.DamageableModuleDefinition == null || !Building.BuildingDefinition.DamageableModuleDefinition.DisableDestructionSmokeFX)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(PlayDestructionSmokeCoroutine());
			MainModule main = smokeSystem.main;
			yield return SharedYields.WaitForSeconds(((MainModule)(ref main)).duration * 0.5f);
		}
		Building.OriginTile.TileController.AddDeadBuilding(Building.BuildingDefinition);
		yield return (object)new WaitWhile((Func<bool>)(() => BuildingHUD.IsAnimating));
		yield return SharedYields.WaitForSeconds(0.1f);
		canFinalizedDeath = true;
		dieAnimCoroutine = null;
	}

	private void SetSkillTargetingMarkOffset()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!skillTargetingMarkOffsetHasBeenSet)
		{
			skillTargetingMarkOffsetHasBeenSet = true;
			if ((Object)(object)skillTargetingMarkAnchor != (Object)null)
			{
				Transform transform = ((Component)skillTargetingMarkAnchor).transform;
				transform.position += skillTargetingOffset;
			}
		}
	}
}
