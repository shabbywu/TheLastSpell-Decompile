using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class UnitStatGaugeDisplay : MonoBehaviour
{
	[SerializeField]
	private UnitStatDefinition.E_Stat statToUse = UnitStatDefinition.E_Stat.Health;

	[SerializeField]
	private Gradient decreaseGradient;

	[SerializeField]
	protected float decreasePhaseOneDuration = 0.1f;

	[SerializeField]
	protected Ease decreasePhaseOneEasing = (Ease)14;

	[SerializeField]
	[Tooltip("Phase 2 will start [this duration] after the end of Phase 1")]
	private float decreaseDelayBetweenPhases = 0.2f;

	[SerializeField]
	private float decreasePhaseTwoDuration = 0.3f;

	[SerializeField]
	private Ease decreasePhaseTwoEasing = (Ease)14;

	[SerializeField]
	private Gradient increaseGradient;

	[SerializeField]
	private float increasePhaseOneDuration = 0.1f;

	[SerializeField]
	private Ease increasePhaseOneEasing = (Ease)14;

	[SerializeField]
	[Tooltip("Phase 2 will start [this duration] after the end of Phase 1")]
	private float increaseDelayBetweenPhases = 0.2f;

	[SerializeField]
	protected float increasePhaseTwoDuration = 0.3f;

	[SerializeField]
	protected Ease increasePhaseTwoEasing = (Ease)14;

	[SerializeField]
	private RectTransform markersContainer;

	[SerializeField]
	private TextMeshProUGUI healthValueLbl;

	[SerializeField]
	private bool onlyDisplayValueDuringPhaseTwo;

	[SerializeField]
	private float hideValueLabelDelay = 0.3f;

	[SerializeField]
	protected Slider statGauge;

	[SerializeField]
	protected Slider hitGauge;

	protected GaugeMarkersDisplayer gaugeMarkersDisplayer;

	private IDamageable damageable;

	private TheLastStand.Model.Building.Building building;

	private float currentMaxValue = -1f;

	private Image hitGaugeImage;

	private float decreasePhaseTwoDelayTimer = -1f;

	private Tweener decreaseDisplayPhaseOneTweener;

	private Tween decreaseColorPhaseOneTweener;

	private Tweener decreaseDisplayPhaseTwoTweener;

	private Coroutine decreaseDisplayPhaseTwoCoroutine;

	private Coroutine decreasePhaseTwoDelayCoroutine;

	private float increasePhaseTwoDelayTimer = -1f;

	private Tweener increaseDisplayPhaseOneTweener;

	private Tween increaseColorPhaseOneTweener;

	private Tweener increaseDisplayPhaseTwoTweener;

	private Coroutine increaseDisplayPhaseTwoCoroutine;

	private Coroutine increasePhaseTwoDelayCoroutine;

	private Coroutine hideValueLabelCoroutine;

	public TheLastStand.Model.Building.Building Building
	{
		get
		{
			return building;
		}
		set
		{
			if (value != null)
			{
				building = value;
				RefreshStatInstantly();
			}
		}
	}

	public IDamageable Damageable
	{
		get
		{
			return damageable;
		}
		set
		{
			if ((Object)(object)healthValueLbl != (Object)null)
			{
				((Behaviour)healthValueLbl).enabled = false;
			}
			damageable = value;
			if (value != null)
			{
				RefreshStatInstantly();
			}
		}
	}

	public bool IsAnimating
	{
		get
		{
			if (!IsAnimatingDamage)
			{
				return IsAnimatingHeal;
			}
			return true;
		}
	}

	public bool IsAnimatingDamage
	{
		get
		{
			if (decreaseDisplayPhaseOneTweener == null && decreaseDisplayPhaseTwoCoroutine == null)
			{
				return decreasePhaseTwoDelayCoroutine != null;
			}
			return true;
		}
	}

	public bool IsAnimatingHeal
	{
		get
		{
			if (increaseDisplayPhaseOneTweener == null && increaseDisplayPhaseTwoCoroutine == null)
			{
				return increasePhaseTwoDelayCoroutine != null;
			}
			return true;
		}
	}

	public event Action AnimatedDisplayFinishEvent;

	public void CompleteCurrentAnimation()
	{
		if (IsAnimatingDamage)
		{
			Tween obj = decreaseColorPhaseOneTweener;
			if (obj != null)
			{
				TweenExtensions.Complete(obj);
			}
			Tweener obj2 = decreaseDisplayPhaseOneTweener;
			if (obj2 != null)
			{
				TweenExtensions.Complete((Tween)(object)obj2);
			}
			Tweener obj3 = decreaseDisplayPhaseTwoTweener;
			if (obj3 != null)
			{
				TweenExtensions.Complete((Tween)(object)obj3);
			}
		}
		if (IsAnimatingHeal)
		{
			Tween obj4 = increaseColorPhaseOneTweener;
			if (obj4 != null)
			{
				TweenExtensions.Complete(obj4);
			}
			Tweener obj5 = increaseDisplayPhaseOneTweener;
			if (obj5 != null)
			{
				TweenExtensions.Complete((Tween)(object)obj5);
			}
			Tweener obj6 = increaseDisplayPhaseTwoTweener;
			if (obj6 != null)
			{
				TweenExtensions.Complete((Tween)(object)obj6);
			}
		}
		if (IsAnimating)
		{
			((MonoBehaviour)this).StopAllCoroutines();
		}
	}

	public IEnumerator DecreaseDisplayCoroutine(float targetNormalizedValue)
	{
		if ((Object)(object)statGauge == (Object)null)
		{
			yield break;
		}
		if (IsAnimatingHeal)
		{
			yield return (object)new WaitUntil((Func<bool>)(() => !IsAnimatingHeal));
		}
		decreasePhaseTwoDelayCoroutine = ((MonoBehaviour)this).StartCoroutine(DecreasePhaseTwoDelayCoroutine(targetNormalizedValue));
		yield return ((MonoBehaviour)this).StartCoroutine(DecreasePhaseOneDisplayCoroutine(targetNormalizedValue));
	}

	public IEnumerator IncreaseDisplayCoroutine(float targetNormalizedValue)
	{
		if ((Object)(object)statGauge == (Object)null)
		{
			yield break;
		}
		if (IsAnimatingDamage)
		{
			yield return (object)new WaitUntil((Func<bool>)(() => !IsAnimatingDamage));
		}
		increasePhaseTwoDelayCoroutine = ((MonoBehaviour)this).StartCoroutine(IncreasePhaseTwoDelayCoroutine(targetNormalizedValue));
		yield return ((MonoBehaviour)this).StartCoroutine(IncreasePhaseOneDisplayCoroutine(targetNormalizedValue));
	}

	public virtual void RefreshStatInstantly(float statValue = -1f, float statMaxValue = -1f)
	{
		if (statValue == -1f)
		{
			statValue = GetReferenceValue();
		}
		if (statMaxValue == -1f)
		{
			statMaxValue = GetReferenceValue(useTotal: true);
		}
		if (statMaxValue != currentMaxValue)
		{
			gaugeMarkersDisplayer.RefreshMarkers(statMaxValue, statValue);
			currentMaxValue = statMaxValue;
		}
		float normalizedValue = statValue / statMaxValue;
		gaugeMarkersDisplayer.RefreshEnabledMarkers(normalizedValue);
		if ((Object)(object)statGauge != (Object)null)
		{
			statGauge.normalizedValue = normalizedValue;
		}
		if ((Object)(object)hitGauge != (Object)null)
		{
			hitGauge.normalizedValue = normalizedValue;
		}
		if ((Object)(object)healthValueLbl != (Object)null)
		{
			((TMP_Text)healthValueLbl).text = $"{(int)statValue}";
		}
	}

	public void ResetTweeners()
	{
		Tweener obj = decreaseDisplayPhaseOneTweener;
		if (obj != null)
		{
			TweenExtensions.Kill((Tween)(object)obj, false);
		}
		Tweener obj2 = decreaseDisplayPhaseTwoTweener;
		if (obj2 != null)
		{
			TweenExtensions.Kill((Tween)(object)obj2, false);
		}
		decreaseDisplayPhaseOneTweener = null;
		decreaseDisplayPhaseTwoTweener = null;
	}

	public virtual void ToggleSliders(bool toggle)
	{
		((Behaviour)hitGauge).enabled = toggle;
		((Behaviour)statGauge).enabled = toggle;
	}

	protected virtual IEnumerator DecreasePhaseOneDisplayCoroutine(float targetNormalizedValue, bool updateMarkers = true)
	{
		float normalizedValue = statGauge.normalizedValue;
		if (normalizedValue == targetNormalizedValue)
		{
			yield break;
		}
		if (decreaseDisplayPhaseOneTweener == null)
		{
			if ((Object)(object)healthValueLbl != (Object)null && onlyDisplayValueDuringPhaseTwo)
			{
				DisplayValueLabel();
			}
			decreaseDisplayPhaseOneTweener = (Tweener)(object)TweenSettingsExtensions.OnKill<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => normalizedValue), (DOSetter<float>)delegate(float v)
			{
				normalizedValue = v;
				if ((Object)(object)statGauge != (Object)null)
				{
					statGauge.normalizedValue = normalizedValue;
				}
				if (updateMarkers)
				{
					gaugeMarkersDisplayer.RefreshEnabledMarkers(normalizedValue);
				}
			}, targetNormalizedValue, decreasePhaseOneDuration).SetFullId<TweenerCore<float, float, FloatOptions>>("HealthDisplay DamageDisplayPhase1", (Component)(object)this), decreasePhaseOneEasing), (TweenCallback)delegate
			{
				decreaseDisplayPhaseOneTweener = null;
			});
			decreaseColorPhaseOneTweener = (Tween)(object)TweenSettingsExtensions.OnKill<Sequence>(DOTweenModuleUI.DOGradientColor(hitGaugeImage, decreaseGradient, decreasePhaseOneDuration + decreaseDelayBetweenPhases + decreasePhaseTwoDuration).SetFullId<Sequence>("HealthDisplay DamageColor", (Component)(object)this), (TweenCallback)delegate
			{
				decreaseColorPhaseOneTweener = null;
			});
		}
		else
		{
			CLoggerManager.Log((object)$"reuse tween phase1, changing value to : {targetNormalizedValue}", (Object)(object)this, (LogType)3, (CLogLevel)1, false, "StaticLog", false);
			decreaseDisplayPhaseOneTweener.ChangeEndValue((object)targetNormalizedValue, true);
			TweenExtensions.Restart(decreaseColorPhaseOneTweener, true, -1f);
		}
		yield return TweenExtensions.WaitForCompletion((Tween)(object)decreaseDisplayPhaseOneTweener);
	}

	protected virtual IEnumerator DecreasePhaseTwoDisplayCoroutine(float targetNormalizedValue)
	{
		float normalizedValue = hitGauge.normalizedValue;
		if (decreaseDisplayPhaseTwoTweener == null)
		{
			decreaseDisplayPhaseTwoTweener = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => normalizedValue), (DOSetter<float>)delegate(float v)
			{
				normalizedValue = v;
				if ((Object)(object)hitGauge != (Object)null)
				{
					hitGauge.normalizedValue = normalizedValue;
				}
				if ((Object)(object)healthValueLbl != (Object)null)
				{
					((TMP_Text)healthValueLbl).text = Mathf.RoundToInt(normalizedValue * GetReferenceValue(useTotal: true)).ToString();
				}
			}, targetNormalizedValue, decreasePhaseTwoDuration).SetFullId<TweenerCore<float, float, FloatOptions>>("UnitHUD DamageDisplayPhase2", (Component)(object)this), decreasePhaseTwoEasing), (TweenCallback)delegate
			{
				decreaseDisplayPhaseTwoTweener = null;
				decreaseDisplayPhaseTwoCoroutine = null;
				this.AnimatedDisplayFinishEvent?.Invoke();
				if ((Object)(object)healthValueLbl != (Object)null && onlyDisplayValueDuringPhaseTwo)
				{
					hideValueLabelCoroutine = ((MonoBehaviour)this).StartCoroutine(HideValueLabelDelayedCoroutine());
				}
			});
		}
		else
		{
			decreaseDisplayPhaseTwoTweener.ChangeEndValue((object)targetNormalizedValue, true);
		}
		yield return TweenExtensions.WaitForCompletion((Tween)(object)decreaseDisplayPhaseTwoTweener);
	}

	protected virtual IEnumerator IncreasePhaseOneDisplayCoroutine(float targetNormalizedValue)
	{
		float normalizedValue = hitGauge.normalizedValue;
		if (normalizedValue == targetNormalizedValue)
		{
			yield break;
		}
		if (increaseDisplayPhaseOneTweener == null)
		{
			if ((Object)(object)healthValueLbl != (Object)null && onlyDisplayValueDuringPhaseTwo)
			{
				DisplayValueLabel();
			}
			increaseDisplayPhaseOneTweener = (Tweener)(object)TweenSettingsExtensions.OnKill<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => normalizedValue), (DOSetter<float>)delegate(float v)
			{
				normalizedValue = v;
				if ((Object)(object)hitGauge != (Object)null)
				{
					hitGauge.normalizedValue = normalizedValue;
				}
			}, targetNormalizedValue, increasePhaseOneDuration).SetFullId<TweenerCore<float, float, FloatOptions>>("HealthDisplay HealDisplayPhase1", (Component)(object)this), increasePhaseOneEasing), (TweenCallback)delegate
			{
				increaseDisplayPhaseOneTweener = null;
			});
			increaseColorPhaseOneTweener = (Tween)(object)TweenSettingsExtensions.OnKill<Sequence>(TweenSettingsExtensions.SetId<Sequence>(DOTweenModuleUI.DOGradientColor(hitGaugeImage, increaseGradient, increasePhaseOneDuration + increaseDelayBetweenPhases + increasePhaseTwoDuration), "HealthDisplay HealColor"), (TweenCallback)delegate
			{
				increaseColorPhaseOneTweener = null;
			});
		}
		else
		{
			increaseDisplayPhaseOneTweener.ChangeEndValue((object)targetNormalizedValue, true);
			TweenExtensions.Restart(increaseColorPhaseOneTweener, true, -1f);
		}
		yield return TweenExtensions.WaitForCompletion((Tween)(object)increaseDisplayPhaseOneTweener);
	}

	protected virtual IEnumerator IncreasePhaseTwoDisplayCoroutine(float targetNormalizedValue, bool updateMarkers = true)
	{
		float normalizedValue = statGauge.normalizedValue;
		if (increaseDisplayPhaseTwoTweener == null)
		{
			increaseDisplayPhaseTwoTweener = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => normalizedValue), (DOSetter<float>)delegate(float v)
			{
				normalizedValue = v;
				if ((Object)(object)hitGauge != (Object)null)
				{
					statGauge.normalizedValue = normalizedValue;
				}
				if ((Object)(object)healthValueLbl != (Object)null)
				{
					((TMP_Text)healthValueLbl).text = Mathf.RoundToInt(normalizedValue * GetReferenceValue(useTotal: true)).ToString();
				}
				if (updateMarkers)
				{
					gaugeMarkersDisplayer.RefreshEnabledMarkers(normalizedValue);
				}
			}, targetNormalizedValue, increasePhaseTwoDuration).SetFullId<TweenerCore<float, float, FloatOptions>>("UnitHUD HealDisplayPhase2", (Component)(object)this), increasePhaseTwoEasing), (TweenCallback)delegate
			{
				increaseDisplayPhaseTwoTweener = null;
				increaseDisplayPhaseTwoCoroutine = null;
				this.AnimatedDisplayFinishEvent?.Invoke();
				if ((Object)(object)healthValueLbl != (Object)null && onlyDisplayValueDuringPhaseTwo)
				{
					hideValueLabelCoroutine = ((MonoBehaviour)this).StartCoroutine(HideValueLabelDelayedCoroutine());
				}
			});
		}
		else
		{
			increaseDisplayPhaseTwoTweener.ChangeEndValue((object)targetNormalizedValue, true);
		}
		yield return TweenExtensions.WaitForCompletion((Tween)(object)increaseDisplayPhaseTwoTweener);
	}

	private void Awake()
	{
		decreasePhaseTwoDelayTimer = decreasePhaseOneDuration + decreaseDelayBetweenPhases;
		increasePhaseTwoDelayTimer = increasePhaseOneDuration + increaseDelayBetweenPhases;
		if ((Object)(object)hitGauge != (Object)null)
		{
			hitGaugeImage = ((Component)hitGauge.fillRect).GetComponent<Image>();
		}
		if ((Object)(object)healthValueLbl != (Object)null && onlyDisplayValueDuringPhaseTwo)
		{
			((Behaviour)healthValueLbl).enabled = false;
		}
		gaugeMarkersDisplayer = new GaugeMarkersDisplayer(markersContainer);
	}

	private IEnumerator DecreasePhaseTwoDelayCoroutine(float targetNormalizedValue)
	{
		yield return SharedYields.WaitForSeconds(decreasePhaseTwoDelayTimer);
		decreaseDisplayPhaseTwoCoroutine = ((MonoBehaviour)this).StartCoroutine(DecreasePhaseTwoDisplayCoroutine(targetNormalizedValue));
		decreasePhaseTwoDelayCoroutine = null;
	}

	private void DisplayValueLabel()
	{
		((Behaviour)healthValueLbl).enabled = true;
		if (hideValueLabelCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(hideValueLabelCoroutine);
			hideValueLabelCoroutine = null;
		}
	}

	private float GetReferenceValue(bool useTotal = false)
	{
		switch (statToUse)
		{
		case UnitStatDefinition.E_Stat.Armor:
			if (!useTotal)
			{
				return damageable.Armor;
			}
			return damageable.ArmorTotal;
		case UnitStatDefinition.E_Stat.Health:
			if (damageable is DamageableModule { BuildingParent: MagicCircle buildingParent })
			{
				if (!useTotal)
				{
					return damageable.Health;
				}
				return buildingParent.CurrentHealthTotal;
			}
			if (!useTotal)
			{
				return damageable.Health;
			}
			return damageable.HealthTotal;
		case UnitStatDefinition.E_Stat.Mana:
			if (damageable is TheLastStand.Model.Unit.Unit unit)
			{
				if (!useTotal)
				{
					return unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana);
				}
				return unit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal);
			}
			if (damageable is PlayableUnit playableUnit)
			{
				if (!useTotal)
				{
					return playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana);
				}
				return playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal);
			}
			break;
		default:
			if (building?.BrazierModule != null)
			{
				return useTotal ? building.BrazierModule.BrazierPointsTotal : building.BrazierModule.BrazierPoints;
			}
			break;
		}
		Debug.LogError((object)"Could not get a correct reference value to update gauge display!", (Object)(object)((Component)this).gameObject);
		return 0f;
	}

	private IEnumerator IncreasePhaseTwoDelayCoroutine(float targetNormalizedValue)
	{
		yield return SharedYields.WaitForSeconds(increasePhaseTwoDelayTimer);
		increaseDisplayPhaseTwoCoroutine = ((MonoBehaviour)this).StartCoroutine(IncreasePhaseTwoDisplayCoroutine(targetNormalizedValue));
		increasePhaseTwoDelayCoroutine = null;
	}

	private IEnumerator HideValueLabelDelayedCoroutine()
	{
		yield return SharedYields.WaitForSeconds(hideValueLabelDelay);
		((Behaviour)healthValueLbl).enabled = false;
		hideValueLabelCoroutine = null;
	}
}
