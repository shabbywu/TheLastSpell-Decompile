using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Framework;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View;

public class SaveIndicatorView : MonoBehaviour
{
	[SerializeField]
	private Image indicatorImage;

	[SerializeField]
	private float minimumLifeSpan = 2f;

	[SerializeField]
	private TextMeshProUGUI saveText;

	[SerializeField]
	private DataColor badColor;

	[SerializeField]
	private DataColor progressColor;

	[SerializeField]
	private DataColor goodColor;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private float showDuration = 0.35f;

	[SerializeField]
	private float showPosition = 120f;

	[SerializeField]
	private Ease showEasing = (Ease)27;

	[SerializeField]
	private float hideDuration = 0.35f;

	[SerializeField]
	private float hidePosition = 120f;

	[SerializeField]
	private Ease hideEasing = (Ease)27;

	[SerializeField]
	private float blinkDuration = 0.35f;

	[SerializeField]
	private float blinkMinValue = 0.7f;

	[SerializeField]
	private Ease blinkEasing = (Ease)27;

	private Coroutine hideCoroutine;

	private float openedTime;

	private Tween hideTween;

	private Tween showTween;

	private Tween blinkTween;

	private void Start()
	{
		SaverLoader.OnGameSavingStarts += OnGameSavingStarts;
		SaverLoader.OnGameSavingEnds += OnGameSavingEnds;
	}

	private float GetOpacityValue()
	{
		return Mathf.Abs(Mathf.Sin(Time.time * 3f)) * 0.3f + 0.7f;
	}

	private void Hide(bool isSuccessful = true)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)saveText).text = (isSuccessful ? Localizer.Get("Save_Success") : Localizer.Get("Save_Failure"));
		((Graphic)saveText).color = (isSuccessful ? goodColor._Color : badColor._Color);
		Tween obj = showTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		Tween obj2 = hideTween;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		hideTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(rectTransform, hidePosition, hideDuration, false), hideEasing);
		TweenExtensions.Play<Tween>(hideTween);
		Tween obj3 = blinkTween;
		if (obj3 != null)
		{
			TweenExtensions.Kill(obj3, false);
		}
		((Graphic)indicatorImage).color = new Color(((Graphic)indicatorImage).color.r, ((Graphic)indicatorImage).color.g, ((Graphic)indicatorImage).color.b, 1f);
		openedTime = 0f;
	}

	private IEnumerator HideAfterTime(bool isSuccessful)
	{
		while (openedTime > Time.time - minimumLifeSpan)
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		Hide(isSuccessful);
		hideCoroutine = null;
	}

	private void OnDestroy()
	{
		SaverLoader.OnGameSavingStarts -= OnGameSavingStarts;
		SaverLoader.OnGameSavingEnds -= OnGameSavingEnds;
	}

	[ContextMenu("Play Blink Animation")]
	private void BlinkAnimation()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Tween obj = blinkTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		((Graphic)indicatorImage).color = new Color(((Graphic)indicatorImage).color.r, ((Graphic)indicatorImage).color.g, ((Graphic)indicatorImage).color.b, 1f);
		blinkTween = (Tween)(object)TweenSettingsExtensions.SetLoops<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleUI.DOFade(indicatorImage, blinkMinValue, blinkDuration), blinkEasing), -1, (LoopType)1);
		TweenExtensions.Play<Tween>(blinkTween);
	}

	private void OnGameSavingStarts()
	{
		Show();
	}

	private void OnGameSavingEnds(bool isSuccessful)
	{
		if (!isSuccessful)
		{
			GenericPopUp.Open("Popup_SaveWriting_ErrorTitle", "Popup_SaveWriting_ErrorText", ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Stats/Icons/VerySmall_Critical", failSilently: false));
			if (hideCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(hideCoroutine);
				hideCoroutine = null;
			}
			Hide(isSuccessful: false);
		}
		else if (hideCoroutine == null)
		{
			hideCoroutine = ((MonoBehaviour)this).StartCoroutine(HideAfterTime(isSuccessful));
		}
	}

	private void Show()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (UIManager.DebugToggleUI != false)
		{
			((TMP_Text)saveText).text = Localizer.Get("Save_Saving");
			((Graphic)saveText).color = progressColor._Color;
			Tween obj = hideTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			Tween obj2 = showTween;
			if (obj2 != null)
			{
				TweenExtensions.Kill(obj2, false);
			}
			showTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(rectTransform, showPosition, showDuration, false), showEasing);
			TweenExtensions.Play<Tween>(showTween);
			BlinkAnimation();
			openedTime = Time.time;
		}
	}
}
