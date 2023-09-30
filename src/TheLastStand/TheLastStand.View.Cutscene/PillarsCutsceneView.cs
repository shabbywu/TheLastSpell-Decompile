using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RedBlueGames.Tools.TextTyper;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.WorldMap;
using UnityEngine;

namespace TheLastStand.View.Cutscene;

public class PillarsCutsceneView : CutsceneView
{
	[Serializable]
	public struct CityElements
	{
		public string CityId;

		public GameObject PillarUnlocked;

		public GameObject[] PillarInVisualEffects;
	}

	public static class Constants
	{
		public const string PlaySequenceAnimatorTrigger = "Play";
	}

	[SerializeField]
	private CityElements[] citiesElements;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private GameObject[] disabledDuringFade;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private LocalizedFont textLocalizedFont;

	[SerializeField]
	private TextTyper textTyper;

	[SerializeField]
	[Min(0f)]
	private float fadeInDuration = 1f;

	[SerializeField]
	[Min(0f)]
	private float fadeOutDuration = 1f;

	[SerializeField]
	private Ease fadeInCurve = (Ease)4;

	[SerializeField]
	private Ease fadeOutCurve = (Ease)4;

	[SerializeField]
	[Min(0f)]
	private float delayAfterFadeIn = 1f;

	[SerializeField]
	[Min(0f)]
	private float delayBeforeFadeOut = 0.5f;

	private Tween canvasFadeTween;

	private int nextTextIndex;

	[HideInInspector]
	public bool animatorIdleStateEnter;

	public PlayPillarsCutsceneDefinition PlayPillarsCutsceneDefinition { get; set; }

	public override IEnumerator PlayCutscene(Action callback = null)
	{
		base.IsPlaying = true;
		Callback = callback;
		PrepareView();
		animatorIdleStateEnter = false;
		ToggleAssetsDuringFade(show: false);
		((Behaviour)canvas).enabled = true;
		canvasFadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 1f, fadeInDuration), fadeInCurve), (TweenCallback)delegate
		{
			ToggleAssetsDuringFade(show: true);
		});
		yield return TweenExtensions.WaitForCompletion(canvasFadeTween);
		if (TPSingleton<CutsceneManager>.Instance.VictorySequenceView.IsPlaying)
		{
			TPSingleton<CutsceneManager>.Instance.VictorySequenceView.ResetPillarVisualEffects();
		}
		yield return SharedYields.WaitForSeconds(delayAfterFadeIn);
		animator.SetTrigger("Play");
		yield return (object)new WaitUntil((Func<bool>)(() => animatorIdleStateEnter));
		if (!PlayPillarsCutsceneDefinition.SkipFadeOut)
		{
			yield return SharedYields.WaitForSeconds(delayBeforeFadeOut);
			ToggleAssetsDuringFade(show: false);
			canvasFadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 0f, fadeOutDuration), fadeOutCurve), (TweenCallback)delegate
			{
				((Behaviour)canvas).enabled = false;
			});
			yield return TweenExtensions.WaitForCompletion(canvasFadeTween);
		}
		base.IsPlaying = false;
		callback?.Invoke();
	}

	public void AppendNextText()
	{
		string id = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
		string text = $"VictoryPillarsCutscene_{id}_{nextTextIndex}";
		string text2 = default(string);
		if (Localizer.TryGet(text, ref text2))
		{
			textTyper.TypeText(text2, -1f);
		}
		else
		{
			((CLogger<CutsceneManager>)TPSingleton<CutsceneManager>.Instance).Log((object)("Could not find a localization entry for key " + text + " during pillars cutscene, skipping it."), (CLogLevel)0, true, false);
		}
		nextTextIndex++;
	}

	public void ClearText()
	{
		((TMP_Text)text).text = string.Empty;
	}

	public override IEnumerator Skip()
	{
		((Behaviour)animator).enabled = false;
		textTyper.Skip();
		ClearText();
		ToggleAssetsDuringFade(show: false);
		if (TweenExtensions.IsPlaying(canvasFadeTween))
		{
			Tween obj = canvasFadeTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			canvasFadeTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.5f), fadeOutCurve);
			yield return TweenExtensions.WaitForCompletion(canvasFadeTween);
		}
		else
		{
			yield return (object)new WaitForSecondsRealtime(0.5f);
		}
		yield return base.Skip();
	}

	private void PrepareView()
	{
		string text = (string.IsNullOrEmpty(TPSingleton<CutsceneManager>.Instance.VictorySequenceView.debugCityIdOverride) ? TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id : TPSingleton<CutsceneManager>.Instance.VictorySequenceView.debugCityIdOverride);
		textLocalizedFont.RefreshFont();
		for (int i = 0; i < citiesElements.Length; i++)
		{
			CityElements cityElements = citiesElements[i];
			WorldMapCity worldMapCity = TPSingleton<WorldMapCityManager>.Instance.Cities.Find((WorldMapCity o) => o.CityDefinition.Id == cityElements.CityId);
			cityElements.PillarUnlocked.SetActive(worldMapCity != null && worldMapCity.NumberOfWins > 0 && text != cityElements.CityId);
			for (int j = 0; j < cityElements.PillarInVisualEffects.Length; j++)
			{
				cityElements.PillarInVisualEffects[j].SetActive(cityElements.CityId == text);
			}
		}
		nextTextIndex = 0;
		ClearText();
	}

	private void Awake()
	{
		int cityIndex = 0;
		while (cityIndex < citiesElements.Length)
		{
			if (TPSingleton<WorldMapCityManager>.Instance.Cities.All((WorldMapCity o) => o.CityDefinition.Id != citiesElements[cityIndex].CityId))
			{
				((CLogger<CutsceneManager>)TPSingleton<CutsceneManager>.Instance).LogWarning((object)("A city with Id " + citiesElements[cityIndex].CityId + " is referenced in PillarsCutsceneView although it has not been found in Database."), (CLogLevel)1, true, false);
			}
			int num = cityIndex + 1;
			cityIndex = num;
		}
	}

	private void ToggleAssetsDuringFade(bool show)
	{
		for (int i = 0; i < disabledDuringFade.Length; i++)
		{
			disabledDuringFade[i].SetActive(show);
		}
	}
}
