using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Framework.UI;

public static class BetterSliderExtensions
{
	public static void SetValueWithTween(this BetterSlider slider, float targetValue, Sequence sequence, float tweenDuration)
	{
		if (!(Mathf.Abs(slider.value - targetValue) < Mathf.Epsilon))
		{
			if (sequence == null)
			{
				sequence = TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), "SetValueWithTween");
			}
			TweenSettingsExtensions.Append(sequence, (Tween)(object)DOTween.To((DOGetter<float>)(() => slider.value), (DOSetter<float>)delegate(float value)
			{
				slider.value = value;
			}, targetValue, tweenDuration).SetFullId<TweenerCore<float, float, FloatOptions>>("SetValueWithTween", (Component)(object)slider));
		}
	}
}
