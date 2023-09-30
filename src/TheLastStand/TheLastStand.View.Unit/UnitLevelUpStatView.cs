using System;
using System.IO;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Model.Animation;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Unit;

public class UnitLevelUpStatView : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	private static class Constants
	{
		public static readonly string LevelUpRarityBackgroundPath = Path.Combine("View", "Sprites", "UI", "LevelUp", "CharacterSheet_LevelUp_Box{0}_Off");

		public static readonly string LevelUpRarityBackgroundPathSelected = Path.Combine("View", "Sprites", "UI", "LevelUp", "CharacterSheet_LevelUp_Box{0}_On");

		public static readonly string[] LevelUpRarityLevelNames = new string[4] { "Common", "Magic", "Rare", "Epic" };
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__44_0;

		public static Action<Entry> _003C_003E9__53_0;

		internal void _003CInitializeToggle_003Eb__44_0()
		{
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonClickAudioClip);
		}

		internal void _003COnDestroy_003Eb__53_0(Entry x)
		{
			((UnityEventBase)x.callback).RemoveAllListeners();
		}
	}

	[SerializeField]
	private Image bonusIcon;

	[SerializeField]
	private Image bonusJewel;

	[SerializeField]
	private TextMeshProUGUI bonusJewelName;

	[SerializeField]
	private DataSpriteTable bonusJewelSprites;

	[SerializeField]
	private CanvasGroup statBoxCanvasGroup;

	[SerializeField]
	private ImprovedToggle statBoxToggle;

	[SerializeField]
	private Animator statBoxValidationAnimator;

	[SerializeField]
	private CanvasGroup statBoxValidationCanvasGroup;

	[SerializeField]
	private Image statArrowImage;

	[SerializeField]
	private Image statBoxBG;

	[SerializeField]
	private UnitStatDisplay statDisplay;

	[SerializeField]
	private StatTooltipDisplayer statTooltipDisplayer;

	[SerializeField]
	private TextMeshProUGUI statResultText;

	[SerializeField]
	[Range(0f, 1f)]
	private float unselectedAlpha = 0.5f;

	[SerializeField]
	private CanvasGroup selectedCanvasGroup;

	[SerializeField]
	private RectTransform confirmBox;

	[SerializeField]
	private Image confirmBoxImage;

	[SerializeField]
	private CanvasGroup confirmBoxCanvasGroup;

	[SerializeField]
	private Sprite confirmBoxOffSprite;

	[SerializeField]
	private Sprite confirmBoxOnSprite;

	[SerializeField]
	private EventTrigger confirmBoxEventTrigger;

	[SerializeField]
	private TextMeshProUGUI confirmBoxText;

	[SerializeField]
	private Vector2TweenAnimation confirmBoxAnimationDatas = new Vector2TweenAnimation();

	[SerializeField]
	private ColorTweenAnimation confirmBoxTextAnimationDatas = new ColorTweenAnimation();

	[SerializeField]
	private Selectable selectable;

	private Sprite background;

	private Sprite highlightedBackground;

	public Selectable Selectable => selectable;

	public UnityEvent OnConfirmBoxClicked { get; } = new UnityEvent();


	public UnitLevelUp.SelectedStatToLevelUp StatBonus { get; set; }

	public ImprovedToggle StatBoxToggle => statBoxToggle;

	public StatTooltipDisplayer StatTooltipDisplayer => statTooltipDisplayer;

	public PlayableUnit TargetUnit { get; set; }

	public void InitializeToggle()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		UnityEvent onPointerClickEvent = statBoxToggle.OnPointerClickEvent;
		object obj = _003C_003Ec._003C_003E9__44_0;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonClickAudioClip);
			};
			_003C_003Ec._003C_003E9__44_0 = val;
			obj = (object)val;
		}
		onPointerClickEvent.AddListener((UnityAction)obj);
		statBoxToggle.OnPointerEnterEvent.AddListener((UnityAction)delegate
		{
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonHoverAudioClip);
			statBoxBG.sprite = highlightedBackground;
			OnPointerEnterOverConfirmBox();
		});
		statBoxToggle.OnPointerExitEvent.AddListener((UnityAction)delegate
		{
			OnPointerExitOverConfirmBox();
			statBoxBG.sprite = background;
		});
		Entry val2 = new Entry
		{
			eventID = (EventTriggerType)0
		};
		((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			UnityEvent onPointerExitEvent2 = statBoxToggle.OnPointerExitEvent;
			if (onPointerExitEvent2 != null)
			{
				onPointerExitEvent2.Invoke();
			}
		});
		confirmBoxEventTrigger.triggers.Add(val2);
		Entry val3 = new Entry
		{
			eventID = (EventTriggerType)1
		};
		((UnityEvent<BaseEventData>)(object)val3.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			UnityEvent onPointerExitEvent = statBoxToggle.OnPointerExitEvent;
			if (onPointerExitEvent != null)
			{
				onPointerExitEvent.Invoke();
			}
		});
		confirmBoxEventTrigger.triggers.Add(val3);
		Entry val4 = new Entry
		{
			eventID = (EventTriggerType)2
		};
		((UnityEvent<BaseEventData>)(object)val4.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			UnityEvent onConfirmBoxClicked = OnConfirmBoxClicked;
			if (onConfirmBoxClicked != null)
			{
				onConfirmBoxClicked.Invoke();
			}
		});
		confirmBoxEventTrigger.triggers.Add(val4);
	}

	private void OnPointerExitOverConfirmBox()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Tween statusTransitionTween = confirmBoxTextAnimationDatas.StatusTransitionTween;
		if (statusTransitionTween != null)
		{
			TweenExtensions.Kill(statusTransitionTween, false);
		}
		confirmBoxTextAnimationDatas.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleUI.DOColor((Graphic)(object)confirmBoxText, confirmBoxTextAnimationDatas.StatusOne, confirmBoxTextAnimationDatas.TransitionDuration), confirmBoxTextAnimationDatas.TransitionEase);
		confirmBoxImage.sprite = confirmBoxOffSprite;
	}

	private void OnPointerEnterOverConfirmBox()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Tween statusTransitionTween = confirmBoxTextAnimationDatas.StatusTransitionTween;
		if (statusTransitionTween != null)
		{
			TweenExtensions.Kill(statusTransitionTween, false);
		}
		confirmBoxTextAnimationDatas.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleUI.DOColor((Graphic)(object)confirmBoxText, confirmBoxTextAnimationDatas.StatusTwo, confirmBoxTextAnimationDatas.TransitionDuration), confirmBoxTextAnimationDatas.TransitionEase);
		confirmBoxImage.sprite = confirmBoxOnSprite;
	}

	public void Refresh()
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		int rarityLevel = (int)StatBonus.RarityLevel;
		statBoxCanvasGroup.alpha = 0f;
		DOTweenModuleUI.DOFade(statBoxCanvasGroup, 1f, 0.5f);
		statDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[StatBonus.Definition.Stat];
		statDisplay.TargetUnit = TargetUnit;
		statDisplay.Refresh();
		((Behaviour)statBoxValidationAnimator).enabled = false;
		statBoxValidationCanvasGroup.alpha = 0f;
		bonusIcon.sprite = UnitStatDisplay.GetStatIconSprite(UnitDatabase.UnitStatDefinitions[StatBonus.Definition.Stat].Id, UnitStatDisplay.E_IconSize.VerySmall);
		background = GetRarityBackgroundSprite(rarityLevel);
		highlightedBackground = GetRarityBackgroundSprite(rarityLevel, isHovered: true);
		statBoxBG.sprite = background;
		SpriteState spriteState = ((Selectable)statBoxToggle).spriteState;
		((SpriteState)(ref spriteState)).highlightedSprite = GetRarityBackgroundSprite(rarityLevel, isHovered: true);
		((Selectable)statBoxToggle).spriteState = spriteState;
		bonusJewel.sprite = bonusJewelSprites.GetSpriteAt(rarityLevel);
		((TMP_Text)bonusJewelName).text = Localizer.Get("RarityName_" + Constants.LevelUpRarityLevelNames[rarityLevel]);
		((TMP_Text)statResultText).text = string.Format("{0}{1}", Mathf.Round(statDisplay.TargetUnit.UnitStatsController.GetStat(StatBonus.Definition.Stat).Base + (float)StatBonus.Definition.Bonuses[StatBonus.BonusIndex]), statDisplay.StatDefinition.Id.ShownAsPercentage() ? "<size=80%>%</size>" : string.Empty);
	}

	public void Select(bool isSelected, bool isAnythingSelected = false)
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		DOTweenModuleUI.DOFade(selectedCanvasGroup, isSelected ? 1f : 0f, 0f);
		DOTweenModuleUI.DOFade(confirmBoxCanvasGroup, isSelected ? 1f : 0f, 0.1f);
		confirmBoxCanvasGroup.interactable = isSelected;
		confirmBoxCanvasGroup.blocksRaycasts = isSelected;
		if (!isSelected)
		{
			TweenExtensions.Complete(confirmBoxAnimationDatas.StatusTransitionTween);
			if (!confirmBoxAnimationDatas.InStatusOne)
			{
				confirmBoxAnimationDatas.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(confirmBox, confirmBoxAnimationDatas.StatusOne, confirmBoxAnimationDatas.TransitionDuration, false), confirmBoxAnimationDatas.TransitionEase), (TweenCallback)delegate
				{
					confirmBoxAnimationDatas.InStatusOne = true;
				});
			}
		}
		else
		{
			TweenExtensions.Complete(confirmBoxAnimationDatas.StatusTransitionTween);
			if (confirmBoxAnimationDatas.InStatusOne)
			{
				confirmBoxAnimationDatas.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(confirmBox, confirmBoxAnimationDatas.StatusTwo, confirmBoxAnimationDatas.TransitionDuration, false), confirmBoxAnimationDatas.TransitionEase), (TweenCallback)delegate
				{
					confirmBoxAnimationDatas.InStatusOne = false;
				});
			}
		}
		if (isAnythingSelected && !isSelected)
		{
			TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(statBoxCanvasGroup, unselectedAlpha, 0.25f), (Ease)9);
		}
		else
		{
			TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(statBoxCanvasGroup, 1f, 0.25f), (Ease)9);
		}
	}

	public void Validate(bool isChosen)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		DOTweenModuleUI.DOFade(selectedCanvasGroup, 0f, 0f);
		if (isChosen)
		{
			if (InputManager.IsLastControllerJoystick)
			{
				SelectableExtensions.ClearNavigation(Selectable);
			}
			TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleUI.DOFade(statArrowImage, 0f, 0.5f), (Ease)9);
			DOTweenModuleUI.DOColor((Graphic)(object)statResultText, Color.white, 1f);
			DOTweenModuleUI.DOColor((Graphic)(object)statDisplay.StatValueText, ColorExtensions.WithA(Color.grey, 0f), 1f);
			ShortcutExtensions.DOPunchScale(((TMP_Text)statResultText).transform, Vector3.one * 0.3f, 0.2f, 0, 0.1f);
			((Behaviour)statBoxValidationAnimator).enabled = true;
			TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(statBoxValidationCanvasGroup, 1f, 0.2f), (Ease)9);
		}
		else
		{
			TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(statBoxCanvasGroup, 0f, 0.5f), (Ease)9);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		UnityEvent onPointerEnterEvent = statBoxToggle.OnPointerEnterEvent;
		if (onPointerEnterEvent != null)
		{
			onPointerEnterEvent.Invoke();
		}
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			StatTooltipDisplayer.DisplayTooltip(display: true);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		UnityEvent onPointerExitEvent = statBoxToggle.OnPointerExitEvent;
		if (onPointerExitEvent != null)
		{
			onPointerExitEvent.Invoke();
		}
		StatTooltipDisplayer.DisplayTooltip(display: false);
	}

	private Sprite GetRarityBackgroundSprite(int rarityLevel, bool isHovered = false)
	{
		return ResourcePooler<Sprite>.LoadOnce(string.Format(isHovered ? Constants.LevelUpRarityBackgroundPathSelected : Constants.LevelUpRarityBackgroundPath, (rarityLevel + 1).ToString("00")), false);
	}

	private void OnDestroy()
	{
		((UnityEventBase)statBoxToggle.OnPointerClickEvent).RemoveAllListeners();
		((UnityEventBase)statBoxToggle.OnPointerEnterEvent).RemoveAllListeners();
		((UnityEventBase)((Toggle)StatBoxToggle).onValueChanged).RemoveAllListeners();
		((UnityEventBase)statBoxToggle.OnPointerExitEvent).RemoveAllListeners();
		((UnityEventBase)statBoxToggle.OnBeforePointerClickEvent).RemoveAllListeners();
		confirmBoxEventTrigger.triggers.ForEach(delegate(Entry x)
		{
			((UnityEventBase)x.callback).RemoveAllListeners();
		});
		confirmBoxEventTrigger.triggers.Clear();
		((UnityEventBase)OnConfirmBoxClicked).RemoveAllListeners();
	}
}
