using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Rewired;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Log;
using TPLib.UI;
using TheLastStand.Controller;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.Menus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class SettingsPanel : SerializedMonoBehaviour, IOverlayUser
{
	public enum E_State
	{
		Closed,
		Opened
	}

	public static class Constants
	{
		public static class LocalizationKeys
		{
			public const string ConsentWillNotBeSavedTitle = "Consent_WillNotbeSaved";

			public const string ConsentBackToMainMenuTitle = "Consent_BackToMainMenu";

			public const string ConsentAbandon = "Consent_AbandonGame";

			public const string ConsentSkipTutorial = "Consent_SkipTutorial";
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static TweenCallback _003C_003E9__56_2;

		internal void _003COpen_003Eb__56_2()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	[SerializeField]
	[Range(0.1f, 1f)]
	private float fadeDuration = 0.2f;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private LanguagePanel languagePanel;

	[SerializeField]
	private ScreenSettingsPanel screenSettingsPanel;

	[SerializeField]
	private VolumesPanel volumesPanel;

	[SerializeField]
	private ScreenShakesOptionPanel screenShakesOptionPanel;

	[SerializeField]
	private SettingsBottomPanel settingsBottomPanel;

	[SerializeField]
	private TurnEndWarningsOptionPanel turnEndWarningsOptionPanel;

	[SerializeField]
	private RunInBackgroundPanel runInBackgroundPanel;

	[SerializeField]
	private RestrictedCursorPanel restrictedCursorPanel;

	[SerializeField]
	private EraseSavePanel eraseSavePanel;

	[SerializeField]
	private UISizePanel uiSizePanel;

	[SerializeField]
	private SmartCastPanel smartCastPanel;

	[SerializeField]
	private SpeedModePanel speedModePanel;

	[SerializeField]
	private SpeedScalePanel speedScalePanel;

	[SerializeField]
	private AlwaysDisplayMaxStatValuePanel alwaysDisplayMaxStatValuePanel;

	[SerializeField]
	private AlwaysDisplayUnitPortraitAttribute alwaysDisplayUnitPortraitAttribute;

	[SerializeField]
	private HideCompendium hideCompendium;

	[SerializeField]
	private ShowSkillsHotkeysPanel showSkillsHotkeysPanel;

	[SerializeField]
	private EdgePanPanel edgePanPanel;

	[SerializeField]
	private FocusCamOnSelectionsPanel focusCamOnSelectionsPanel;

	[SerializeField]
	private ResolutionDropdownPanel resolutionPanel;

	[FormerlySerializedAs("edgePanOverUIPanel")]
	[SerializeField]
	private EdgePanOverUIPanel edgePanOverUIPanel;

	[SerializeField]
	private GameObject[] hiddenGameObjectsWithController;

	[SerializeField]
	private Button mainMenuButton;

	[SerializeField]
	private Button abandonButton;

	[SerializeField]
	private Dictionary<Toggle, TabbedPageView> tabPagePairs = new Dictionary<Toggle, TabbedPageView>();

	[SerializeField]
	private Toggle keyRemappingToggle;

	[SerializeField]
	private TabbedPageView keyRemappingTab;

	[SerializeField]
	private Toggle toggleGame;

	[SerializeField]
	private ToggleGroup toggleGroup;

	[SerializeField]
	private Transform tabPosOn;

	[SerializeField]
	private Transform tabPosOff;

	[SerializeField]
	[Min(0f)]
	private float snapshotTransitionDurationIn;

	[SerializeField]
	[Min(0f)]
	private float snapshotTransitionDurationOut;

	[SerializeField]
	private HUDJoystickDynamicTarget joystickTarget;

	[SerializeField]
	private HUDJoystickDynamicTarget dynamicTabTarget;

	private Tween fadeTween;

	private Game.E_State lastState;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public ResolutionDropdownPanel ResolutionDropdownPanel => resolutionPanel;

	public E_State State { get; private set; }

	public HUDJoystickTarget JoystickTarget => joystickTarget;

	public void Close(bool backToGame = true)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		if (State == E_State.Closed)
		{
			return;
		}
		SaveSettings();
		bool flag = TPSingleton<MainMenuView>.Exist();
		if (InputManager.IsLastControllerJoystick && flag)
		{
			TPSingleton<MainMenuView>.Instance.JoystickSelectOptionToggle();
		}
		if (backToGame)
		{
			CameraView.AttenuateWorldForPopupFocus(null);
			TPSingleton<SoundManager>.Instance.TransitionToNormalSnapshot(snapshotTransitionDurationOut);
			if (TPSingleton<GameManager>.Exist())
			{
				GameController.SetState(lastState);
			}
			Tween obj = fadeTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => canvasGroup.alpha), (DOSetter<float>)delegate(float a)
			{
				canvasGroup.alpha = a;
			}, 0f, fadeDuration), (TweenCallback)delegate
			{
				((Behaviour)canvas).enabled = false;
			});
			TweenExtensions.Play<Tween>(fadeTween);
			if (InputManager.IsLastControllerJoystick && !flag)
			{
				((MonoBehaviour)this).StartCoroutine(ExitHUDNavigationModeEndOfFrame());
			}
			State = E_State.Closed;
		}
	}

	private IEnumerator ExitHUDNavigationModeEndOfFrame()
	{
		yield return null;
		TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
	}

	public void OnAbandonButtonClick()
	{
		if (State == E_State.Opened)
		{
			GenericConsent.Open(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap ? "Consent_SkipTutorial" : "Consent_AbandonGame", Abandon, OnAbandonPopupClosed, smallVersion: true);
			if (InputManager.IsLastControllerJoystick)
			{
				DiscardJoystickSelection();
			}
		}
	}

	public void OnResumeButtonClick()
	{
		Resume();
	}

	public void OnGameQuitButtonClick()
	{
		if (State != E_State.Opened)
		{
			return;
		}
		if (TPSingleton<GameManager>.Instance.IsSaveAllowed())
		{
			GenericConsent.Open("Consent_BackToMainMenu", delegate
			{
				Quit();
			}, OnMainMenuPopupClosed, smallVersion: true);
		}
		else
		{
			GenericConsent.Open("Consent_WillNotbeSaved", delegate
			{
				Quit();
			}, OnMainMenuPopupClosed);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			DiscardJoystickSelection();
		}
	}

	public void Open()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		if (State == E_State.Opened)
		{
			return;
		}
		((Behaviour)canvas).enabled = true;
		if (TPSingleton<GameManager>.Exist())
		{
			lastState = TPSingleton<GameManager>.Instance.Game.State;
			GameController.SetState(Game.E_State.Settings);
			TPSingleton<SoundManager>.Instance.TransitionToSettingsSnapshot(snapshotTransitionDurationIn);
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		Tween obj = fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		TweenerCore<float, float, FloatOptions> obj2 = DOTween.To((DOGetter<float>)(() => canvasGroup.alpha), (DOSetter<float>)delegate(float a)
		{
			canvasGroup.alpha = a;
		}, 1f, fadeDuration);
		object obj3 = _003C_003Ec._003C_003E9__56_2;
		if (obj3 == null)
		{
			TweenCallback val = delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			};
			_003C_003Ec._003C_003E9__56_2 = val;
			obj3 = (object)val;
		}
		fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(obj2, (TweenCallback)obj3);
		TweenExtensions.Play<Tween>(fadeTween);
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)this);
		State = E_State.Opened;
		toggleGame.isOn = true;
		toggleGroup.NotifyToggleOn(toggleGame, true);
		Refresh();
		if (InputManager.IsLastControllerJoystick)
		{
			if (!TPSingleton<MainMenuView>.Exist())
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			}
			((Selectable)keyRemappingToggle).interactable = false;
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
		}
		else
		{
			((Selectable)keyRemappingToggle).interactable = true;
		}
	}

	public void Refresh()
	{
		screenSettingsPanel.Refresh();
		volumesPanel.Refresh();
		screenShakesOptionPanel.Refresh();
		settingsBottomPanel.Refresh();
		languagePanel.Refresh();
		turnEndWarningsOptionPanel.Refresh();
		runInBackgroundPanel.Refresh();
		restrictedCursorPanel.Refresh();
		uiSizePanel.Refresh();
		smartCastPanel.Refresh();
		speedModePanel.Refresh();
		speedScalePanel.Refresh();
		alwaysDisplayMaxStatValuePanel.Refresh();
		alwaysDisplayUnitPortraitAttribute.Refresh();
		hideCompendium.Refresh();
		edgePanPanel.Refresh();
		edgePanOverUIPanel.Refresh();
		showSkillsHotkeysPanel.Refresh();
		focusCamOnSelectionsPanel.Refresh();
		for (int i = 0; i < hiddenGameObjectsWithController.Length; i++)
		{
			hiddenGameObjectsWithController[i].SetActive(!InputManager.IsLastControllerJoystick);
		}
		RefreshOpenedPage();
	}

	public void RefreshOpenedPage()
	{
		foreach (KeyValuePair<Toggle, TabbedPageView> tabPagePair in tabPagePairs)
		{
			if ((Object)(object)tabPagePair.Value != (Object)null && tabPagePair.Key.isOn)
			{
				tabPagePair.Value.IsDirty = true;
			}
		}
	}

	public void RefreshFrameRateCap()
	{
		screenSettingsPanel.FrameRateCapPanel.Refresh();
	}

	public void OnAbandonPopupClosed()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
			EventSystem.current.SetSelectedGameObject(((Component)abandonButton).gameObject);
		}
	}

	public void OnMainMenuPopupClosed()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
			EventSystem.current.SetSelectedGameObject(((Component)mainMenuButton).gameObject);
		}
	}

	public void OnEraseSavePopupClosed()
	{
		((MonoBehaviour)this).StartCoroutine(OnEraseSavePopupClosedCoroutine());
	}

	private IEnumerator OnEraseSavePopupClosedCoroutine()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
		TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(dynamicTabTarget.GetSelectionInfo());
		yield return null;
		EventSystem.current.SetSelectedGameObject(((Component)eraseSavePanel.Selectable).gameObject);
	}

	private void Abandon()
	{
		SettingsManager.CloseSettings(backToGame: false);
		TPSingleton<SoundManager>.Instance.TransitionToDefaultSnapshot(GameManager.AmbientSoundsFadeOutDuration);
		GameController.TriggerGameOver(Game.E_GameOverCause.Abandon);
	}

	private void DiscardJoystickSelection()
	{
		EventSystem.current.SetSelectedGameObject((GameObject)null);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
	}

	private void OnDestroy()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		}
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		if (State == E_State.Opened)
		{
			((Selectable)keyRemappingToggle).interactable = (int)controllerType != 2;
			for (int i = 0; i < hiddenGameObjectsWithController.Length; i++)
			{
				hiddenGameObjectsWithController[i].SetActive((int)controllerType != 2);
			}
		}
	}

	private void Quit(bool killSave = false)
	{
		SettingsManager.CloseSettings(backToGame: false);
		if (TPSingleton<GameManager>.Exist())
		{
			TileObjectSelectionManager.ResetStaticProperties();
			GameController.GoBackToMainMenu(killSave);
		}
	}

	private void Resume()
	{
		if (State == E_State.Opened)
		{
			SettingsManager.CloseSettings();
		}
	}

	private void SaveSettings()
	{
		SettingsManager.Save();
	}

	private void Start()
	{
		TPSingleton<InputManager>.Instance.LastActiveControllerChanged += OnLastActiveControllerChanged;
		((Behaviour)canvas).enabled = false;
		canvasGroup.alpha = 0f;
		if (tabPagePairs == null)
		{
			((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).LogError((object)"tabPagePairs is null.", (Object)(object)this, (CLogLevel)1, true, true);
		}
		else
		{
			foreach (KeyValuePair<Toggle, TabbedPageView> tabbedPage in tabPagePairs)
			{
				((UnityEvent<bool>)(object)tabbedPage.Key.onValueChanged).AddListener((UnityAction<bool>)delegate(bool value)
				{
					TabbedPageToggle_ValueChanged(tabbedPage.Key, value);
				});
			}
		}
		eraseSavePanel.Refresh();
	}

	private void TabbedPageToggle_ValueChanged(Toggle sender, bool value)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		((Component)sender).transform.localPosition = new Vector3(((Component)sender).transform.localPosition.x, value ? ((Component)tabPosOn).transform.localPosition.y : ((Component)tabPosOff).transform.localPosition.y, ((Component)sender).transform.localPosition.z);
		if (value)
		{
			tabPagePairs[sender].Open();
		}
		else
		{
			tabPagePairs[sender].Close();
		}
	}

	private void SelectNeighbourTab(bool next)
	{
		int count = tabPagePairs.Count;
		for (int i = 0; i < count; i++)
		{
			KeyValuePair<Toggle, TabbedPageView> keyValuePair = tabPagePairs.ElementAt(i);
			if (keyValuePair.Key.isOn)
			{
				keyValuePair.Key.isOn = false;
				int index = IntExtensions.Mod(i + (next ? 1 : (-1)), count);
				if (InputManager.IsLastControllerJoystick && (Object)(object)tabPagePairs.ElementAt(index).Value == (Object)(object)keyRemappingTab)
				{
					index = IntExtensions.Mod(i + (next ? 2 : (-2)), count);
				}
				tabPagePairs.ElementAt(index).Key.isOn = true;
				break;
			}
		}
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(dynamicTabTarget.GetSelectionInfo());
		}
	}

	private void Update()
	{
		if (State != E_State.Opened)
		{
			return;
		}
		if (InputManager.GetButtonDown(80))
		{
			if (!GenericConsent.IsWaitingForInput())
			{
				Resume();
			}
		}
		else if (InputManager.GetButtonDown(109))
		{
			SelectNeighbourTab(next: true);
		}
		else if (InputManager.GetButtonDown(110))
		{
			SelectNeighbourTab(next: false);
		}
	}
}
