using System;
using System.Collections;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition;
using TheLastStand.Definition.Skill;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

namespace TheLastStand.View.Unit;

public class EnemyUnitView : UnitView
{
	public enum E_EnemyHudType
	{
		Small,
		Large,
		BossSmall,
		BossLarge
	}

	public new static class Constants
	{
		public static class Sprites
		{
			public const string SpritesFolderPath = "View/Sprites/Units/";

			public const string BossSpritesFolderName = "BossUnits";

			public const string SpritesFolderName = "EnemyUnits";

			public const string DeadBodiesFolderName = "DeadBodies";

			public const string DefaultSpritesFolderName = "DefaultSprites";

			public const string PortraitPathPrefix = "View/Sprites/UI/Units/Portaits/Enemy Unit/EnemiesPortrait_";
		}

		public static readonly int AnimatorSpawnStateHash = Animator.StringToHash("Spawn");

		public const int DefaultSortingOrder = 30;

		public const string IdleSpeedParameterName = "IdleSpeed";

		public const string IdleStateName = "Idle";

		public const string SpawnAnimationClipName = "Ennemy_Spawn";

		public const string SpawnAnimatorStateName = "Spawn";

		public const string EnemyMaterialPath = "View/Materials/EnemyUnit";

		public const string BossMaterialPath = "View/Materials/BossUnit";
	}

	[SerializeField]
	private EnemyAttackFeedback enemyAttackFeedbackPrefab;

	[SerializeField]
	private SpriteRenderer frontSpriteRenderer;

	[SerializeField]
	private SpriteRenderer backSpriteRenderer;

	[SerializeField]
	private Transform front;

	[SerializeField]
	private Transform back;

	[SerializeField]
	private SortingGroup sortingGroup;

	private static readonly int IdleSpeedParameter = Animator.StringToHash("IdleSpeed");

	private static readonly int MaskingColorId = Shader.PropertyToID("_MaskingColor");

	private Material material;

	private HashSet<PlayableUnit> playableUnitsInRange = new HashSet<PlayableUnit>();

	protected WaitUntil waitUntilAnimatorStateIsSpawn;

	public EnemyAttackFeedback EnemyAttackFeedbackPrefab => enemyAttackFeedbackPrefab;

	public EnemyUnit EnemyUnit { get; protected set; }

	public bool IsVisible
	{
		get
		{
			if (!((Renderer)frontSpriteRenderer).isVisible)
			{
				return ((Renderer)backSpriteRenderer).isVisible;
			}
			return true;
		}
	}

	public bool HasSpawnAnim { get; private set; }

	public override bool Hovered
	{
		get
		{
			return hovered;
		}
		set
		{
			bool flag = value || (EnemyUnit?.LinkedBuilding != null && EnemyUnit.LinkedBuilding == TileObjectSelectionManager.SelectedBuilding);
			if (hovered != flag)
			{
				hovered = flag;
				RefreshCursorFeedback();
			}
		}
	}

	public override float MoveSpeed
	{
		get
		{
			if (!EnemyUnitManager.TurboMode)
			{
				return EnemyUnit.EnemyUnitTemplateDefinition.MoveSpeed * GameManager.MoveSpeedMultiplier;
			}
			return EnemyUnit.EnemyUnitTemplateDefinition.MoveSpeed * GameManager.MoveSpeedMultiplier * 10f;
		}
	}

	public EnemyUnitHUD EnemyUnitHUD => base.UnitHUD as EnemyUnitHUD;

	public override TheLastStand.Model.Unit.Unit Unit
	{
		get
		{
			return base.Unit;
		}
		set
		{
			base.Unit = value;
			EnemyUnit = Unit as EnemyUnit;
			InitZoneControlSkill();
			InstantiateHudIfNeeded();
			base.UnitHUD.Unit = Unit;
		}
	}

	public WaitUntil WaitUntilAnimatorStateIsSpawn => waitUntilAnimatorStateIsSpawn;

	public static Sprite GetHiddenEnemySprite()
	{
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Units/Portaits/Enemy Unit/EnemiesPortrait_Hidden", failSilently: false);
	}

	public static Sprite GetUiSprite(string enemyUnitDefinitionId)
	{
		if (string.IsNullOrEmpty(enemyUnitDefinitionId))
		{
			return null;
		}
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Units/Portaits/Enemy Unit/EnemiesPortrait_" + enemyUnitDefinitionId, failSilently: false);
	}

	public override void ClearWaitUntils()
	{
		base.ClearWaitUntils();
		waitUntilAnimatorStateIsSpawn = null;
	}

	public virtual string GetDefaultSpritesFolder()
	{
		if (!(EnemyUnit is BossUnit))
		{
			return "EnemyUnits";
		}
		return "BossUnits";
	}

	public override void InitVisuals(bool playSpawnAnim)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)animator == (Object)null))
		{
			string specificAssetsId = EnemyUnit.SpecificAssetsId;
			InitDefaultSprites(specificAssetsId);
			Unit.UiSprite = GetUiSprite(specificAssetsId);
			front.localPosition = Vector2.op_Implicit(EnemyUnit.EnemyUnitTemplateDefinition.VisualOffset);
			back.localPosition = Vector2.op_Implicit(EnemyUnit.EnemyUnitTemplateDefinition.VisualOffset);
			if (EnemyUnit.EnemyUnitTemplateDefinition.SortingOrderOverride.HasValue)
			{
				sortingGroup.sortingOrder = EnemyUnit.EnemyUnitTemplateDefinition.SortingOrderOverride.Value;
			}
			if ((Object)(object)Unit.UiSprite == (Object)null)
			{
				Unit.UiSprite = EnemyUnit.DefaultSpriteFront;
			}
			animator.runtimeAnimatorController = null;
			frontSpriteRenderer.sprite = EnemyUnit.DefaultSpriteFront;
			backSpriteRenderer.sprite = EnemyUnit.DefaultSpriteBack;
			InitMaterial();
			base.InitVisuals(playSpawnAnim);
		}
	}

	public override void LookAtDirection(GameDefinition.E_Direction direction)
	{
		if (EnemyUnit != null && EnemyUnit.EnemyUnitTemplateDefinition.LockedOrientation != GameDefinition.E_Direction.None)
		{
			direction = EnemyUnit.EnemyUnitTemplateDefinition.LockedOrientation;
		}
		base.LookAtDirection(direction);
	}

	public override void RefreshCursorFeedback()
	{
		DisplayHover(!base.Selected && Hovered);
		RefreshZoneControl();
		if (EnemyUnit.LinkedBuilding != null)
		{
			EnemyUnit.LinkedBuilding.BuildingView.Hovered = base.HoveredOrSelected;
		}
	}

	protected virtual void InitMaterial()
	{
		material = ResourcePooler.LoadOnce<Material>(((EnemyUnit is BossUnit) ? "View/Materials/BossUnit" : "View/Materials/EnemyUnit") + "_" + EnemyUnit.SpecificAssetsId, failSilently: true) ?? EnemyUnitManager.DefaultEnemyMaterial;
		((Renderer)frontSpriteRenderer).material = material;
		((Renderer)backSpriteRenderer).material = material;
		RefreshMaterial();
	}

	public void RefreshMaterial()
	{
		if (Unit.OriginTile.HasAnyFog)
		{
			((Renderer)frontSpriteRenderer).material = TPSingleton<FogManager>.Instance.FogView.EnemiesInFogMaterial;
			((Renderer)backSpriteRenderer).material = TPSingleton<FogManager>.Instance.FogView.EnemiesInFogMaterial;
		}
		else
		{
			((Renderer)frontSpriteRenderer).material = material;
			((Renderer)backSpriteRenderer).material = material;
		}
	}

	protected override void DisableHUD()
	{
		((Component)base.UnitHUD).gameObject.SetActive(false);
	}

	protected override bool InitAnimations()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		((Behaviour)animator).enabled = true;
		animator.runtimeAnimatorController = ResourcePooler.LoadOnce<RuntimeAnimatorController>("Animators/Units/EnemyUnits/" + EnemyUnit.SpecificAssetsId + "/" + EnemyUnit.SpecificAssetsId + "_" + EnemyUnit.VariantId, failSilently: false);
		animator.SetFloat(IdleSpeedParameter, (Unit is BossUnit) ? 1f : RandomManager.GetRandomRange(this, EnemyUnitManager.IdleAnimSpeedMultRange.x, EnemyUnitManager.IdleAnimSpeedMultRange.y));
		if (!base.InitAnimations())
		{
			return false;
		}
		if (waitUntilAnimatorStateIsSpawn == null)
		{
			waitUntilAnimatorStateIsSpawn = new WaitUntil((Func<bool>)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				return ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).shortNameHash == Constants.AnimatorSpawnStateHash;
			});
		}
		List<KeyValuePair<AnimationClip, AnimationClip>> list = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		animatorOverrideController.GetOverrides(list);
		HasSpawnAnim = false;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			KeyValuePair<AnimationClip, AnimationClip> keyValuePair = list[num];
			if (((Object)keyValuePair.Key).name == "Ennemy_Spawn" && (Object)(object)keyValuePair.Value != (Object)null)
			{
				HasSpawnAnim = true;
				break;
			}
		}
		return true;
	}

	protected override void InitAndStartAnimations(bool playSpawnAnim)
	{
		base.InitAndStartAnimations(playSpawnAnim);
		animator.Play("Idle", -1, 0f);
		if (playSpawnAnim && HasSpawnAnim)
		{
			animator.Play("Spawn");
		}
	}

	protected virtual void InitDefaultSprites(string unitId)
	{
		EnemyUnit.DefaultSpriteFront = ResourcePooler.LoadOnce<Sprite>("View/Sprites/Units/" + GetDefaultSpritesFolder() + "/DefaultSprites/" + unitId + "/" + EnemyUnit.VariantId + "/" + unitId + "_Lvl1_" + EnemyUnit.VariantId + "_Idle_Front_00", failSilently: false);
		Sprite val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/Units/" + GetDefaultSpritesFolder() + "/DefaultSprites/" + unitId + "/" + EnemyUnit.VariantId + "/" + unitId + "_Lvl1_" + EnemyUnit.VariantId + "_Idle_Back_00", failSilently: false);
		EnemyUnit.DefaultSpriteBack = val ?? EnemyUnit.DefaultSpriteFront;
	}

	protected override void InitHud()
	{
	}

	protected virtual E_EnemyHudType GetBestFittingHUD()
	{
		if (EnemyUnit is BossUnit bossUnit)
		{
			return bossUnit.BossUnitTemplateDefinition.HealthGaugeSize switch
			{
				E_GaugeSize.Small => E_EnemyHudType.BossSmall, 
				E_GaugeSize.Large => E_EnemyHudType.BossLarge, 
				_ => E_EnemyHudType.BossLarge, 
			};
		}
		return EnemyUnit.EnemyUnitTemplateDefinition.HealthGaugeSize switch
		{
			E_GaugeSize.Small => E_EnemyHudType.Small, 
			E_GaugeSize.Large => E_EnemyHudType.Large, 
			_ => E_EnemyHudType.Large, 
		};
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Hovered = false;
		base.Selected = false;
		PlayableUnitManager.OnPlayableUnitMoved -= HandleOnPlayableUnitMoved;
		PlayableUnitManager.OnPlayableUnitDied -= HandleOnPlayableUnitDied;
	}

	protected override IEnumerator PlayDieAnimCoroutine()
	{
		if (!EnemyUnit.IgnoreFromEnemyUnitsCount && !(EnemyUnit is BossUnit))
		{
			TPSingleton<EnemyUnitManager>.Instance.EnemiesDying.Add(EnemyUnit);
		}
		yield return base.PlayDieAnimCoroutine();
	}

	private void DisplayHover(bool show)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!TPSingleton<TileMapManager>.Exist())
		{
			return;
		}
		TileBase val = (show ? TileMapView.EnemiesHoverTileBase : null);
		foreach (Tile occupiedTile in EnemyUnit.OccupiedTiles)
		{
			TileMapView.EnemiesHoverTilemap.SetTile((Vector3Int)occupiedTile.Position, val);
		}
	}

	private void InstantiateHudIfNeeded()
	{
		if ((Object)(object)base.UnitHUD != (Object)null)
		{
			((Component)base.UnitHUD).gameObject.SetActive(true);
		}
		else if (Unit != null)
		{
			E_EnemyHudType bestFittingHUD = GetBestFittingHUD();
			base.UnitHUD = ObjectPooler.GetPooledComponent<UnitHUD>(bestFittingHUD.GetPoolId(), bestFittingHUD.GetPrefab(), PlayableUnitManager.UnitHudsTransform, dontSetParent: false);
		}
	}

	public void DisplayZoneControlSkill()
	{
		TPSingleton<TileMapView>.Instance.DisplaySkillAoE(EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill, EnemyUnit.OriginTile, TileMapView.EnemiesHoverAreaOfEffectTilemap);
	}

	public void RefreshZoneControl()
	{
		if (EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill != null)
		{
			TPSingleton<EnemyUnitManager>.Instance.HookZoneControlEnemy(this, ShouldDisplayZoneControlSkill());
		}
	}

	private bool ShouldDisplayZoneControlSkill()
	{
		if (!EnemyUnit.IsDeadOrDeathRattling)
		{
			if (!base.Selected && !Hovered)
			{
				return playableUnitsInRange.Count > 0;
			}
			return true;
		}
		return false;
	}

	private void InitZoneControlSkill()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		HookZoneControlSkill();
		if (EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill == null)
		{
			return;
		}
		AreaOfEffectDefinition areaOfEffectDefinition = EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill.AreaOfEffectDefinition;
		for (int i = 0; i < areaOfEffectDefinition.Pattern.Count; i++)
		{
			for (int j = 0; j < areaOfEffectDefinition.Pattern[i].Count; j++)
			{
				if (areaOfEffectDefinition.Pattern[i][j] == 'X')
				{
					Vector2Int val = EnemyUnit.OriginTile.Position;
					int x = ((Vector2Int)(ref val)).x;
					val = areaOfEffectDefinition.Origin;
					int x2 = x - ((Vector2Int)(ref val)).x + j;
					val = EnemyUnit.OriginTile.Position;
					int y = ((Vector2Int)(ref val)).y;
					val = areaOfEffectDefinition.Origin;
					if (TileMapManager.GetTile(x2, y - ((Vector2Int)(ref val)).y + i)?.Unit is PlayableUnit item)
					{
						playableUnitsInRange.Add(item);
					}
				}
			}
		}
		RefreshZoneControl();
	}

	private void HookZoneControlSkill()
	{
		PlayableUnitManager.OnPlayableUnitMoved -= HandleOnPlayableUnitMoved;
		PlayableUnitManager.OnPlayableUnitDied -= HandleOnPlayableUnitDied;
		if (EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill != null)
		{
			PlayableUnitManager.OnPlayableUnitMoved += HandleOnPlayableUnitMoved;
			PlayableUnitManager.OnPlayableUnitDied += HandleOnPlayableUnitDied;
		}
	}

	private void HandleOnPlayableUnitMoved(PlayableUnit playableUnit, Tile tile)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		AreaOfEffectDefinition areaOfEffectDefinition = EnemyUnit.EnemyUnitTemplateDefinition.ZoneControlSkill.AreaOfEffectDefinition;
		Vector2Int val = playableUnit.OriginTile.Position - EnemyUnit.OriginTile.Position + areaOfEffectDefinition.Origin;
		if (((Vector2Int)(ref val)).y >= 0 && ((Vector2Int)(ref val)).y < areaOfEffectDefinition.Pattern.Count && ((Vector2Int)(ref val)).x >= 0 && ((Vector2Int)(ref val)).x < areaOfEffectDefinition.Pattern[((Vector2Int)(ref val)).y].Count && areaOfEffectDefinition.Pattern[((Vector2Int)(ref val)).y][((Vector2Int)(ref val)).x] == 'X')
		{
			playableUnitsInRange.Add(playableUnit);
		}
		else
		{
			playableUnitsInRange.Remove(playableUnit);
		}
		RefreshZoneControl();
	}

	private void HandleOnPlayableUnitDied(PlayableUnit playableUnit)
	{
		playableUnitsInRange.Remove(playableUnit);
		RefreshZoneControl();
	}
}
