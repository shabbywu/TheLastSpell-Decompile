using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Framework;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Animation;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap;

public class WorldMapCityView : MonoBehaviour
{
	private static class Constants
	{
		public const string AnimatorPathFormat = "Animators/Cities/Worldmap/{0}Animations/{0}_Animator";

		public const string IdleAnimation = "CityIdle";

		public const string HoveredAnimation = "CityHovered";

		public const string CompletedIdleAnimation = "CityCompletedIdle";

		public const string CompletedHoveredAnimation = "CityCompletedHovered";
	}

	public WorldMapCity WorldMapCity;

	private bool isSelected;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private EventTrigger startEventTrigger;

	[SerializeField]
	private Transform targetPos;

	[SerializeField]
	private GameObject cantStartNewGameText;

	[SerializeField]
	private Canvas cityNameCanvas;

	[SerializeField]
	private Image cityNamePanel;

	[SerializeField]
	private Sprite cityNamePanelHovered;

	[SerializeField]
	private Sprite cityNamePanelNormal;

	[SerializeField]
	private TextMeshProUGUI cityNameText;

	[SerializeField]
	private GameObject selectionCursor;

	[SerializeField]
	private BetterButton startButton;

	[SerializeField]
	private Animator apocalypseFlameAnimator;

	[SerializeField]
	private Image apocalypseLevelImage;

	[SerializeField]
	private TextMeshProUGUI apocalypseNameText;

	[SerializeField]
	private Vector2TweenAnimation zoomPanelCityNameAnimation;

	[SerializeField]
	private RectTransform panelCityNameRect;

	[SerializeField]
	private Vector2TweenAnimation zoomCityNameAnimation;

	[SerializeField]
	private RectTransform cityNameRect;

	[SerializeField]
	private Vector2TweenAnimation zoomWingsAnimation;

	[SerializeField]
	private RectTransform wingsRect;

	[SerializeField]
	private Vector2TweenAnimation zoomFlamesSizeAnimation;

	[SerializeField]
	private Vector2TweenAnimation zoomFlamesPosAnimation;

	[SerializeField]
	private RectTransform flamesRect;

	[SerializeField]
	private Vector2TweenAnimation zoomApocalypseNameSizeAnimation;

	[SerializeField]
	private Vector2TweenAnimation zoomApocalypseNamePosAnimation;

	[SerializeField]
	private RectTransform apocalypseNameRect;

	[SerializeField]
	private Vector2TweenAnimation zoomApocalypseLevelSizeAnimation;

	[SerializeField]
	private Vector2TweenAnimation zoomApocalypseLevelPosAnimation;

	[SerializeField]
	private RectTransform apocalypseLevelRect;

	public bool ValidApocalypseStateToStart
	{
		get
		{
			if (!GameConfigurationsView.IsThereAnApocalypseSelected())
			{
				return TPSingleton<GameConfigurationsView>.Instance.ApocalypseLines.Count == 0;
			}
			return true;
		}
	}

	public Transform TargetPos => targetPos;

	public bool IsHovered { get; private set; }

	public void Init()
	{
		if (WorldMapCity != null)
		{
			WorldMapCity.RefreshIsSelectable();
			selectionCursor.SetActive(false);
			animator.runtimeAnimatorController = ResourcePooler<RuntimeAnimatorController>.LoadOnce(string.Format("Animators/Cities/Worldmap/{0}Animations/{0}_Animator", WorldMapCity.CityDefinition.Id));
			animator.Play(GetAnimationName(hovered: false), 0, Random.value);
			((TMP_Text)cityNameText).text = ((WorldMapCity.NumberOfRuns > 0) ? $"{WorldMapCity.CityDefinition.Name} #{WorldMapCity.NumberOfRuns + 1}" : WorldMapCity.CityDefinition.Name);
			if (!WorldMapCity.IsSelectable)
			{
				((Behaviour)cityNameCanvas).enabled = false;
			}
			if (WorldMapCity.MaxApocalypsePassed >= 1)
			{
				apocalypseLevelImage.sprite = ResourcePooler<Sprite>.LoadOnce($"View/Sprites/UI/WorldMap/ApocalypseLevels/ApocalypseLevel_{WorldMapCity.MaxApocalypsePassed:00}");
			}
			else
			{
				((Behaviour)apocalypseLevelImage).enabled = false;
			}
			((Component)cityNamePanel).gameObject.SetActive(false);
			((TMP_Text)apocalypseNameText).text = Localizer.Get((WorldMapCity.MaxApocalypsePassed == 0) ? "WorldMap_ApocalypseDifficulty_Normal" : "WorldMap_ApocalypseDifficulty_Apocalypse");
			((Component)this).gameObject.SetActive(WorldMapCity.IsUnlocked && !WorldMapCity.CityDefinition.Hidden);
			ACameraView.OnZoomHasChanged = (ACameraView.DelZoom)Delegate.Combine(ACameraView.OnZoomHasChanged, new ACameraView.DelZoom(OnZoom));
			GameConfigurationsView instance = TPSingleton<GameConfigurationsView>.Instance;
			instance.OnApocalypseSelectionHasChanged = (GameConfigurationsView.DelApocalypseSelectionChanged)Delegate.Combine(instance.OnApocalypseSelectionHasChanged, new GameConfigurationsView.DelApocalypseSelectionChanged(OnSelectionHasChanged));
			InitStartButton();
		}
	}

	public void OnDeselection()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		isSelected = false;
		SwitchCursorActiveState(show: false);
		SwitchMaxApocalypseLevelActiveState(IsHovered && WorldMapCity.MaxApocalypsePassed != -1 && TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable > 0);
		SwitchNamePanelSprite(IsHovered);
		SwitchButtonStart(show: false);
		Animator obj = animator;
		string animationName = GetAnimationName(IsHovered);
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		obj.Play(animationName, 0, ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime);
	}

	public void OnSelection()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		isSelected = true;
		SwitchCursorActiveState(show: true);
		SwitchMaxApocalypseLevelActiveState(WorldMapCity.MaxApocalypsePassed != -1 && TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable > 0);
		SwitchNamePanelSprite(show: true);
		SwitchButtonStart(show: true, ValidApocalypseStateToStart);
		Animator obj = animator;
		string animationName = GetAnimationName(hovered: false);
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		obj.Play(animationName, 0, ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime);
	}

	private string GetAnimationName(bool hovered)
	{
		if (hovered)
		{
			if (WorldMapCity.NumberOfWins <= 0)
			{
				return "CityHovered";
			}
			return "CityCompletedHovered";
		}
		if (WorldMapCity.NumberOfWins <= 0)
		{
			return "CityIdle";
		}
		return "CityCompletedIdle";
	}

	private void HandleMouseHover(bool hovering)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (IsHovered != hovering)
		{
			IsHovered = hovering;
			if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity == null && !isSelected)
			{
				Animator obj = animator;
				string animationName = GetAnimationName(hovering && TPSingleton<WorldMapCityManager>.Instance.SelectedCity != WorldMapCity);
				AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				obj.Play(animationName, 0, ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime);
				SwitchNamePanelSprite(hovering || TPSingleton<WorldMapCityManager>.Instance.SelectedCity == WorldMapCity);
				SwitchMaxApocalypseLevelActiveState((hovering || TPSingleton<WorldMapCityManager>.Instance.SelectedCity == WorldMapCity) && WorldMapCity.MaxApocalypsePassed != -1 && TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable > 0);
			}
		}
	}

	private void InitStartButton()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		((UnityEvent)((Button)startButton).onClick).AddListener(new UnityAction(GameConfigurationsView.StartNewGameIfEnoughGlyph));
		((Selectable)startButton).interactable = false;
		Entry val = new Entry
		{
			eventID = (EventTriggerType)0
		};
		Entry val2 = new Entry
		{
			eventID = (EventTriggerType)1
		};
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			if (!startButton.Interactable)
			{
				cantStartNewGameText.SetActive(true);
			}
		});
		startEventTrigger.triggers.Add(val);
		((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			cantStartNewGameText.SetActive(false);
		});
		startEventTrigger.triggers.Add(val2);
	}

	private void OnDestroy()
	{
		ACameraView.OnZoomHasChanged = (ACameraView.DelZoom)Delegate.Remove(ACameraView.OnZoomHasChanged, new ACameraView.DelZoom(OnZoom));
	}

	private void OnMouseDown()
	{
		TPSingleton<WorldMapCityManager>.Instance.SelectCity(WorldMapCity);
	}

	private void OnMouseEnter()
	{
		if (WorldMapCity.IsSelectable)
		{
			HandleMouseHover(hovering: true);
		}
	}

	private void OnMouseExit()
	{
		if (WorldMapCity.IsSelectable)
		{
			HandleMouseHover(hovering: false);
		}
	}

	private void OnSelectionHasChanged(bool selected)
	{
		if (isSelected)
		{
			startButton.Interactable = selected;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		OnMouseEnter();
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		OnMouseExit();
	}

	private void OnZoom(bool zoomed)
	{
		SwitchSizeZoomStatus(zoomed, zoomPanelCityNameAnimation, panelCityNameRect);
		SwitchSizeZoomStatus(zoomed, zoomCityNameAnimation, cityNameRect);
		SwitchSizeZoomStatus(zoomed, zoomWingsAnimation, wingsRect);
		SwitchSizeZoomStatus(zoomed, zoomFlamesSizeAnimation, flamesRect);
		SwitchPosZoomStatus(zoomed, zoomFlamesPosAnimation, flamesRect);
		SwitchSizeZoomStatus(zoomed, zoomApocalypseNameSizeAnimation, apocalypseNameRect);
		SwitchPosZoomStatus(zoomed, zoomApocalypseNamePosAnimation, apocalypseNameRect);
		SwitchSizeZoomStatus(zoomed, zoomApocalypseLevelSizeAnimation, apocalypseLevelRect);
		SwitchPosZoomStatus(zoomed, zoomApocalypseLevelPosAnimation, apocalypseLevelRect);
	}

	private void SwitchButtonStart(bool show, bool interactable = false)
	{
		((Component)startButton).gameObject.SetActive(show);
		startButton.Interactable = interactable;
	}

	private void SwitchCursorActiveState(bool show)
	{
		selectionCursor.SetActive(show);
	}

	private void SwitchMaxApocalypseLevelActiveState(bool show)
	{
		((Component)apocalypseFlameAnimator).gameObject.SetActive(show);
		((Component)apocalypseLevelImage).gameObject.SetActive(show);
		((Component)apocalypseNameText).gameObject.SetActive(show);
		if (show)
		{
			apocalypseFlameAnimator.Play("WorldMapFlamesIdle", 0, Random.value);
		}
	}

	private void SwitchNamePanelSprite(bool show)
	{
		((Component)cityNamePanel).gameObject.SetActive(IsHovered || isSelected);
		cityNamePanel.sprite = (show ? cityNamePanelHovered : cityNamePanelNormal);
	}

	private void SwitchSizeZoomStatus(bool zoomed, Vector2TweenAnimation vector2Animation, RectTransform rectTransform)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (zoomed == vector2Animation.InStatusOne)
		{
			Tween statusTransitionTween = vector2Animation.StatusTransitionTween;
			if (statusTransitionTween != null)
			{
				TweenExtensions.Kill(statusTransitionTween, false);
			}
			Vector2 val = (vector2Animation.InStatusOne ? vector2Animation.StatusTwo : vector2Animation.StatusOne);
			vector2Animation.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(rectTransform, val, vector2Animation.TransitionDuration, false), vector2Animation.TransitionEase);
			vector2Animation.InStatusOne = !vector2Animation.InStatusOne;
		}
	}

	private void SwitchPosZoomStatus(bool zoomed, Vector2TweenAnimation vector2Animation, RectTransform rectTransform)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (zoomed == vector2Animation.InStatusOne)
		{
			Tween statusTransitionTween = vector2Animation.StatusTransitionTween;
			if (statusTransitionTween != null)
			{
				TweenExtensions.Kill(statusTransitionTween, false);
			}
			Vector2 val = (vector2Animation.InStatusOne ? vector2Animation.StatusTwo : vector2Animation.StatusOne);
			vector2Animation.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(rectTransform, val, vector2Animation.TransitionDuration, false), vector2Animation.TransitionEase);
			vector2Animation.InStatusOne = !vector2Animation.InStatusOne;
		}
	}
}
