using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class AppearingEffectDisplay : EffectDisplay
{
	[SerializeField]
	protected CanvasGroup fadeTarget;

	[SerializeField]
	protected float fadeInDuration = 0.3f;

	[SerializeField]
	protected float fadeOutDuration = 0.1f;

	[SerializeField]
	protected Transform translateTarget;

	[SerializeField]
	protected float translateDuration = 0.4f;

	[SerializeField]
	protected float translateOffsetY = 50f;

	[SerializeField]
	protected Ease translateEasing = (Ease)12;

	protected float internalTranslateOffsetY;

	protected override float DisplayDuration => Mathf.Max(new float[3] { base.DisplayDuration, fadeInDuration, translateDuration });

	protected override IEnumerator DisplayCoroutine()
	{
		if (translateDuration < 0f)
		{
			translateDuration = displayDuration;
		}
		if (fadeInDuration < 0f)
		{
			fadeInDuration = displayDuration;
		}
		if (fadeInDuration > 0f && (Object)(object)fadeTarget != (Object)null)
		{
			TweenSettingsExtensions.From<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(fadeTarget, 0f, fadeInDuration));
		}
		if (translateDuration > 0f && (Object)(object)translateTarget != (Object)null)
		{
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.From<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(translateTarget, 0f - internalTranslateOffsetY, translateDuration, false), true), translateEasing);
		}
		yield return base.DisplayCoroutine();
		if (fadeOutDuration > 0f && (Object)(object)fadeTarget != (Object)null)
		{
			yield return TweenExtensions.WaitForCompletion((Tween)(object)DOTweenModuleUI.DOFade(fadeTarget, 0f, fadeOutDuration));
		}
	}

	public virtual void Init()
	{
		internalTranslateOffsetY = translateOffsetY;
	}

	public virtual void Init(int value)
	{
		Init();
	}
}
