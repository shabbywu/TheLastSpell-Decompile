using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class UnitHealthStatGaugeDisplay : UnitStatGaugeDisplay
{
	[SerializeField]
	private Slider poisonGauge;

	private Tweener poisonTweener;

	public override void RefreshStatInstantly(float statValue = -1f, float statMaxValue = -1f)
	{
		if (base.Damageable != null)
		{
			base.RefreshStatInstantly(statValue, statMaxValue);
			poisonGauge.normalizedValue = statGauge.normalizedValue;
			TheLastStand.Model.Unit.Unit unit = base.Damageable as TheLastStand.Model.Unit.Unit;
			float nextTurnPoisonDamage = unit.GetNextTurnPoisonDamage();
			SetPoisonGaugeValue(nextTurnPoisonDamage / unit.GetClampedStatValue(UnitStatDefinition.E_Stat.HealthTotal));
		}
	}

	public void SetPoisonGaugeValue(float targetNormalizedValue)
	{
		statGauge.normalizedValue = Mathf.Max(poisonGauge.normalizedValue - targetNormalizedValue, 0f);
	}

	public override void ToggleSliders(bool toggle)
	{
		base.ToggleSliders(toggle);
		((Behaviour)poisonGauge).enabled = toggle;
	}

	protected override IEnumerator DecreasePhaseOneDisplayCoroutine(float targetNormalizedValue, bool updateMarkers = true)
	{
		if (poisonGauge.normalizedValue == statGauge.normalizedValue)
		{
			DecreasePoisonDisplay(targetNormalizedValue);
			return base.DecreasePhaseOneDisplayCoroutine(targetNormalizedValue, updateMarkers: false);
		}
		DecreasePoisonDisplay(targetNormalizedValue);
		float nextTurnPoisonDamage = ((TheLastStand.Model.Unit.Unit)base.Damageable).GetNextTurnPoisonDamage();
		if (nextTurnPoisonDamage == 0f)
		{
			return base.DecreasePhaseOneDisplayCoroutine(targetNormalizedValue, updateMarkers: false);
		}
		float num = nextTurnPoisonDamage / ((TheLastStand.Model.Unit.Unit)base.Damageable).GetClampedStatValue(UnitStatDefinition.E_Stat.HealthTotal);
		return base.DecreasePhaseOneDisplayCoroutine(Mathf.Max(targetNormalizedValue - num, 0f), updateMarkers: false);
	}

	protected override IEnumerator IncreasePhaseTwoDisplayCoroutine(float targetNormalizedValue, bool updateMarkers = true)
	{
		if (poisonGauge.normalizedValue == statGauge.normalizedValue)
		{
			IncreasePoisonDisplay(targetNormalizedValue);
			return base.IncreasePhaseTwoDisplayCoroutine(targetNormalizedValue, updateMarkers: false);
		}
		TheLastStand.Model.Unit.Unit unit = base.Damageable as TheLastStand.Model.Unit.Unit;
		float num = unit.GetNextTurnPoisonDamage() / unit.GetClampedStatValue(UnitStatDefinition.E_Stat.HealthTotal);
		IncreasePoisonDisplay(targetNormalizedValue);
		return base.IncreasePhaseTwoDisplayCoroutine(Mathf.Max(targetNormalizedValue - num, 0f), updateMarkers: false);
	}

	private void DecreasePoisonDisplay(float targetNormalizedValue)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		StartPoisonDisplay(targetNormalizedValue, decreasePhaseOneDuration, decreasePhaseOneEasing);
	}

	private void IncreasePoisonDisplay(float targetNormalizedValue)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		StartPoisonDisplay(targetNormalizedValue, increasePhaseTwoDuration, increasePhaseTwoEasing);
	}

	private void StartPoisonDisplay(float targetNormalizedValue, float duration, Ease easing)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (poisonTweener == null)
		{
			poisonTweener = (Tweener)(object)TweenSettingsExtensions.OnKill<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => poisonGauge.normalizedValue), (DOSetter<float>)delegate(float v)
			{
				poisonGauge.normalizedValue = v;
				gaugeMarkersDisplayer.RefreshEnabledMarkers(v);
			}, targetNormalizedValue, duration), easing), (TweenCallback)delegate
			{
				poisonTweener = null;
			});
		}
		else
		{
			poisonTweener.ChangeEndValue((object)targetNormalizedValue, true);
		}
	}
}
