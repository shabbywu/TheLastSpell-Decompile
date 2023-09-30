using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Definition;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Building;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.View.Unit;

public abstract class UnitView : MonoBehaviour, IDamageableView, ITileObjectView
{
	public enum E_GaugeSize
	{
		Small,
		Large
	}

	public static class Constants
	{
		public static class Animation
		{
			public const string BackLayerName = "Back Layer";

			public const string FrontLayerName = "Front Layer";

			public const string AnimatorEnemyUnitFolderPath = "Animators/Units/EnemyUnits";

			public const string SkillCastAnimBackSuffix = "Back";

			public const string SkillCastAnimFrontSuffix = "Front";

			public const string SkillCastAnimRootFolderPath = "Animation/Caster Anims/";

			private const string SkillCastDefaultAnimClipBaseName = "Hero_CastSkill_Default_";

			public const string ParamBoolWalk = "Walk";

			public const string ParamTriggerCastSkill = "Cast Skill";

			public const string ParamTriggerDie = "Die";

			public const string ParamTriggerPrepareDie = "PrepareDie";

			public const string ParamTriggerTakeDamage = "Take Damage";

			public static readonly int AnimatorCastSkillStateHash;

			public static readonly int AnimatorDeadStateHash;

			public static readonly int AnimatorDeathStateHash;

			public static readonly int AnimatorIdleStateHash;

			public static readonly int AnimatorLevelUpStateHash;

			public static readonly int AnimatorPrepareDeathStateHash;

			public static readonly int AnimatorTakeDamageStateHash;

			public static readonly string SkillCastDefaultAnimClipBackName;

			public static readonly string SkillCastDefaultAnimClipFrontName;

			static Animation()
			{
				SkillCastDefaultAnimClipBackName = "Hero_CastSkill_Default_Back";
				SkillCastDefaultAnimClipFrontName = "Hero_CastSkill_Default_Front";
				AnimatorCastSkillStateHash = Animator.StringToHash("Cast Skill");
				AnimatorDeathStateHash = Animator.StringToHash("Death");
				AnimatorDeadStateHash = Animator.StringToHash("Dead");
				AnimatorIdleStateHash = Animator.StringToHash("Idle");
				AnimatorLevelUpStateHash = Animator.StringToHash("LevelUp");
				AnimatorPrepareDeathStateHash = Animator.StringToHash("Prepare Death");
				AnimatorTakeDamageStateHash = Animator.StringToHash("Take Damage");
			}
		}

		public static class Effects
		{
			public const string AttackFeedbackPrefabResourcePath = "Prefab/Displayable Effect/Attack Feedback";

			public const string GainArmorFeedbackPrefabResourcePath = "Prefab/Displayable Effect/Gain Armor Feedback";

			public const string HealFeedbackPrefabResourcePath = "Prefab/Displayable Effect/Heal Feedback";
		}
	}

	[SerializeField]
	[FormerlySerializedAs("bodyTransform")]
	protected Transform rootTransform;

	[SerializeField]
	private GameObject bodyFrontContainer;

	[SerializeField]
	private GameObject bodyBackContainer;

	[SerializeField]
	protected SpriteRenderer bodyFrontRenderer;

	[SerializeField]
	protected SpriteRenderer bodyBackRenderer;

	[SerializeField]
	private RendererEventsListener frontRendererEventsListener;

	[SerializeField]
	private RendererEventsListener backRendererEventsListener;

	[SerializeField]
	protected Animator animator;

	[SerializeField]
	private UnitHUD hudPrefab;

	[SerializeField]
	protected Transform hudFollowTarget;

	[SerializeField]
	protected GameObject damagedParticles;

	private float startZ;

	private TheLastStand.Model.Unit.Unit unit;

	private bool orientationIsFront = true;

	private int backLayerIndex = -1;

	private int frontLayerIndex = -1;

	private bool selected;

	protected bool hovered;

	protected AnimatorOverrideController animatorOverrideController;

	protected WaitUntil waitUntilAnimatorStateIsCastSkill;

	protected WaitUntil waitUntilAnimatorStateIsDie;

	protected WaitUntil waitUntilAnimatorStateIsDead;

	protected WaitUntil waitUntilAnimatorStateIsIdle;

	protected WaitUntil waitUntilAnimatorStateIsTakeDamage;

	private Coroutine displaySkillEffectsCoroutine;

	private Coroutine takeHitAnimCoroutine;

	private HealFeedback healFeedback;

	private GainArmorFeedback gainArmorFeedback;

	private List<Tile> repeledTilesForPreviousTile = new List<Tile>();

	private List<Tile> repeledTilesForCurrentTile = new List<Tile>();

	private int visibleSpritesCount;

	public Animator Animator => animator;

	public bool AreAnimationsInitialized { get; protected set; }

	public AttackFeedback AttackFeedback { get; protected set; }

	public SpriteRenderer BodyBackRenderer => bodyBackRenderer;

	public SpriteRenderer BodyFrontRenderer => bodyFrontRenderer;

	public IDamageable Damageable => Unit;

	public IDamageableHUD DamageableHUD => UnitHUD;

	public bool DieAnimationIsFinished { get; private set; }

	public GainArmorFeedback GainArmorFeedback
	{
		get
		{
			if ((Object)(object)gainArmorFeedback == (Object)null)
			{
				gainArmorFeedback = Object.Instantiate<GainArmorFeedback>(ResourcePooler.LoadOnce<GainArmorFeedback>("Prefab/Displayable Effect/Gain Armor Feedback", false));
				gainArmorFeedback.Init(this);
			}
			return gainArmorFeedback;
		}
	}

	public GameObject GameObject => ((Component)this).gameObject;

	public Transform HudFollowTarget => hudFollowTarget;

	public HealFeedback HealFeedback
	{
		get
		{
			if ((Object)(object)healFeedback == (Object)null)
			{
				healFeedback = Object.Instantiate<HealFeedback>(ResourcePooler.LoadOnce<HealFeedback>("Prefab/Displayable Effect/Heal Feedback", false));
				healFeedback.Init(this);
			}
			return healFeedback;
		}
	}

	public virtual bool Hovered
	{
		get
		{
			return hovered;
		}
		set
		{
			if (hovered != value)
			{
				hovered = value;
				RefreshCursorFeedback();
			}
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

	public bool IsCastingSkill
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)animator != (Object)null)
			{
				AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				return ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).shortNameHash == Constants.Animation.AnimatorCastSkillStateHash;
			}
			return false;
		}
	}

	public abstract float MoveSpeed { get; }

	public Transform OrientationRootTransform => rootTransform;

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
			if (TPSingleton<TileObjectSelectionManager>.Exist())
			{
				if (Selected)
				{
					TileObjectSelectionManager.SelectedUnitFeedback.Unit = Unit;
				}
				TileObjectSelectionManager.SelectedUnitFeedback.Display(Selected);
				RefreshCursorFeedback();
			}
		}
	}

	public List<IDisplayableEffect> SkillEffectDisplays { get; private set; } = new List<IDisplayableEffect>();


	public bool IsTakingDamage { get; set; }

	public virtual TheLastStand.Model.Unit.Unit Unit
	{
		get
		{
			return unit;
		}
		set
		{
			unit = value;
			if ((Object)(object)UnitHUD != (Object)null)
			{
				UnitHUD.Unit = unit;
			}
		}
	}

	public UnitHUD UnitHUD { get; protected set; }

	public WaitUntil WaitUntilAnimatorStateIsIdle => waitUntilAnimatorStateIsIdle;

	public WaitUntil WaitUntilDeathCanBeFinalized => waitUntilAnimatorStateIsDead;

	public WaitUntil WaitUntilIsDying => waitUntilAnimatorStateIsDie;

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

	public virtual void ClearWaitUntils()
	{
		AreAnimationsInitialized = false;
		waitUntilAnimatorStateIsCastSkill = null;
		waitUntilAnimatorStateIsDead = null;
		waitUntilAnimatorStateIsDie = null;
		waitUntilAnimatorStateIsIdle = null;
		waitUntilAnimatorStateIsTakeDamage = null;
	}

	public Task CreateMoveTask(bool followPathOrientation = true, float moveSpeed = -1f, float delay = 0f, bool isMovementInstant = false, bool isCompensate = false)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		List<Tile> list = new List<Tile>();
		int i = 0;
		for (int count = Unit.Path.Count; i < count; i++)
		{
			if (i == 0 || i == count - 1 || !isMovementInstant)
			{
				list.Add(Unit.Path[i]);
			}
		}
		if (isMovementInstant)
		{
			moveSpeed = float.PositiveInfinity;
		}
		return (Task)new CoroutineTask((MonoBehaviour)(object)this, MoveUnitCoroutine(list, followPathOrientation, moveSpeed, delay));
	}

	public IEnumerator DisableWhenPossible()
	{
		if (Unit.HasBeenExiled && Unit.ExileForcePlayDieAnim)
		{
			yield return waitUntilAnimatorStateIsDead;
		}
		((Component)this).gameObject.SetActive(false);
	}

	public Coroutine DisplaySkillEffects(float delay)
	{
		if (displaySkillEffectsCoroutine != null)
		{
			return displaySkillEffectsCoroutine;
		}
		displaySkillEffectsCoroutine = ((MonoBehaviour)TPSingleton<EffectManager>.Instance).StartCoroutine(DisplaySkillEffectsCoroutine(delay));
		return displaySkillEffectsCoroutine;
	}

	public virtual void ToggleSkillTargeting(bool show)
	{
		UnitHUD.ToggleSkillTargeting(show);
	}

	public virtual void InitVisuals(bool playSpawnAnim)
	{
		SetFrontAndBackActive(active: true);
		InitAndStartAnimations(playSpawnAnim);
		AttackFeedback = Object.Instantiate<AttackFeedback>(ResourcePooler.LoadOnce<AttackFeedback>("Prefab/Displayable Effect/Attack Feedback", false));
		AttackFeedback.Init(this);
	}

	public virtual void LookAtDirection(GameDefinition.E_Direction direction)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		switch (direction)
		{
		case GameDefinition.E_Direction.North:
			rootTransform.localScale = new Vector3(0f - Mathf.Abs(rootTransform.localScale.x), rootTransform.localScale.y, rootTransform.localScale.z);
			SetOrientation(front: false);
			break;
		case GameDefinition.E_Direction.South:
			rootTransform.localScale = new Vector3(Mathf.Abs(rootTransform.localScale.x), rootTransform.localScale.y, rootTransform.localScale.z);
			SetOrientation(front: true);
			break;
		case GameDefinition.E_Direction.East:
			rootTransform.localScale = new Vector3(Mathf.Abs(rootTransform.localScale.x), rootTransform.localScale.y, rootTransform.localScale.z);
			SetOrientation(front: false);
			break;
		case GameDefinition.E_Direction.West:
			rootTransform.localScale = new Vector3(0f - Mathf.Abs(rootTransform.localScale.x), rootTransform.localScale.y, rootTransform.localScale.z);
			SetOrientation(front: true);
			break;
		}
	}

	public void PlayDieAnim()
	{
		((MonoBehaviour)this).StartCoroutine(PlayDieAnimCoroutine());
	}

	public void PlaySkillCastAnim(SkillActionExecution skillExecution)
	{
		((MonoBehaviour)this).StartCoroutine(PlaySkillCastAnimCoroutine(skillExecution));
	}

	public void PlayTakeDamageAnim()
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		if (takeHitAnimCoroutine == null)
		{
			if (Unit.IsExecutingSkill)
			{
				IsTakingDamage = false;
			}
			else
			{
				takeHitAnimCoroutine = ((MonoBehaviour)this).StartCoroutine(PlayTakeDamageAnimCoroutine());
			}
			if (string.IsNullOrEmpty(Unit.UnitTemplateDefinition.DamagedParticlesId))
			{
				Debug.Log((object)(Unit.Id + " damaged particles has not been set in the definition, using default particles."));
			}
			GameObject pooledGameObject = ObjectPooler.GetPooledGameObject(string.IsNullOrEmpty(Unit.UnitTemplateDefinition.DamagedParticlesId) ? Unit.Id : Unit.UnitTemplateDefinition.DamagedParticlesId, string.IsNullOrEmpty(Unit.UnitTemplateDefinition.DamagedParticlesId) ? damagedParticles : null, (Transform)null, false);
			if ((Object)(object)pooledGameObject != (Object)null)
			{
				pooledGameObject.transform.position = Vector2.op_Implicit(TileMapView.GetTileCenter(Unit.OriginTile));
			}
			else
			{
				Unit.LogError("Unable to instantiate or get a pooled DamagedParticles for this unit. PoolName is : " + (string.IsNullOrEmpty(Unit.UnitTemplateDefinition.DamagedParticlesId) ? Unit.Id : Unit.UnitTemplateDefinition.DamagedParticlesId), (CLogLevel)1);
			}
		}
	}

	public void PlayWalkAnim(bool doWalk)
	{
		((Behaviour)animator).enabled = true;
		animator.SetBool("Walk", doWalk);
	}

	[ContextMenu("Refresh Armor")]
	public void RefreshArmor()
	{
		UnitHUD?.RefreshArmor();
	}

	[ContextMenu("Refresh Health")]
	public void RefreshHealth()
	{
		UnitHUD?.RefreshHealth();
	}

	public abstract void RefreshCursorFeedback();

	public virtual void RefreshHud(UnitStatDefinition.E_Stat stat)
	{
		switch (stat)
		{
		case UnitStatDefinition.E_Stat.Health:
		case UnitStatDefinition.E_Stat.HealthTotal:
			RefreshHealth();
			break;
		case UnitStatDefinition.E_Stat.Armor:
		case UnitStatDefinition.E_Stat.ArmorTotal:
			RefreshArmor();
			break;
		}
	}

	public void RefreshHudPositionInstantly()
	{
		UnitHUD.RefreshPositionInstantly();
	}

	[ContextMenu("Refresh Injury Stage")]
	public void RefreshInjuryStage()
	{
		UnitHUD.RefreshInjuryStage();
	}

	[ContextMenu("Refresh Status")]
	public void RefreshStatus()
	{
		UnitHUD.RefreshStatuses();
	}

	public IEnumerator ToggleFollowElementOffWhenIdle()
	{
		yield return WaitUntilAnimatorStateIsIdle;
		UnitHUD.ToggleFollowElement(toggle: false);
	}

	public void OnSkillTargetHover(bool hover)
	{
		UnitHUD.OnSkillTargetHover(hover);
	}

	public virtual void SetFrontAndBackActive(bool active)
	{
		bodyFrontContainer.SetActive(active && orientationIsFront);
		bodyBackContainer.SetActive(active && !orientationIsFront);
	}

	public void UpdatePosition()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (Unit.OriginTile.Building != null && Unit.OriginTile.Building.BuildingView is WatchtowerView watchtowerView)
		{
			((Component)this).transform.position = ((GridLayout)TileMapView.BuildingTilemap).CellToWorld(new Vector3Int(Unit.OriginTile.X, Unit.OriginTile.Y, 0)) + watchtowerView.TowerHeight.localPosition;
		}
		else if (Unit.OriginTile.Building != null)
		{
			((Component)this).transform.position = ((GridLayout)TileMapView.BuildingTilemap).CellToWorld(new Vector3Int(Unit.OriginTile.X, Unit.OriginTile.Y, 0)) - new Vector3(0.01f, 0.01f, 0.01f);
		}
		else
		{
			((Component)this).transform.position = ((GridLayout)TileMapView.BuildingTilemap).CellToWorld(new Vector3Int(Unit.OriginTile.X, Unit.OriginTile.Y, 0)) + Vector3.forward * startZ;
		}
	}

	protected virtual void DisableHUD()
	{
		Object.Destroy((Object)(object)((Component)UnitHUD).gameObject);
	}

	protected virtual bool InitAnimations()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected O, but got Unknown
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Expected O, but got Unknown
		AreAnimationsInitialized = false;
		if ((Object)(object)animator == (Object)null)
		{
			CLoggerManager.Log((object)("InitAnimations(): Need an animator for " + ((Object)((Component)this).transform).name + "! Aborting"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return false;
		}
		if (!((Behaviour)animator).isActiveAndEnabled)
		{
			return false;
		}
		animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
		RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
		AnimatorOverrideController val = (AnimatorOverrideController)(object)((runtimeAnimatorController is AnimatorOverrideController) ? runtimeAnimatorController : null);
		if (val != null)
		{
			List<KeyValuePair<AnimationClip, AnimationClip>> list = new List<KeyValuePair<AnimationClip, AnimationClip>>(val.overridesCount);
			val.GetOverrides(list);
			animatorOverrideController.ApplyOverrides((IList<KeyValuePair<AnimationClip, AnimationClip>>)list);
		}
		animator.runtimeAnimatorController = (RuntimeAnimatorController)(object)animatorOverrideController;
		backLayerIndex = animator.GetLayerIndex("Back Layer");
		frontLayerIndex = animator.GetLayerIndex("Front Layer");
		SetOrientation(front: true, forceRefresh: true);
		MecanimStartRandomFrame component = ((Component)animator).GetComponent<MecanimStartRandomFrame>();
		if (component != null)
		{
			component.Execute();
		}
		if (waitUntilAnimatorStateIsCastSkill == null)
		{
			waitUntilAnimatorStateIsCastSkill = new WaitUntil((Func<bool>)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				AnimatorStateInfo currentAnimatorStateInfo5 = animator.GetCurrentAnimatorStateInfo(0);
				return ((AnimatorStateInfo)(ref currentAnimatorStateInfo5)).shortNameHash == Constants.Animation.AnimatorCastSkillStateHash;
			});
		}
		if (waitUntilAnimatorStateIsDead == null)
		{
			waitUntilAnimatorStateIsDead = new WaitUntil((Func<bool>)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				AnimatorStateInfo currentAnimatorStateInfo4 = animator.GetCurrentAnimatorStateInfo(0);
				return ((AnimatorStateInfo)(ref currentAnimatorStateInfo4)).shortNameHash == Constants.Animation.AnimatorDeadStateHash;
			});
		}
		if (waitUntilAnimatorStateIsDie == null)
		{
			waitUntilAnimatorStateIsDie = new WaitUntil((Func<bool>)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				AnimatorStateInfo currentAnimatorStateInfo3 = animator.GetCurrentAnimatorStateInfo(0);
				return ((AnimatorStateInfo)(ref currentAnimatorStateInfo3)).shortNameHash == Constants.Animation.AnimatorDeathStateHash;
			});
		}
		if (waitUntilAnimatorStateIsIdle == null)
		{
			waitUntilAnimatorStateIsIdle = new WaitUntil((Func<bool>)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				AnimatorStateInfo currentAnimatorStateInfo2 = animator.GetCurrentAnimatorStateInfo(0);
				return ((AnimatorStateInfo)(ref currentAnimatorStateInfo2)).shortNameHash == Constants.Animation.AnimatorIdleStateHash;
			});
		}
		if (waitUntilAnimatorStateIsTakeDamage == null)
		{
			waitUntilAnimatorStateIsTakeDamage = new WaitUntil((Func<bool>)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				return ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).shortNameHash == Constants.Animation.AnimatorTakeDamageStateHash;
			});
		}
		return true;
	}

	protected virtual void InitAndStartAnimations(bool playSpawnAnim)
	{
		AreAnimationsInitialized = InitAnimations();
	}

	protected virtual void InitHud()
	{
		UnitHUD = Object.Instantiate<UnitHUD>(hudPrefab, PlayableUnitManager.UnitHudsTransform);
	}

	protected virtual void OnEnable()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		startZ = ((Component)this).transform.position.z;
		InitHud();
		IsTakingDamage = false;
		animator.keepAnimatorControllerStateOnDisable = true;
		if ((Object)(object)frontRendererEventsListener != (Object)null)
		{
			frontRendererEventsListener.OnSpriteVisibilityToggle += OnSpriteVisibilityToggle;
		}
		if ((Object)(object)backRendererEventsListener != (Object)null)
		{
			backRendererEventsListener.OnSpriteVisibilityToggle += OnSpriteVisibilityToggle;
		}
	}

	protected virtual void OnDisable()
	{
		HandleHudOnDisable();
		displaySkillEffectsCoroutine = null;
		DieAnimationIsFinished = false;
		Animator obj = animator;
		if (obj != null)
		{
			obj.ResetTrigger("Die");
		}
		if ((Object)(object)frontRendererEventsListener != (Object)null)
		{
			frontRendererEventsListener.OnSpriteVisibilityToggle -= OnSpriteVisibilityToggle;
		}
		if ((Object)(object)backRendererEventsListener != (Object)null)
		{
			backRendererEventsListener.OnSpriteVisibilityToggle -= OnSpriteVisibilityToggle;
		}
		visibleSpritesCount = 0;
	}

	protected virtual IEnumerator PlayDieAnimCoroutine()
	{
		((Behaviour)animator).enabled = true;
		animator.SetTrigger("Die");
		Unit.IsDying = true;
		yield return waitUntilAnimatorStateIsDie;
		yield return waitUntilAnimatorStateIsDead;
		Unit.IsDying = false;
		DieAnimationIsFinished = true;
	}

	protected void SetOrientation(bool front, bool forceRefresh = false)
	{
		if (orientationIsFront != front || forceRefresh)
		{
			orientationIsFront = front;
			bodyFrontContainer.SetActive(orientationIsFront);
			animator.SetLayerWeight(frontLayerIndex, (float)(orientationIsFront ? 1 : 0));
			bodyBackContainer.SetActive(!orientationIsFront);
			animator.SetLayerWeight(backLayerIndex, (float)((!orientationIsFront) ? 1 : 0));
		}
	}

	private void Awake()
	{
		animator.GetBehaviour<UnitStateMachine>()?.Init(this);
	}

	private IEnumerator DisplaySkillEffectsCoroutine(float delay)
	{
		if (SkillEffectDisplays.Count == 0)
		{
			displaySkillEffectsCoroutine = null;
			yield break;
		}
		int skillEffectDisplaysCount = SkillEffectDisplays.Count;
		if (delay > 0f)
		{
			yield return SharedYields.WaitForSeconds(delay);
		}
		while (skillEffectDisplaysCount > 0)
		{
			skillEffectDisplaysCount--;
			IDisplayableEffect displayableEffect = SkillEffectDisplays[0];
			SkillEffectDisplays.Remove(displayableEffect);
			yield return displayableEffect.Display();
		}
		displaySkillEffectsCoroutine = null;
	}

	private void HandleHudOnDisable()
	{
		if ((Object)(object)UnitHUD != (Object)null)
		{
			if ((Object)(object)SingletonBehaviour<ObjectPooler>.Instance != (Object)null)
			{
				UnitHUD.ReleaseSkillTargeting();
			}
			DisableHUD();
			UnitHUD = null;
		}
	}

	private IEnumerator MoveUnitCoroutine(List<Tile> path, bool followPathOrientation = true, float moveSpeed = -1f, float delay = 0f, bool isMovementInstant = false)
	{
		if (path == null || path.Count == 0)
		{
			yield break;
		}
		UnitHUD.ToggleFollowElement(toggle: true);
		bool isMisty = false;
		ILightFogSupplier lightFogSupplier = null;
		if (Unit is EnemyUnit enemyUnit && enemyUnit.HasLightFogSupplier(out lightFogSupplier))
		{
			lightFogSupplier.IsLightFogSupplierMoving = true;
			isMisty = true;
			lightFogSupplier.LightFogSupplierMoveDatas.StartTile = path[0];
			lightFogSupplier.LightFogSupplierMoveDatas.DestinationTile = path[path.Count - 1];
		}
		bool isPlayableUnit = Unit is PlayableUnit;
		if (delay > 0f)
		{
			yield return SharedYields.WaitForSeconds(delay);
		}
		if (moveSpeed == -1f)
		{
			moveSpeed = MoveSpeed;
		}
		bool previousTileHadAnyFog = path[0].HasAnyFog;
		int wayPointIndex = 1;
		while (wayPointIndex < path.Count)
		{
			Tile tile2 = path[wayPointIndex - 1];
			Tile tile3 = path[wayPointIndex];
			if (followPathOrientation)
			{
				Unit.UnitController.LookAt(tile3, tile2);
			}
			if (this is EnemyUnitView enemyUnitView && tile3.HasAnyFog != previousTileHadAnyFog)
			{
				enemyUnitView.RefreshMaterial();
				enemyUnitView.RefreshStatus();
				enemyUnitView.RefreshInjuryStage();
			}
			previousTileHadAnyFog = tile3.HasAnyFog;
			if ((tile3.Unit == null || tile3.Unit == Unit) && tile3.Building != null && tile3.Building.IsGate)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ForceOpenGate(tile3.Building, tile3.Building.OriginTile);
			}
			if (tile2.Unit == null && tile2.Building != null && tile2.Building.IsGate)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuilding(tile2.Building, tile2.Building.OriginTile);
			}
			if (isPlayableUnit)
			{
				repeledTilesForPreviousTile = FogManager.GetLightFogRepelTiles(tile2);
				repeledTilesForCurrentTile = FogManager.GetLightFogRepelTiles(tile3);
				List<Tile> tiles = repeledTilesForPreviousTile.Except(repeledTilesForCurrentTile).ToList();
				Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> tilesToUpdateByLightFogMode = FogController.ToggleLightFogTiles((from tile in repeledTilesForCurrentTile.Except(repeledTilesForPreviousTile).ToList()
					where tile.HasLightFogOn
					select tile).ToList());
				Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> tilesToUpdateByLightFogMode2 = FogController.ToggleLightFogTiles(tiles);
				FogController.SetLightFogTilesFromDictionnary(tilesToUpdateByLightFogMode, FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration, instant: false, independently: true);
				FogController.SetLightFogTilesFromDictionnary(tilesToUpdateByLightFogMode2, FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration, instant: false, independently: true);
			}
			DictionaryExtensions.GetValueOrDefault<E_EffectTime, Action<PerkDataContainer>>(unit.Events, E_EffectTime.OnTileCrossed)?.Invoke(null);
			Vector3 targetPosition = ((GridLayout)TileMapView.BuildingTilemap).CellToWorld(new Vector3Int(tile3.X, tile3.Y, 0)) + Vector3.forward * startZ;
			((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, ((Component)this).transform.position.y, targetPosition.z);
			Vector3 positionBuffer = ((Component)this).transform.position;
			if (isMisty)
			{
				(lightFogSupplier as EnemyMistyAffix)?.EnemyMistyAffixController.TriggerLightFogDuringMovement(tile2, tile3);
			}
			do
			{
				positionBuffer = Vector3.MoveTowards(positionBuffer, targetPosition, moveSpeed * Time.deltaTime);
				((Component)this).transform.position = positionBuffer;
				yield return SharedYields.WaitForEndOfFrame;
			}
			while (!TPHelpers.IsApproxEqualVect3(((Component)this).transform.position, targetPosition, 0.01f));
			if (path[path.Count - 1].Building != null)
			{
				((Component)this).transform.position = targetPosition - new Vector3(0.01f, 0.01f, 0.01f);
			}
			else
			{
				((Component)this).transform.position = targetPosition;
			}
			int num = wayPointIndex + 1;
			wayPointIndex = num;
		}
		if (isMisty)
		{
			lightFogSupplier.IsLightFogSupplierMoving = false;
			lightFogSupplier.LightFogSupplierMoveDatas.CurrentTile = null;
			lightFogSupplier.LightFogSupplierMoveDatas.StartTile = null;
			lightFogSupplier.LightFogSupplierMoveDatas.DestinationTile = null;
		}
		UnitHUD.ToggleFollowElement(toggle: false);
		DictionaryExtensions.GetValueOrDefault<E_EffectTime, Action<PerkDataContainer>>(unit.Events, E_EffectTime.OnMovementEnd)?.Invoke(null);
	}

	private void OnSpriteVisibilityToggle(bool toggle)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (toggle)
		{
			if (++visibleSpritesCount >= 1)
			{
				((Behaviour)animator).enabled = true;
			}
		}
		else if (--visibleSpritesCount == 0)
		{
			Animator obj = animator;
			int enabled;
			if (!Unit.IsExecutingSkill)
			{
				AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				enabled = ((((AnimatorStateInfo)(ref currentAnimatorStateInfo)).shortNameHash != Constants.Animation.AnimatorIdleStateHash) ? 1 : 0);
			}
			else
			{
				enabled = 1;
			}
			((Behaviour)obj).enabled = (byte)enabled != 0;
		}
	}

	private IEnumerator PlaySkillCastAnimCoroutine(SkillActionExecution skillExecution)
	{
		TheLastStand.Model.Skill.Skill skill = skillExecution.Skill;
		if (skill.SkillDefinition.SkillCastFxDefinition.CasterAnimDef == null || string.IsNullOrEmpty(skill.SkillDefinition.SkillCastFxDefinition.CasterAnimDef.Path))
		{
			yield break;
		}
		string text = "Animation/Caster Anims/" + skill.SkillDefinition.SkillCastFxDefinition.CasterAnimDef.Path;
		AnimationClip val = ResourcePooler.LoadOnce<AnimationClip>(text + "Front", false);
		animatorOverrideController[Constants.Animation.SkillCastDefaultAnimClipFrontName] = val;
		val = ResourcePooler.LoadOnce<AnimationClip>(text + "Back", false);
		animatorOverrideController[Constants.Animation.SkillCastDefaultAnimClipBackName] = val;
		float num = skill.SkillDefinition.SkillCastFxDefinition.CasterAnimDef.Delay.EvalToFloat((InterpreterContext)(object)skillExecution.CastFx.CastFXInterpreterContext);
		if (num > 0f)
		{
			yield return SharedYields.WaitForSeconds(num);
		}
		((Behaviour)animator).enabled = true;
		animator.SetTrigger("Cast Skill");
		yield return waitUntilAnimatorStateIsCastSkill;
		if (Unit is EnemyUnit enemyUnit && enemyUnit.Id == "SpawnerCocoon" && skill.SkillAction is SpawnSkillAction)
		{
			int num2 = enemyUnit.CurrentVariantIndex + 1;
			if (((enemyUnit.EnemyUnitTemplateDefinition.VisualEvolutions.Count > num2) ? enemyUnit.EnemyUnitTemplateDefinition.VisualEvolutions[num2] : string.Empty) != string.Empty)
			{
				animator.SetTrigger("LevelUp");
			}
		}
		else
		{
			yield return waitUntilAnimatorStateIsIdle;
		}
		if (unit.OriginTile.Building != null && !unit.OriginTile.Building.IsWatchtower)
		{
			((Component)this).transform.position = ((Component)Unit.OriginTile.TileView).transform.position;
		}
		if (unit.OriginTile.Building != null)
		{
			unit.UnitView.UpdatePosition();
		}
	}

	private IEnumerator PlayTakeDamageAnimCoroutine()
	{
		((Behaviour)animator).enabled = true;
		animator.SetTrigger("Take Damage");
		yield return waitUntilAnimatorStateIsTakeDamage;
		yield return waitUntilAnimatorStateIsIdle;
		takeHitAnimCoroutine = null;
		IsTakingDamage = false;
	}
}
