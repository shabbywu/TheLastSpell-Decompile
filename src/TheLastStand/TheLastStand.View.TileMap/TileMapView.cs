using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Database.Fog;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.BonePile;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Fog;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Dev.View;
using TheLastStand.Framework;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Building;
using TheLastStand.View.Building.Construction;
using TheLastStand.View.Camera;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheLastStand.View.TileMap;

public class TileMapView : TPSingleton<TileMapView>
{
	public enum E_AreaOfEffectTileDisplayType
	{
		AreaOfEffect,
		Maneuver,
		Surrounding
	}

	public static class Constants
	{
		public static class Assets
		{
			public const string BuildingConstructionAnimationPath = "View/Sprites/ConstructionAnimation";

			public const string BuildingDestructionAnimationPath = "View/Sprites/DestructionAnimation";

			public const string BuildingLUTConstructionAnimationPath = "View/Sprites/LUTConstructionAnimation";

			public const string BuildingGhostPathPrefix = "View/Tiles/Buildings/Ghost";

			public const string BuildingDamagedTilePathPrefix = "View/Tiles/Buildings/Damaged Diffuse";

			public const string BuildingDamagedTilePathSuffix = "_DamagedDiffuse";

			public const string BuildingDamagedTileMaskPathPrefix = "View/Tiles/Buildings/Damaged Mask";

			public const string BuildingDamagedTileMaskPathSuffix = "_DamagedMask";

			public const string BuildingOutlinePathPrefix = "View/Tiles/Buildings/Outline";

			public const string BuildingSelectionFeedbackTilePath = "View/Tiles/Feedbacks/BuildingSelectionFeedback";

			public const string BuildingShadowsPathPrefix = "View/Tiles/Buildings/Diffuse/_Shadows";

			public const string BuildingTilePathPrefix = "View/Tiles/Buildings/Diffuse";

			public const string BuildingTileMaskPathPrefix = "View/Tiles/Buildings/Mask";

			public const string FogAreaTilePath = "View/Tiles/Feedbacks/MistRange/MistRange";

			public const string FogTilePath = "View/Tiles/World/Fog";

			public const string LightFogTilePath = "View/Tiles/World/LightFog";

			public const string LightFogDispelledTilePath = "View/Tiles/World/LightFog_Dispelled";

			public const string Ghost = "Ghost";

			public const string GridTilePath = "View/Tiles/Feedbacks/Grid Cell";

			public const string GroundTilePathPrefix = "View/Tiles/World";

			public const string GroundTileShapePath = "View/Tiles/World/TileShape";

			public const string LevelArtsPathFormat = "Prefab/Level Art/{0}/{0}_Level Art";

			public const string ReachableTilePath = "View/Tiles/Feedbacks/Movement/MoveRange";

			public const string OccupationVolumeTilePath = "View/Tiles/Feedbacks/Occupation Volume";

			public const string OccupationVolumeGhostTilePath = "View/Tiles/Feedbacks/Occupation Volume Ghost";

			public const string OutlineSuffix = "_Outline";

			public const string PanicOnEnemyTilePath = "View/Tiles/Feedbacks/PanicOnEnemy";

			public const string PlaceholderDiffusePath = "View/Tiles/Buildings/Diffuse/Placeholder/Placeholder";

			public const string PoisonDeathFeedbackPath = "View/Tiles/Feedbacks/PoisonDeath";

			public const string SidewalkPathPrefix = "View/Tiles/Buildings/Diffuse/_Sidewalks";

			public const string SidewalkShadowPathPrefix = "View/Tiles/Buildings/Diffuse/_Sidewalks/_Shadows";

			public const string SkillAoeTilePath = "View/Tiles/Feedbacks/Skill/SkillAoe Back";

			public const string SkillInaccurateRangeTilePath = "View/Tiles/Feedbacks/Skill/InaccurateRange";

			public const string SkillManeuverTilePath = "View/Tiles/Feedbacks/Skill/SkillManeuver";

			public const string SkillRangeTilePath = "View/Tiles/Feedbacks/Skill/SkillRange";

			public const string SkillSurroundingTilePath = "View/Tiles/Feedbacks/Skill/SkillSurrounding";

			public const string DialsTileTopLinePath = "View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_top";

			public const string DialsTileBotLinePath = "View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_bot";

			public const string DialsTileLeftRightLinePath = "View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_LeftRight";

			public const string SkillRotationFeedbackOnTilePath = "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_RotationSkill_On";

			public const string LimitFogTilePath = "View/Tiles/Feedbacks/MistLimits/MistLimits";

			public const string WorldLimitTilePath = "View/Tiles/Feedbacks/WorldLimits/WorldLimits";
		}

		public static class PoolNames
		{
			public const string BuildingConstructionAnimation = "Building Construction Animation";

			public const string BuildingDestructionAnimation = "Building Destruction Animation";
		}

		public const string LevelArtNoneId = "None";
	}

	public class StringToBonePileIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(BonePileDatabase.BonePileGeneratorsDefinition.Buildings.Keys);
	}

	[SerializeField]
	private Grid grid;

	[SerializeField]
	private Transform levelArtContainer;

	[SerializeField]
	private int mapSizeReference = 51;

	[SerializeField]
	private float backgroundSizeRatio = 1.03f;

	[SerializeField]
	private Tilemap worldLimitTilemap;

	[SerializeField]
	private Transform levelBackground;

	[SerializeField]
	private SpriteRenderer levelBackgroundRenderer;

	[SerializeField]
	private Tilemap groundCityTilemap;

	[SerializeField]
	private Tilemap groundCraterTilemap;

	[SerializeField]
	private Tilemap groundBackgroundTilemap;

	[SerializeField]
	private Tilemap gridTilemap;

	[SerializeField]
	private Tilemap tilesFlagTilemapTemplate;

	[SerializeField]
	private Tilemap buildingSelectionFeedbackTilemap;

	[SerializeField]
	private Tilemap buildingTilemap;

	[SerializeField]
	private Tilemap buildingDamagedTilemap;

	[SerializeField]
	private Tilemap buildingDamagedMaskTilemap;

	[SerializeField]
	private Tilemap buildingFrontTilemap;

	[SerializeField]
	private Tilemap buildingMasksTilemap;

	[SerializeField]
	private Tilemap buildingFrontMasksTilemap;

	[SerializeField]
	private ConstructionAnimationView constructionAnimationViewPrefab;

	[SerializeField]
	private Tilemap occupationVolumeBuildingTilemap;

	[SerializeField]
	private Tilemap ghostBuildingsTilemap;

	[SerializeField]
	private Tilemap ghostBuildingsFrontTilemap;

	[SerializeField]
	private DestructionAnimationView destructionAnimationViewPrefab;

	[SerializeField]
	private Vector2 destructionAnimationRandomDelay = new Vector2(0f, 0.2f);

	[SerializeField]
	[Range(0f, 1f)]
	private float ghostTweenMaxAlpha = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float ghostTweenMinAlpha = 0.5f;

	[SerializeField]
	private float ghostTweenDuration = 1f;

	[SerializeField]
	private Ease ghostTweenEaseCurve = (Ease)4;

	[SerializeField]
	private Tilemap buildingSelectionOutlinesTilemap;

	[SerializeField]
	private Tilemap buildingHoverOutlinesTilemap;

	[SerializeField]
	[Range(0f, 1f)]
	private float hoverOutlineTweenMaxAlpha = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float hoverOutlineTweenMinAlpha = 1f;

	[SerializeField]
	private float hoverOutlineTweenDuration = 1f;

	[SerializeField]
	private Ease hoverOutlineTweenEaseCurve = (Ease)4;

	[SerializeField]
	private Tilemap buildingShadows;

	[SerializeField]
	private Tilemap sideWalksTilemap;

	[SerializeField]
	private Tilemap sideWalkShadowsTilemap;

	[SerializeField]
	private DataColor buildingValidColor;

	[SerializeField]
	private DataColor buildingInvalidColor;

	[SerializeField]
	private Tilemap reachableTilesTilemap;

	[SerializeField]
	private DataColor reachableTilesColor;

	[SerializeField]
	private Tilemap movePathTilemap;

	[SerializeField]
	private Tilemap dialsTileMap;

	[SerializeField]
	private Tilemap skillRotationFeedbackTileMap;

	[SerializeField]
	private DataColor inRangeDialsColor;

	[SerializeField]
	private DataColor inRangeInvalidOrientationDialsColor;

	[SerializeField]
	private DataColor outOfRangeDialsColor;

	[SerializeField]
	private DataColor outOfRangeInvalidOrientationDialsColor;

	[SerializeField]
	private Tilemap skillRangeTilemap;

	[SerializeField]
	private DataColor skillRangeTilesColor;

	[SerializeField]
	private DataColor skillRangeTilesColorInvalidOrientation;

	[SerializeField]
	private DataColor skillHiddenRangeTilesColor;

	[SerializeField]
	private DataColor skillHiddenRangeTilesColorInvalidOrientation;

	[SerializeField]
	private Tilemap skillInaccurateRangeTilemap;

	[SerializeField]
	private Tilemap areaOfEffectTilemap;

	[SerializeField]
	private Tilemap enemiesHoverAreaOfEffectTilemap;

	[SerializeField]
	private DataColor skillAoeValidColor;

	[SerializeField]
	private DataColor skillAoeInvalidColor;

	[SerializeField]
	private DataColor skillManeuverValidColor;

	[SerializeField]
	private DataColor skillManeuverInvalidColor;

	[SerializeField]
	private DataColor skillSurroundingValidColor;

	[SerializeField]
	private DataColor skillSurroundingInvalidColor;

	[SerializeField]
	private Transform rangedSkillsDodgeMultiplierContainer;

	[SerializeField]
	private Tilemap rangedSkillsDodgeMultiplierTemplate;

	[SerializeField]
	private GameObject hitPrefab;

	[SerializeField]
	private Transform boneZoneTilemapsContainer;

	[SerializeField]
	private Tilemap boneZoneTilemapTemplate;

	[SerializeField]
	private Tilemap perkRangeTilemap;

	[SerializeField]
	private Tilemap perkHoverRangeSeparatorTemplate;

	[SerializeField]
	private Transform perkHoverRangeSeparatorContainer;

	[SerializeField]
	private Tilemap fogTilemap;

	[SerializeField]
	private Tilemap fogAreaTilemap;

	[SerializeField]
	private Tilemap lightFogOnTilemap;

	[SerializeField]
	private Tilemap lightFogOffTilemap;

	[SerializeField]
	private Tilemap fogMinMaxTilemap;

	[SerializeField]
	private TileBase fogMinMaxTileBase;

	[SerializeField]
	private DataColor fogOutlineColor;

	[SerializeField]
	private Tilemap fogLimitTilemap;

	[SerializeField]
	private Tilemap unitFeedbackTilemap;

	[SerializeField]
	private Tilemap enemiesReachableTilemap;

	[SerializeField]
	private Tilemap enemiesHoverTilemap;

	[SerializeField]
	private TileBase enemiesHoverTileBase;

	[SerializeField]
	private EnemyUnitDeadBodyView deadBodyPrefab;

	[SerializeField]
	private BuildingCorpseView deadBuildingPrefab;

	private Dictionary<Tilemap, Tween> alphaTweens = new Dictionary<Tilemap, Tween>();

	private HashSet<Tile> enemiesReachableTiles = new HashSet<Tile>();

	private Tween ghostFadeTween;

	private Tween hoverOutlineTween;

	private Tilemap[] rangedSkillsDodgeMultiplierTilemaps;

	private List<Tilemap> boneZoneTilemaps = new List<Tilemap>();

	private List<Tilemap> perkHoverRangeSeparatorTilemaps = new List<Tilemap>();

	private Dictionary<TileFlagDefinition.E_TileFlagTag, Tilemap> tilemapsByFlag;

	private Queue<int> activatedTileMaps = new Queue<int>();

	private HashSet<Vector3Int> fogMinMaxTiles;

	private bool levelArtLoaded;

	public static Tilemap AreaOfEffectTilemap => TPSingleton<TileMapView>.Instance.areaOfEffectTilemap;

	public static Tilemap EnemiesHoverAreaOfEffectTilemap => TPSingleton<TileMapView>.Instance.enemiesHoverAreaOfEffectTilemap;

	public static Tilemap BuildingTilemap => TPSingleton<TileMapView>.Instance.buildingTilemap;

	public static Tilemap BuildingFrontTilemap => TPSingleton<TileMapView>.Instance.buildingFrontTilemap;

	public static Tilemap BuildingFrontMasksTilemap => TPSingleton<TileMapView>.Instance.buildingFrontMasksTilemap;

	public static Tilemap BuildingSelectionOutlineTilemap => TPSingleton<TileMapView>.Instance.buildingSelectionOutlinesTilemap;

	public static Tilemap BuildingMasksTilemap => TPSingleton<TileMapView>.Instance.buildingMasksTilemap;

	public static Tilemap BuildingSelectionFeedbackTilemap => TPSingleton<TileMapView>.Instance.buildingSelectionFeedbackTilemap;

	public static Tilemap BuildingShadowsTilemap => TPSingleton<TileMapView>.Instance.buildingShadows;

	public static EnemyUnitDeadBodyView DeadBodyPrefab => TPSingleton<TileMapView>.Instance.deadBodyPrefab;

	public static BuildingCorpseView DeadBuildingPrefab => TPSingleton<TileMapView>.Instance.deadBuildingPrefab;

	public static Tilemap EnemiesReachableTilemap => TPSingleton<TileMapView>.Instance.enemiesReachableTilemap;

	public static Tilemap EnemiesHoverTilemap => TPSingleton<TileMapView>.Instance.enemiesHoverTilemap;

	public static TileBase EnemiesHoverTileBase => TPSingleton<TileMapView>.Instance.enemiesHoverTileBase;

	public static HashSet<Tile> EnemiesReachableTiles => TPSingleton<TileMapView>.Instance.enemiesReachableTiles;

	public static Tilemap FogAreaTilemap => TPSingleton<TileMapView>.Instance.fogAreaTilemap;

	public static Tilemap FogTilemap => TPSingleton<TileMapView>.Instance.fogTilemap;

	public static Tilemap FogLimitTilemap => TPSingleton<TileMapView>.Instance.fogLimitTilemap;

	public static Grid Grid => TPSingleton<TileMapView>.Instance.grid;

	public static Tilemap LightFogOnTilemap => TPSingleton<TileMapView>.Instance.lightFogOnTilemap;

	public static Tilemap LightFogOffTilemap => TPSingleton<TileMapView>.Instance.lightFogOffTilemap;

	public static Tilemap FogMinMaxTilemap => TPSingleton<TileMapView>.Instance.fogMinMaxTilemap;

	public static Tilemap GhostBuildingsTilemap => TPSingleton<TileMapView>.Instance.ghostBuildingsTilemap;

	public static Tilemap GhostBuildingsFrontTilemap => TPSingleton<TileMapView>.Instance.ghostBuildingsFrontTilemap;

	public static Tilemap GridTilemap => TPSingleton<TileMapView>.Instance.gridTilemap;

	public static Tilemap GroundBackgroundTilemap => TPSingleton<TileMapView>.Instance.groundBackgroundTilemap;

	public static Tilemap GroundCityTilemap => TPSingleton<TileMapView>.Instance.groundCityTilemap;

	public static Tilemap GroundCraterTilemap => TPSingleton<TileMapView>.Instance.groundCraterTilemap;

	public static Tilemap MovePathTilemap => TPSingleton<TileMapView>.Instance.movePathTilemap;

	public static Tilemap OccupationVolumeBuildingTilemap => TPSingleton<TileMapView>.Instance.occupationVolumeBuildingTilemap;

	public static Tilemap ReachableTilesTilemap => TPSingleton<TileMapView>.Instance.reachableTilesTilemap;

	public static Tilemap SideWalksTilemap => TPSingleton<TileMapView>.Instance.sideWalksTilemap;

	public static Tilemap SideWalkShadowsTilemap => TPSingleton<TileMapView>.Instance.sideWalkShadowsTilemap;

	public static Tilemap SkillRangeTilemap => TPSingleton<TileMapView>.Instance.skillRangeTilemap;

	public static Tilemap SkillRotationFeedbackTileMap => TPSingleton<TileMapView>.Instance.skillRotationFeedbackTileMap;

	public static DataColor SkillRangeTilesColor => TPSingleton<TileMapView>.Instance.skillRangeTilesColor;

	public static DataColor SkillRangeTilesColorInvalidOrientation => TPSingleton<TileMapView>.Instance.skillRangeTilesColorInvalidOrientation;

	public static DataColor SkillHiddenRangeTilesColor => TPSingleton<TileMapView>.Instance.skillHiddenRangeTilesColor;

	public static DataColor SkillHiddenRangeTilesColorInvalidOrientation => TPSingleton<TileMapView>.Instance.skillHiddenRangeTilesColorInvalidOrientation;

	public static Tilemap UnitFeedbackTilemap => TPSingleton<TileMapView>.Instance.unitFeedbackTilemap;

	public static Tilemap WorldLimitsTilemap => TPSingleton<TileMapView>.Instance.worldLimitTilemap;

	public static Dictionary<TileFlagDefinition.E_TileFlagTag, Tilemap> TilemapsByFlag
	{
		get
		{
			if (TPSingleton<TileMapView>.Instance.tilemapsByFlag == null)
			{
				TPSingleton<TileMapView>.Instance.tilemapsByFlag = new Dictionary<TileFlagDefinition.E_TileFlagTag, Tilemap>();
				TileFlagDefinition[] tileFlagDefinitions = TileMapManager.TileFlagDefinitions;
				foreach (TileFlagDefinition tileFlagDefinition in tileFlagDefinitions)
				{
					Tilemap val = Object.Instantiate<Tilemap>(TPSingleton<TileMapView>.Instance.tilesFlagTilemapTemplate, ((Component)TPSingleton<TileMapView>.Instance.tilesFlagTilemapTemplate).transform.parent);
					((Object)((Component)val).transform).name = ((Object)((Component)val).transform).name.Replace("(Clone)", $" ({tileFlagDefinition.TileFlagTag})");
					TPSingleton<TileMapView>.Instance.tilemapsByFlag.Add(tileFlagDefinition.TileFlagTag, val);
				}
			}
			((Component)TPSingleton<TileMapView>.Instance.tilesFlagTilemapTemplate).gameObject.SetActive(false);
			return TPSingleton<TileMapView>.Instance.tilemapsByFlag;
		}
		private set
		{
			TPSingleton<TileMapView>.Instance.tilemapsByFlag = value;
		}
	}

	public Vector2 DestructionAnimationRandomDelay => destructionAnimationRandomDelay;

	public static void ClearTiles(Tilemap tileMap)
	{
		tileMap.ClearAllTiles();
	}

	public static void DisplayLevel(bool displayInCoroutine = false)
	{
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<TileMapView>.Instance.LoadTileAssets();
		GroundBackgroundTilemap.ClearAllTiles();
		GroundCityTilemap.ClearAllTiles();
		GroundCraterTilemap.ClearAllTiles();
		GridTilemap.ClearAllTiles();
		BuildingTilemap.ClearAllTiles();
		TPSingleton<TileMapView>.Instance.buildingDamagedTilemap.ClearAllTiles();
		TPSingleton<TileMapView>.Instance.buildingDamagedMaskTilemap.ClearAllTiles();
		BuildingFrontTilemap.ClearAllTiles();
		BuildingMasksTilemap.ClearAllTiles();
		BuildingFrontMasksTilemap.ClearAllTiles();
		TPSingleton<TileMapView>.Instance.tilesFlagTilemapTemplate.ClearAllTiles();
		TransformExtensions.DestroyChildren(TPSingleton<TileMapView>.Instance.levelArtContainer);
		if (displayInCoroutine)
		{
			((MonoBehaviour)TPSingleton<TileMapView>.Instance).StartCoroutine(TPSingleton<TileMapView>.Instance.DisplayLevelCoroutine());
			return;
		}
		bool flag = ((StateMachine)ApplicationManager.Application).State.GetName() == "LevelEditor";
		CityDefinition cityDefinition = (flag ? null : TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition);
		string text = cityDefinition?.Id ?? LevelEditorManager.CityToLoadId;
		TPSingleton<TileMapView>.Instance.levelArtLoaded = false;
		string text2 = cityDefinition?.LevelArtPrefabId ?? LevelEditorManager.CityToLoadId;
		if (text2 != "None")
		{
			GameObject val = ResourcePooler.LoadOnce<GameObject>(string.Format("Prefab/Level Art/{0}/{0}_Level Art", text2), false);
			if ((Object)(object)val != (Object)null)
			{
				Object.Instantiate<GameObject>(val, TPSingleton<TileMapView>.Instance.levelArtContainer);
				TPSingleton<TileMapView>.Instance.levelArtLoaded = true;
			}
			else
			{
				((CLogger<TileMapManager>)TPSingleton<TileMapManager>.Instance).LogWarning((object)("No Level Art prefab has been found for city Id " + text2 + ". This could be due to loading template city inside LevelEditor, but if it's not the case, then there's something going wrong."), (CLogLevel)2, true, false);
			}
		}
		else
		{
			((CLogger<TileMapManager>)TPSingleton<TileMapManager>.Instance).Log((object)"Level art Id set to None, then no level art is being loaded.", (CLogLevel)2, false, false);
		}
		for (int i = -1; i <= TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
		{
			for (int j = -1; j <= TPSingleton<TileMapManager>.Instance.TileMap.Height; j++)
			{
				TPSingleton<TileMapView>.Instance.SetWorldLimitTile(new Vector3Int(i, j, 0));
				if (i != -1 && j != -1 && i != TPSingleton<TileMapManager>.Instance.TileMap.Width && j != TPSingleton<TileMapManager>.Instance.TileMap.Height)
				{
					Tile tile = TileMapManager.GetTile(i, j);
					SetTile(GridTilemap, tile, "View/Tiles/Feedbacks/Grid Cell");
					TileBase val2 = null;
					if (TPSingleton<TileMapView>.Instance.levelArtLoaded)
					{
						val2 = ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/TileShape", false);
					}
					else
					{
						val2 = ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/" + tile.GroundDefinition.Id + "_" + text, false) ?? ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/" + tile.GroundDefinition.Id, false);
						TileBase tileBase = ResourcePooler.LoadOnce<TileBase>("View/Tiles/World/Ground_" + text, false) ?? ResourcePooler.LoadOnce<TileBase>("View/Tiles/World/Ground", false);
						SetTile(GroundBackgroundTilemap, tile, tileBase);
						GroundCityTilemap.color = Color.white;
						GroundCraterTilemap.color = Color.white;
					}
					switch (tile.GroundDefinition.GroundCategory)
					{
					case GroundDefinition.E_GroundCategory.City:
						GroundCityTilemap.SetTile(new Vector3Int(i, j, 0), val2);
						break;
					case GroundDefinition.E_GroundCategory.NoBuilding:
						GroundCraterTilemap.SetTile(new Vector3Int(i, j, 0), val2);
						break;
					}
					if (tile.Building != null && tile.Building.OriginTile == tile)
					{
						TPSingleton<TileMapView>.Instance.DisplayBuilding(tile.Building, tile);
					}
				}
			}
		}
		bool blackenBackground = cityDefinition?.BlackenBackground ?? false;
		TPSingleton<TileMapView>.Instance.UpdateBackground(blackenBackground);
		if (!flag)
		{
			TPSingleton<TileMapView>.Instance.SetGroundTilemapsAlpha(0f);
		}
	}

	public static Vector3 GetCellCenterWorldPosition(Tile tile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GetCellCenterWorldPosition(tile.Position);
	}

	public static Vector3 GetCellCenterWorldPosition(Vector2Int tilePosition)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return GridTilemap.GetCellCenterWorld(new Vector3Int(((Vector2Int)(ref tilePosition)).x, ((Vector2Int)(ref tilePosition)).y, 0));
	}

	public static Vector3 GetLocalInterpolatedPosition(Vector3 localTilePosition)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((GridLayout)GridTilemap).CellToLocalInterpolated(localTilePosition);
	}

	public static Vector2 GetTileCenter(Tile tile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(GetCellCenterWorldPosition(tile) + new Vector3(0f, TPSingleton<TileMapView>.Instance.grid.cellSize.y * 0.5f, 0f));
	}

	public static Vector3 GetWorldPosition(Tile tile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GetWorldPosition(tile.Position);
	}

	public static Vector3 GetWorldPosition(Vector2Int tilePosition)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return ((GridLayout)GridTilemap).CellToWorld(new Vector3Int(((Vector2Int)(ref tilePosition)).x, ((Vector2Int)(ref tilePosition)).y, 0));
	}

	public static Vector3 GetCameraCenterTilePosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)ACameraView.MainCam).transform.position;
		Vector3Int val = ((GridLayout)GridTilemap).WorldToCell(Vector3Int.op_Implicit(new Vector3Int((int)position.x, (int)position.y, 0)));
		return GetCellCenterWorldPosition(new Vector2Int(((Vector3Int)(ref val)).x, ((Vector3Int)(ref val)).y));
	}

	public static void SetFogOutlinesTileBases()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		TileBase val = ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/Fog", false);
		TileBase[] array = (TileBase[])(object)new TileBase[4] { val, val, val, val };
		for (int i = -1; i <= TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
		{
			Vector3Int[] array2 = (Vector3Int[])(object)new Vector3Int[4]
			{
				new Vector3Int(i, -1, 0),
				new Vector3Int(-1, i, 0),
				new Vector3Int(i, TPSingleton<TileMapManager>.Instance.TileMap.Width, 0),
				new Vector3Int(TPSingleton<TileMapManager>.Instance.TileMap.Width, i, 0)
			};
			FogTilemap.SetTiles(array2, array);
		}
		for (int j = -2; j <= TPSingleton<TileMapManager>.Instance.TileMap.Width + 1; j++)
		{
			Vector3Int[] array3 = (Vector3Int[])(object)new Vector3Int[4]
			{
				new Vector3Int(j, -2, 0),
				new Vector3Int(-2, j, 0),
				new Vector3Int(j, TPSingleton<TileMapManager>.Instance.TileMap.Width + 1, 0),
				new Vector3Int(TPSingleton<TileMapManager>.Instance.TileMap.Width + 1, j, 0)
			};
			FogTilemap.SetTiles(array3, array);
			for (int k = 0; k < array3.Length; k++)
			{
				FogTilemap.SetColor(array3[k], TPSingleton<TileMapView>.Instance.fogOutlineColor._Color);
			}
		}
	}

	public static void SetTile(Tilemap tileMap, Tile tile, string tileBasePath, string backupTileBasePath = null)
	{
		TileBase val = ((tileBasePath != null) ? ResourcePooler<TileBase>.LoadOnce(tileBasePath, false) : null);
		if ((Object)(object)val == (Object)null)
		{
			val = ((backupTileBasePath != null) ? ResourcePooler<TileBase>.LoadOnce(backupTileBasePath, false) : null);
		}
		SetTile(tileMap, tile, val);
	}

	public static void SetTile(Tilemap tileMap, Tile tile, TileBase tileBase = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		tileMap.SetTile((Vector3Int)tile.Position, tileBase);
	}

	public static void SetTiles(Tilemap tileMap, List<Tile> tiles, string tileBasePath)
	{
		TileBase tileBase = ((tileBasePath != null) ? ResourcePooler<TileBase>.LoadOnce(tileBasePath, false) : null);
		SetTiles(tileMap, tiles, tileBase);
	}

	public static void SetTiles(Tilemap tileMap, List<Tile> tiles, TileBase tileBase = null)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Vector3Int[] array = (Vector3Int[])(object)new Vector3Int[tiles.Count];
		TileBase[] array2 = (TileBase[])(object)new TileBase[tiles.Count];
		for (int i = 0; i < tiles.Count; i++)
		{
			array[i] = (Vector3Int)tiles[i].Position;
			array2[i] = tileBase;
		}
		tileMap.SetTiles(array, array2);
	}

	public static void SetTiles(Tilemap tileMap, HashSet<Vector3Int> positions, TileBase tileBase = null)
	{
		int count = positions.Count;
		Vector3Int[] array = positions.ToArray();
		TileBase[] array2 = (TileBase[])(object)new TileBase[count];
		for (int i = 0; i < count; i++)
		{
			array2[i] = tileBase;
		}
		tileMap.SetTiles(array, array2);
	}

	public static void SetTileColor(Tilemap tileMap, Tile tile, Color color)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		tileMap.SetColor((Vector3Int)tile.Position, color);
	}

	public static void SpawnConstructionAnimation(Vector3 worldPosition, Sprite[] sprites, int sortingOrder, int animationFrameRate, int shockwaveFrame, Sprite[] spritesLUT = null)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ConstructionAnimationView pooledComponent = ObjectPooler.GetPooledComponent<ConstructionAnimationView>("Building Construction Animation", TPSingleton<TileMapView>.Instance.constructionAnimationViewPrefab, (Transform)null, false);
		((Component)pooledComponent).transform.position = worldPosition;
		pooledComponent.Init(sortingOrder, sprites, animationFrameRate, shockwaveFrame, spritesLUT);
		pooledComponent.PlayConstructionAnimation();
	}

	public static void SpawnDestructionAnimation(TheLastStand.Model.Building.Building building, Tile tile, float delay)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = ((GridLayout)BuildingTilemap).CellToWorld((Vector3Int)tile.Position);
		Vector2Int relativeBuildingTilePosition = building.BlueprintModule.GetRelativeBuildingTilePosition(tile);
		Sprite[] array = ResourcePooler.LoadAllOnce<Sprite>(string.Format("{0}/{1}/{2}{3}", "View/Sprites/DestructionAnimation", building.BuildingDefinition.Id, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y), false);
		if (array != null && array.Length != 0)
		{
			DestructionAnimationView pooledComponent = ObjectPooler.GetPooledComponent<DestructionAnimationView>("Building Destruction Animation", TPSingleton<TileMapView>.Instance.destructionAnimationViewPrefab, (Transform)null, false);
			pooledComponent.Init(worldPosition, array, delay);
			pooledComponent.PlayDestructionAnimation();
		}
	}

	public void AddEnemyReachableTiles(List<Tile> tiles)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			Tile item = tiles[i];
			enemiesReachableTiles.Add(item);
		}
	}

	public void ChangeBuildingGhostTileMapsColor(bool isValid)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Color color = (isValid ? buildingValidColor._Color : buildingInvalidColor._Color);
		color.a = GhostBuildingsTilemap.color.a;
		GhostBuildingsTilemap.color = color;
		GhostBuildingsFrontTilemap.color = color;
	}

	public void ClearAllEnemiesReachableTiles()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3Int> list = new List<Vector3Int>(enemiesReachableTiles.Count);
		List<TileBase> list2 = new List<TileBase>(enemiesReachableTiles.Count);
		foreach (Tile enemiesReachableTile in enemiesReachableTiles)
		{
			list.Add((Vector3Int)enemiesReachableTile.Position);
			list2.Add(null);
		}
		EnemiesReachableTilemap.SetTiles(list.ToArray(), list2.ToArray());
		enemiesReachableTiles.Clear();
	}

	public void ClearBuilding(Tile tile)
	{
		SetTile(BuildingTilemap, tile);
		SetTile(buildingDamagedTilemap, tile);
		SetTile(buildingDamagedMaskTilemap, tile);
		SetTile(BuildingFrontTilemap, tile);
		SetTile(BuildingMasksTilemap, tile);
		SetTile(BuildingFrontMasksTilemap, tile);
		SetTile(BuildingShadowsTilemap, tile);
		SetTile(SideWalksTilemap, tile);
		SetTile(SideWalkShadowsTilemap, tile);
	}

	public void ClearBuildingGhost(Tile tile, BuildingDefinition buildingDefinition)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		List<Tile> occupiedTiles = tile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		for (int num = occupiedTiles.Count - 1; num >= 0; num--)
		{
			SetTile(GhostBuildingsTilemap, occupiedTiles[num]);
			SetTile(GhostBuildingsFrontTilemap, occupiedTiles[num]);
			if (buildingDefinition.ConstructionModuleDefinition.OccupationVolumeType == BuildingDefinition.E_OccupationVolumeType.Adjacent && TileMapController.CanPlaceBuilding(buildingDefinition, tile, ignoreUnit: true))
			{
				for (int i = -1; i < 2; i++)
				{
					for (int j = -1; j < 2; j++)
					{
						TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
						Vector2Int position = occupiedTiles[num].Position;
						int x = ((Vector2Int)(ref position)).x + i;
						position = occupiedTiles[num].Position;
						Tile tile2 = tileMap.GetTile(x, ((Vector2Int)(ref position)).y + j);
						if (tile2 != null && !occupiedTiles.Contains(tile2) && TPSingleton<ConstructionManager>.Instance.Construction.BuildingAvailableSpaceTiles.Contains(tile2))
						{
							SetTile(OccupationVolumeBuildingTilemap, tile2, "View/Tiles/Feedbacks/Occupation Volume");
						}
					}
				}
			}
		}
		ClearBuildingGhostRangeAndZoneTiles();
	}

	public void ClearDialsTiles(Tile sourceTile)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (sourceTile == null)
		{
			return;
		}
		Vector2Int zero = Vector2Int.zero;
		for (int i = 0; i < TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
		{
			((Vector2Int)(ref zero)).x = i;
			int num = i;
			Vector2Int position = sourceTile.Position;
			if (num != ((Vector2Int)(ref position)).x)
			{
				position = sourceTile.Position;
				int y = ((Vector2Int)(ref position)).y;
				position = sourceTile.Position;
				((Vector2Int)(ref zero)).y = y + (((Vector2Int)(ref position)).x - i);
				dialsTileMap.SetTile((Vector3Int)zero, (TileBase)null);
				position = sourceTile.Position;
				int y2 = ((Vector2Int)(ref position)).y;
				position = sourceTile.Position;
				((Vector2Int)(ref zero)).y = y2 - (((Vector2Int)(ref position)).x - i);
				dialsTileMap.SetTile((Vector3Int)zero, (TileBase)null);
			}
		}
	}

	public void ClearHoverOutline(Tile tile, BuildingDefinition buildingDefinition)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		List<Tile> occupiedTiles = tile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		for (int i = 0; i < occupiedTiles.Count; i++)
		{
			buildingHoverOutlinesTilemap.SetTile((Vector3Int)occupiedTiles[i].Position, (TileBase)null);
		}
	}

	public void ClearInaccurateRangeTiles(IEnumerable<Tile> tiles)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3Int> list = new List<Vector3Int>(tiles.Count());
		List<TileBase> list2 = new List<TileBase>(tiles.Count());
		foreach (Tile tile in tiles)
		{
			list.Add((Vector3Int)tile.Position);
			list2.Add(null);
		}
		skillInaccurateRangeTilemap.SetTiles(list.ToArray(), list2.ToArray());
	}

	public void ClearPerkHoverRangeTiles()
	{
		ClearTiles(perkRangeTilemap);
		for (int i = 0; i < perkHoverRangeSeparatorTilemaps.Count; i++)
		{
			ClearTiles(perkHoverRangeSeparatorTilemaps[i]);
		}
	}

	public void ClearRangedSkillsModifiers()
	{
		for (int num = rangedSkillsDodgeMultiplierTilemaps.Length - 1; num >= 0; num--)
		{
			ClearTiles(rangedSkillsDodgeMultiplierTilemaps[num]);
		}
	}

	public void GenerateBoneZoneTilemaps()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		List<BoneZoneDefinition> boneZoneDefinitions = BonePileDatabase.BoneZonesDefinition.BoneZoneDefinitions;
		TileBase tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/TileShape", false);
		Color colorA = Color.red;
		Color colorB = Color.white;
		int i = 0;
		int previousMinDistanceFromCity = int.MaxValue;
		int previousMaxMagicCircleDistance = -1;
		HashSet<Tile> usedTiles = new HashSet<Tile>();
		foreach (BoneZoneDefinition boneZoneDefinition2 in boneZoneDefinitions)
		{
			if (boneZoneDefinition2.MinHavenDistance > -1)
			{
				SetBoneZoneTiles(boneZoneDefinition2, (Tile o) => o.DistanceToCity >= boneZoneDefinition2.MinHavenDistance && o.DistanceToCity < previousMinDistanceFromCity && !usedTiles.Contains(o));
				previousMinDistanceFromCity = boneZoneDefinition2.MinHavenDistance;
			}
			else if (boneZoneDefinition2.MaxMagicCircleDistance > -1)
			{
				SetBoneZoneTiles(boneZoneDefinition2, (Tile o) => o.DistanceToMagicCircle <= boneZoneDefinition2.MaxMagicCircleDistance && o.DistanceToMagicCircle > previousMaxMagicCircleDistance && !usedTiles.Contains(o));
				previousMaxMagicCircleDistance = boneZoneDefinition2.MaxMagicCircleDistance;
			}
			int num = i;
			i = num + 1;
		}
		((Component)boneZoneTilemapTemplate).gameObject.SetActive(false);
		void SetBoneZoneTiles(BoneZoneDefinition boneZoneDefinition, Func<Tile, bool> match)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			Tilemap val = Object.Instantiate<Tilemap>(TPSingleton<TileMapView>.Instance.boneZoneTilemapTemplate, TPSingleton<TileMapView>.Instance.boneZoneTilemapsContainer);
			((Object)val).name = "BoneZone_" + boneZoneDefinition.Id;
			val.ClearAllTiles();
			val.color = ColorExtensions.WithA(Color.LerpUnclamped(colorA, colorB, (float)i / (float)boneZoneDefinitions.Count), 0f);
			foreach (Tile item in TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Where(match))
			{
				val.SetTile((Vector3Int)item.Position, tileBase);
				usedTiles.Add(item);
			}
			TPSingleton<TileMapView>.Instance.boneZoneTilemaps.Add(val);
		}
	}

	public void ClearRangeTiles(Dictionary<Tile, TilesInRangeInfos.TileDisplayInfos> tiles)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3Int> list = new List<Vector3Int>(tiles.Count);
		List<TileBase> list2 = new List<TileBase>(tiles.Count);
		foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> tile in tiles)
		{
			list.Add((Vector3Int)tile.Key.Position);
			list2.Add(null);
		}
		SkillRangeTilemap.SetTiles(list.ToArray(), list2.ToArray());
	}

	public void ClearSelectionOutline(Tile tile, BuildingDefinition buildingDefinition)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		List<Tile> occupiedTiles = tile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		for (int i = 0; i < occupiedTiles.Count; i++)
		{
			buildingSelectionOutlinesTilemap.SetTile((Vector3Int)occupiedTiles[i].Position, (TileBase)null);
		}
	}

	public void Display()
	{
		while (activatedTileMaps.Count > 0)
		{
			int num = activatedTileMaps.Dequeue();
			((Component)((Component)this).transform.GetChild(num)).gameObject.SetActive(true);
		}
	}

	public void DisplayAllEnemiesReachableTiles()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		TileBase item = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Movement/MoveRange", false);
		List<Vector3Int> list = new List<Vector3Int>(enemiesReachableTiles.Count);
		List<TileBase> list2 = new List<TileBase>(enemiesReachableTiles.Count);
		foreach (Tile enemiesReachableTile in enemiesReachableTiles)
		{
			list.Add((Vector3Int)enemiesReachableTile.Position);
			list2.Add(item);
		}
		EnemiesReachableTilemap.SetTiles(list.ToArray(), list2.ToArray());
	}

	public void DisplayAreaOfEffectHitFeedback(Tile tile)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Object.Instantiate<GameObject>(hitPrefab).transform.position = GetWorldPosition(tile);
	}

	public void DisplayAreaOfEffectTile(Tile tile, E_AreaOfEffectTileDisplayType areaOfEffectTileDisplayType, bool unreachable, Tilemap tilemap)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (tile != null)
		{
			TileBase tileBase = null;
			Color color = skillAoeValidColor._Color;
			Color color2 = skillAoeInvalidColor._Color;
			switch (areaOfEffectTileDisplayType)
			{
			case E_AreaOfEffectTileDisplayType.AreaOfEffect:
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/SkillAoe Back", false);
				color = skillAoeValidColor._Color;
				color2 = skillAoeInvalidColor._Color;
				break;
			case E_AreaOfEffectTileDisplayType.Maneuver:
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/SkillManeuver", false);
				color = skillManeuverValidColor._Color;
				color2 = skillManeuverInvalidColor._Color;
				break;
			case E_AreaOfEffectTileDisplayType.Surrounding:
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/SkillSurrounding", false);
				color = skillSurroundingValidColor._Color;
				color2 = skillSurroundingInvalidColor._Color;
				break;
			}
			SetTile(tilemap, tile, tileBase);
			SetTileColor(tilemap, tile, unreachable ? color2 : color);
		}
	}

	public void DisplayBuilding(TheLastStand.Model.Building.Building building, Tile baseTile, string suffix = "")
	{
		switch (building.BuildingDefinition.ConstructionModuleDefinition.ConstructionAnimationType)
		{
		case BuildingDefinition.E_ConstructionAnimationType.Instantaneous:
			DisplayBuildingInstantly(building, baseTile, suffix);
			break;
		case BuildingDefinition.E_ConstructionAnimationType.Animated:
			DisplayBuildingAnimated(building, baseTile, suffix);
			break;
		default:
			DisplayBuildingInstantly(building, baseTile, suffix);
			break;
		}
	}

	public void DisplayBuildingAnimated(TheLastStand.Model.Building.Building building, Tile baseTile, string suffix = "")
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		int animationSpritesCount = 0;
		Vector3Int val = default(Vector3Int);
		for (int i = 0; i < building.BlueprintModule.OccupiedTiles.Count; i++)
		{
			Vector2Int relativeBuildingTilePosition = building.BlueprintModule.GetRelativeBuildingTilePosition(building.BlueprintModule.OccupiedTiles[i]);
			((Vector3Int)(ref val))._002Ector(baseTile.X + ((Vector2Int)(ref relativeBuildingTilePosition)).x - building.BuildingDefinition.BlueprintModuleDefinition.OriginX, baseTile.Y + ((Vector2Int)(ref relativeBuildingTilePosition)).y - building.BuildingDefinition.BlueprintModuleDefinition.OriginY, 0);
			Sprite[] array = ResourcePooler<Sprite>.LoadAllOnce(string.Format("{0}/{1}/{2}{3}", "View/Sprites/ConstructionAnimation", building.BuildingDefinition.Id, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y), false);
			Sprite[] spritesLUT = ResourcePooler<Sprite>.LoadAllOnce(string.Format("{0}/{1}/{2}{3}", "View/Sprites/LUTConstructionAnimation", building.BuildingDefinition.Id, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y), true);
			if (array.Length != 0)
			{
				animationSpritesCount = array.Length;
				SpawnConstructionAnimation(((GridLayout)BuildingTilemap).CellToWorld(val), array, ((Renderer)((Component)BuildingTilemap).GetComponent<TilemapRenderer>()).sortingOrder, building.BuildingDefinition.ConstructionModuleDefinition.ConstructionAnimationFrameRate, building.BuildingDefinition.ConstructionModuleDefinition.ConstructionAnimationShockwaveFrame, spritesLUT);
			}
			TileBase val2 = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Shadows/" + building.BuildingDefinition.BlueprintModuleDefinition.ShadowType, false);
			if ((Object)(object)val2 != (Object)null)
			{
				BuildingShadowsTilemap.SetTile(val, val2);
			}
			if (building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType != "None")
			{
				val2 = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Sidewalks/" + building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType, false);
				if ((Object)(object)val2 != (Object)null)
				{
					SideWalksTilemap.SetTile(val, val2);
				}
				val2 = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Sidewalks/_Shadows/" + building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType + "Shadow", false);
				if ((Object)(object)val2 != (Object)null)
				{
					SideWalkShadowsTilemap.SetTile(val, val2);
				}
			}
		}
		((Component)building.BuildingView).transform.position = GetWorldPosition(baseTile);
		((Component)building.BuildingView).gameObject.SetActive(true);
		building.BuildingView.PlaceBuildingTilesAfterConstructionAnimation(baseTile, animationSpritesCount, building.BuildingDefinition.ConstructionModuleDefinition.ConstructionAnimationFrameRate, suffix);
	}

	public void DisplayBuildingGhost(BuildingDefinition buildingDefinition, Tile originTile)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		List<Tile> occupiedTiles = originTile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		Vector3Int val = default(Vector3Int);
		for (int num = occupiedTiles.Count - 1; num >= 0; num--)
		{
			Vector2Int relativeBuildingTilePosition = BlueprintModule.GetRelativeBuildingTilePosition(occupiedTiles[num], originTile, buildingDefinition.BlueprintModuleDefinition);
			((Vector3Int)(ref val))._002Ector(originTile.X + ((Vector2Int)(ref relativeBuildingTilePosition)).x - buildingDefinition.BlueprintModuleDefinition.OriginX, originTile.Y + ((Vector2Int)(ref relativeBuildingTilePosition)).y - buildingDefinition.BlueprintModuleDefinition.OriginY, 0);
			TileBase ghostTileBase = BuildingView.GetGhostTileBase(buildingDefinition.Id, string.Empty, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y);
			if ((Object)(object)ghostTileBase != (Object)null)
			{
				GhostBuildingsTilemap.SetTile(val, ghostTileBase);
			}
			if (buildingDefinition.ConstructionModuleDefinition.OccupationVolumeType == BuildingDefinition.E_OccupationVolumeType.Adjacent && TileMapController.CanPlaceBuilding(buildingDefinition, originTile, ignoreUnit: true))
			{
				for (int i = -1; i < 2; i++)
				{
					for (int j = -1; j < 2; j++)
					{
						TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
						Vector2Int position = occupiedTiles[num].Position;
						int x = ((Vector2Int)(ref position)).x + i;
						position = occupiedTiles[num].Position;
						Tile tile = tileMap.GetTile(x, ((Vector2Int)(ref position)).y + j);
						if (tile != null && !occupiedTiles.Contains(tile) && TPSingleton<ConstructionManager>.Instance.Construction.BuildingAvailableSpaceTiles.Contains(tile))
						{
							SetTile(OccupationVolumeBuildingTilemap, tile, "View/Tiles/Feedbacks/Occupation Volume Ghost");
						}
					}
				}
			}
		}
		if (buildingDefinition.BattleModuleDefinition == null)
		{
			return;
		}
		string text = string.Empty;
		Dictionary<string, int> skills = buildingDefinition.BattleModuleDefinition.Skills;
		if (skills != null && skills.Count > 0)
		{
			text = buildingDefinition.BattleModuleDefinition.Skills.First().Key;
		}
		else
		{
			BehaviorDefinition behavior = buildingDefinition.BattleModuleDefinition.Behavior;
			if (behavior != null && behavior.GoalDefinitions?.Length > 0)
			{
				text = buildingDefinition.BattleModuleDefinition.Behavior.GoalDefinitions[0].SkillId;
			}
		}
		if (text != string.Empty && SkillManager.TryGetSkillDefinitionOrDatabase(buildingDefinition.BattleModuleDefinition.SkillProgressions, text, TPSingleton<GameManager>.Instance.Game.DayNumber, out var skillDefinition))
		{
			if (buildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Trap))
			{
				DisplaySkillAoE(skillDefinition, originTile, AreaOfEffectTilemap);
			}
			else if (((StateMachine)ApplicationManager.Application).State.GetName() != "LevelEditor")
			{
				DisplayBuildingGhostRange(skillDefinition, originTile, buildingDefinition.BlueprintModuleDefinition);
			}
		}
	}

	public void DisplayBuildingOutline(TheLastStand.Model.Building.Building building, bool show, bool hover, string suffix = "")
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		if (!show)
		{
			if (hover)
			{
				ClearHoverOutline(building.OriginTile, building.BuildingDefinition);
			}
			else
			{
				ClearSelectionOutline(building.OriginTile, building.BuildingDefinition);
			}
		}
		else
		{
			if (building.DamageableModule is TrapDamageableModule && building.BattleModule.RemainingTrapCharges == 0)
			{
				return;
			}
			StartHoverOutlineTilemapAlphaTweening();
			Vector3Int val = default(Vector3Int);
			for (int num = building.BlueprintModule.OccupiedTiles.Count - 1; num >= 0; num--)
			{
				Vector2Int relativeBuildingTilePosition = building.BlueprintModule.GetRelativeBuildingTilePosition(building.BlueprintModule.OccupiedTiles[num]);
				((Vector3Int)(ref val))._002Ector(building.OriginTile.X + ((Vector2Int)(ref relativeBuildingTilePosition)).x - building.BuildingDefinition.BlueprintModuleDefinition.OriginX, building.OriginTile.Y + ((Vector2Int)(ref relativeBuildingTilePosition)).y - building.BuildingDefinition.BlueprintModuleDefinition.OriginY, 0);
				TileBase val2 = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}{4}{5}{6}", "View/Tiles/Buildings/Outline", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y, "_Outline") ?? "", true);
				Tilemap val3 = (hover ? buildingHoverOutlinesTilemap : buildingSelectionOutlinesTilemap);
				if ((Object)(object)val2 != (Object)null)
				{
					val3.SetTile(val, val2);
				}
				else
				{
					val2 = BuildingDatabase.TileBySpriteDictionary.GetTileBySprite(buildingTilemap.GetSprite(val));
					if ((Object)(object)val2 != (Object)null)
					{
						val3.SetTile(val, val2);
					}
				}
			}
		}
	}

	public void DisplayBuildingInstantly(TheLastStand.Model.Building.Building building, Tile baseTile, string suffix = "")
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(suffix) && building.BlueprintModule is GateBlueprintModule gateBlueprintModule && gateBlueprintModule.IsOpen)
		{
			suffix = "Opened";
		}
		Vector3Int val = default(Vector3Int);
		for (int i = 0; i < building.BlueprintModule.OccupiedTiles.Count; i++)
		{
			Vector2Int relativeBuildingTilePosition = building.BlueprintModule.GetRelativeBuildingTilePosition(building.BlueprintModule.OccupiedTiles[i]);
			((Vector3Int)(ref val))._002Ector(baseTile.X + ((Vector2Int)(ref relativeBuildingTilePosition)).x - building.BuildingDefinition.BlueprintModuleDefinition.OriginX, baseTile.Y + ((Vector2Int)(ref relativeBuildingTilePosition)).y - building.BuildingDefinition.BlueprintModuleDefinition.OriginY, 0);
			TileBase tileBase = null;
			if (!building.BlueprintModule.IsIndestructible && building.DamageableModule.IsUnderDamagedThreshold)
			{
				tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}{4}{5}{6}", "View/Tiles/Buildings/Damaged Diffuse", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y, "_DamagedDiffuse"), true);
				if ((Object)(object)tileBase != (Object)null)
				{
					buildingDamagedTilemap.SetTile(val, tileBase);
				}
				tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}{4}{5}{6}", "View/Tiles/Buildings/Damaged Mask", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y, "_DamagedMask"), true);
				if ((Object)(object)tileBase != (Object)null)
				{
					buildingDamagedMaskTilemap.SetTile(val, tileBase);
				}
			}
			else
			{
				buildingDamagedTilemap.SetTile(val, (TileBase)null);
				buildingDamagedMaskTilemap.SetTile(val, (TileBase)null);
			}
			bool flag = BuildingView.TryGetDiffuseTileBase(building.BuildingDefinition.Id, suffix, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y, out tileBase);
			if (flag && (Object)(object)tileBase != (Object)null)
			{
				BuildingTilemap.SetTile(val, tileBase);
			}
			tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}Front{4}{5}", "View/Tiles/Buildings/Diffuse", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y), true);
			if ((Object)(object)tileBase != (Object)null)
			{
				BuildingFrontTilemap.SetTile(val, tileBase);
			}
			tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}{4}{5}_Mask", "View/Tiles/Buildings/Mask", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y), true);
			if ((Object)(object)tileBase != (Object)null)
			{
				BuildingMasksTilemap.SetTile(val, tileBase);
			}
			tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}Front{4}{5}_Mask", "View/Tiles/Buildings/Mask", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, ((Vector2Int)(ref relativeBuildingTilePosition)).x, ((Vector2Int)(ref relativeBuildingTilePosition)).y), true);
			if ((Object)(object)tileBase != (Object)null)
			{
				BuildingFrontMasksTilemap.SetTile(val, tileBase);
			}
			else if (!flag)
			{
				building.BuildingView.PlaceholderView = true;
			}
			tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Shadows/" + building.BuildingDefinition.BlueprintModuleDefinition.ShadowType, true);
			if ((Object)(object)tileBase != (Object)null)
			{
				BuildingShadowsTilemap.SetTile(val, tileBase);
			}
			if (building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType != "None")
			{
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Sidewalks/" + building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType, true);
				if ((Object)(object)tileBase != (Object)null)
				{
					SideWalksTilemap.SetTile(val, tileBase);
				}
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Sidewalks/_Shadows/" + building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType + "Shadow", true);
				if ((Object)(object)tileBase != (Object)null)
				{
					SideWalkShadowsTilemap.SetTile(val, tileBase);
				}
			}
		}
		((Component)building.BuildingView).transform.position = GetWorldPosition(baseTile);
		((Component)building.BuildingView).gameObject.SetActive(true);
	}

	public void DisplayBuildingSelectionFeedback(TheLastStand.Model.Building.Building building, bool show)
	{
		TileBase val = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/BuildingSelectionFeedback", false);
		foreach (Tile occupiedTile in building.BlueprintModule.OccupiedTiles)
		{
			SetTile(BuildingSelectionFeedbackTilemap, occupiedTile, show ? val : null);
		}
	}

	public void DisplayDialsTilesFrom(Tile sourceTile, Dictionary<Tile, TilesInRangeInfos.TileDisplayInfos> inRangeTiles = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		if (sourceTile == null)
		{
			return;
		}
		Vector2Int tilePos = Vector2Int.zero;
		TileBase val = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_LeftRight", false);
		TileBase val2 = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_top", false);
		TileBase val3 = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_bot", false);
		int num = Mathf.Max(TPSingleton<TileMapManager>.Instance.TileMap.Width, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		for (int i = 0; i < num; i++)
		{
			((Vector2Int)(ref tilePos)).x = i;
			int num2 = i;
			Vector2Int position = sourceTile.Position;
			if (num2 == ((Vector2Int)(ref position)).x)
			{
				continue;
			}
			ref Vector2Int reference = ref tilePos;
			position = sourceTile.Position;
			int y = ((Vector2Int)(ref position)).y;
			position = sourceTile.Position;
			((Vector2Int)(ref reference)).y = y + (((Vector2Int)(ref position)).x - i);
			if (((Vector2Int)(ref tilePos)).y >= 0 && ((Vector2Int)(ref tilePos)).y < TPSingleton<TileMapManager>.Instance.TileMap.Height && ((Vector2Int)(ref tilePos)).x < TPSingleton<TileMapManager>.Instance.TileMap.Width)
			{
				TileObjectSelectionManager.E_Orientation orientationFromSelectionToPos = TileObjectSelectionManager.GetOrientationFromSelectionToPos(tilePos);
				Color val4 = ((inRangeTiles == null || !inRangeTiles.Any((KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> tile) => tile.Key.Position == tilePos)) ? (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? outOfRangeDialsColor._Color : outOfRangeInvalidOrientationDialsColor._Color) : (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? inRangeDialsColor._Color : inRangeInvalidOrientationDialsColor._Color));
				dialsTileMap.SetTile((Vector3Int)tilePos, val);
				dialsTileMap.SetColor((Vector3Int)tilePos, val4);
			}
			ref Vector2Int reference2 = ref tilePos;
			position = sourceTile.Position;
			int y2 = ((Vector2Int)(ref position)).y;
			position = sourceTile.Position;
			((Vector2Int)(ref reference2)).y = y2 - (((Vector2Int)(ref position)).x - i);
			if (((Vector2Int)(ref tilePos)).y >= 0 && ((Vector2Int)(ref tilePos)).y < TPSingleton<TileMapManager>.Instance.TileMap.Height && ((Vector2Int)(ref tilePos)).x < TPSingleton<TileMapManager>.Instance.TileMap.Width)
			{
				TileObjectSelectionManager.E_Orientation orientationFromSelectionToPos = TileObjectSelectionManager.GetOrientationFromSelectionToPos(tilePos);
				Color val4 = ((inRangeTiles == null || !inRangeTiles.Any((KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> tile) => tile.Key.Position == tilePos)) ? (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? outOfRangeDialsColor._Color : outOfRangeInvalidOrientationDialsColor._Color) : (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? inRangeDialsColor._Color : inRangeInvalidOrientationDialsColor._Color));
				Tilemap obj = dialsTileMap;
				Vector3Int val5 = (Vector3Int)tilePos;
				int num3 = i;
				position = sourceTile.Position;
				obj.SetTile(val5, (num3 > ((Vector2Int)(ref position)).x) ? val2 : val3);
				dialsTileMap.SetColor((Vector3Int)tilePos, val4);
			}
		}
	}

	public void DisplayFogMinMax(bool show, int densityMin, int densityMax, bool forceRecompute = false)
	{
		FogMinMaxTilemap.ClearAllTiles();
		if (show)
		{
			if (fogMinMaxTiles == null || forceRecompute)
			{
				ComputeFogMinMaxTiles(densityMin, densityMax);
			}
			SetTiles(FogMinMaxTilemap, fogMinMaxTiles, fogMinMaxTileBase ?? ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/InaccurateRange", false));
		}
	}

	public void DisplayInaccurateRangeTiles(List<Tile> tiles)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		TileBase item = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/InaccurateRange", false);
		List<Vector3Int> list = new List<Vector3Int>(tiles.Count);
		List<TileBase> list2 = new List<TileBase>(tiles.Count);
		foreach (Tile tile in tiles)
		{
			list.Add((Vector3Int)tile.Position);
			list2.Add(item);
		}
		skillInaccurateRangeTilemap.SetTiles(list.ToArray(), list2.ToArray());
	}

	public void DisplayPerkHoverRangeTiles(Perk perk)
	{
		List<Tile> list = new List<Tile>();
		if (perk.PerkDefinition.HoverRanges.Count == 1)
		{
			foreach (Tile item in perk.Owner.TileObjectController.GetTilesInRange(perk.PerkDefinition.HoverRanges[0].EvalToInt((InterpreterContext)(object)perk)))
			{
				list.Add(item);
			}
			SetTiles(perkRangeTilemap, list, "View/Tiles/Feedbacks/Skill/SkillRange");
			return;
		}
		int num = -1;
		int num2 = -1;
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < perk.PerkDefinition.HoverRanges.Count; i++)
		{
			int num3 = perk.PerkDefinition.HoverRanges[i].EvalToInt((InterpreterContext)(object)perk);
			if (num3 > num)
			{
				num = num3;
				num2 = i;
			}
		}
		foreach (Tile item2 in perk.Owner.TileObjectController.GetTilesInRange(num))
		{
			list.Add(item2);
		}
		SetTiles(perkRangeTilemap, list, "View/Tiles/Feedbacks/Skill/SkillRange");
		int num4 = 0;
		for (int j = 0; j < perk.PerkDefinition.HoverRanges.Count; j++)
		{
			if (j == num2)
			{
				continue;
			}
			int num5 = perk.PerkDefinition.HoverRanges[j].EvalToInt((InterpreterContext)(object)perk);
			if (!hashSet.Add(num5))
			{
				continue;
			}
			list.Clear();
			foreach (Tile item3 in perk.Owner.TileObjectController.GetTilesInRange(num5))
			{
				list.Add(item3);
			}
			if (num4 >= perkHoverRangeSeparatorTilemaps.Count)
			{
				AddPerkHoverRangeSeparatorTilemap();
			}
			SetTiles(perkHoverRangeSeparatorTilemaps[num4], list, "View/Tiles/Feedbacks/Skill/InaccurateRange");
			num4++;
		}
	}

	public void DisplayRangedSkillsModifiers(ITileObject tileObjectSource, TheLastStand.Model.Skill.Skill skill)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		int num3 = skill.SkillController.ComputeMaxRange();
		TileBase val = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/InaccurateRange", false);
		foreach (int key in SkillDatabase.DamageTypeModifiersDefinition.DodgeMultiplierByDistance.Keys)
		{
			if (key > num3 && ++num2 == 2)
			{
				rangedSkillsDodgeMultiplierTilemaps[num].ClearAllTiles();
				continue;
			}
			int maxRange = Mathf.Min(key - 1, num3);
			List<Tile> tilesInRange = tileObjectSource.TileObjectController.GetTilesInRange(maxRange, 0, skill.SkillDefinition.CardinalDirectionOnly);
			TileBase[] array = (TileBase[])(object)new TileBase[tilesInRange.Count];
			Vector3Int[] array2 = (Vector3Int[])(object)new Vector3Int[tilesInRange.Count];
			for (int i = 0; i < tilesInRange.Count; i++)
			{
				array2[i] = (Vector3Int)tilesInRange[i].Position;
				array[i] = val;
			}
			rangedSkillsDodgeMultiplierTilemaps[num].SetTiles(array2, array);
			num++;
		}
	}

	public void DisplayRangeTiles(Dictionary<Tile, TilesInRangeInfos.TileDisplayInfos> tiles)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		TileBase item = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/SkillRange", false);
		List<Vector3Int> list = new List<Vector3Int>(tiles.Count);
		List<TileBase> list2 = new List<TileBase>(tiles.Count);
		foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> tile in tiles)
		{
			list.Add((Vector3Int)tile.Key.Position);
			list2.Add(item);
		}
		SkillRangeTilemap.SetTiles(list.ToArray(), list2.ToArray());
		foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> tile2 in tiles)
		{
			SkillRangeTilemap.SetColor((Vector3Int)tile2.Key.Position, tile2.Value.TileColor);
		}
	}

	public void DisplayReachableTile(Tile tile)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		SetTile(ReachableTilesTilemap, tile, "View/Tiles/Feedbacks/Movement/MoveRange");
		SetTileColor(ReachableTilesTilemap, tile, reachableTilesColor._Color);
	}

	public void DisplaySkillAoE(SkillDefinition skillDefinition, Tile sourceTile, Tilemap tilemap)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val2 = default(Vector2Int);
		for (int i = 0; i < skillDefinition.AreaOfEffectDefinition.Pattern.Count; i++)
		{
			for (int j = 0; j < skillDefinition.AreaOfEffectDefinition.Pattern[i].Count; j++)
			{
				if (skillDefinition.AreaOfEffectDefinition.Pattern[i][j] == 'X' || skillDefinition.AreaOfEffectDefinition.Pattern[i][j] == 'e')
				{
					int num = i;
					Vector2Int val = skillDefinition.AreaOfEffectDefinition.Origin;
					int num2 = num - ((Vector2Int)(ref val)).x;
					int num3 = j;
					val = skillDefinition.AreaOfEffectDefinition.Origin;
					((Vector2Int)(ref val2))._002Ector(num2, num3 - ((Vector2Int)(ref val)).y);
					TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
					val = sourceTile.Position;
					int x = ((Vector2Int)(ref val)).x + ((Vector2Int)(ref val2)).x;
					val = sourceTile.Position;
					Tile tile = tileMap.GetTile(x, ((Vector2Int)(ref val)).y + ((Vector2Int)(ref val2)).y);
					if (tile != null)
					{
						E_AreaOfEffectTileDisplayType areaOfEffectTileDisplayType = ((skillDefinition.AreaOfEffectDefinition.Pattern[i][j] != 'X') ? E_AreaOfEffectTileDisplayType.Surrounding : E_AreaOfEffectTileDisplayType.AreaOfEffect);
						DisplayAreaOfEffectTile(tile, areaOfEffectTileDisplayType, unreachable: true, tilemap);
					}
				}
			}
		}
	}

	public void EndGhostAlphaTilemapsTweening()
	{
		Tween obj = ghostFadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
	}

	public void EndHoverOutlineAlphaTilemapTweening()
	{
		Tween obj = hoverOutlineTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
	}

	public IEnumerator FadeTilesAlphaCoroutine(IEnumerable<Tile> tiles, bool fadeIn, Tilemap tileMap, string tileBasePath, float duration, Ease easing, bool completeIfRunningAlready = true)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (completeIfRunningAlready && alphaTweens.ContainsKey(tileMap))
		{
			TweenExtensions.Complete(alphaTweens[tileMap]);
		}
		if (fadeIn)
		{
			SetTiles(tileMap, tiles.ToList(), tileBasePath);
			foreach (Tile tile in tiles)
			{
				SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, 0f));
			}
		}
		alphaTweens[tileMap] = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => (!fadeIn) ? 1f : 0f), (DOSetter<float>)delegate(float a)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			foreach (Tile tile2 in tiles)
			{
				SetTileColor(tileMap, tile2, new Color(1f, 1f, 1f, a));
			}
		}, fadeIn ? 1f : 0f, duration), easing);
		TweenSettingsExtensions.OnComplete<Tween>(alphaTweens[tileMap], (TweenCallback)delegate
		{
			if (!fadeIn)
			{
				SetTiles(tileMap, tiles.ToList());
			}
			alphaTweens.Remove(tileMap);
		});
		yield return TweenExtensions.WaitForCompletion(alphaTweens[tileMap]);
	}

	public void FadeTilesIndependently(ref Dictionary<Tile, Tween> dico, IEnumerable<Tile> tiles, bool fadeIn, Tilemap tileMap, string tileBasePath, float duration, Ease easing)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Expected O, but got Unknown
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Tile tile in tiles)
		{
			if (fadeIn)
			{
				SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, 0f));
				SetTile(tileMap, tile, fadeIn ? tileBasePath : null);
			}
			if (dico.ContainsKey(tile))
			{
				if (dico[tile] != null && dico[tile].active)
				{
					TweenExtensions.Complete(dico[tile]);
					dico[tile] = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => (!fadeIn) ? 1f : 0f), (DOSetter<float>)delegate(float a)
					{
						//IL_0021: Unknown result type (might be due to invalid IL or missing references)
						SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
					}, fadeIn ? 1f : 0f, duration), easing);
				}
				else
				{
					dico[tile] = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => (!fadeIn) ? 1f : 0f), (DOSetter<float>)delegate(float a)
					{
						//IL_0021: Unknown result type (might be due to invalid IL or missing references)
						SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
					}, fadeIn ? 1f : 0f, duration), easing);
				}
			}
			else
			{
				dico.Add(tile, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => (!fadeIn) ? 1f : 0f), (DOSetter<float>)delegate(float a)
				{
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
				}, fadeIn ? 1f : 0f, duration), easing));
			}
			TweenSettingsExtensions.OnComplete<Tween>(dico[tile], (TweenCallback)delegate
			{
				if (!fadeIn)
				{
					SetTile(tileMap, tile);
				}
			});
			TweenExtensions.Play<Tween>(dico[tile]);
		}
	}

	public void FadeTileIndependently(ref Dictionary<Tile, Tween> dico, Tile tile, bool fadeIn, Tilemap tileMap, string tileBasePath, float duration, Ease easing)
	{
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		if (dico.ContainsKey(tile))
		{
			if (dico[tile] != null && dico[tile].active)
			{
				TweenExtensions.Complete(dico[tile]);
				dico[tile] = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => (!fadeIn) ? 1f : 0f), (DOSetter<float>)delegate(float a)
				{
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
				}, fadeIn ? 1f : 0f, duration), easing);
			}
			else
			{
				SetTile(tileMap, tile, fadeIn ? tileBasePath : null);
				dico[tile] = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => (!fadeIn) ? 1f : 0f), (DOSetter<float>)delegate(float a)
				{
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
				}, fadeIn ? 1f : 0f, duration), easing);
			}
		}
		else
		{
			SetTile(tileMap, tile, fadeIn ? tileBasePath : null);
			dico.Add(tile, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => (!fadeIn) ? 1f : 0f), (DOSetter<float>)delegate(float a)
			{
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
			}, fadeIn ? 1f : 0f, duration), easing));
		}
		TweenExtensions.Play<Tween>(dico[tile]);
	}

	public IEnumerator FadeTileAlphaCoroutine(Tile tile, bool fadeIn, Tilemap tileMap, string tileBasePath, float duration, Ease easing, bool completeIfRunningAlready = true)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (completeIfRunningAlready && alphaTweens.ContainsKey(tileMap))
		{
			TweenExtensions.Complete(alphaTweens[tileMap]);
		}
		if (fadeIn)
		{
			SetTile(tileMap, tile, tileBasePath);
			SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, 0f));
		}
		alphaTweens[tileMap] = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => (!fadeIn) ? 1f : 0f), (DOSetter<float>)delegate(float a)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
		}, fadeIn ? 1f : 0f, duration), easing);
		TweenSettingsExtensions.OnComplete<Tween>(alphaTweens[tileMap], (TweenCallback)delegate
		{
			if (!fadeIn)
			{
				SetTile(tileMap, tile);
			}
			alphaTweens.Remove(tileMap);
		});
		yield return TweenExtensions.WaitForCompletion(alphaTweens[tileMap]);
	}

	public void ForceOpenGate(TheLastStand.Model.Building.Building building, Tile baseTile)
	{
		DisplayBuilding(building, baseTile, "Opened");
	}

	public void Hide()
	{
		activatedTileMaps.Clear();
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			if (((Component)((Component)this).transform.GetChild(i)).gameObject.activeInHierarchy)
			{
				activatedTileMaps.Enqueue(i);
				((Component)((Component)this).transform.GetChild(i)).gameObject.SetActive(false);
			}
		}
	}

	public void SetWorldLimitTile(Vector3Int position)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		WorldLimitsTilemap.SetTile(position, ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/WorldLimits/WorldLimits", false));
	}

	public void StartGhostTilemapsAlphaTweening()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Tween obj = ghostFadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		float alpha = ghostTweenMaxAlpha;
		ghostFadeTween = (Tween)(object)TweenExtensions.SetFullId<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLoops<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => alpha), (DOSetter<float>)delegate(float x)
		{
			SetTilemapsAlpha(x, GhostBuildingsTilemap, GhostBuildingsFrontTilemap);
		}, ghostTweenMinAlpha, ghostTweenDuration), ghostTweenEaseCurve), -1, (LoopType)1), "BuildingGhostTween", (Component)(object)this);
	}

	public void UpdateDisplayRangeTilesColors(Dictionary<Tile, TilesInRangeInfos.TileDisplayInfos> tiles)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> tile in tiles)
		{
			if (SkillRangeTilemap.HasTile((Vector3Int)tile.Key.Position))
			{
				if (!tile.Value.Orientation.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) && SkillManager.SelectedSkill != null && (SkillManager.SelectedSkill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate))
				{
					tile.Value.TileColor = (tile.Value.HasLineOfSight ? skillRangeTilesColorInvalidOrientation._Color : skillHiddenRangeTilesColorInvalidOrientation._Color);
				}
				else
				{
					tile.Value.TileColor = (tile.Value.HasLineOfSight ? skillRangeTilesColor._Color : skillHiddenRangeTilesColor._Color);
				}
				SkillRangeTilemap.SetColor((Vector3Int)tile.Key.Position, tile.Value.TileColor);
			}
		}
	}

	public void UpdateDialsTilesColorsFrom(Tile sourceTile, Dictionary<Tile, TilesInRangeInfos.TileDisplayInfos> inRangeTiles = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		if (sourceTile == null)
		{
			return;
		}
		Vector2Int tilePos = Vector2Int.zero;
		int num = Mathf.Max(TPSingleton<TileMapManager>.Instance.TileMap.Width, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		for (int i = 0; i < num; i++)
		{
			((Vector2Int)(ref tilePos)).x = i;
			int num2 = i;
			Vector2Int position = sourceTile.Position;
			if (num2 == ((Vector2Int)(ref position)).x)
			{
				continue;
			}
			ref Vector2Int reference = ref tilePos;
			position = sourceTile.Position;
			int y = ((Vector2Int)(ref position)).y;
			position = sourceTile.Position;
			((Vector2Int)(ref reference)).y = y + (((Vector2Int)(ref position)).x - i);
			if (dialsTileMap.HasTile((Vector3Int)tilePos))
			{
				TileObjectSelectionManager.E_Orientation orientationFromSelectionToPos = TileObjectSelectionManager.GetOrientationFromSelectionToPos(tilePos);
				Color val = ((inRangeTiles == null || !inRangeTiles.Any((KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> tile) => tile.Key.Position == tilePos)) ? (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? outOfRangeDialsColor._Color : outOfRangeInvalidOrientationDialsColor._Color) : (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? inRangeDialsColor._Color : inRangeInvalidOrientationDialsColor._Color));
				dialsTileMap.SetColor((Vector3Int)tilePos, val);
			}
			ref Vector2Int reference2 = ref tilePos;
			position = sourceTile.Position;
			int y2 = ((Vector2Int)(ref position)).y;
			position = sourceTile.Position;
			((Vector2Int)(ref reference2)).y = y2 - (((Vector2Int)(ref position)).x - i);
			if (dialsTileMap.HasTile((Vector3Int)tilePos))
			{
				TileObjectSelectionManager.E_Orientation orientationFromSelectionToPos = TileObjectSelectionManager.GetOrientationFromSelectionToPos(tilePos);
				Color val = ((inRangeTiles == null || !inRangeTiles.Any((KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> tile) => tile.Key.Position == tilePos)) ? (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? outOfRangeDialsColor._Color : outOfRangeInvalidOrientationDialsColor._Color) : (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? inRangeDialsColor._Color : inRangeInvalidOrientationDialsColor._Color));
				dialsTileMap.SetColor((Vector3Int)tilePos, val);
			}
		}
	}

	private void AddGhostRangeTile(Tile tile, Vector2Int distance, Tile sourceTile, bool ignoreLineOfSight, Vector2Int range)
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		if (tile == null)
		{
			return;
		}
		if (Mathf.Abs(((Vector2Int)(ref distance)).x) + Mathf.Abs(((Vector2Int)(ref distance)).y) >= ((Vector2Int)(ref range)).x)
		{
			if (ignoreLineOfSight)
			{
				TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: true, TileObjectSelectionManager.E_Orientation.NONE, isSkillSelected: false));
				return;
			}
			if (TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Exclude.Contains(tile))
			{
				TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: false, TileObjectSelectionManager.E_Orientation.NONE, isSkillSelected: false));
				return;
			}
			if (SkillActionExecutionController.CheckAndUpdateLineOfSight(tile, sourceTile, distance, ((Vector2Int)(ref range)).y, TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Obstacle, TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Exclude))
			{
				TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: true, TileObjectSelectionManager.E_Orientation.NONE, isSkillSelected: false));
			}
			else
			{
				TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: false, TileObjectSelectionManager.E_Orientation.NONE, isSkillSelected: false));
				TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Exclude.Add(tile);
			}
		}
		if (distance != Vector2Int.zero && SkillActionExecutionController.IsBlockingLineOfSight(tile) && !ignoreLineOfSight)
		{
			TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Obstacle.Add(tile);
			SkillActionExecutionController.ExcludeTiles(tile, distance, ((Vector2Int)(ref range)).y, TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Obstacle, TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Exclude);
		}
	}

	private void AddPerkHoverRangeSeparatorTilemap()
	{
		Tilemap val = Object.Instantiate<Tilemap>(perkHoverRangeSeparatorTemplate, perkHoverRangeSeparatorContainer);
		ClearTiles(val);
		perkHoverRangeSeparatorTilemaps.Add(val);
	}

	private void ClearBuildingGhostRangeAndZoneTiles()
	{
		ClearRangeTiles(TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range);
		ClearTiles(AreaOfEffectTilemap);
		TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Clear();
	}

	private void ComputeFogMinMaxTiles(int densityMin, int densityMax)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		Tile centerTile = TileMapController.GetCenterTile();
		fogMinMaxTiles = new HashSet<Vector3Int>();
		int num = Mathf.Abs(centerTile.X - densityMax) + 1;
		int num2 = Mathf.Abs(centerTile.X + densityMax) - 1;
		int num3 = Mathf.Abs(centerTile.Y - densityMax) + 1;
		int num4 = Mathf.Abs(centerTile.Y + densityMax) - 1;
		int num5 = densityMax - densityMin;
		for (int i = 0; i < num5; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				fogMinMaxTiles.Add(new Vector3Int(j, num3 + i, 0));
				fogMinMaxTiles.Add(new Vector3Int(j, num4 - i, 0));
			}
			for (int k = num3; k <= num4; k++)
			{
				fogMinMaxTiles.Add(new Vector3Int(num + i, k, 0));
				fogMinMaxTiles.Add(new Vector3Int(num2 - i, k, 0));
			}
		}
	}

	private void DisplayBuildingGhostRange(SkillDefinition skillDefinition, Tile sourceTile, ITileObjectDefinition tileObjectDefinition)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (skillDefinition.InfiniteRange)
		{
			return;
		}
		bool ignoreLineOfSight = skillDefinition.SkillActionDefinition.HasEffect("IgnoreLineOfSight");
		Vector2Int range = skillDefinition.Range;
		Vector2Int distance = default(Vector2Int);
		foreach (KeyValuePair<Tile, Tile> item in sourceTile.GetTilesInRangeWithClosestOccupiedTile(tileObjectDefinition, ((Vector2Int)(ref range)).y, 0, skillDefinition.CardinalDirectionOnly))
		{
			((Vector2Int)(ref distance))._002Ector(item.Key.X - item.Value.X, item.Key.Y - item.Value.Y);
			AddGhostRangeTile(item.Key, distance, item.Value, ignoreLineOfSight, skillDefinition.Range);
		}
		TilesInRangeInfos lineOfSightTiles = TPSingleton<ConstructionManager>.Instance.LineOfSightTiles;
		range = skillDefinition.Range;
		int x = ((Vector2Int)(ref range)).x;
		range = skillDefinition.Range;
		lineOfSightTiles.ClearLonelyTilesInLineOfSight(sourceTile, x, ((Vector2Int)(ref range)).y);
		DisplayRangeTiles(TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range);
	}

	private IEnumerator DisplayLevelCoroutine()
	{
		int frames = 0;
		int x = 0;
		while (x < TPSingleton<TileMapManager>.Instance.TileMap.Width)
		{
			int num;
			for (int y = 0; y < TPSingleton<TileMapManager>.Instance.TileMap.Height; y = num)
			{
				Tile tile = TileMapManager.GetTile(x, y);
				string id = tile.GroundDefinition.Id;
				TileBase val = ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/" + id, false);
				((tile.GroundDefinition.GroundCategory == GroundDefinition.E_GroundCategory.NoBuilding) ? GroundCraterTilemap : GroundCityTilemap).SetTile(new Vector3Int(x, y, 0), val);
				SetTile(GridTilemap, tile, "View/Tiles/Feedbacks/Grid Cell");
				if (frames++ % TileMapManager.LoadingSpeed == 0)
				{
					yield return SharedYields.WaitForEndOfFrame;
				}
				if (tile.Building != null && tile.Building.OriginTile == tile)
				{
					DisplayBuilding(tile.Building, tile);
					if (frames++ % TileMapManager.LoadingSpeed == 0)
					{
						yield return SharedYields.WaitForEndOfFrame;
					}
				}
				num = y + 1;
			}
			num = x + 1;
			x = num;
		}
	}

	private void LoadTileAssets()
	{
		foreach (BuildingDefinition value in BuildingDatabase.BuildingDefinitions.Values)
		{
			for (int num = value.BlueprintModuleDefinition.Tiles.Count - 1; num >= 0; num--)
			{
				for (int num2 = value.BlueprintModuleDefinition.Tiles[num].Count - 1; num2 >= 0; num2--)
				{
					if (value.BlueprintModuleDefinition.Tiles[num][num2] == Tile.E_UnitAccess.Blocked || value.BlueprintModuleDefinition.Tiles[num][num2] == Tile.E_UnitAccess.Hero)
					{
						ResourcePooler<Sprite>.CacheAll(string.Format("{0}/{1}/{2}{3}", "View/Sprites/ConstructionAnimation", value.Id, num, num2));
					}
				}
			}
		}
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/BuildingSelectionFeedback");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/MistRange/MistRange");
		ResourcePooler<TileBase>.Cache("View/Tiles/World/Fog");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Grid Cell");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Movement/MoveRange");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Occupation Volume");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Occupation Volume Ghost");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/PanicOnEnemy");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/PoisonDeath");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Skill/SkillAoe Back");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Skill/SkillManeuver");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Skill/SkillRange");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Skill/SkillSurrounding");
	}

	private void SetTilemapsAlpha(float alpha, params Tilemap[] tilemaps)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Color color = tilemaps[0].color;
		color.a = alpha;
		for (int num = tilemaps.Length - 1; num >= 0; num--)
		{
			tilemaps[num].color = color;
		}
	}

	private void Start()
	{
		rangedSkillsDodgeMultiplierTilemaps = (Tilemap[])(object)new Tilemap[SkillDatabase.DamageTypeModifiersDefinition.DodgeMultiplierByDistance.Count];
		int num = 0;
		foreach (int key in SkillDatabase.DamageTypeModifiersDefinition.DodgeMultiplierByDistance.Keys)
		{
			_ = key;
			rangedSkillsDodgeMultiplierTilemaps[num] = Object.Instantiate<Tilemap>(rangedSkillsDodgeMultiplierTemplate, rangedSkillsDodgeMultiplierContainer);
			rangedSkillsDodgeMultiplierTilemaps[num].ClearAllTiles();
			((Object)((Component)rangedSkillsDodgeMultiplierTilemaps[num]).transform).name = ((Object)((Component)rangedSkillsDodgeMultiplierTilemaps[num]).transform).name.Replace("Template(Clone)", num.ToString());
			num++;
		}
		((Component)rangedSkillsDodgeMultiplierTemplate).gameObject.SetActive(false);
		foreach (KeyValuePair<TileFlagDefinition.E_TileFlagTag, List<Tile>> tilesByFlag in TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag)
		{
			TileFlagDefinition tileFlagDefinition = TileMapManager.TileFlagDefinitions.FirstOrDefault((TileFlagDefinition o) => o.TileFlagTag == tilesByFlag.Key);
			if ((Object)(object)tileFlagDefinition == (Object)null)
			{
				((CLogger<TileMapManager>)TPSingleton<TileMapManager>.Instance).LogError((object)$"{tilesByFlag.Key} was not found in TileMapManager.TileFlagDefinitions even though it is present in TileMapManager.Instance.TileMap.TilesWithFlag. Something is wrong here!", (CLogLevel)1, true, true);
				continue;
			}
			for (int num2 = tilesByFlag.Value.Count - 1; num2 >= 0; num2--)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayFlagTile(tileFlagDefinition, tilesByFlag.Value[num2]);
			}
		}
		if (TPSingleton<LevelEditorManager>.Exist())
		{
			FogMinMaxTilemap.ClearAllTiles();
		}
	}

	private void StartHoverOutlineTilemapAlphaTweening()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (hoverOutlineTween == null || !TweenExtensions.IsActive(hoverOutlineTween) || !TweenExtensions.IsPlaying(hoverOutlineTween))
		{
			hoverOutlineTween = (Tween)(object)TweenExtensions.SetFullId<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLoops<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => hoverOutlineTweenMaxAlpha), (DOSetter<float>)delegate(float x)
			{
				SetTilemapsAlpha(x, buildingHoverOutlinesTilemap);
			}, hoverOutlineTweenMinAlpha, hoverOutlineTweenDuration), hoverOutlineTweenEaseCurve), -1, (LoopType)1), "BuildingHoverOutlineTween", (Component)(object)this);
		}
	}

	private void UpdateBackground(bool blackenBackground = false)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Max(TPSingleton<TileMapManager>.Instance.TileMap.Width, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		levelBackground.position = GetCellCenterWorldPosition(TPSingleton<TileMapManager>.Instance.TileMap.GetTile(Mathf.FloorToInt((float)TPSingleton<TileMapManager>.Instance.TileMap.Width / 2f), Mathf.FloorToInt((float)TPSingleton<TileMapManager>.Instance.TileMap.Height / 2f)));
		Transform obj = levelBackground;
		obj.position += new Vector3(0f, 0.75f, 0f);
		((Component)levelBackground).transform.localScale = Vector3.one * ((float)num * backgroundSizeRatio / (float)mapSizeReference);
		if (blackenBackground)
		{
			levelBackgroundRenderer.color = Color.black;
		}
	}

	public static void ToggleLevelArt()
	{
		((Component)TPSingleton<TileMapView>.Instance.levelArtContainer).gameObject.SetActive(!((Component)TPSingleton<TileMapView>.Instance.levelArtContainer).gameObject.activeSelf);
	}

	public static void ToggleTilesFlag(TileFlagDefinition.E_TileFlagTag flag = TileFlagDefinition.E_TileFlagTag.None, bool? forcedState = null, bool clearPreviousState = true)
	{
		if (clearPreviousState)
		{
			foreach (Tilemap value in TilemapsByFlag.Values)
			{
				((Component)value).gameObject.SetActive(false);
			}
		}
		if (flag != 0)
		{
			((Component)TilemapsByFlag[flag]).gameObject.SetActive(forcedState ?? (!((Component)TilemapsByFlag[flag]).gameObject.activeSelf));
		}
	}

	public static void ToggleTilesFlagAll(bool? state = null)
	{
		foreach (Tilemap value in TilemapsByFlag.Values)
		{
			((Component)value).gameObject.SetActive(state ?? (!((Component)value).gameObject.activeSelf));
		}
	}

	public void ClearFlagTile(TileFlagDefinition flagDefinition, Tile tile)
	{
		SetTile(TilemapsByFlag[flagDefinition.TileFlagTag], tile);
	}

	public void DisplayFlagTile(TileFlagDefinition flagDefinition, Tile tile)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Tilemap tileMap = TilemapsByFlag[flagDefinition.TileFlagTag];
		SetTile(tileMap, tile, "View/Tiles/Feedbacks/Skill/SkillRange");
		SetTileColor(tileMap, tile, flagDefinition.DebugColor);
	}

	public void DisplayGround(Tile tile, string cityId)
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		TileBase val = ((!TPSingleton<TileMapView>.Instance.levelArtLoaded) ? (ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/" + tile.GroundDefinition.Id + "_" + cityId, false) ?? ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/" + tile.GroundDefinition.Id, false)) : ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/TileShape", false));
		switch (tile.GroundDefinition.GroundCategory)
		{
		case GroundDefinition.E_GroundCategory.City:
			GroundCityTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), val);
			GroundCraterTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), (TileBase)null);
			break;
		case GroundDefinition.E_GroundCategory.NoBuilding:
			GroundCraterTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), val);
			GroundCityTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), (TileBase)null);
			break;
		case GroundDefinition.E_GroundCategory.Outside:
			GroundCraterTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), (TileBase)null);
			GroundCityTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), (TileBase)null);
			break;
		}
	}

	public void SetGroundTilemapsAlpha(float value)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		GroundCityTilemap.color = ColorExtensions.WithA(GroundCityTilemap.color, value);
		GroundCraterTilemap.color = ColorExtensions.WithA(GroundCraterTilemap.color, value);
	}

	[DevConsoleCommand(Name = "TilesFlagsHideAll")]
	public static void DebugHideTilesFlagsAll()
	{
		ToggleTilesFlagAll(false);
	}

	[DevConsoleCommand(Name = "TilesFlagShow")]
	public static void DebugShowTilesFlag([StringConverter(typeof(TileFlagDefinition.E_TileFlagTag))] TileFlagDefinition.E_TileFlagTag flag, bool show = true)
	{
		ToggleTilesFlag(flag, show, clearPreviousState: false);
	}

	[DevConsoleCommand(Name = "TilesFlagsShowAll")]
	public static void DebugShowTilesFlagsAll()
	{
		ToggleTilesFlagAll(true);
	}

	[DevConsoleCommand(Name = "FogShowMinMax")]
	public static void DebugShowFogMinMax(bool show = true)
	{
		FogDefinition fogDefinition = FogDatabase.FogsDefinitions[TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.FogDefinitionId];
		TPSingleton<TileMapView>.Instance.DisplayFogMinMax(show, fogDefinition.FogDensities[^1].Value, fogDefinition.FogDensities[0].Value);
	}

	[DevConsoleCommand(Name = "ShowGroundsLogic")]
	public static void DebugShowGroundsLogic(float alpha = 0.4f)
	{
		TPSingleton<TileMapView>.Instance.SetGroundTilemapsAlpha(alpha);
	}

	[DevConsoleCommand(Name = "ShowBoneZones")]
	public static void DebugShowBoneZones(float alpha = 0.8f)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<TileMapView>.Instance.boneZoneTilemaps.Count == 0)
		{
			TPSingleton<TileMapView>.Instance.GenerateBoneZoneTilemaps();
		}
		foreach (Tilemap boneZoneTilemap in TPSingleton<TileMapView>.Instance.boneZoneTilemaps)
		{
			boneZoneTilemap.color = ColorExtensions.WithA(boneZoneTilemap.color, alpha);
		}
	}

	[DevConsoleCommand(Name = "HideBoneZones")]
	public static void DebugHideBoneZones()
	{
		DebugShowBoneZones(0f);
	}

	[DevConsoleCommand(Name = "ShowBonePilesPercentage")]
	public static void DebugShowBonePilesPercentage()
	{
		Object.FindObjectOfType<BonePilePercentagesView>()?.Toggle(state: true);
	}

	[DevConsoleCommand(Name = "HideBonePilesPercentage")]
	public static void DebugHideBonePilesPercentage()
	{
		Object.FindObjectOfType<BonePilePercentagesView>()?.Toggle(state: false);
	}

	[DevConsoleCommand(Name = "SetBonePilePercentage")]
	public static void DebugSetBonePilePercentage([StringConverter(typeof(StringToBonePileIdConverter))] string pileId, int percentage)
	{
		Tile selectedTile = TileObjectSelectionManager.SelectedTile;
		if (selectedTile != null)
		{
			if (!TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.TryGetValue(selectedTile, out var value))
			{
				value = new Dictionary<string, int>();
				TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.Add(selectedTile, value);
			}
			if (value.ContainsKey(pileId))
			{
				value[pileId] += percentage;
			}
			else
			{
				value.Add(pileId, percentage);
			}
		}
	}

	[DevConsoleCommand(Name = "SetBonePilePercentageForTile")]
	public static void DebugSetBonePilePercentageForTile(int x, int y, [StringConverter(typeof(StringToBonePileIdConverter))] string pileId, int percentage)
	{
		Tile tile = TileMapManager.GetTile(x, y);
		if (tile != null)
		{
			if (!TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.TryGetValue(tile, out var value))
			{
				value = new Dictionary<string, int>();
				TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.Add(tile, value);
			}
			if (value.ContainsKey(pileId))
			{
				value[pileId] += percentage;
			}
			else
			{
				value.Add(pileId, percentage);
			}
		}
	}

	[DevConsoleCommand(Name = "AddBonePiles")]
	public static void DebugAddBonePiles([StringConverter(typeof(StringToBonePileIdConverter))] string pileId, int tilesCount)
	{
		for (int i = 0; i < tilesCount; i++)
		{
			Tile randomTile = TileMapManager.GetRandomTile();
			if (!TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.TryGetValue(randomTile, out var value))
			{
				value = new Dictionary<string, int>();
				TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.Add(randomTile, value);
			}
			if (value.ContainsKey(pileId))
			{
				value[pileId] += 100;
			}
			else
			{
				value.Add(pileId, 100);
			}
		}
	}
}
