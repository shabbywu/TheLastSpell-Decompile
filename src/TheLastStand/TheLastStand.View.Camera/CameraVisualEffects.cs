using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace TheLastStand.View.Camera;

public class CameraVisualEffects : MonoBehaviour
{
	[SerializeField]
	private AmplifyColorEffect invertColor;

	[SerializeField]
	private AmplifyColorEffect redFlashColor;

	[SerializeField]
	private float redFlashInitialBlend = 0.6f;

	[SerializeField]
	private float redFlashDuration = 0.5f;

	[SerializeField]
	private Ease redFlashOutEase = (Ease)11;

	[SerializeField]
	private PostProcessVolume pillarChromaticAberrationVolume;

	private Tween invertTween;

	private Tween redFlashTween;

	public void InvertImage(float targetValue = 1f)
	{
		SetBlendAmount(invertColor, targetValue);
	}

	public void ResetInvertEffect()
	{
		SetBlendAmount(invertColor, 0f);
	}

	public void ToggleChromaticAberration(bool state)
	{
		((Behaviour)pillarChromaticAberrationVolume).enabled = state;
	}

	public IEnumerator InvertImageCoroutine(float duration, Ease ease, float targetValue = 1f)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Tween obj = invertTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		invertTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => ((AmplifyColorBase)invertColor).BlendAmount), (DOSetter<float>)delegate(float x)
		{
			SetBlendAmount(invertColor, x);
		}, targetValue, duration), ease);
		yield return TweenExtensions.WaitForCompletion(invertTween);
	}

	public IEnumerator RedFlashCoroutine(float duration = -1f, Ease ease = 0)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((AmplifyColorBase)redFlashColor).BlendAmount = redFlashInitialBlend;
		Tween obj = redFlashTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		if (duration == -1f)
		{
			duration = redFlashDuration;
		}
		if ((int)ease == 0)
		{
			ease = redFlashOutEase;
		}
		redFlashTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => ((AmplifyColorBase)redFlashColor).BlendAmount), (DOSetter<float>)delegate(float x)
		{
			SetBlendAmount(redFlashColor, x);
		}, 0f, duration), ease);
		yield return TweenExtensions.WaitForCompletion(redFlashTween);
	}

	public IEnumerator ResetInvertCoroutine(float duration, Ease ease)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Tween obj = invertTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		invertTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => ((AmplifyColorBase)invertColor).BlendAmount), (DOSetter<float>)delegate(float x)
		{
			SetBlendAmount(invertColor, x);
		}, 0f, duration), ease);
		yield return TweenExtensions.WaitForCompletion(invertTween);
	}

	private void SetBlendAmount(AmplifyColorEffect colorEffect, float blendAmount)
	{
		((AmplifyColorBase)colorEffect).BlendAmount = blendAmount;
	}
}
