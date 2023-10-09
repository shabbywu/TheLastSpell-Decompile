using System;
using System.Collections;
using System.Collections.Generic;
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
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.ProductionReport;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using TheLastStand.View.Panic;
using TheLastStand.View.SoulsReward;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.NightReport;

public class NightReportPanel : TPSingleton<NightReportPanel>, IOverlayUser
{
	public static class Constants
	{
		public const string CommonLocalizedFontChildren = "Common";

		public const string PageOneLocalizedFontChildren = "PageOne";

		public const string PageTwoLocalizedFontChildren = "PageTwo";
	}

	[SerializeField]
	private float panelDeltaPosY = -20f;

	[SerializeField]
	private Animator titleAnimator;

	[SerializeField]
	private DataSpriteTable smallRankStamp;

	[SerializeField]
	private DataSpriteTable bigRankStamp;

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	[SerializeField]
	private Image page1Cache;

	[SerializeField]
	private CanvasGroup xpPanelCanvasGroup;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitAfterTitleAppear = 1f;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitBeforeShowingAllKillsReports = 0.5f;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitBetweenEachKillsReportAppear = 0.3f;

	[SerializeField]
	private KillReportDisplay killReportPrefab;

	[SerializeField]
	private RectTransform killReportParent;

	[SerializeField]
	private RectTransform killsBoardMask;

	[SerializeField]
	private BetterButton killReportLeftButton;

	[SerializeField]
	private BetterButton killReportRightButton;

	[SerializeField]
	[Range(1f, 3f)]
	private float stampPunchStrength = 1.25f;

	[SerializeField]
	[Range(1f, 3f)]
	private float sharedXPPunchStrength = 1.5f;

	[SerializeField]
	[Range(0f, 5f)]
	private float stampPunchTweenDuration = 0.4f;

	[SerializeField]
	[Range(0f, 5f)]
	private float stampFadeTweenDuration = 0.4f;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("Can't be higher than waitBetweenEachKillsReportAppear")]
	private float sharedXPPunchTweenDuration = 0.2f;

	[SerializeField]
	private TextMeshProUGUI sharedXPText;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("From the end of kill reports appearing")]
	private float waitBeforeShowingPlayableReports;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitBeforePlayableXPAnimation = 0.3f;

	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("Playable XP animation can continue when the next objects appear")]
	private float waitAfterXPAnimationBeginning = 1f;

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
	private ScrollRect playablesScrollRect;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitAfterBottomCanvasAppear = 0.5f;

	[SerializeField]
	private CanvasGroup battleReportCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI unitAliveValueText;

	[SerializeField]
	private TextMeshProUGUI hpLostValueText;

	[SerializeField]
	private TextMeshProUGUI deadUnitValueText;

	[SerializeField]
	private Image battleReportRankStamp;

	[SerializeField]
	private CanvasGroup page1ContinueCanvasGroup;

	[SerializeField]
	private ScrollRect killsScrollRect;

	[SerializeField]
	private CanvasGroup page1SkipCanvasGroup;

	[SerializeField]
	private Image page2Cache;

	[SerializeField]
	private CanvasGroup page2SkipCanvasGroup;

	[SerializeField]
	private CanvasGroup nightRewardCanvasGroup;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitAfterNightRwardCanvasAppear = 1f;

	[SerializeField]
	private PanicPanel nightReportPanicPanel;

	[SerializeField]
	private PanicRewardIndicator panicRewardIndicator;

	[SerializeField]
	private Image panicRankStamp;

	[SerializeField]
	private CanvasGroup nightRewardContainerCanvasGroup;

	[SerializeField]
	private HUDJoystickSimpleTarget rewardJoystickTarget;

	[SerializeField]
	private LayoutNavigationInitializer layoutRewardNavigationInitializer;

	[SerializeField]
	private SoulsRewardPanel soulsRewardPanel;

	[SerializeField]
	private CanvasGroup nightRatingCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI nightCountText;

	[SerializeField]
	private TextMeshProUGUI commandersSentence;

	[SerializeField]
	private Image nightRankStamp;

	[SerializeField]
	private AudioSource audioSourceTemplate;

	[SerializeField]
	private int audioSourcesCount = 5;

	[SerializeField]
	private AudioClip fireAudioClip;

	[SerializeField]
	private AudioClip[] killReportAudioClips;

	[SerializeField]
	private AudioClip gainXPAudioClip;

	[SerializeField]
	private AudioClip stampAudioClip;

	[SerializeField]
	private AudioClip finalStampAudioClip;

	[SerializeField]
	private AudioClip characterAppearAudioClip;

	[SerializeField]
	private AudioClip rewardsAppearAudioClip;

	private Canvas canvas;

	private CanvasGroup canvasGroup;

	private RectTransform rectTransform;

	private Tween moveTween;

	private Tween sharedXPTextTween;

	private float posYInit;

	private int tabIndex;

	private bool firstTimeOpenedThisNight = true;

	private bool isOpened;

	private readonly List<KillReportDisplay> killReports = new List<KillReportDisplay>();

	private readonly List<PlayableReportDisplay> playableReports = new List<PlayableReportDisplay>();

	private bool wantToSkipAnimations;

	private AudioSource[] audioSources;

	private int nextAudioSourceIndex;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public void Close()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (isOpened)
		{
			CLoggerManager.Log((object)"NightReportPanel closed", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
			CameraView.AttenuateWorldForPopupFocus(null);
			if (moveTween != null)
			{
				TweenExtensions.Kill(moveTween, false);
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);
			}
			isOpened = false;
			moveTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(rectTransform, posYInit, 0.25f, false), (Ease)26).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("NightReportPanelClose", (Component)(object)this), (TweenCallback)delegate
			{
				((Behaviour)canvas).enabled = false;
				GameView.TopScreenPanel.TurnPanel.Refresh();
				SoundManager.PlayAudioClip(GameManager.AudioSource, GameManager.ProductionPhaseAudioClip);
			});
			firstTimeOpenedThisNight = true;
			canvasGroup.blocksRaycasts = false;
			soulsRewardPanel.ClearTrophies();
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		}
	}

	public void Update()
	{
		if (!isOpened)
		{
			return;
		}
		if (InputManager.GetButtonDown(7))
		{
			if (((Behaviour)page1Cache).enabled && page1SkipCanvasGroup.alpha != 0f)
			{
				SkipPageOneAnimations();
			}
			else if (((Behaviour)page2Cache).enabled && page2SkipCanvasGroup.alpha != 0f)
			{
				SkipPageTwoAnimations();
			}
			else
			{
				OnContinueClick();
			}
		}
		else if (InputManager.GetButtonDown(80) && ((Behaviour)page2Cache).enabled && page2SkipCanvasGroup.alpha == 0f)
		{
			OnBackToPage1Click();
		}
	}

	public void Open()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (!isOpened)
		{
			CLoggerManager.Log((object)"NightReportPanel opened", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
			CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)this);
			GameView.TopScreenPanel.Display(show: false);
			if ((Object)(object)complexFontLocalizedParent != (Object)null)
			{
				complexFontLocalizedParent.TargetKey = "Common";
				((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
			}
			if (moveTween != null)
			{
				TweenExtensions.Kill(moveTween, false);
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posYInit);
			}
			tabIndex = 0;
			page1SkipCanvasGroup.alpha = 1f;
			page1SkipCanvasGroup.blocksRaycasts = true;
			OpenPage1();
			((Behaviour)canvas).enabled = true;
			canvasGroup.blocksRaycasts = true;
			soulsRewardPanel.RefreshPosition();
			isOpened = true;
		}
	}

	public void OnBackToPage1Click()
	{
		tabIndex = 0;
		OpenPage1();
	}

	public void OnContinueClick()
	{
		switch (tabIndex)
		{
		case 0:
			tabIndex = 1;
			OpenPage2();
			break;
		case 1:
			TPSingleton<PlayableUnitManager>.Instance.NightReport.NightReportController.CloseNightReportPanel();
			break;
		}
	}

	public void OnLeftKillsReportButtonClick()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition = new Vector3(((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition.x + 114f, ((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition.y, ((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition.z);
		TPSingleton<NightReportPanel>.Instance.ToggleKillsReportButtons();
	}

	public void OnRightKillsReportButtonClick()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition = new Vector3(((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition.x - 114f, ((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition.y, ((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition.z);
		TPSingleton<NightReportPanel>.Instance.ToggleKillsReportButtons();
	}

	public void OnLeftPlayableReportButtonClick()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Transform)TPSingleton<NightReportPanel>.Instance.playableReportParent).localPosition = new Vector3(((Transform)TPSingleton<NightReportPanel>.Instance.playableReportParent).localPosition.x + 219f, ((Transform)TPSingleton<NightReportPanel>.Instance.playableReportParent).localPosition.y, ((Transform)TPSingleton<NightReportPanel>.Instance.playableReportParent).localPosition.z);
	}

	public void OnRightPlayableReportButtonClick()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Transform)TPSingleton<NightReportPanel>.Instance.playableReportParent).localPosition = new Vector3(((Transform)TPSingleton<NightReportPanel>.Instance.playableReportParent).localPosition.x - 219f, ((Transform)TPSingleton<NightReportPanel>.Instance.playableReportParent).localPosition.y, ((Transform)TPSingleton<NightReportPanel>.Instance.playableReportParent).localPosition.z);
	}

	public void PlayAudioClip(AudioClip audioClip, float delay = 0f, bool doNotInterrupt = false)
	{
		SoundManager.PlayAudioClip(GetNextAudioSource(), audioClip, delay, doNotInterrupt);
	}

	public void RefreshPlayables()
	{
		foreach (PlayableReportDisplay playableReport in playableReports)
		{
			playableReport.RefreshView();
		}
	}

	public AudioSource GetNextAudioSource()
	{
		return audioSources[nextAudioSourceIndex++ % audioSources.Length];
	}

	public void SkipPageOneAnimations()
	{
		Time.timeScale = 50f;
		wantToSkipAnimations = true;
		page1SkipCanvasGroup.alpha = 0f;
		page1SkipCanvasGroup.blocksRaycasts = false;
	}

	public void SkipPageTwoAnimations()
	{
		Time.timeScale = 50f;
		wantToSkipAnimations = true;
		page2SkipCanvasGroup.alpha = 0f;
		page2SkipCanvasGroup.blocksRaycasts = false;
	}

	private void AddKillReport(int killReportIndex, KillReportData killReportData, ref int totalXP)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		((Component)TPSingleton<NightReportPanel>.Instance.killReports[killReportIndex]).gameObject.SetActive(true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(TPSingleton<NightReportPanel>.Instance.killReportParent);
		if (TPSingleton<NightReportPanel>.Instance.killReportParent.sizeDelta.x > TPSingleton<NightReportPanel>.Instance.killsBoardMask.sizeDelta.x)
		{
			((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition = new Vector3(((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition.x - 1000f, ((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition.y, ((Transform)TPSingleton<NightReportPanel>.Instance.killReportParent).localPosition.z);
		}
		int tweenSharedXP = totalXP;
		int num = (int)killReportData.TotalExperienceToShare;
		TPSingleton<NightReportPanel>.Instance.killReports[killReportIndex].Refresh(killReportData.SpecificAssetsId, killReportData.KillAmount, num);
		totalXP += num;
		Tween obj = sharedXPTextTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		sharedXPTextTween = (Tween)(object)DOTween.To((DOGetter<int>)(() => tweenSharedXP), (DOSetter<int>)delegate(int x)
		{
			tweenSharedXP = x;
			((TMP_Text)TPSingleton<NightReportPanel>.Instance.sharedXPText).text = $"{tweenSharedXP}";
		}, totalXP, waitBetweenEachKillsReportAppear).SetFullId<TweenerCore<int, int, NoOptions>>("SharedXPTransfer", (Component)(object)this);
		if (!PlayableUnitManager.DebugForceSkipNightReport)
		{
			ShortcutExtensions.DOPunchScale(((TMP_Text)TPSingleton<NightReportPanel>.Instance.sharedXPText).transform, Vector3.one * TPSingleton<NightReportPanel>.Instance.sharedXPPunchStrength, TPSingleton<NightReportPanel>.Instance.sharedXPPunchTweenDuration, 1, 0.1f).SetFullId<Tweener>("SharedXPPunchScale", (Component)(object)this);
		}
	}

	private void CollectGoldReward()
	{
		if (PanicManager.Panic.PanicReward.Gold > 0)
		{
			TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + PanicManager.Panic.PanicReward.Gold);
		}
	}

	private void CollectItemReward()
	{
		if (PanicManager.Panic.PanicReward.HasAtLeastOneItem)
		{
			ProductionItems productionItem = new ProductionItemController().ProductionItem;
			productionItem.IsNightProduction = true;
			TheLastStand.Model.Item.Item[] items = PanicManager.Panic.PanicReward.Items;
			foreach (TheLastStand.Model.Item.Item item in items)
			{
				productionItem.Items.Add(item);
			}
			TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.AddProductionObject(productionItem);
		}
	}

	private void CollectMaterialsReward()
	{
		if (PanicManager.Panic.PanicReward.Materials > 0)
		{
			TPSingleton<ResourceManager>.Instance.Materials += PanicManager.Panic.PanicReward.Materials;
		}
	}

	private void OpenPage1()
	{
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Expected O, but got Unknown
		nightRewardCanvasGroup.alpha = 0f;
		nightRewardCanvasGroup.blocksRaycasts = false;
		nightRatingCanvasGroup.alpha = 0f;
		nightRatingCanvasGroup.blocksRaycasts = false;
		page2SkipCanvasGroup.alpha = 0f;
		page2SkipCanvasGroup.blocksRaycasts = false;
		((Behaviour)page2Cache).enabled = false;
		soulsRewardPanel.Hide();
		TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		((Behaviour)playablesScrollRect).enabled = true;
		((Behaviour)killsScrollRect).enabled = true;
		((Behaviour)page1Cache).enabled = true;
		if (firstTimeOpenedThisNight)
		{
			if ((Object)(object)complexFontLocalizedParent != (Object)null)
			{
				complexFontLocalizedParent.TargetKey = "PageOne";
				((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
			}
			TPSingleton<PlayableUnitManager>.Instance.NightReport.NightReportController.GetTonightRanks();
			((Component)playableReportParent).gameObject.SetActive(true);
			titleAnimator.SetTrigger("Hide");
			xpPanelCanvasGroup.alpha = 0f;
			xpPanelCanvasGroup.blocksRaycasts = false;
			((TMP_Text)sharedXPText).text = string.Empty;
			playableReportCanvasGroup.alpha = 0f;
			battleReportCanvasGroup.alpha = 0f;
			battleReportCanvasGroup.blocksRaycasts = false;
			((TMP_Text)unitAliveValueText).text = string.Empty;
			((TMP_Text)hpLostValueText).text = string.Empty;
			((TMP_Text)deadUnitValueText).text = string.Empty;
			((Graphic)battleReportRankStamp).color = new Color(((Graphic)battleReportRankStamp).color.r, ((Graphic)battleReportRankStamp).color.g, ((Graphic)battleReportRankStamp).color.b, 0f);
			battleReportRankStamp.sprite = smallRankStamp.GetSpriteAt(TPSingleton<PlayableUnitManager>.Instance.NightReport.BattleRank);
			page1ContinueCanvasGroup.alpha = 0f;
			page1ContinueCanvasGroup.blocksRaycasts = false;
			((Component)killReportLeftButton).gameObject.SetActive(false);
			((Component)killReportRightButton).gameObject.SetActive(false);
			playableReportLeftButton.SetActive(false);
			playableReportRightButton.SetActive(false);
			nightReportPanicPanel.Refresh(PanicManager.Panic);
			((Transform)killReportParent).localPosition = new Vector3(0f, ((Transform)killReportParent).localPosition.y, ((Transform)killReportParent).localPosition.z);
			((Transform)playableReportParent).localPosition = new Vector3(0f, ((Transform)playableReportParent).localPosition.y, ((Transform)playableReportParent).localPosition.z);
			for (int i = 0; i < killReports.Count; i++)
			{
				((Component)killReports[i]).gameObject.SetActive(false);
			}
			moveTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(rectTransform, panelDeltaPosY, 0.25f, false), (Ease)27).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("NightReportPanelOpen", (Component)(object)this), (TweenCallback)delegate
			{
				titleAnimator.SetTrigger("FadeIn");
				((MonoBehaviour)this).StartCoroutine(ShowKills());
			});
			PlayAudioClip(fireAudioClip);
		}
		else
		{
			((Component)playableReportParent).gameObject.SetActive(true);
			xpPanelCanvasGroup.alpha = 1f;
			xpPanelCanvasGroup.blocksRaycasts = true;
			battleReportCanvasGroup.alpha = 1f;
			battleReportCanvasGroup.blocksRaycasts = true;
		}
	}

	private void OpenPage2()
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		((Component)playableReportParent).gameObject.SetActive(false);
		xpPanelCanvasGroup.alpha = 0f;
		xpPanelCanvasGroup.blocksRaycasts = false;
		battleReportCanvasGroup.alpha = 0f;
		battleReportCanvasGroup.blocksRaycasts = false;
		((Behaviour)page1Cache).enabled = false;
		((Behaviour)playablesScrollRect).enabled = false;
		((Behaviour)killsScrollRect).enabled = false;
		soulsRewardPanel.Display(firstTimeOpenedThisNight);
		((Behaviour)page2Cache).enabled = true;
		if (firstTimeOpenedThisNight)
		{
			if ((Object)(object)complexFontLocalizedParent != (Object)null)
			{
				complexFontLocalizedParent.TargetKey = "PageTwo";
				((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
			}
			nightRewardCanvasGroup.alpha = 0f;
			nightRewardCanvasGroup.blocksRaycasts = false;
			nightRewardContainerCanvasGroup.alpha = 0f;
			nightRewardContainerCanvasGroup.blocksRaycasts = false;
			((Graphic)panicRankStamp).color = new Color(((Graphic)panicRankStamp).color.r, ((Graphic)panicRankStamp).color.g, ((Graphic)panicRankStamp).color.b, 0f);
			panicRankStamp.sprite = smallRankStamp.GetSpriteAt(TPSingleton<PlayableUnitManager>.Instance.NightReport.PanicRank);
			page2SkipCanvasGroup.alpha = 1f;
			page2SkipCanvasGroup.blocksRaycasts = true;
			nightRatingCanvasGroup.alpha = 0f;
			nightRatingCanvasGroup.blocksRaycasts = false;
			((Graphic)nightRankStamp).color = new Color(((Graphic)nightRankStamp).color.r, ((Graphic)nightRankStamp).color.g, ((Graphic)nightRankStamp).color.b, 0f);
			nightRankStamp.sprite = bigRankStamp.GetSpriteAt(TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightRank);
			nightRankStamp.sprite = bigRankStamp.GetSpriteAt(TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightRank);
			((TMP_Text)nightCountText).text = Localizer.Format("NightReportPanel_NightCount", new object[1] { TPSingleton<GameManager>.Instance.Game.DayNumber });
			((TMP_Text)commandersSentence).text = Localizer.Get("NightReportPanel_RankSentence_" + NightReportController.Constants.IndexToRankLabels[TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightRank]);
			panicRewardIndicator.Init(PanicManager.Panic);
			((MonoBehaviour)this).StartCoroutine(ShowNightRewardPanel());
		}
		else
		{
			nightRewardCanvasGroup.alpha = 1f;
			nightRewardCanvasGroup.blocksRaycasts = true;
			nightRatingCanvasGroup.alpha = 1f;
			nightRatingCanvasGroup.blocksRaycasts = true;
			page2SkipCanvasGroup.alpha = 0f;
			page2SkipCanvasGroup.blocksRaycasts = false;
		}
		firstTimeOpenedThisNight = false;
	}

	private IEnumerator ShowBattleReportPanel()
	{
		DOTweenModuleUI.DOFade(battleReportCanvasGroup, 1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 1f).SetFullId<TweenerCore<float, float, FloatOptions>>("BattleReportFadeIn", (Component)(object)this);
		battleReportCanvasGroup.blocksRaycasts = true;
		((TMP_Text)unitAliveValueText).text = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count.ToString();
		((TMP_Text)hpLostValueText).text = Mathf.FloorToInt(TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightHpLost).ToString();
		((TMP_Text)deadUnitValueText).text = (TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.ContainsKey(TPSingleton<GameManager>.Instance.DayNumber) ? TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber].Count.ToString() : "0");
		DOTweenModuleUI.DOFade(battleReportRankStamp, 1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampFadeTweenDuration).SetFullId<TweenerCore<Color, Color, ColorOptions>>("BattleReportRankStampFadeIn", (Component)(object)this);
		ShortcutExtensions.DOPunchScale((Transform)(object)((Graphic)battleReportRankStamp).rectTransform, Vector3.one * stampPunchStrength, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampPunchTweenDuration, 1, 0.1f).SetFullId<Tweener>("BattleReportRankStampFadeIn", (Component)(object)this);
		PlayAudioClip(stampAudioClip);
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitAfterBottomCanvasAppear);
		DOTweenModuleUI.DOFade(page1ContinueCanvasGroup, 1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 1f).SetFullId<TweenerCore<float, float, FloatOptions>>("BattleReportContinueFadeIn", (Component)(object)this);
		page1ContinueCanvasGroup.blocksRaycasts = true;
		page1SkipCanvasGroup.alpha = 0f;
		page1SkipCanvasGroup.blocksRaycasts = false;
		((Component)TPSingleton<NightReportPanel>.Instance.killReportLeftButton).gameObject.SetActive(TPSingleton<NightReportPanel>.Instance.killReportParent.sizeDelta.x > TPSingleton<NightReportPanel>.Instance.killsBoardMask.sizeDelta.x);
		((Component)TPSingleton<NightReportPanel>.Instance.killReportRightButton).gameObject.SetActive(TPSingleton<NightReportPanel>.Instance.killReportParent.sizeDelta.x > TPSingleton<NightReportPanel>.Instance.killsBoardMask.sizeDelta.x);
		TPSingleton<NightReportPanel>.Instance.playableReportLeftButton.SetActive(TPSingleton<NightReportPanel>.Instance.playableReportParent.sizeDelta.x > TPSingleton<NightReportPanel>.Instance.playableBoardMask.sizeDelta.x);
		TPSingleton<NightReportPanel>.Instance.playableReportRightButton.SetActive(TPSingleton<NightReportPanel>.Instance.playableReportParent.sizeDelta.x > TPSingleton<NightReportPanel>.Instance.playableBoardMask.sizeDelta.x);
		killReportRightButton.Interactable = false;
		if (wantToSkipAnimations)
		{
			Time.timeScale = 1f;
			wantToSkipAnimations = false;
		}
		if (PlayableUnitManager.DebugForceSkipNightReport)
		{
			OnContinueClick();
		}
	}

	private IEnumerator ShowKills()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitAfterTitleAppear);
		DOTweenModuleUI.DOFade(xpPanelCanvasGroup, 1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 0.5f).SetFullId<TweenerCore<float, float, FloatOptions>>("XPPanelFadeIn", (Component)(object)this);
		xpPanelCanvasGroup.blocksRaycasts = true;
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingAllKillsReports);
		int index = 0;
		int totalXP = 0;
		((TMP_Text)TPSingleton<NightReportPanel>.Instance.sharedXPText).text = $"{totalXP}";
		foreach (KillReportData item2 in TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight)
		{
			if (!item2.HideInNightReport)
			{
				if (TPSingleton<NightReportPanel>.Instance.killReports.Count <= index)
				{
					KillReportDisplay item = Object.Instantiate<KillReportDisplay>(TPSingleton<NightReportPanel>.Instance.killReportPrefab, (Transform)(object)TPSingleton<NightReportPanel>.Instance.killReportParent);
					TPSingleton<NightReportPanel>.Instance.killReports.Add(item);
				}
				PlayAudioClip(killReportAudioClips[Random.Range(0, killReportAudioClips.Length)]);
				AddKillReport(index, item2, ref totalXP);
				index++;
				yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBetweenEachKillsReportAppear);
			}
		}
		((MonoBehaviour)this).StartCoroutine(ShowPlayableReports());
	}

	private IEnumerator ShowPlayableReports()
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforeShowingPlayableReports);
		int playableUnitCount = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count;
		int num = (TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.ContainsKey(TPSingleton<GameManager>.Instance.DayNumber) ? TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber].Count : 0);
		int i;
		for (i = 0; i < playableUnitCount; i++)
		{
			while (playableReports.Count <= i)
			{
				PlayableReportDisplay item = Object.Instantiate<PlayableReportDisplay>(TPSingleton<NightReportPanel>.Instance.playableReportPrefab, (Transform)(object)TPSingleton<NightReportPanel>.Instance.playableReportParent);
				playableReports.Add(item);
			}
			((Component)playableReports[i]).gameObject.SetActive(true);
			playableReports[i].Refresh(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i]);
		}
		int j;
		for (j = 0; j < num; j++)
		{
			while (playableReports.Count <= i + j)
			{
				PlayableReportDisplay item2 = Object.Instantiate<PlayableReportDisplay>(TPSingleton<NightReportPanel>.Instance.playableReportPrefab, (Transform)(object)TPSingleton<NightReportPanel>.Instance.playableReportParent);
				playableReports.Add(item2);
			}
			((Component)playableReports[i + j]).gameObject.SetActive(true);
			playableReports[i + j].Refresh(TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber][j], isDead: true);
		}
		for (int k = i + j; k < playableReports.Count; k++)
		{
			((Component)playableReports[k]).gameObject.SetActive(false);
		}
		PlayAudioClip(characterAppearAudioClip);
		TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(playableReportCanvasGroup, 1f, 1f), (Ease)9).SetFullId<TweenerCore<float, float, FloatOptions>>("PlayableReportFadeIn", (Component)(object)this);
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBeforePlayableXPAnimation);
		TPSingleton<PlayableUnitManager>.Instance.DistributeDailyExperience();
		PlayAudioClip(gainXPAudioClip);
		for (int l = 0; l < playableUnitCount; l++)
		{
			playableReports[l].UnitLevelDisplay.Refresh(instant: false);
		}
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitAfterXPAnimationBeginning);
		((MonoBehaviour)this).StartCoroutine(ShowBattleReportPanel());
	}

	private IEnumerator ShowNightRewardPanel()
	{
		DOTweenModuleUI.DOFade(nightRewardCanvasGroup, 1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 1f).SetFullId<TweenerCore<float, float, FloatOptions>>("NightRewardFadeIn", (Component)(object)this);
		nightRewardCanvasGroup.blocksRaycasts = true;
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitAfterNightRwardCanvasAppear);
		panicRewardIndicator.Refresh(PanicManager.Panic, PlayableUnitManager.DebugForceSkipNightReport);
		yield return (object)new WaitWhile((Func<bool>)(() => panicRewardIndicator.IsMoving));
		DOTweenModuleUI.DOFade(panicRankStamp, 1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampFadeTweenDuration).SetFullId<TweenerCore<Color, Color, ColorOptions>>("PanicRankStampFadeIn", (Component)(object)this);
		ShortcutExtensions.DOPunchScale((Transform)(object)((Graphic)panicRankStamp).rectTransform, Vector3.one * stampPunchStrength, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampPunchTweenDuration, 1, 0.1f).SetFullId<Tweener>("PanicRankStampFadeIn", (Component)(object)this);
		PlayAudioClip(stampAudioClip);
		yield return (object)new WaitForSeconds(stampFadeTweenDuration);
		CollectGoldReward();
		CollectMaterialsReward();
		CollectItemReward();
		((MonoBehaviour)this).StartCoroutine((ApplicationManager.Application.RunsCompleted != 0) ? ShowSoulsRewardPanel() : ShowNightRatingPanel());
	}

	private IEnumerator ShowNightRatingPanel()
	{
		yield return null;
		DOTweenModuleUI.DOFade(nightRatingCanvasGroup, 1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 1f).SetFullId<TweenerCore<float, float, FloatOptions>>("NightRatingFadeIn", (Component)(object)this);
		nightRatingCanvasGroup.blocksRaycasts = true;
		DOTweenModuleUI.DOFade(nightRankStamp, 1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampFadeTweenDuration).SetFullId<TweenerCore<Color, Color, ColorOptions>>("NightRankStampFadeIn", (Component)(object)this);
		ShortcutExtensions.DOPunchScale((Transform)(object)((Graphic)nightRankStamp).rectTransform, Vector3.one * stampPunchStrength, PlayableUnitManager.DebugForceSkipNightReport ? 0f : stampPunchTweenDuration, 1, 0.1f).SetFullId<Tweener>("NightRankStampFadeIn", (Component)(object)this);
		PlayAudioClip(finalStampAudioClip);
		page2SkipCanvasGroup.alpha = 0f;
		page2SkipCanvasGroup.blocksRaycasts = false;
		if (wantToSkipAnimations)
		{
			Time.timeScale = 1f;
			wantToSkipAnimations = false;
		}
		if (PlayableUnitManager.DebugForceSkipNightReport)
		{
			OnContinueClick();
		}
	}

	private IEnumerator ShowSoulsRewardPanel()
	{
		yield return soulsRewardPanel.ShowTrophiesPanel(TPSingleton<GameManager>.Instance.Game.IsDefeat);
		yield return ShowNightRatingPanel();
	}

	private void ToggleKillsReportButtons()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		killReportLeftButton.Interactable = TPSingleton<NightReportPanel>.Instance.killReportParent.anchoredPosition.x != 114f;
		killReportRightButton.Interactable = TPSingleton<NightReportPanel>.Instance.killReportParent.anchoredPosition.x != 0f - TPSingleton<NightReportPanel>.Instance.killReportParent.sizeDelta.x + 678f - 114f;
	}

	protected override void Awake()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		canvas = ((Component)TPSingleton<NightReportPanel>.Instance).GetComponent<Canvas>();
		((Behaviour)canvas).enabled = false;
		canvasGroup = ((Component)TPSingleton<NightReportPanel>.Instance).GetComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		isOpened = false;
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		posYInit = rectTransform.anchoredPosition.y;
		audioSources = (AudioSource[])(object)new AudioSource[audioSourcesCount];
		audioSources[0] = audioSourceTemplate;
		for (int i = 1; i < audioSources.Length; i++)
		{
			audioSources[i] = Object.Instantiate<AudioSource>(audioSourceTemplate, ((Component)audioSourceTemplate).transform.parent);
		}
	}

	[ContextMenu("Open")]
	public void DebugOpen()
	{
		if (!Application.isPlaying)
		{
			Debug.LogError((object)"Unable to use this context menu when the application is not running");
		}
		else if (!TPSingleton<NightReportPanel>.Instance.isOpened)
		{
			Open();
		}
	}

	[ContextMenu("Close")]
	public void DebugClose()
	{
		if (!Application.isPlaying)
		{
			Debug.LogError((object)"Unable to use this context menu when the application is not running");
		}
		else if (TPSingleton<NightReportPanel>.Instance.isOpened)
		{
			Close();
		}
	}
}
