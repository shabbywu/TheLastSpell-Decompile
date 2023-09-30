using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Automaton;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Manager;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFadeManager : Manager<CanvasFadeManager>
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Ease defaultEase = (Ease)8;

	[SerializeField]
	private int defaultSortingOrder = 9999;

	[SerializeField]
	private float fadeDuration;

	[SerializeField]
	private Image image;

	private bool hasBeenInitialized;

	private Tween currentTween;

	public CanvasGroup CanvasGroup => canvasGroup;

	public bool FadeIsOver { get; private set; }

	public float FadeDuration => fadeDuration;

	public static void FadeIn(float duration = -1f, int sortingOrder = -1, Ease ease = 0, CanvasGroup canvasGroup = null, Action callback = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<CanvasFadeManager>.Instance.DoFadeTween(Color.black, 1f, duration, sortingOrder, ease, canvasGroup, callback);
	}

	public static void FadeIn(Color color, float duration = -1f, int sortingOrder = -1, Ease ease = 0, CanvasGroup canvasGroup = null, Action callback = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<CanvasFadeManager>.Instance.DoFadeTween(color, 1f, duration, sortingOrder, ease, canvasGroup, callback);
	}

	public static void FadeOut(float duration = -1f, int sortingOrder = -1, Ease ease = 0, CanvasGroup canvasGroup = null, Action callback = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<CanvasFadeManager>.Instance.DoFadeTween(Color.black, 0f, duration, sortingOrder, ease, canvasGroup, callback);
	}

	public static void FadeOut(Color color, float duration = -1f, int sortingOrder = -1, Ease ease = 0, CanvasGroup canvasGroup = null, Action callback = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<CanvasFadeManager>.Instance.DoFadeTween(color, 0f, duration, sortingOrder, ease, canvasGroup, callback);
	}

	public static void FadeTo(float alpha, float duration = -1f, int sortingOrder = -1, Ease ease = 0, CanvasGroup canvasGroup = null, Action callback = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<CanvasFadeManager>.Instance.DoFadeTween(Color.black, Mathf.Clamp(alpha, 0f, 1f), duration, sortingOrder, ease, canvasGroup, callback);
	}

	public void OnApplicationStateChange(State state)
	{
		string name = state.GetName();
		if (name != null)
		{
			switch (name)
			{
			case "Credits":
			case "AnimatedCutscene":
			case "GameLobby":
			case "Game":
			case "LevelEditor":
			case "MetaShops":
			case "WorldMap":
				FadeOut(-1f, -1, (Ease)0);
				break;
			}
		}
	}

	private void DoFadeTween(Color color, float alpha, float duration = -1f, int sortingOrder = -1, Ease ease = 0, CanvasGroup canvasGroup = null, Action callback = null)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		TPSingleton<CanvasFadeManager>.Instance.canvas.sortingOrder = ((sortingOrder == -1) ? TPSingleton<CanvasFadeManager>.Instance.defaultSortingOrder : sortingOrder);
		TPSingleton<CanvasFadeManager>.Instance.FadeIsOver = false;
		((Graphic)TPSingleton<CanvasFadeManager>.Instance.image).color = color;
		if (duration < 0f)
		{
			duration = TPSingleton<CanvasFadeManager>.Instance.FadeDuration;
		}
		if ((Object)(object)canvasGroup == (Object)null)
		{
			canvasGroup = TPSingleton<CanvasFadeManager>.Instance.canvasGroup;
		}
		if ((int)ease == 0)
		{
			ease = TPSingleton<CanvasFadeManager>.Instance.defaultEase;
		}
		if (TPSingleton<CanvasFadeManager>.Instance.currentTween != null)
		{
			TweenExtensions.Kill(TPSingleton<CanvasFadeManager>.Instance.currentTween, false);
			TPSingleton<CanvasFadeManager>.Instance.currentTween = null;
		}
		TPSingleton<CanvasFadeManager>.Instance.currentTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(this.canvasGroup, alpha, duration), ease), (TweenCallback)delegate
		{
			TPSingleton<CanvasFadeManager>.Instance.FadeIsOver = true;
			TPSingleton<CanvasFadeManager>.Instance.currentTween = null;
			callback?.Invoke();
		});
	}

	private void Init()
	{
		if (!hasBeenInitialized)
		{
			hasBeenInitialized = true;
			FadeIsOver = true;
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	protected override void OnDestroy()
	{
		((CLogger<CanvasFadeManager>)this).OnDestroy();
		if (TPSingleton<ApplicationManager>.Exist())
		{
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent -= OnApplicationStateChange;
		}
	}
}
