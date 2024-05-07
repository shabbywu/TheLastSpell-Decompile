using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Database;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using TheLastStand.View.NightReport;
using TheLastStand.View.SoulsReward;
using TheLastStand.View.Tooltip;
using TheLastStand.View.WorldMap;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View;

public class GameOverPanel : TPSingleton<GameOverPanel>, IOverlayUser
{
	[SerializeField]
	private float panelDeltaPosY = -20f;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private Image gameOverPanel;

	[SerializeField]
	private GameObject pageOne;

	[SerializeField]
	private GameObject pageTwo;

	[SerializeField]
	private GameObject skipButtonParent;

	[FormerlySerializedAs("continueButtonParent")]
	[SerializeField]
	private GameObject buttonParent;

	[SerializeField]
	private SoulsRewardPanel soulsRewardPanel;

	[SerializeField]
	private TextMeshProUGUI nextMessage;

	[FormerlySerializedAs("victoryBG")]
	[SerializeField]
	private Sprite victoryBGPageTwo;

	[SerializeField]
	private Sprite victoryBGPageOne;

	[FormerlySerializedAs("defeatBG")]
	[SerializeField]
	private Sprite defeatBGPageTwo;

	[SerializeField]
	private Sprite defeatBGPageOne;

	[SerializeField]
	private float blackBGFadeInDuration = 0.6f;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the end of easy mode and apocalypse appearing")]
	private float waitBeforeShowingPlayableReports;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the popup appearing")]
	private float waitBeforeShowingEasyModeAndApocalypse;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the end of playable reports appearing")]
	private float waitBeforeShowingStats;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the end of stats appearing")]
	private float waitBeforeShowingUnlocks;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the end of unlocks appearing")]
	private float waitBeforeShowingContinue;

	[SerializeField]
	private Ease unfoldEasing = (Ease)8;

	[SerializeField]
	[Range(0f, 5f)]
	private float unfoldDuration = 0.4f;

	[SerializeField]
	private TextMeshProUGUI gameOverTitle;

	[SerializeField]
	private TextMeshProUGUI cityTitle;

	[SerializeField]
	private Image cityPortrait;

	[SerializeField]
	private Image cityPortraitBox;

	[SerializeField]
	private Sprite cityPortraitBoxDefeat;

	[SerializeField]
	private Sprite cityPortraitBoxVictory;

	[SerializeField]
	private Sprite modifierBoxDefeat;

	[SerializeField]
	private Sprite modifierBoxVictory;

	[SerializeField]
	private RectTransform glyphsRectTransform;

	[SerializeField]
	private float glyphsUnfoldedPositionY;

	[SerializeField]
	private TextMeshProUGUI glyphsText;

	[SerializeField]
	private Image glyphsBox;

	[SerializeField]
	private TextMeshProUGUI glyphsCustomModeText;

	[SerializeField]
	private HUDJoystickTarget glyphsJoystickTarget;

	[SerializeField]
	private JoystickSelectable glyphsSelectable;

	[SerializeField]
	private JoystickSelectable weaponRestrictionsSelectable;

	[SerializeField]
	private RectTransform apocalypseRectTransform;

	[SerializeField]
	private ApocalypseLevelView chosenApocalypseView;

	[SerializeField]
	private float apocalypseUnfoldedPositionY;

	[SerializeField]
	private Image apocalypseModeBox;

	[SerializeField]
	private ApocalypseEffectsTooltip currentApocalypseTooltip;

	[SerializeField]
	private HUDJoystickTarget apocalypseJoystickTarget;

	[SerializeField]
	private JoystickSelectable apocalypseSelectable;

	[SerializeField]
	private PlayableReportDisplay playableReportPrefab;

	[SerializeField]
	private RectTransform playableReportParent;

	[SerializeField]
	private RectTransform playableBoardMask;

	[SerializeField]
	private CanvasGroup playableReportCanvasGroup;

	[SerializeField]
	private GameObject playableReportLeftButton;

	[SerializeField]
	private GameObject playableReportRightButton;

	[SerializeField]
	private HUDJoystickSimpleTarget playableJoystickTarget;

	[SerializeField]
	private LayoutNavigationInitializer playableNavigationInitializer;

	[SerializeField]
	private CanvasGroup statsCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI statsTitle;

	[SerializeField]
	private TextMeshProUGUI firstStatLine;

	[SerializeField]
	private TextMeshProUGUI secondStatLine;

	[SerializeField]
	private TextMeshProUGUI thirdStatLine;

	[SerializeField]
	private CanvasGroup unlocksCanvasGroup;

	[SerializeField]
	private ApocalypseLevelView unlockedApocalypseView;

	[SerializeField]
	private TextMeshProUGUI unlockTitle;

	[SerializeField]
	private GameObject unlockParticlesSquare;

	[SerializeField]
	private GameObject unlockParticlesRays;

	[SerializeField]
	private ApocalypseEffectsTooltip unlockedApocalypseTooltip;

	[SerializeField]
	private HUDJoystickTarget unlockJoystickTarget;

	[SerializeField]
	private JoystickSelectable unlocksSelectable;

	[SerializeField]
	private CanvasGroup continueCanvasGroup;

	[SerializeField]
	private CanvasGroup backCanvasGroup;

	private Canvas canvas;

	private CanvasGroup canvasGroup;

	private bool firstTimeOpened = true;

	private bool isOpened;

	private Tween moveTween;

	private int pageIndex;

	private float posYInit;

	private List<PlayableReportDisplay> playableReports = new List<PlayableReportDisplay>();

	private RectTransform rectTransform;

	private float unlocksAlphaTarget = 1f;

	private bool alreadySeenPlayableReport;

	public int OverlaySortingOrder => TPSingleton<GameOverPanel>.Instance.canvas.sortingOrder - 2;

	public Canvas Canvas => TPSingleton<GameOverPanel>.Instance.canvas;

	public void Close(bool toAnotherPopup = false)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (isOpened)
		{
			CLoggerManager.Log((object)"GameOverPanel closed", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
			if (!toAnotherPopup)
			{
				CameraView.AttenuateWorldForPopupFocus(null);
			}
			if (moveTween != null)
			{
				TweenExtensions.Kill(moveTween, false);
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);
			}
			isOpened = false;
			moveTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(rectTransform, posYInit, 0.25f, false), (Ease)26).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("GameOverPanelClose", (Component)(object)this), (TweenCallback)delegate
			{
				((Behaviour)canvas).enabled = false;
				soulsRewardPanel.ClearTrophies();
			});
			canvasGroup.blocksRaycasts = false;
			alreadySeenPlayableReport = false;
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		}
	}

	public void OnLeftPlayableReportButtonClick()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Transform)TPSingleton<GameOverPanel>.Instance.playableReportParent).localPosition = new Vector3(((Transform)TPSingleton<GameOverPanel>.Instance.playableReportParent).localPosition.x + 219f, ((Transform)TPSingleton<GameOverPanel>.Instance.playableReportParent).localPosition.y, ((Transform)TPSingleton<GameOverPanel>.Instance.playableReportParent).localPosition.z);
	}

	public void OnMainMenuClick()
	{
		TPSingleton<ApocalypseManager>.Instance.FreshlyUnlockedApocalypse = false;
		firstTimeOpened = true;
		GameController.GoBackToMainMenu();
	}

	public void OnNextClick()
	{
		SetPageIndex(pageIndex + 1);
	}

	public void OnBackClick()
	{
		if (pageIndex != 0)
		{
			SetPageIndex(pageIndex - 1);
		}
	}

	public void Open()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!isOpened)
		{
			CLoggerManager.Log((object)"GameOverPanel opened", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
			CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)this);
			if (moveTween != null)
			{
				TweenExtensions.Kill(moveTween, false);
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posYInit);
			}
			moveTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(rectTransform, panelDeltaPosY, 0.25f, false), (Ease)27).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("GameOverPanelOpen", (Component)(object)this);
			OnOpenRelease();
			SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
			if (obj != null)
			{
				((FontLocalizedParent)obj).RefreshChilds();
			}
			RefreshText();
			((Behaviour)canvas).enabled = true;
			canvasGroup.blocksRaycasts = true;
			isOpened = true;
			firstTimeOpened = false;
		}
	}

	public void OnRestartClick()
	{
		GameManager.DebugReloadGameScene();
	}

	public void OnRightPlayableReportButtonClick()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Transform)TPSingleton<GameOverPanel>.Instance.playableReportParent).localPosition = new Vector3(((Transform)TPSingleton<GameOverPanel>.Instance.playableReportParent).localPosition.x - 219f, ((Transform)TPSingleton<GameOverPanel>.Instance.playableReportParent).localPosition.y, ((Transform)TPSingleton<GameOverPanel>.Instance.playableReportParent).localPosition.z);
	}

	public void RefreshPlayables()
	{
		foreach (PlayableReportDisplay playableReport in playableReports)
		{
			playableReport.RefreshView();
		}
	}

	public void SelectPlayablePanelJoystick()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(playableJoystickTarget.GetSelectionInfo());
		}
	}

	public void SkipAnimations()
	{
		Time.timeScale = 50f;
		skipButtonParent.SetActive(false);
	}

	public void OnTrophiesAnimationEnd()
	{
		Time.timeScale = 1f;
		skipButtonParent.SetActive(false);
		buttonParent.SetActive(true);
	}

	public void Update()
	{
		if (isOpened && ((Behaviour)Canvas).enabled)
		{
			if (InputManager.GetButtonDown(7))
			{
				OnNextClick();
			}
			else if (InputManager.GetButtonDown(80))
			{
				OnBackClick();
			}
		}
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshText();
		}
	}

	private void RefreshText()
	{
		string text = string.Empty;
		switch (TPSingleton<GameManager>.Instance.Game.GameOverCause)
		{
		case Game.E_GameOverCause.MagicSealsCompleted:
			text = Localizer.Get("GameOverPanel_Cause_MagicSealsCompleted");
			break;
		case Game.E_GameOverCause.MagicCircleDestroyed:
			text = Localizer.Get("GameOverPanel_Cause_MagicCircleDestroyed");
			break;
		case Game.E_GameOverCause.HeroesDeath:
			text = Localizer.Get("GameOverPanel_Cause_HeroesDeath");
			break;
		}
		((TMP_Text)cityTitle).text = $"{TPSingleton<WorldMapCityManager>.Instance.SelectedCity?.CityDefinition.Name} #{TPSingleton<WorldMapCityManager>.Instance.SelectedCity?.NumberOfRuns}";
		((TMP_Text)gameOverTitle).text = text;
		((TMP_Text)nextMessage).text = Localizer.Get("GameOverPanel_Next");
		((TMP_Text)glyphsText).text = Localizer.Get("Glyphs_Title");
		((TMP_Text)statsTitle).text = Localizer.Get("GameOverPanel_StatsTitle");
		((TMP_Text)firstStatLine).text = Localizer.Format("GameOverPanel_FirstStatLine", new object[1] { TPSingleton<GameManager>.Instance.Game.DayNumber });
		TimeSpan timeSpan = TimeSpan.FromSeconds(TPSingleton<GameManager>.Instance.TotalTimeSpent);
		((TMP_Text)secondStatLine).text = Localizer.Format("GameOverPanel_SecondStatLine", new object[3]
		{
			Math.Floor(timeSpan.TotalHours),
			timeSpan.Minutes,
			timeSpan.Seconds
		});
		((TMP_Text)thirdStatLine).text = Localizer.Format("GameOverPanel_ThirdStatLine", new object[1] { ApplicationManager.Application.RunsCompleted });
		((TMP_Text)unlockTitle).text = Localizer.Get("GameOverPanel_UnlocksTitle");
	}

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		canvas = ((Component)TPSingleton<GameOverPanel>.Instance).GetComponent<Canvas>();
		((Behaviour)canvas).enabled = false;
		canvasGroup = ((Component)TPSingleton<GameOverPanel>.Instance).GetComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		isOpened = false;
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		posYInit = rectTransform.anchoredPosition.y;
		buttonParent.SetActive(false);
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnOpenRelease()
	{
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		bool isVictory = TPSingleton<GameManager>.Instance.Game.IsVictory;
		cityPortrait.sprite = ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/Cities/Portraits/WorldMap_CityPortrait_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + (isVictory ? "02" : "03"));
		cityPortraitBox.sprite = (isVictory ? cityPortraitBoxVictory : cityPortraitBoxDefeat);
		glyphsBox.sprite = (isVictory ? modifierBoxVictory : modifierBoxDefeat);
		apocalypseModeBox.sprite = (isVictory ? modifierBoxVictory : modifierBoxDefeat);
		gameOverPanel.sprite = (isVictory ? victoryBGPageTwo : defeatBGPageOne);
		SetPageIndex(0);
		if (!isVictory)
		{
			CanvasFadeManager.FadeIn(blackBGFadeInDuration, canvas.sortingOrder - 1, (Ease)0);
		}
		soulsRewardPanel.RefreshPosition();
		soulsRewardPanel.Display(firstTimeOpenedThisNight: true);
		chosenApocalypseView.Init(ApocalypseManager.CurrentApocalypseIndex);
		currentApocalypseTooltip.SetApocalypsesToDisplay(ApocalypseManager.CurrentApocalypseIndex);
		if (TPSingleton<ApocalypseManager>.Instance.FreshlyUnlockedApocalypse)
		{
			unlockedApocalypseView.Init(TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable);
			unlocksAlphaTarget = 1f;
			unlockedApocalypseTooltip.SetApocalypsesToDisplay(TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable, TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable);
			unlockedApocalypseTooltip.SetDamnedSoulsPercentageModifier((uint)TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable * ApocalypseDatabase.ConfigurationDefinition.DamnedSoulsPercentagePerLevel);
			unlockJoystickTarget.NavigationEnabled = true;
		}
		else
		{
			unlockedApocalypseView.Init(-1);
			unlocksCanvasGroup.blocksRaycasts = false;
			unlockJoystickTarget.NavigationEnabled = false;
		}
		if (firstTimeOpened)
		{
			playableReportCanvasGroup.alpha = 0f;
			statsCanvasGroup.alpha = 0f;
			unlocksCanvasGroup.alpha = 0f;
			continueCanvasGroup.alpha = 0f;
			continueCanvasGroup.interactable = false;
			((MonoBehaviour)this).StartCoroutine(ShowSoulsRewardPanel());
			playableReportLeftButton.SetActive(playableReportParent.sizeDelta.x > playableBoardMask.sizeDelta.x);
			playableReportRightButton.SetActive(playableReportParent.sizeDelta.x > playableBoardMask.sizeDelta.x);
		}
	}

	private void SetPageIndex(int value)
	{
		int num = pageIndex;
		pageIndex = value;
		TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		switch (pageIndex)
		{
		case 0:
			pageOne.SetActive(true);
			gameOverPanel.sprite = (TPSingleton<GameManager>.Instance.Game.IsVictory ? victoryBGPageOne : defeatBGPageOne);
			((Component)backCanvasGroup).gameObject.SetActive(false);
			pageTwo.SetActive(false);
			if (num == 1)
			{
				soulsRewardPanel.Display(firstTimeOpenedThisNight: false);
			}
			break;
		case 1:
			soulsRewardPanel.Hide();
			pageOne.SetActive(false);
			gameOverPanel.sprite = (TPSingleton<GameManager>.Instance.Game.IsVictory ? victoryBGPageTwo : defeatBGPageTwo);
			((Component)backCanvasGroup).gameObject.SetActive(true);
			pageTwo.SetActive(true);
			if (!alreadySeenPlayableReport)
			{
				((MonoBehaviour)this).StartCoroutine(ShowPlayableReports());
			}
			else
			{
				SelectPlayablePanelJoystick();
			}
			break;
		default:
			TPSingleton<ApocalypseManager>.Instance.FreshlyUnlockedApocalypse = false;
			firstTimeOpened = true;
			if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsLastMap && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.NumberOfWins == 1)
			{
				ApplicationManager.Application.ApplicationController.SetState("Credits");
			}
			else
			{
				GameController.GoToMetaShops();
			}
			break;
		}
	}

	private IEnumerator ShowSoulsRewardPanel()
	{
		yield return soulsRewardPanel.ShowTrophiesPanel(TPSingleton<GameManager>.Instance.Game.IsDefeat);
		OnTrophiesAnimationEnd();
		((MonoBehaviour)this).StartCoroutine(ShowEasyModeAndApocalypse());
	}

	private IEnumerator ShowEasyModeAndApocalypse()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Count > 0 || ApocalypseManager.CurrentApocalypseIndex > 0)
		{
			yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingEasyModeAndApocalypse);
			bool flag = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CurrentGlyphPoints > 0;
			bool flag2 = !TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.AreAllUnlockedFamiliesSelected();
			bool flag3 = ApocalypseManager.CurrentApocalypseIndex > 0;
			if (flag || flag2)
			{
				glyphsJoystickTarget.NavigationEnabled = true;
				TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(glyphsRectTransform, glyphsUnfoldedPositionY, unfoldDuration, false), unfoldEasing).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("easyModeUnfoldTween", (Component)(object)this);
				((Component)glyphsSelectable).gameObject.SetActive(flag);
				if (flag)
				{
					((Selectable)(object)glyphsSelectable).SetMode((Mode)4);
					((Selectable)(object)glyphsSelectable).SetSelectOnDown(soulsRewardPanel.GetFirstSelectableTrophy());
					((Selectable)(object)glyphsSelectable).SetSelectOnRight((Selectable)(object)(flag2 ? weaponRestrictionsSelectable : (flag3 ? apocalypseSelectable : null)));
					((Selectable)(object)apocalypseSelectable).SetSelectOnLeft((Selectable)(object)glyphsSelectable);
				}
				((Component)weaponRestrictionsSelectable).gameObject.SetActive(flag2);
				if (flag2)
				{
					((Selectable)(object)weaponRestrictionsSelectable).SetMode((Mode)4);
					((Selectable)(object)weaponRestrictionsSelectable).SetSelectOnDown(soulsRewardPanel.GetFirstSelectableTrophy());
					((Selectable)(object)weaponRestrictionsSelectable).SetSelectOnLeft((Selectable)(object)(flag ? glyphsSelectable : null));
					((Selectable)(object)weaponRestrictionsSelectable).SetSelectOnRight((Selectable)(object)(flag3 ? apocalypseSelectable : null));
					((Selectable)(object)apocalypseSelectable).SetSelectOnLeft((Selectable)(object)weaponRestrictionsSelectable);
				}
				if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled)
				{
					((Behaviour)glyphsCustomModeText).enabled = true;
					((TMP_Text)glyphsCustomModeText).text = $"+{TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GetCustomModeBonusPoints()}";
				}
			}
			else
			{
				glyphsJoystickTarget.NavigationEnabled = false;
			}
			if (flag3)
			{
				TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(apocalypseRectTransform, apocalypseUnfoldedPositionY, unfoldDuration, false), unfoldEasing).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("apocalypseUnfoldTween", (Component)(object)this);
				apocalypseJoystickTarget.NavigationEnabled = true;
				((Selectable)(object)apocalypseSelectable).SetMode((Mode)4);
				((Selectable)(object)apocalypseSelectable).SetSelectOnDown(soulsRewardPanel.GetFirstSelectableTrophy());
			}
			else
			{
				apocalypseJoystickTarget.NavigationEnabled = false;
			}
		}
		else
		{
			glyphsJoystickTarget.NavigationEnabled = false;
			apocalypseJoystickTarget.NavigationEnabled = false;
		}
		((MonoBehaviour)this).StartCoroutine(ShowStats());
	}

	private IEnumerator ShowPlayableReports()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingPlayableReports);
		int count = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count;
		int i = 0;
		for (int j = playableReports.Count; j < count; j++)
		{
			PlayableReportDisplay playableReportDisplay = Object.Instantiate<PlayableReportDisplay>(TPSingleton<GameOverPanel>.Instance.playableReportPrefab, (Transform)(object)TPSingleton<GameOverPanel>.Instance.playableReportParent);
			playableReportDisplay.Init(playableReportParent, playableBoardMask);
			playableReports.Add(playableReportDisplay);
		}
		for (; i < count; i++)
		{
			((Component)playableReports[i]).gameObject.SetActive(true);
			playableReports[i].Refresh(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i], isDead: false, isEndGamePanel: true);
		}
		int num = 0;
		foreach (KeyValuePair<int, List<PlayableUnit>> deadPlayableUnit in TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits)
		{
			for (int k = playableReports.Count; k < i + num + deadPlayableUnit.Value.Count; k++)
			{
				PlayableReportDisplay playableReportDisplay2 = Object.Instantiate<PlayableReportDisplay>(TPSingleton<GameOverPanel>.Instance.playableReportPrefab, (Transform)(object)TPSingleton<GameOverPanel>.Instance.playableReportParent);
				playableReportDisplay2.Init(playableReportParent, playableBoardMask);
				playableReports.Add(playableReportDisplay2);
			}
			foreach (PlayableUnit item in deadPlayableUnit.Value)
			{
				((Component)playableReports[i + num]).gameObject.SetActive(true);
				playableReports[i + num].Refresh(item, isDead: true, isEndGamePanel: true, deadPlayableUnit.Key);
				num++;
			}
		}
		for (int l = i + num; l < playableReports.Count; l++)
		{
			((Component)playableReports[l]).gameObject.SetActive(false);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)((Component)playableNavigationInitializer).transform);
		playableNavigationInitializer.InitNavigation();
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			playableJoystickTarget.AddSelectables((IEnumerable<Selectable>)playableReports.Select((PlayableReportDisplay x) => x.Selectable));
			SelectPlayablePanelJoystick();
		}
		TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(playableReportCanvasGroup, 1f, 1f), (Ease)9).SetFullId<TweenerCore<float, float, FloatOptions>>("PlayableReportFadeIn", (Component)(object)this);
		alreadySeenPlayableReport = true;
	}

	private IEnumerator ShowStats()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingStats);
		TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(statsCanvasGroup, 1f, 1f), (Ease)9).SetFullId<TweenerCore<float, float, FloatOptions>>("StatsFadeIn", (Component)(object)this);
		((MonoBehaviour)this).StartCoroutine(ShowUnlocks());
	}

	private IEnumerator ShowUnlocks()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingUnlocks);
		unlockParticlesSquare.SetActive(TPSingleton<ApocalypseManager>.Instance.FreshlyUnlockedApocalypse);
		unlockParticlesRays.SetActive(TPSingleton<ApocalypseManager>.Instance.FreshlyUnlockedApocalypse);
		TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(unlocksCanvasGroup, unlocksAlphaTarget, 1f), (Ease)9).SetFullId<TweenerCore<float, float, FloatOptions>>("UnlocksFadeIn", (Component)(object)this);
		((Selectable)(object)unlocksSelectable).SetMode((Mode)4);
		((Selectable)(object)unlocksSelectable).SetSelectOnUp(soulsRewardPanel.GetFirstSelectableTrophy());
		((MonoBehaviour)this).StartCoroutine(ShowContinueButton());
	}

	private IEnumerator ShowContinueButton()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingContinue);
		TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(continueCanvasGroup, 1f, 1f), (Ease)9).SetFullId<TweenerCore<float, float, FloatOptions>>("ContinueFadeIn", (Component)(object)this), (TweenCallback)delegate
		{
			continueCanvasGroup.interactable = true;
		});
	}

	[ContextMenu("Open")]
	public void DebugOpen()
	{
		if (!Application.isPlaying)
		{
			TPDebug.LogError((object)"Unable to use this context menu when the application is not running", (Object)(object)this);
		}
		else if (!TPSingleton<GameOverPanel>.Instance.isOpened)
		{
			Open();
		}
	}

	[ContextMenu("Close")]
	public void DebugClose()
	{
		if (!Application.isPlaying)
		{
			TPDebug.LogError((object)"Unable to use this context menu when the application is not running", (Object)(object)this);
		}
		else if (TPSingleton<GameOverPanel>.Instance.isOpened)
		{
			Close();
		}
	}
}
