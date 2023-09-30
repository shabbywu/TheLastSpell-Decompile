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
using TheLastStand.Controller.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Animation;
using TheLastStand.Model.Unit;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Unit;

public class UnitLevelUpView : TPSingleton<UnitLevelUpView>, IOverlayUser
{
	public enum E_LevelUpShownStat
	{
		Main,
		Secondary
	}

	public static class Constants
	{
		public const string RerollAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_LevelUp";
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private GenericTooltipDisplayer tooltipDisplayer;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private TextMeshProUGUI mainAttributesTitle;

	[SerializeField]
	private RectTransform mainStatBoxParent;

	[SerializeField]
	private TextMeshProUGUI secondaryAttributesTitle;

	[SerializeField]
	private RectTransform secondaryStatBoxParent;

	[SerializeField]
	private Image statBoxesImageMask;

	[SerializeField]
	private Mask statBoxesMask;

	[SerializeField]
	private UnitLevelUpStatView statBoxPrefab;

	[SerializeField]
	private BetterButton showMainStatButton;

	[SerializeField]
	private BetterButton showSecondaryStatButton;

	[SerializeField]
	private BetterButton rerollButton;

	[SerializeField]
	private TextMeshProUGUI rerollCountText;

	[SerializeField]
	private Vector2TweenAnimation toggleMainStatTweenDatas = new Vector2TweenAnimation();

	[SerializeField]
	private Vector2TweenAnimation toggleMainStatTitleTweenDatas = new Vector2TweenAnimation();

	[SerializeField]
	private Vector2TweenAnimation toggleSecondaryStatTweenDatas = new Vector2TweenAnimation();

	[SerializeField]
	private Vector2TweenAnimation toggleSecondaryStatTitleTweenDatas = new Vector2TweenAnimation();

	[SerializeField]
	private float validateCloseDelay = 1f;

	[SerializeField]
	private AudioSource levelUpAudioSource;

	[SerializeField]
	private AudioClip[] levelUpClips;

	[SerializeField]
	private DataColor validRemainingRerollColor;

	[SerializeField]
	private DataColor invalidRemainingRerollColor;

	[SerializeField]
	private LayoutNavigationInitializer mainStatsNavigationInitializer;

	[SerializeField]
	private LayoutNavigationInitializer secondaryStatsNavigationInitializer;

	[SerializeField]
	private HUDJoystickSimpleTarget hudTarget;

	[SerializeField]
	private JoystickSelectableDynamic traitsToSecondaryAttributesLeft;

	[SerializeField]
	private JoystickSelectableDynamic traitsToSecondaryAttributesRight;

	[SerializeField]
	private JoystickSelectableDynamic leftBottomToRightPanel;

	private bool initialized;

	private Tween fadeTween;

	private RectTransform mainAttributesTitleTransform;

	private RectTransform secondaryAttributesTitleTransform;

	private readonly List<UnitLevelUpStatView> mainStatBoxes = new List<UnitLevelUpStatView>();

	private readonly List<UnitLevelUpStatView> secondaryStatBoxes = new List<UnitLevelUpStatView>();

	private ToggleGroup mainStatBoxToggleGroup;

	private ToggleGroup secondStatBoxToggleGroup;

	private float moveDuration;

	private bool isChangingBox;

	private bool avoidRefresh;

	private bool changedFromButton;

	private int lastUnlockPerkClipIndex = -1;

	private AudioClip[] rerollAudioClips;

	public HUDJoystickSimpleTarget HudTarget => hudTarget;

	public E_LevelUpShownStat CurrentLevelUpShownStat { get; private set; }

	public bool IsOpened { get; private set; }

	public bool IsProceedingToALevelUp { get; private set; }

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public UnitLevelUp UnitLevelUp { get; set; }

	public void Init()
	{
		if (!TPSingleton<UnitLevelUpView>.Instance.initialized)
		{
			initialized = true;
			((Behaviour)canvas).enabled = false;
			canvasGroup.blocksRaycasts = false;
			IsOpened = false;
			mainStatBoxToggleGroup = ((Component)mainStatBoxParent).GetComponent<ToggleGroup>();
			secondStatBoxToggleGroup = ((Component)secondaryStatBoxParent).GetComponent<ToggleGroup>();
			mainAttributesTitleTransform = ((Component)mainAttributesTitle).GetComponent<RectTransform>();
			secondaryAttributesTitleTransform = ((Component)secondaryAttributesTitle).GetComponent<RectTransform>();
			rerollAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_LevelUp", false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.TooltipsToggled += OnTooltipsToggled;
		}
	}

	public void Close(bool instant = false, bool fromLastPointUsed = false)
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		if (!IsOpened)
		{
			return;
		}
		CLoggerManager.Log((object)"UnitLevelUpView closed", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
		avoidRefresh = false;
		if (UnitLevelUpController.CanCloseUnitLevelUpView)
		{
			GameController.SetState(Game.E_State.CharacterSheet);
			Tween obj = fadeTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			ToggleStatTooltipDisplayers(state: false);
			((Component)mainStatBoxParent).gameObject.SetActive(true);
			((Component)secondaryStatBoxParent).gameObject.SetActive(true);
			TransformExtensions.DestroyChildren((Transform)(object)mainStatBoxParent);
			TransformExtensions.DestroyChildren((Transform)(object)secondaryStatBoxParent);
			mainStatBoxes.Clear();
			secondaryStatBoxes.Clear();
			IsOpened = false;
			if (instant)
			{
				canvasGroup.alpha = 0f;
				OnPanelClosed();
			}
			else
			{
				fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.25f), (Ease)26), new TweenCallback(OnPanelClosed));
			}
			canvasGroup.interactable = false;
			TPSingleton<CharacterSheetPanel>.Instance.RefreshCharacterDetailsNotif(TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshUnitHeader(TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshOpenedPage();
		}
		void OnPanelClosed()
		{
			((Behaviour)canvas).enabled = false;
			canvasGroup.blocksRaycasts = false;
			if (fromLastPointUsed && InputManager.IsLastControllerJoystick)
			{
				PlayableUnit playableUnit = UnitLevelUp.PlayableUnit;
				if (playableUnit != null && playableUnit.PerksPoints > 0)
				{
					TPSingleton<CharacterSheetPanel>.Instance.OpenPerkTree();
					TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<CharacterSheetPanel>.Instance.PerksJoystickTarget.GetSelectionInfo());
				}
				else
				{
					TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(GameView.CharacterDetailsView.SecondaryAttributesHUDJoystickTarget.GetSelectionInfo());
				}
			}
		}
	}

	public void Open()
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		if (IsOpened)
		{
			TPSingleton<CharacterSheetPanel>.Instance.OpenUnitDetails();
		}
		else
		{
			if (UnitLevelUp == null)
			{
				return;
			}
			CLoggerManager.Log((object)"UnitLevelUpView opened", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CharacterSheet)
			{
				TPSingleton<CharacterSheetPanel>.Instance.Open();
				GameController.SetState(Game.E_State.CharacterSheet);
				TPSingleton<CharacterSheetPanel>.Instance.RefreshTabs();
			}
			if ((Object)(object)simpleFontLocalizedParent != (Object)null)
			{
				((FontLocalizedParent)simpleFontLocalizedParent).RefreshChilds();
			}
			TPSingleton<CharacterSheetPanel>.Instance.OpenUnitDetails();
			Tween obj = fadeTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			IsOpened = true;
			((Behaviour)canvas).enabled = true;
			canvasGroup.interactable = true;
			canvasGroup.alpha = 0f;
			if (InputManager.IsLastControllerJoystick)
			{
				EventSystem.current.SetSelectedGameObject((GameObject)null);
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			}
			fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.25f), (TweenCallback)delegate
			{
				ToggleStatTooltipDisplayers(state: true);
				if (InputManager.IsLastControllerJoystick)
				{
					TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(HudTarget.GetSelectionInfo());
				}
			});
			tooltipDisplayer.LocalizationArguments = new object[1] { UnitLevelUp.UnitLevelUpDefinition.MaxAmountOfReroll };
			Reinitialize();
			RefreshJoystickNavigation();
		}
	}

	public void OnCloseButtonClick()
	{
		Close();
	}

	public void OnRerollButtonClick()
	{
		SoundManager.PlayAudioClip(ListExtensions.PickRandom<AudioClip>((IEnumerable<AudioClip>)rerollAudioClips));
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
			UnitLevelUp.UnitLevelUpController.DrawAvailableMainStats();
			break;
		case E_LevelUpShownStat.Secondary:
			UnitLevelUp.UnitLevelUpController.DrawAvailableSecondaryStats();
			break;
		}
		Refresh();
		if (InputManager.IsLastControllerJoystick)
		{
			if (!rerollButton.Interactable)
			{
				EventSystem.current.SetSelectedGameObject((CurrentLevelUpShownStat == E_LevelUpShownStat.Main) ? ((Component)mainStatBoxes[0]).gameObject : ((Component)secondaryStatBoxes[0]).gameObject);
			}
			((MonoBehaviour)this).StartCoroutine(OnJoystickRerollCoroutine());
		}
	}

	public void OnToggleStatButtonClick()
	{
		if (!isChangingBox)
		{
			changedFromButton = true;
			((MonoBehaviour)this).StartCoroutine(ToggleMainAndSecondaryStatBoxes());
		}
	}

	public void Reinitialize()
	{
		if (UnitLevelUp == null)
		{
			Close();
			return;
		}
		UnitLevelUp.UnitLevelUpController.DeselectStat();
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = true;
		Refresh(forceToggleToMainStat: true);
	}

	private void InitializeLevelUpStat(UnitLevelUp.SelectedStatToLevelUp stat, bool isMainStat, bool isFirst)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		UnitLevelUpStatView unitLevelUpStatView = Object.Instantiate<UnitLevelUpStatView>(statBoxPrefab, (Transform)(object)(isMainStat ? mainStatBoxParent : secondaryStatBoxParent));
		unitLevelUpStatView.InitializeToggle();
		if (isMainStat)
		{
			mainStatBoxes.Add(unitLevelUpStatView);
		}
		else
		{
			secondaryStatBoxes.Add(unitLevelUpStatView);
		}
		ImprovedToggle component = ((Component)unitLevelUpStatView).GetComponent<ImprovedToggle>();
		((Toggle)component).group = (isMainStat ? mainStatBoxToggleGroup : secondStatBoxToggleGroup);
		unitLevelUpStatView.OnConfirmBoxClicked.AddListener(new UnityAction(OnValidateButtonClick));
		((UnityEvent<PointerEventData>)component.OnBeforePointerClickEvent).AddListener((UnityAction<PointerEventData>)delegate(PointerEventData eventData)
		{
			OnBeforePointerClickedStatBox(eventData, unitLevelUpStatView);
		});
		((UnityEvent<bool>)(object)((Toggle)component).onValueChanged).AddListener((UnityAction<bool>)delegate(bool value)
		{
			OnStatBoxSelectedChanged(unitLevelUpStatView, value);
		});
		unitLevelUpStatView.StatBonus = stat;
		unitLevelUpStatView.TargetUnit = UnitLevelUp.PlayableUnit;
		if (isFirst)
		{
			if (isMainStat)
			{
				traitsToSecondaryAttributesLeft.Selectables[0] = (Selectable)(object)component;
				traitsToSecondaryAttributesRight.Selectables[0] = (Selectable)(object)component;
				if ((Object)(object)leftBottomToRightPanel != (Object)null)
				{
					leftBottomToRightPanel.Selectables[0] = (Selectable)(object)component;
				}
			}
			else
			{
				traitsToSecondaryAttributesLeft.Selectables[1] = (Selectable)(object)component;
				traitsToSecondaryAttributesRight.Selectables[1] = (Selectable)(object)component;
				if ((Object)(object)leftBottomToRightPanel != (Object)null)
				{
					leftBottomToRightPanel.Selectables[1] = (Selectable)(object)component;
				}
			}
		}
		unitLevelUpStatView.Refresh();
	}

	private IEnumerator OnJoystickRerollCoroutine()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		yield return SharedYields.WaitForSeconds(InputManager.JoystickConfig.HUDNavigation.HighlightTweenDuration);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
	}

	private void OnTooltipsToggled(bool state)
	{
		if (!IsOpened)
		{
			return;
		}
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
		{
			for (int num2 = mainStatBoxes.Count - 1; num2 >= 0; num2--)
			{
				if ((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)mainStatBoxes[num2]).gameObject)
				{
					mainStatBoxes[num2].StatTooltipDisplayer.DisplayTooltip(state);
					break;
				}
			}
			break;
		}
		case E_LevelUpShownStat.Secondary:
		{
			for (int num = secondaryStatBoxes.Count - 1; num >= 0; num--)
			{
				if ((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)secondaryStatBoxes[num]).gameObject)
				{
					secondaryStatBoxes[num].StatTooltipDisplayer.DisplayTooltip(state);
					break;
				}
			}
			break;
		}
		}
	}

	private void RefreshJoystickNavigation()
	{
		mainStatsNavigationInitializer.InitNavigation();
		secondaryStatsNavigationInitializer.InitNavigation();
		foreach (UnitLevelUpStatView mainStatBox in mainStatBoxes)
		{
			SelectableExtensions.SetMode(mainStatBox.Selectable, (Mode)4);
			if (rerollButton.Interactable)
			{
				SelectableExtensions.SetSelectOnLeft(mainStatBox.Selectable, (Selectable)(object)rerollButton);
			}
		}
		foreach (UnitLevelUpStatView secondaryStatBox in secondaryStatBoxes)
		{
			SelectableExtensions.SetMode(secondaryStatBox.Selectable, (Mode)4);
			if (rerollButton.Interactable)
			{
				SelectableExtensions.SetSelectOnLeft(secondaryStatBox.Selectable, (Selectable)(object)rerollButton);
			}
		}
		RefreshRerollButtonJoystickNavigation();
		HudTarget.ClearMissingSelectables();
		HudTarget.AddSelectables(mainStatBoxes.Select((UnitLevelUpStatView o) => o.Selectable));
		HudTarget.AddSelectables(secondaryStatBoxes.Select((UnitLevelUpStatView o) => o.Selectable));
	}

	private void RefreshRerollButtonJoystickNavigation()
	{
		SelectableExtensions.SetMode((Selectable)(object)rerollButton, (Mode)4);
		SelectableExtensions.SetSelectOnRight((Selectable)(object)rerollButton, (CurrentLevelUpShownStat == E_LevelUpShownStat.Main) ? mainStatBoxes[0].Selectable : secondaryStatBoxes[0].Selectable);
	}

	private void Update()
	{
		if (IsOpened)
		{
			if (InputManager.GetButtonDown(91) && (!TweenExtensions.IsActive(fadeTween) || !TweenExtensions.IsPlaying(fadeTween)) && !TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened && !TPSingleton<CharacterSheetPanel>.Instance.IsPerksPanelOpened && UnitLevelUp.UnitLevelUpController.CanReroll())
			{
				OnRerollButtonClick();
				EventSystem.current.SetSelectedGameObject((CurrentLevelUpShownStat == E_LevelUpShownStat.Main) ? ((Component)mainStatBoxes[0]).gameObject : ((Component)secondaryStatBoxes[0]).gameObject);
			}
			if ((InputManager.GetButtonDown(92) && CurrentLevelUpShownStat == E_LevelUpShownStat.Main && UnitLevelUp.PlayableUnit.SecondaryStatsPoints > 0) || (InputManager.GetButtonDown(93) && CurrentLevelUpShownStat == E_LevelUpShownStat.Secondary && UnitLevelUp.PlayableUnit.MainStatsPoints > 0))
			{
				EventSystem.current.SetSelectedGameObject((GameObject)null);
				OnToggleStatButtonClick();
			}
			if (InputManager.GetButtonDown(29) && IsOpened)
			{
				OnCloseButtonClick();
			}
		}
	}

	private void MoveStatsBox()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		TweenExtensions.Kill(toggleSecondaryStatTweenDatas.StatusTransitionTween, false);
		TweenExtensions.Kill(toggleMainStatTweenDatas.StatusTransitionTween, false);
		TweenExtensions.Kill(toggleSecondaryStatTitleTweenDatas.StatusTransitionTween, false);
		TweenExtensions.Kill(toggleMainStatTitleTweenDatas.StatusTransitionTween, false);
		bool flag = CurrentLevelUpShownStat == E_LevelUpShownStat.Main;
		moveDuration = Mathf.Max(new float[4] { toggleSecondaryStatTweenDatas.TransitionDuration, toggleMainStatTweenDatas.TransitionDuration, toggleSecondaryStatTitleTweenDatas.TransitionDuration, toggleMainStatTitleTweenDatas.TransitionDuration });
		toggleSecondaryStatTweenDatas.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(secondaryStatBoxParent, flag ? toggleSecondaryStatTweenDatas.StatusTwo : toggleSecondaryStatTweenDatas.StatusOne, toggleSecondaryStatTweenDatas.TransitionDuration, false), toggleSecondaryStatTweenDatas.TransitionEase);
		toggleMainStatTweenDatas.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(mainStatBoxParent, flag ? toggleMainStatTweenDatas.StatusOne : toggleMainStatTweenDatas.StatusTwo, toggleMainStatTweenDatas.TransitionDuration, false), toggleMainStatTweenDatas.TransitionEase);
		toggleSecondaryStatTitleTweenDatas.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(secondaryAttributesTitleTransform, flag ? toggleSecondaryStatTitleTweenDatas.StatusTwo : toggleSecondaryStatTitleTweenDatas.StatusOne, toggleSecondaryStatTitleTweenDatas.TransitionDuration, false), toggleSecondaryStatTitleTweenDatas.TransitionEase);
		toggleMainStatTitleTweenDatas.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(mainAttributesTitleTransform, flag ? toggleMainStatTitleTweenDatas.StatusOne : toggleMainStatTitleTweenDatas.StatusTwo, toggleMainStatTitleTweenDatas.TransitionDuration, false), toggleMainStatTitleTweenDatas.TransitionEase);
	}

	private void OnBeforePointerClickedStatBox(PointerEventData e, UnitLevelUpStatView unitLevelUpStatView)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (canvasGroup.interactable)
		{
			if (((Toggle)unitLevelUpStatView.StatBoxToggle).isOn && (int)e.button == 0)
			{
				unitLevelUpStatView.StatBoxToggle.ShouldExecuteOnPointerClickEvent = false;
				OnValidateButtonClick();
			}
			else
			{
				unitLevelUpStatView.StatBoxToggle.ShouldExecuteOnPointerClickEvent = true;
			}
		}
	}

	private void OnDestroy()
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Exist())
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.TooltipsToggled -= OnTooltipsToggled;
		}
	}

	private void OnStatBoxSelectedChanged(UnitLevelUpStatView sender, bool selected)
	{
		if (InputManager.IsLastControllerJoystick && (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)sender).gameObject && !selected)
		{
			OnValidateButtonClick();
			return;
		}
		if (selected)
		{
			UnitLevelUp.UnitLevelUpController.SelectStat(sender.StatBonus);
		}
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
		{
			foreach (UnitLevelUpStatView mainStatBox in mainStatBoxes)
			{
				mainStatBox.Select(selected && (Object)(object)sender == (Object)(object)mainStatBox, selected);
			}
			break;
		}
		case E_LevelUpShownStat.Secondary:
		{
			foreach (UnitLevelUpStatView secondaryStatBox in secondaryStatBoxes)
			{
				secondaryStatBox.Select(selected && (Object)(object)sender == (Object)(object)secondaryStatBox, selected);
			}
			break;
		}
		}
	}

	private void OnValidateButtonClick()
	{
		if (!canvasGroup.interactable)
		{
			return;
		}
		IsProceedingToALevelUp = true;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = false;
		int num = Random.Range(0, levelUpClips.Length);
		if (levelUpClips.Length > 1)
		{
			while (num == lastUnlockPerkClipIndex)
			{
				num = Random.Range(0, levelUpClips.Length);
			}
		}
		lastUnlockPerkClipIndex = num;
		levelUpAudioSource.PlayOneShot(levelUpClips[num]);
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
		{
			UnitLevelUp.UnitLevelUpController.ValidateStatToIncrease(isValidatingMainStat: true);
			int j = 0;
			for (int count2 = mainStatBoxes.Count; j < count2; j++)
			{
				mainStatBoxes[j].Validate(UnitLevelUp.HasSelectedStat && mainStatBoxes[j].StatBonus.Equals(UnitLevelUp.SelectedStat));
			}
			break;
		}
		case E_LevelUpShownStat.Secondary:
		{
			UnitLevelUp.UnitLevelUpController.ValidateStatToIncrease(isValidatingMainStat: false);
			int i = 0;
			for (int count = secondaryStatBoxes.Count; i < count; i++)
			{
				secondaryStatBoxes[i].Validate(UnitLevelUp.HasSelectedStat && secondaryStatBoxes[i].StatBonus.Equals(UnitLevelUp.SelectedStat));
			}
			break;
		}
		default:
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Tried to validate a level up stat which is nor main nor secondary!", (CLogLevel)1, true, true);
			break;
		}
		avoidRefresh = true;
		TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
		((MonoBehaviour)this).StartCoroutine(WaitCloseAfterValidate());
	}

	private void Refresh(bool forceToggleToMainStat = false)
	{
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)mainAttributesTitle).text = Localizer.Format((UnitLevelUp.PlayableUnit.MainStatsPoints > 1) ? "UnitLevelUpPanel_MainTitle_SeveralLevels" : "UnitLevelUpPanel_MainTitle", new object[1] { UnitLevelUp.PlayableUnit.MainStatsPoints });
		((TMP_Text)secondaryAttributesTitle).text = Localizer.Format((UnitLevelUp.PlayableUnit.SecondaryStatsPoints > 1) ? "UnitLevelUpPanel_SecondaryTitle_SeveralLevels" : "UnitLevelUpPanel_SecondaryTitle", new object[1] { UnitLevelUp.PlayableUnit.SecondaryStatsPoints });
		if (avoidRefresh)
		{
			avoidRefresh = false;
			return;
		}
		if (UnitLevelUp.AvailableMainStats.Count == 0 && UnitLevelUp.PlayableUnit.MainStatsPoints > 0)
		{
			UnitLevelUp.UnitLevelUpController.DrawAvailableStats();
		}
		if (UnitLevelUp.AvailableSecondaryStats.Count == 0 && UnitLevelUp.PlayableUnit.SecondaryStatsPoints > 0)
		{
			UnitLevelUp.UnitLevelUpController.DrawAvailableStats(isDrawingMainStat: false);
		}
		((Behaviour)statBoxesImageMask).enabled = true;
		((Behaviour)statBoxesMask).enabled = true;
		((Component)mainStatBoxParent).gameObject.SetActive(true);
		((Component)secondaryStatBoxParent).gameObject.SetActive(true);
		TransformExtensions.DestroyChildren((Transform)(object)mainStatBoxParent);
		TransformExtensions.DestroyChildren((Transform)(object)secondaryStatBoxParent);
		mainStatBoxes.Clear();
		secondaryStatBoxes.Clear();
		int i = 0;
		for (int count = UnitLevelUp.AvailableMainStats.Count; i < count; i++)
		{
			InitializeLevelUpStat(UnitLevelUp.AvailableMainStats[i], isMainStat: true, i == 0);
		}
		int j = 0;
		for (int count2 = UnitLevelUp.AvailableSecondaryStats.Count; j < count2; j++)
		{
			InitializeLevelUpStat(UnitLevelUp.AvailableSecondaryStats[j], isMainStat: false, j == 0);
		}
		rerollButton.Interactable = UnitLevelUp.UnitLevelUpController.CanReroll();
		((TMP_Text)rerollCountText).text = $"x{UnitLevelUp.CommonNbReroll}";
		((Graphic)rerollCountText).color = ((UnitLevelUp.CommonNbReroll > 0) ? validRemainingRerollColor._Color : invalidRemainingRerollColor._Color);
		if ((CurrentLevelUpShownStat == E_LevelUpShownStat.Main && UnitLevelUp.AvailableMainStats.Count == 0) || (CurrentLevelUpShownStat == E_LevelUpShownStat.Secondary && UnitLevelUp.AvailableSecondaryStats.Count == 0) || (forceToggleToMainStat && CurrentLevelUpShownStat == E_LevelUpShownStat.Secondary && UnitLevelUp.AvailableMainStats.Count != 0))
		{
			((MonoBehaviour)this).StartCoroutine(ToggleMainAndSecondaryStatBoxes());
		}
		((Component)showMainStatButton).gameObject.SetActive(CurrentLevelUpShownStat == E_LevelUpShownStat.Secondary && UnitLevelUp.PlayableUnit.MainStatsPoints > 0);
		((Component)showSecondaryStatButton).gameObject.SetActive(CurrentLevelUpShownStat == E_LevelUpShownStat.Main && UnitLevelUp.PlayableUnit.SecondaryStatsPoints > 0);
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
			((Component)secondaryStatBoxParent).gameObject.SetActive(false);
			break;
		case E_LevelUpShownStat.Secondary:
			((Component)mainStatBoxParent).gameObject.SetActive(false);
			break;
		}
		((Behaviour)statBoxesImageMask).enabled = false;
		((Behaviour)statBoxesMask).enabled = false;
		RefreshJoystickNavigation();
	}

	private IEnumerator ToggleMainAndSecondaryStatBoxes()
	{
		isChangingBox = true;
		canvasGroup.interactable = false;
		if (changedFromButton)
		{
			switch (CurrentLevelUpShownStat)
			{
			case E_LevelUpShownStat.Main:
				mainStatBoxToggleGroup.SetAllTogglesOff(true);
				break;
			case E_LevelUpShownStat.Secondary:
				secondStatBoxToggleGroup.SetAllTogglesOff(true);
				break;
			}
		}
		changedFromButton = false;
		((Behaviour)statBoxesImageMask).enabled = true;
		((Behaviour)statBoxesMask).enabled = true;
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
			((Component)secondaryStatBoxParent).gameObject.SetActive(true);
			break;
		case E_LevelUpShownStat.Secondary:
			((Component)mainStatBoxParent).gameObject.SetActive(true);
			break;
		}
		CurrentLevelUpShownStat = ((CurrentLevelUpShownStat == E_LevelUpShownStat.Main) ? E_LevelUpShownStat.Secondary : E_LevelUpShownStat.Main);
		MoveStatsBox();
		ToggleStatTooltipDisplayers(state: false);
		RefreshRerollButtonJoystickNavigation();
		((Component)showMainStatButton).gameObject.SetActive(CurrentLevelUpShownStat == E_LevelUpShownStat.Secondary && UnitLevelUp.PlayableUnit.MainStatsPoints > 0);
		((Component)showSecondaryStatButton).gameObject.SetActive(CurrentLevelUpShownStat == E_LevelUpShownStat.Main && UnitLevelUp.PlayableUnit.SecondaryStatsPoints > 0);
		canvasGroup.interactable = true;
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject((GameObject)null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
		yield return null;
		yield return SharedYields.WaitForSeconds(moveDuration);
		ToggleStatTooltipDisplayers(state: true);
		switch (CurrentLevelUpShownStat)
		{
		case E_LevelUpShownStat.Main:
			((Component)secondaryStatBoxParent).gameObject.SetActive(false);
			break;
		case E_LevelUpShownStat.Secondary:
			((Component)mainStatBoxParent).gameObject.SetActive(false);
			break;
		}
		if (InputManager.IsLastControllerJoystick)
		{
			yield return SharedYields.WaitForEndOfFrame;
			switch (CurrentLevelUpShownStat)
			{
			case E_LevelUpShownStat.Main:
				if (mainStatBoxes.Count > 0)
				{
					EventSystem.current.SetSelectedGameObject(((Component)mainStatBoxes[0]).gameObject);
				}
				break;
			case E_LevelUpShownStat.Secondary:
				if (secondaryStatBoxes.Count > 0)
				{
					EventSystem.current.SetSelectedGameObject(((Component)secondaryStatBoxes[0]).gameObject);
				}
				break;
			}
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ForcePositionUpdate();
			RefreshJoystickNavigation();
		}
		((Behaviour)statBoxesImageMask).enabled = false;
		((Behaviour)statBoxesMask).enabled = false;
		isChangingBox = false;
	}

	private void ToggleStatTooltipDisplayers(bool state)
	{
		List<UnitLevelUpStatView> list = new List<UnitLevelUpStatView>(mainStatBoxes);
		list.AddRange(secondaryStatBoxes);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			((Component)list[num].StatTooltipDisplayer).gameObject.SetActive(state);
			if (!state)
			{
				list[num].StatTooltipDisplayer.DisplayTooltip(display: false);
			}
		}
	}

	private IEnumerator WaitCloseAfterValidate()
	{
		yield return SharedYields.WaitForSeconds(validateCloseDelay);
		UnitLevelUp unitLevelUp = UnitLevelUp;
		if (unitLevelUp != null && unitLevelUp.PlayableUnit?.StatsPoints > 0)
		{
			switch (CurrentLevelUpShownStat)
			{
			case E_LevelUpShownStat.Main:
			{
				UnitLevelUp unitLevelUp3 = UnitLevelUp;
				if (unitLevelUp3 != null && unitLevelUp3.PlayableUnit?.MainStatsPoints > 0)
				{
					UnitLevelUp.UnitLevelUpController.DrawAvailableStats();
					avoidRefresh = false;
					Refresh();
					canvasGroup.interactable = true;
					if (InputManager.IsLastControllerJoystick)
					{
						yield return SharedYields.WaitForEndOfFrame;
						EventSystem.current.SetSelectedGameObject(((Component)mainStatBoxes[0]).gameObject);
					}
				}
				else
				{
					avoidRefresh = false;
					((MonoBehaviour)this).StartCoroutine(ToggleMainAndSecondaryStatBoxes());
				}
				break;
			}
			case E_LevelUpShownStat.Secondary:
			{
				UnitLevelUp unitLevelUp2 = UnitLevelUp;
				if (unitLevelUp2 != null && unitLevelUp2.PlayableUnit?.SecondaryStatsPoints > 0)
				{
					UnitLevelUp.UnitLevelUpController.DrawAvailableStats(isDrawingMainStat: false);
					avoidRefresh = false;
					Refresh();
					canvasGroup.interactable = true;
					if (InputManager.IsLastControllerJoystick)
					{
						yield return SharedYields.WaitForEndOfFrame;
						EventSystem.current.SetSelectedGameObject(((Component)secondaryStatBoxes[0]).gameObject);
					}
				}
				else
				{
					avoidRefresh = false;
					((MonoBehaviour)this).StartCoroutine(ToggleMainAndSecondaryStatBoxes());
				}
				break;
			}
			}
		}
		else
		{
			UnitLevelUp.CommonNbReroll = 0;
			avoidRefresh = false;
			Close(instant: false, fromLastPointUsed: true);
		}
		IsProceedingToALevelUp = false;
	}
}
