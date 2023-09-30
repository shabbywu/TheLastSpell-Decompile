using System;
using System.Collections;
using Sirenix.OdinInspector;
using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public abstract class UnitHUD : SerializedMonoBehaviour, IDamageableHUD
{
	[SerializeField]
	[FormerlySerializedAs("NoArmorVerticalOffset")]
	private int noArmorVerticalOffset = -5;

	[SerializeField]
	private RectTransform bgRectTransform;

	[SerializeField]
	private Canvas bgHighlightCanvas;

	[SerializeField]
	[FormerlySerializedAs("healthValueRectTransform")]
	private RectTransform offsetContainerRectTransform;

	[SerializeField]
	protected UnitHealthStatGaugeDisplay healthDisplay;

	[SerializeField]
	protected UnitStatGaugeDisplay armorDisplay;

	[SerializeField]
	private RectTransform skillTargetingAnchor;

	[SerializeField]
	private SkillActionEstimationView attackEstimationDisplay;

	[SerializeField]
	protected Image highlightImage;

	[SerializeField]
	protected Canvas highlightCanvas;

	[SerializeField]
	protected Canvas hudCanvas;

	[SerializeField]
	protected Canvas gaugesCanvas;

	[SerializeField]
	private FollowElement followElement;

	[SerializeField]
	protected Image healthGaugeImage;

	[SerializeField]
	protected Sprite healthGaugeBaseSprite;

	[SerializeField]
	protected DataSpriteTable injuryStageSpritesTable;

	[SerializeField]
	protected Image injuryStageSprite;

	[SerializeField]
	protected GameObject contagionStatusGo;

	[SerializeField]
	protected GameObject buffStatusGo;

	[SerializeField]
	protected GameObject debuffStatusGo;

	[SerializeField]
	protected GameObject chargeStatusGo;

	[SerializeField]
	protected GameObject immunityStatusGo;

	[SerializeField]
	protected GameObject poisonDeathFeedback;

	private TheLastStand.Model.Unit.Unit unit;

	private float bgOriginalVerticalSize;

	private bool hasStatus;

	private float healthValueOriginalVerticalPos;

	private SkillTargetingMark skillTargetingMark;

	private int childrenViewsActive;

	private int followElementToggleBuffer;

	public SkillActionEstimationView AttackEstimationDisplay => attackEstimationDisplay;

	public Canvas BgHighlightCanvas => bgHighlightCanvas;

	public virtual bool HealthDisplayed
	{
		get
		{
			return ((Behaviour)gaugesCanvas).enabled;
		}
		set
		{
			bool flag = !PlayableUnitManager.DebugDisableHealthDisplay && value;
			if (((Behaviour)gaugesCanvas).enabled != flag)
			{
				((Behaviour)gaugesCanvas).enabled = flag;
				healthDisplay.ToggleSliders(flag);
				armorDisplay.ToggleSliders(flag);
				OnChildViewToggled(flag);
			}
		}
	}

	public abstract bool Highlight { get; set; }

	public bool IsAnimating
	{
		get
		{
			UnitHealthStatGaugeDisplay unitHealthStatGaugeDisplay = healthDisplay;
			if (unitHealthStatGaugeDisplay == null || !unitHealthStatGaugeDisplay.IsAnimating)
			{
				return armorDisplay?.IsAnimating ?? false;
			}
			return true;
		}
	}

	public Transform Transform => ((Component)this).transform;

	public TheLastStand.Model.Unit.Unit Unit
	{
		get
		{
			return unit;
		}
		set
		{
			if (value != null)
			{
				unit = value;
				((Object)this).name = unit.UniqueIdentifier + " HUD";
				armorDisplay.Damageable = unit;
				healthDisplay.Damageable = unit;
				RefreshArmor();
				RefreshHealth();
				RefreshStatuses();
				RefreshInjuryStage();
				if (this is PlayableUnitHUD playableUnitHUD)
				{
					playableUnitHUD.RefreshMana();
				}
				if ((Object)(object)followElement != (Object)null)
				{
					followElement.ChangeTarget(Unit.UnitView.HudFollowTarget);
				}
				Highlight = false;
			}
			else
			{
				Highlight = false;
				healthDisplay.ResetTweeners();
				armorDisplay.ResetTweeners();
				this.AnimatedDisplayFinishEvent = null;
				DisplayIconFeedback(show: false);
				unit = null;
				armorDisplay.Damageable = null;
				healthDisplay.Damageable = null;
			}
		}
	}

	protected bool DisplayArmor
	{
		get
		{
			if ((Object)(object)armorDisplay != (Object)null)
			{
				return ((Component)armorDisplay).gameObject.activeSelf;
			}
			return false;
		}
		set
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			if (value != DisplayArmor)
			{
				((Component)armorDisplay).gameObject.SetActive(value);
				if ((Object)(object)bgRectTransform != (Object)null)
				{
					Vector2 sizeDelta = bgRectTransform.sizeDelta;
					sizeDelta.y = (value ? bgOriginalVerticalSize : (bgOriginalVerticalSize + (float)noArmorVerticalOffset));
					bgRectTransform.sizeDelta = sizeDelta;
				}
				if ((Object)(object)offsetContainerRectTransform != (Object)null)
				{
					Vector2 anchoredPosition = offsetContainerRectTransform.anchoredPosition;
					anchoredPosition.y = (value ? healthValueOriginalVerticalPos : (healthValueOriginalVerticalPos + (float)noArmorVerticalOffset));
					offsetContainerRectTransform.anchoredPosition = anchoredPosition;
				}
			}
		}
	}

	protected virtual bool ShouldArmorBeDisplayed => unit.ArmorTotal > 0f;

	protected virtual bool ShouldHealthBeDisplayed
	{
		get
		{
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying && !Unit.OriginTile.HasFog)
			{
				if (!(unit.Armor < unit.ArmorTotal) && !(unit.Health < unit.HealthTotal) && !unit.IsPoisoned)
				{
					return unit.IsStunned;
				}
				return true;
			}
			return false;
		}
	}

	protected virtual bool ShouldInjuryStageBeDisplayed
	{
		get
		{
			if (!Unit.OriginTile.HasFog)
			{
				return Unit.UnitStatsController.UnitStats.InjuryStage > 0;
			}
			return false;
		}
	}

	public event Action AnimatedDisplayFinishEvent;

	public void DisplayArmorIfNeeded()
	{
		DisplayArmor = ShouldArmorBeDisplayed;
	}

	public void DisplayHealthIfNeeded()
	{
		HealthDisplayed = ShouldHealthBeDisplayed;
	}

	public virtual void DisplayIconAndTileFeedback(bool show)
	{
		if (!show || (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits && !Unit.IsDead))
		{
			show &= Unit.WillDieByPoison;
			if (poisonDeathFeedback.activeSelf != show)
			{
				poisonDeathFeedback.SetActive(show);
				TileMapView.SetTiles(TileMapView.UnitFeedbackTilemap, Unit.OccupiedTiles, show ? "View/Tiles/Feedbacks/PoisonDeath" : null);
			}
		}
	}

	public void ToggleSkillTargeting(bool show)
	{
		if (show)
		{
			Tile tile = (((Object)(object)GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered() != (Object)null) ? GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered().PlayableUnit.OriginTile : TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
			OnSkillTargetHover(tile == Unit.OriginTile);
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
			ReleaseSkillTargeting();
			skillTargetingMark = null;
		}
	}

	public virtual void DisplayIconFeedback(bool show = true)
	{
		poisonDeathFeedback.SetActive(show && Unit.WillDieByPoison);
	}

	[ContextMenu("Force Display HUD")]
	public virtual void ForceDisplayHUD()
	{
		RefreshArmor();
		RefreshHealth();
		RefreshStatuses();
		RefreshInjuryStage();
		DisplayIconFeedback();
		DisplayIconAndTileFeedback(show: true);
	}

	public virtual void ForceHideHUD()
	{
		HealthDisplayed = false;
		((Component)injuryStageSprite).gameObject.SetActive(false);
		buffStatusGo.SetActive(false);
		debuffStatusGo.SetActive(false);
		chargeStatusGo.SetActive(false);
		immunityStatusGo.SetActive(false);
		poisonDeathFeedback.SetActive(false);
	}

	public virtual void OnSkillTargetHover(bool hover)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)skillTargetingMark == (Object)null)
		{
			skillTargetingMark = ObjectPooler.GetPooledComponent<SkillTargetingMark>("SkillTargetingMarkUI", SkillManager.SkillTargetingMarkUIPrefab, (Transform)(object)skillTargetingAnchor, false);
			((Component)skillTargetingMark).transform.localPosition = Vector3.zero;
		}
		if (!((Component)skillTargetingMark).gameObject.activeInHierarchy)
		{
			((Component)skillTargetingMark).gameObject.SetActive(true);
		}
		skillTargetingMark.SetHoverAnimatorState(hover);
	}

	public void PlayArmorGainAnim(float armorGain, float armorAfterGain)
	{
		((MonoBehaviour)this).StartCoroutine(GaugeHealArmorDisplayCoroutine(armorGain, armorAfterGain));
	}

	public void PlayDamageAnim(AttackSkillActionExecutionTileData attackData)
	{
		((MonoBehaviour)this).StartCoroutine(GaugeDamageDisplayCoroutine(attackData));
	}

	public void PlayHealthGainAnim(float healthGain, float healthAfterGain)
	{
		((MonoBehaviour)this).StartCoroutine(GaugeHealDisplayCoroutine(healthGain, healthAfterGain));
	}

	public void PlayHealthLossAnim(float healthLoss, float healthAfterLoss)
	{
		float targetNormalizedValue = healthAfterLoss / Unit.HealthTotal;
		((MonoBehaviour)this).StartCoroutine(healthDisplay.DecreaseDisplayCoroutine(targetNormalizedValue));
	}

	public void RefreshArmor()
	{
		if (Unit != null)
		{
			if (ShouldArmorBeDisplayed)
			{
				armorDisplay.RefreshStatInstantly();
			}
			DisplayArmorIfNeeded();
			DisplayHealthIfNeeded();
		}
	}

	public void RefreshHealth()
	{
		if (Unit != null)
		{
			healthDisplay.RefreshStatInstantly();
			DisplayHealthIfNeeded();
		}
	}

	public void RefreshInjuryStage()
	{
		if (Unit != null)
		{
			if (!ShouldInjuryStageBeDisplayed)
			{
				((Component)injuryStageSprite).gameObject.SetActive(false);
				return;
			}
			((Component)injuryStageSprite).gameObject.SetActive(true);
			injuryStageSprite.sprite = injuryStageSpritesTable.GetSpriteAt(Unit.UnitStatsController.UnitStats.InjuryStage - 1);
		}
	}

	public void RefreshPositionInstantly()
	{
		followElement.AutoMove();
	}

	public virtual void RefreshStat(UnitStatDefinition.E_Stat stat)
	{
		switch (stat)
		{
		case UnitStatDefinition.E_Stat.Armor:
			RefreshArmor();
			break;
		case UnitStatDefinition.E_Stat.Health:
			RefreshHealth();
			break;
		}
	}

	public virtual void RefreshStatuses()
	{
		if (Unit != null)
		{
			bool flag = !Unit.OriginTile.HasFog && (Unit.IsBuffed || Unit.IsDebuffed || Unit.IsContagious || Unit.IsCharged || Unit.IsImmune);
			if (flag != hasStatus)
			{
				hasStatus = flag;
				OnChildViewToggled(flag);
			}
			if ((Object)(object)buffStatusGo != (Object)null)
			{
				buffStatusGo.SetActive(!Unit.OriginTile.HasFog && Unit.IsBuffed);
			}
			if ((Object)(object)debuffStatusGo != (Object)null)
			{
				debuffStatusGo.SetActive(!Unit.OriginTile.HasFog && Unit.IsDebuffed);
			}
			if ((Object)(object)contagionStatusGo != (Object)null)
			{
				contagionStatusGo.SetActive(!Unit.OriginTile.HasFog && Unit.IsContagious);
			}
			if ((Object)(object)chargeStatusGo != (Object)null)
			{
				chargeStatusGo.SetActive(!Unit.OriginTile.HasFog && Unit.IsCharged);
			}
			if ((Object)(object)immunityStatusGo != (Object)null)
			{
				immunityStatusGo.SetActive(!Unit.OriginTile.HasFog && Unit.IsImmune);
			}
			DisplayHealthIfNeeded();
		}
	}

	public void ReleaseSkillTargeting()
	{
		if (!((Object)(object)skillTargetingMark == (Object)null))
		{
			ObjectPooler.SetPoolAsParent(((Component)skillTargetingMark).gameObject, "SkillTargetingMarkUI");
		}
	}

	public void ToggleFollowElement(bool toggle)
	{
		if (toggle)
		{
			if (++followElementToggleBuffer == 1)
			{
				((Behaviour)followElement).enabled = true;
			}
		}
		else if (--followElementToggleBuffer == 0)
		{
			((Behaviour)followElement).enabled = false;
		}
	}

	protected virtual void OnChildViewToggled(bool toggle)
	{
		if (toggle)
		{
			if (++childrenViewsActive == 1)
			{
				ToggleGlobalHud(toggle: true);
			}
		}
		else if (--childrenViewsActive == 0)
		{
			ToggleGlobalHud(toggle: false);
		}
	}

	private void Awake()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hudCanvas == (Object)null)
		{
			Debug.LogWarning((object)"Canvas has not been referenced and will be fetched using <b>GetComponent</b> method, which should be avoided.");
			hudCanvas = ((Component)this).GetComponent<Canvas>();
		}
		hudCanvas.worldCamera = ACameraView.MainCam;
		if ((Object)(object)healthDisplay != (Object)null)
		{
			healthDisplay.AnimatedDisplayFinishEvent += OnAnimatedDisplayFinished;
		}
		if ((Object)(object)armorDisplay != (Object)null)
		{
			armorDisplay.AnimatedDisplayFinishEvent += OnAnimatedDisplayFinished;
		}
		if ((Object)(object)bgRectTransform != (Object)null)
		{
			bgOriginalVerticalSize = bgRectTransform.sizeDelta.y;
		}
		if ((Object)(object)offsetContainerRectTransform != (Object)null)
		{
			healthValueOriginalVerticalPos = offsetContainerRectTransform.anchoredPosition.y;
		}
		if ((Object)(object)attackEstimationDisplay != (Object)null)
		{
			attackEstimationDisplay.OnViewToggled += OnChildViewToggled;
		}
	}

	private IEnumerator GaugeDamageDisplayCoroutine(AttackSkillActionExecutionTileData attackData)
	{
		if (!(attackData.TotalDamage <= 0f))
		{
			HealthDisplayed = !Unit.OriginTile.HasFog;
			if (attackData.ArmorDamage > 0f)
			{
				float targetNormalizedValue = attackData.TargetRemainingArmor / attackData.TargetArmorTotal;
				((MonoBehaviour)this).StartCoroutine(armorDisplay.DecreaseDisplayCoroutine(targetNormalizedValue));
			}
			if (attackData.HealthDamage > 0f)
			{
				float targetNormalizedValue2 = attackData.TargetRemainingHealth / attackData.TargetHealthTotal;
				((MonoBehaviour)this).StartCoroutine(healthDisplay.DecreaseDisplayCoroutine(targetNormalizedValue2));
			}
		}
		yield break;
	}

	private IEnumerator GaugeHealArmorDisplayCoroutine(float healAmount, float healthAfterHeal)
	{
		if (!(healAmount <= 0f))
		{
			HealthDisplayed = !Unit.OriginTile.HasFog;
			float targetNormalizedValue = healthAfterHeal / Unit.ArmorTotal;
			yield return armorDisplay.IncreaseDisplayCoroutine(targetNormalizedValue);
		}
	}

	private IEnumerator GaugeHealDisplayCoroutine(float healAmount, float healthAfterHeal)
	{
		if (!(healAmount <= 0f))
		{
			HealthDisplayed = !Unit.OriginTile.HasFog;
			float targetNormalizedValue = healthAfterHeal / Unit.HealthTotal;
			yield return healthDisplay.IncreaseDisplayCoroutine(targetNormalizedValue);
		}
	}

	private void OnAnimatedDisplayFinished()
	{
		if (((Object)(object)healthDisplay == (Object)null || !healthDisplay.IsAnimating) && ((Object)(object)armorDisplay == (Object)null || !armorDisplay.IsAnimating))
		{
			this.AnimatedDisplayFinishEvent?.Invoke();
		}
		if (!Unit.IsDead && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			DisplayIconAndTileFeedback(show: true);
		}
		DisplayHealthIfNeeded();
		DisplayArmorIfNeeded();
	}

	private void OnDestroy()
	{
		if ((Object)(object)healthDisplay != (Object)null)
		{
			healthDisplay.AnimatedDisplayFinishEvent -= OnAnimatedDisplayFinished;
		}
		if ((Object)(object)armorDisplay != (Object)null)
		{
			armorDisplay.AnimatedDisplayFinishEvent -= OnAnimatedDisplayFinished;
		}
		if ((Object)(object)attackEstimationDisplay != (Object)null)
		{
			attackEstimationDisplay.OnViewToggled -= OnChildViewToggled;
		}
	}

	private void ToggleGlobalHud(bool toggle)
	{
		((Behaviour)hudCanvas).enabled = toggle;
	}

	[ContextMenu("Display Armor")]
	public void ForceDisplayArmor()
	{
		((Component)armorDisplay).gameObject.SetActive(true);
		DisplayArmor = true;
	}
}
