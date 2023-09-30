using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RedBlueGames.Tools.TextTyper;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Model;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.View;

public class BarkView : MonoBehaviour
{
	[SerializeField]
	private float scaleInDuration = 0.2f;

	[SerializeField]
	private AnimationCurve scaleInEasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private Transform scaleInTarget;

	[SerializeField]
	private float fadeInDuration = 0.2f;

	[SerializeField]
	private Ease fadeInEasing = (Ease)12;

	[SerializeField]
	private float fadeOutDuration = 1f;

	[SerializeField]
	private Ease fadeOutEasing = (Ease)12;

	[SerializeField]
	private float arrowTweenDuration = 0.2f;

	[SerializeField]
	private Ease arrowTweenEasing = (Ease)4;

	[SerializeField]
	private Transform arrowTransform;

	[SerializeField]
	[Tooltip("Delay between the start of the bark appearance and the start of the text being typed")]
	private float textDisplayDelay = 0.1f;

	[SerializeField]
	[Tooltip("Delay between the end of the text being typed and the start of the bark disappearance")]
	private float postTextApparitionDelay = 0.2f;

	[SerializeField]
	[Tooltip("Min duration of the text being displayed on the screen. If [text being typed duration] < this value, then we still wait this duration before firing the postTextApparitionDelay wait (Prevents small text disappearing too quickly).")]
	private float displayMinDuration = 1.6f;

	[SerializeField]
	private FollowElement followElement;

	[SerializeField]
	private TextTyper textTyper;

	[SerializeField]
	private TextMeshProUGUI sentenceText;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private LocalizedFont localizedFont;

	public Bark Bark { get; set; }

	public void Display(Action callback = null)
	{
		if ((Object)(object)textTyper != (Object)null)
		{
			textTyper.Init();
			((UnityEventBase)textTyper.CharacterPrinted).RemoveAllListeners();
			((UnityEvent<string>)(object)textTyper.CharacterPrinted).AddListener((UnityAction<string>)delegate
			{
				SoundManager.PlayAudioClip(UIManager.BarkTextDisplayAudioClip);
			});
		}
		if ((Object)(object)followElement != (Object)null)
		{
			followElement.ChangeTarget(Bark.Barker.BarkViewFollowTarget);
		}
		LocalizedFont obj = localizedFont;
		if (obj != null)
		{
			obj.RefreshFont();
		}
		((MonoBehaviour)this).StartCoroutine(DisplayCoroutine(dontDestroy: false, callback));
	}

	private void OnDisable()
	{
		if ((Object)(object)textTyper != (Object)null)
		{
			((Component)textTyper).gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		if ((Object)(object)textTyper != (Object)null)
		{
			((Component)textTyper).gameObject.SetActive(true);
		}
	}

	private IEnumerator DisplayCoroutine(bool dontDestroy = false, Action callback = null)
	{
		string arg = string.Empty;
		if (Bark.Barker is TheLastStand.Model.Unit.Unit unit)
		{
			arg = unit.Name;
		}
		else if (Bark.Barker is BlueprintModule blueprintModule)
		{
			arg = blueprintModule.BuildingParent.BuildingDefinition.Id;
		}
		CLoggerManager.Log((object)$"{Time.time} : {arg} barks {Bark.Sentence}", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Barks", false);
		if ((Object)(object)arrowTransform != (Object)null)
		{
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetLoops<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(arrowTransform, -3f, arrowTweenDuration, false)), -1, (LoopType)1), arrowTweenEasing);
		}
		canvasGroup.alpha = 0f;
		Tweener val = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 1f, fadeInDuration), fadeInEasing);
		if ((Object)(object)scaleInTarget != (Object)null)
		{
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.From<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(scaleInTarget, 0f, scaleInDuration)), scaleInEasingCurve);
		}
		if ((Object)(object)textTyper != (Object)null)
		{
			Sequence val2 = DOTween.Sequence();
			TweenSettingsExtensions.AppendInterval(val2, textDisplayDelay);
			if (Bark == null)
			{
				TweenSettingsExtensions.AppendCallback(val2, (TweenCallback)delegate
				{
					textTyper.TypeText(((TMP_Text)sentenceText).text, -1f);
				});
			}
			else
			{
				TweenSettingsExtensions.AppendCallback(val2, (TweenCallback)delegate
				{
					textTyper.TypeText(Bark.Sentence, -1f);
				});
			}
		}
		yield return TweenExtensions.WaitForCompletion((Tween)(object)val);
		float endTime = Time.time + displayMinDuration;
		yield return (object)new WaitWhile((Func<bool>)(() => textTyper.IsTyping || Time.time < endTime));
		yield return SharedYields.WaitForSeconds(postTextApparitionDelay);
		val = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 0f, fadeOutDuration), fadeOutEasing);
		yield return TweenExtensions.WaitForCompletion((Tween)(object)val);
		callback?.Invoke();
		if (!dontDestroy)
		{
			TPSingleton<BarkManager>.Instance.RemoveBark(Bark);
		}
	}
}
