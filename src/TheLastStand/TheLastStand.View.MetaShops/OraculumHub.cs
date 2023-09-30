using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.SDK;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.HUD;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public abstract class OraculumHub<T> : TPSingleton<T> where T : OraculumHub<T>
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static TweenCallback _003C_003E9__25_0;

		internal void _003CDisplay_003Eb__25_0()
		{
			Canvas gameTransitionCanvas = TPSingleton<T>.Instance.gameTransitionCanvas;
			int sortingOrder = gameTransitionCanvas.sortingOrder;
			gameTransitionCanvas.sortingOrder = sortingOrder - 1;
		}
	}

	[SerializeField]
	protected CanvasGroup canvasGroup;

	[SerializeField]
	protected RectTransform globalRect;

	[SerializeField]
	protected GraphicRaycaster hubRaycaster;

	[SerializeField]
	protected BetterButton leaveButton;

	[SerializeField]
	protected JoystickSelectableCanvasScaler joystickCanvasScaler;

	[SerializeField]
	protected RectTransform[] dynamicWidthRectTransforms;

	[SerializeField]
	protected RectTransform[] dynamicHeightRectTransforms;

	[SerializeField]
	protected RectTransform[] globalRectContent;

	[SerializeField]
	[Tooltip("Canvas group used to fade from game view to meta shops view.")]
	private CanvasGroup gameTransitionCanvasGroup;

	[SerializeField]
	protected Canvas gameTransitionCanvas;

	[SerializeField]
	private float blackScreenInDuration = 0.5f;

	[SerializeField]
	private Ease blackScreenInEase = (Ease)3;

	[SerializeField]
	private float blackScreenOutDuration = 0.25f;

	[SerializeField]
	private Ease blackScreenOutEase = (Ease)2;

	private Sequence displaySequence;

	public bool Displayed { get; protected set; }

	public bool OpeningOrClosing { get; protected set; }

	public bool HideGoddesses
	{
		get
		{
			if (((StateMachine)ApplicationManager.Application).State.GetName() == "Game" || ((StateMachine)ApplicationManager.Application).State.GetName() == "MetaShops")
			{
				return TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.HideGoddesses;
			}
			return false;
		}
	}

	public static void Display(bool show, Action onDisplayed = null)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Expected O, but got Unknown
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Expected O, but got Unknown
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Expected O, but got Unknown
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Expected O, but got Unknown
		TPSingleton<T>.Instance.Displayed = show;
		if (TPSingleton<T>.Instance.Displayed)
		{
			TPSingleton<SoundManager>.Instance.FadeMusic(TPSingleton<T>.Instance.HideGoddesses ? TPSingleton<SoundManager>.Instance.MetaShopsNoGoddessesMusic : TPSingleton<SoundManager>.Instance.MetaShopsMusic);
		}
		if ((Object)(object)TPSingleton<T>.Instance.gameTransitionCanvasGroup == (Object)null)
		{
			TPSingleton<LightningSDKManager>.Instance.HandleMetaShopTransition(LightningSDKManager.SDKEvent.HUB_SHOP);
			TPSingleton<T>.Instance.RefreshShopsScene();
			return;
		}
		TPSingleton<T>.Instance.SetActiveLeaveHubButton(TPSingleton<T>.Instance.Displayed);
		TPSingleton<T>.Instance.OnFadeToBlackStarts();
		float value = TPSingleton<T>.Instance.blackScreenInDuration + TPSingleton<T>.Instance.blackScreenOutDuration;
		if (TPSingleton<T>.Instance.Displayed)
		{
			TPSingleton<LightningSDKManager>.Instance.HandleMetaShopTransition(LightningSDKManager.SDKEvent.HUB_SHOP, value);
			Sequence obj = TPSingleton<T>.Instance.displaySequence;
			TweenerCore<float, float, FloatOptions> obj2 = TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(TPSingleton<T>.Instance.gameTransitionCanvasGroup, 1f, TPSingleton<T>.Instance.blackScreenInDuration), TPSingleton<T>.Instance.blackScreenInEase);
			T instance = TPSingleton<T>.Instance;
			TweenSettingsExtensions.Append(obj, (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(obj2, new TweenCallback(instance.OnFadeToBlackComplete)));
			Sequence obj3 = TPSingleton<T>.Instance.displaySequence;
			TweenerCore<float, float, FloatOptions> obj4 = TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(TPSingleton<T>.Instance.gameTransitionCanvasGroup, 0f, TPSingleton<T>.Instance.blackScreenOutDuration), TPSingleton<T>.Instance.blackScreenOutEase);
			object obj5 = _003C_003Ec._003C_003E9__25_0;
			if (obj5 == null)
			{
				TweenCallback val = delegate
				{
					Canvas obj10 = TPSingleton<T>.Instance.gameTransitionCanvas;
					int sortingOrder = obj10.sortingOrder;
					obj10.sortingOrder = sortingOrder - 1;
				};
				_003C_003Ec._003C_003E9__25_0 = val;
				obj5 = (object)val;
			}
			TweenSettingsExtensions.Append(obj3, (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.OnStart<TweenerCore<float, float, FloatOptions>>(obj4, (TweenCallback)obj5), (TweenCallback)delegate
			{
				TPSingleton<T>.Instance.OnHubEnter(onDisplayed);
			}));
		}
		else
		{
			if (((StateMachine)ApplicationManager.Application).State.GetName() == "Game")
			{
				TPSingleton<LightningSDKManager>.Instance.HandleGameCycleColor(value);
			}
			else
			{
				TPSingleton<LightningSDKManager>.Instance.HandleApplicationStateColor(((StateMachine)ApplicationManager.Application).State, value);
			}
			Sequence obj6 = TPSingleton<T>.Instance.displaySequence;
			TweenerCore<float, float, FloatOptions> obj7 = TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(TPSingleton<T>.Instance.gameTransitionCanvasGroup, 1f, TPSingleton<T>.Instance.blackScreenInDuration), TPSingleton<T>.Instance.blackScreenInEase);
			T instance2 = TPSingleton<T>.Instance;
			TweenSettingsExtensions.Append(obj6, (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(obj7, new TweenCallback(instance2.OnFadeToBlackComplete)));
			Sequence obj8 = TPSingleton<T>.Instance.displaySequence;
			TweenerCore<float, float, FloatOptions> obj9 = TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(TPSingleton<T>.Instance.gameTransitionCanvasGroup, 0f, TPSingleton<T>.Instance.blackScreenOutDuration), TPSingleton<T>.Instance.blackScreenOutEase);
			T instance3 = TPSingleton<T>.Instance;
			TweenSettingsExtensions.Append(obj8, (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(obj9, new TweenCallback(instance3.OnHubExit)));
		}
		EventSystem.current.SetSelectedGameObject((GameObject)null);
	}

	public void SetActiveLeaveHubButton(bool isActive)
	{
		leaveButton.Interactable = isActive;
	}

	protected virtual void EnableRaycasters(bool state)
	{
		canvasGroup.interactable = state;
		canvasGroup.blocksRaycasts = state;
		((Behaviour)hubRaycaster).enabled = state;
	}

	protected virtual IEnumerator InitCoroutine()
	{
		yield break;
	}

	protected virtual void OnDestroy()
	{
		((UnityEventBase)((Button)leaveButton).onClick).RemoveAllListeners();
	}

	protected virtual void OnFadeToBlackComplete()
	{
		if (Displayed)
		{
			((Component)this).gameObject.SetActive(true);
			canvasGroup.alpha = 1f;
			Canvas obj = gameTransitionCanvas;
			int sortingOrder = obj.sortingOrder;
			obj.sortingOrder = sortingOrder + 1;
			if (((StateMachine)ApplicationManager.Application).State.GetName() == "Game")
			{
				TPSingleton<TileMapView>.Instance.Hide();
				TPSingleton<GameView>.Instance.HideHud();
			}
		}
		else
		{
			TPSingleton<T>.Instance.canvasGroup.alpha = 0f;
			SaveManager.Save();
			if (((StateMachine)ApplicationManager.Application).State.GetName() == "Game")
			{
				TPSingleton<TileMapView>.Instance.Display();
				TPSingleton<GameView>.Instance.DisplayHud();
				GameView.BottomScreenPanel.BottomLeftPanel.Refresh();
			}
		}
	}

	protected virtual void OnFadeToBlackStarts()
	{
		OpeningOrClosing = true;
		displaySequence = DOTween.Sequence();
		if (TPSingleton<T>.Instance.Displayed)
		{
			((Behaviour)gameTransitionCanvasGroup).enabled = true;
		}
		else if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject((GameObject)null);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		}
	}

	protected virtual void OnHubEnter(Action onDisplayed = null)
	{
		EnableRaycasters(state: true);
		OpeningOrClosing = false;
		onDisplayed?.Invoke();
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	protected virtual void OnHubExit()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		EnableRaycasters(state: false);
		Vector3 position = ((Component)TPSingleton<T>.Instance).transform.position;
		position.x = (float)Screen.width * 0.5f;
		((Component)this).transform.position = position;
		TPSingleton<T>.Instance.OpeningOrClosing = false;
		((Component)TPSingleton<T>.Instance).gameObject.SetActive(false);
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	protected virtual void RefreshShopsScene()
	{
	}

	protected virtual void Start()
	{
		((MonoBehaviour)this).StartCoroutine(InitCoroutine());
		if (((StateMachine)ApplicationManager.Application).State.GetName() == "MetaShops")
		{
			Display(show: true);
			EnableRaycasters(state: true);
		}
		else if (((StateMachine)ApplicationManager.Application).State.GetName() != "Credits")
		{
			canvasGroup.alpha = 0f;
			((Component)this).gameObject.SetActive(false);
			EnableRaycasters(state: false);
		}
	}
}
