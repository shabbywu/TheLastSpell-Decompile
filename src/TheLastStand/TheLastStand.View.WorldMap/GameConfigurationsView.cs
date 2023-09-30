using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Animation;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.Apocalypse;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.MetaShops;
using TheLastStand.View.WorldMap.Glyphs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap;

public class GameConfigurationsView : TPSingleton<GameConfigurationsView>
{
	public delegate void DelApocalypseSelectionChanged(bool selected);

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Predicate<ApocalypseView> _003C_003E9__39_0;

		public static TweenCallback _003C_003E9__53_0;

		internal bool _003CIsThereAnApocalypseSelected_003Eb__39_0(ApocalypseView x)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)x.State == 2;
		}

		internal void _003CUnfold_003Eb__53_0()
		{
			Object.FindObjectOfType<JoystickHighlight>().ToggleAlwaysFollow(state: false);
		}
	}

	public DelApocalypseSelectionChanged OnApocalypseSelectionHasChanged;

	[SerializeField]
	private GlyphSelectionPreview glyphSelectionPreview;

	[SerializeField]
	private Transform apocalypseLineContainer;

	[SerializeField]
	private BetterToggleGaugeGroup apocalypseLineGroup;

	[SerializeField]
	private ApocalypseView apocalypseLinePrefab;

	[SerializeField]
	private BetterButton backButton;

	[SerializeField]
	private RectTransform backButtonRect;

	[SerializeField]
	private GamepadInputDisplay backButtonGamepadInputDisplay;

	[SerializeField]
	private FloatTweenAnimation backButtonAnimation;

	[SerializeField]
	private TextMeshProUGUI cityDescription;

	[SerializeField]
	private Image cityImage;

	[SerializeField]
	private TextMeshProUGUI cityName;

	[SerializeField]
	private RectTransform panelRectTransform;

	[SerializeField]
	private FloatTweenAnimation configurationPanelAnimation;

	[SerializeField]
	private BetterButton closeButton;

	[SerializeField]
	private RectTransform pannelCloseButton;

	[SerializeField]
	private FloatTweenAnimation closeButtonAnimation;

	[SerializeField]
	private Transform apocalypseLinesContainer;

	[SerializeField]
	private BetterButton nextCityButton;

	[SerializeField]
	private BetterButton previousCityButton;

	[SerializeField]
	private TextMeshProUGUI startingSetupText;

	[SerializeField]
	private RectTransform scrollViewport;

	[SerializeField]
	private Scrollbar scrollBar;

	[SerializeField]
	[Range(0f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	[SerializeField]
	private float yOffset = 10f;

	[SerializeField]
	private RectTransform panelTransform;

	[SerializeField]
	private RectTransform boxTransform;

	[SerializeField]
	private RectTransform topTransform;

	[SerializeField]
	private RectTransform contentTransform;

	[SerializeField]
	private RectTransform bottomDecorationTransform;

	[SerializeField]
	private RectTransform boxMinSizeTransform;

	private bool isFolded = true;

	private WorldMapCity currentCity;

	public List<ApocalypseView> ApocalypseLines { get; } = new List<ApocalypseView>();


	public GlyphSelectionPreview GlyphSelectionPreview => glyphSelectionPreview;

	public static bool IsThereAnApocalypseSelected()
	{
		return (Object)(object)TPSingleton<GameConfigurationsView>.Instance.ApocalypseLines.Find((ApocalypseView x) => (int)x.State == 2) != (Object)null;
	}

	public static void StartNewGameIfEnoughGlyph()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<GameConfigurationsView>.Instance.Fold();
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		if (selectedCity.CurrentGlyphPoints < selectedCity.CityDefinition.MaxGlyphPoints)
		{
			GenericConsent.Open(new ParameterizedLocalizationLine("WorldMap_Consent_NotEnoughGlyphs", Array.Empty<string>()), WorldMapCityManager.StartNewGame, TPSingleton<GameConfigurationsView>.Instance.Unfold);
		}
		else
		{
			WorldMapCityManager.StartNewGame();
		}
	}

	public void AdjustScrollView(RectTransform item)
	{
		if (ApocalypseLines.Count > 0 && (Object)(object)item == (Object)/*isinst with value type is only supported in some contexts*/)
		{
			scrollBar.value = 0f;
		}
		else
		{
			GUIHelpers.AdjustScrollViewToFocusedItem(item, scrollViewport, scrollBar, 0.01f, 0.01f, (float?)null);
		}
	}

	public void JoystickSelectPanel()
	{
		EventSystem.current.SetSelectedGameObject(((Component)GlyphSelectionPreview.EditButton).gameObject);
	}

	public void OnBackButtonClicked()
	{
		ApplicationManager.Application.ApplicationController.SetState("GameLobby");
	}

	public void OnBotButtonClick()
	{
		Scrollbar obj = scrollBar;
		obj.value -= scrollBar.size + scrollButtonsSensitivity;
	}

	public void OnCloseButtonClicked()
	{
		if (!ACameraView.IsZooming)
		{
			WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.EXPLORATION);
		}
	}

	public void OnStateChange()
	{
		switch (TPSingleton<WorldMapStateManager>.Instance.CurrentState)
		{
		case WorldMapStateManager.WorldMapState.EXPLORATION:
			Fold();
			ClearSelection();
			PauseAnimations();
			break;
		case WorldMapStateManager.WorldMapState.FOCUSED:
			ContinueAnimations();
			Refresh();
			Unfold();
			break;
		}
	}

	public void OnTopButtonClick()
	{
		Scrollbar obj = scrollBar;
		obj.value += scrollBar.size + scrollButtonsSensitivity;
	}

	public void OpenGlyphSelectionPanel()
	{
		OraculumHub<GlyphSelectionPanel>.Display(show: true);
	}

	public void Refresh()
	{
		((Component)apocalypseLinesContainer).gameObject.SetActive(ApocalypseManager.IsApocalypseUnlocked);
		GlyphSelectionPreview.Refresh();
		FillWithApocalypseLines();
		for (int i = 0; i < ApocalypseLines.Count; i++)
		{
			if (((Component)ApocalypseLines[i]).gameObject.activeSelf)
			{
				ApocalypseLines[i].Refresh();
			}
		}
		currentCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		if (currentCity != null)
		{
			((TMP_Text)cityName).text = currentCity.CityDefinition.Name;
			((TMP_Text)cityDescription).text = currentCity.CityDefinition.Description;
			((TMP_Text)startingSetupText).text = Localizer.Get("WorldMap_StartingSetup_" + currentCity.CityDefinition.StartingSetup);
			cityImage.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/WorldMap/WorldMap_CityPortrait_" + currentCity.CityDefinition.Id, false);
		}
		RefreshJoystickNavigation();
	}

	public void RefreshBoxSize()
	{
		((MonoBehaviour)this).StartCoroutine(RefreshBoxSizeAfterAFrame());
	}

	public IEnumerator RefreshBoxSizeAfterAFrame()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
		yield return null;
		float num = topTransform.sizeDelta.y + contentTransform.sizeDelta.y + bottomDecorationTransform.sizeDelta.y + yOffset;
		RectTransform obj = boxTransform;
		float x = boxTransform.sizeDelta.x;
		float y = boxMinSizeTransform.sizeDelta.y;
		Rect rect = panelTransform.rect;
		obj.sizeDelta = new Vector2(x, Mathf.Max(y, Mathf.Min(((Rect)(ref rect)).height, num)));
	}

	private void Fold()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		if (!isFolded)
		{
			Tween statusTransitionTween = configurationPanelAnimation.StatusTransitionTween;
			if (statusTransitionTween != null)
			{
				TweenExtensions.Kill(statusTransitionTween, false);
			}
			configurationPanelAnimation.StatusTransitionTween = (Tween)(object)TweenExtensions.SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(panelRectTransform, configurationPanelAnimation.StatusOne, configurationPanelAnimation.TransitionDuration, false), configurationPanelAnimation.TransitionEase), "FoldTween", (Component)(object)this);
			configurationPanelAnimation.InStatusOne = true;
			Tween statusTransitionTween2 = closeButtonAnimation.StatusTransitionTween;
			if (statusTransitionTween2 != null)
			{
				TweenExtensions.Kill(statusTransitionTween2, false);
			}
			closeButtonAnimation.StatusTransitionTween = (Tween)(object)TweenExtensions.SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(pannelCloseButton, closeButtonAnimation.StatusOne, closeButtonAnimation.TransitionDuration, false), closeButtonAnimation.TransitionEase), "CloseButtonFoldTween", (Component)(object)this);
			closeButtonAnimation.InStatusOne = true;
			((Component)backButtonGamepadInputDisplay).gameObject.SetActive(true);
			Tween statusTransitionTween3 = backButtonAnimation.StatusTransitionTween;
			if (statusTransitionTween3 != null)
			{
				TweenExtensions.Kill(statusTransitionTween3, false);
			}
			backButtonAnimation.StatusTransitionTween = (Tween)(object)TweenExtensions.SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(backButtonRect, backButtonAnimation.StatusOne, backButtonAnimation.TransitionDuration, false), backButtonAnimation.TransitionEase), "BackButtonFoldTween", (Component)(object)this);
			backButtonAnimation.InStatusOne = true;
			if (InputManager.IsLastControllerJoystick)
			{
				EventSystem.current.SetSelectedGameObject((GameObject)null);
			}
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			isFolded = true;
		}
	}

	private void Unfold()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		if (!isFolded)
		{
			return;
		}
		Tween statusTransitionTween = configurationPanelAnimation.StatusTransitionTween;
		if (statusTransitionTween != null)
		{
			TweenExtensions.Kill(statusTransitionTween, false);
		}
		configurationPanelAnimation.StatusTransitionTween = (Tween)(object)TweenExtensions.SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(panelRectTransform, configurationPanelAnimation.StatusTwo, configurationPanelAnimation.TransitionDuration, false), configurationPanelAnimation.TransitionEase), "UnfoldTween", (Component)(object)this);
		RefreshBoxSize();
		configurationPanelAnimation.InStatusOne = false;
		Tween statusTransitionTween2 = closeButtonAnimation.StatusTransitionTween;
		if (statusTransitionTween2 != null)
		{
			TweenExtensions.Kill(statusTransitionTween2, false);
		}
		closeButtonAnimation.StatusTransitionTween = (Tween)(object)TweenExtensions.SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(pannelCloseButton, closeButtonAnimation.StatusTwo, closeButtonAnimation.TransitionDuration, false), closeButtonAnimation.TransitionEase), "CloseButtonUnfoldTween", (Component)(object)this);
		closeButtonAnimation.InStatusOne = false;
		((Component)backButtonGamepadInputDisplay).gameObject.SetActive(false);
		Tween statusTransitionTween3 = backButtonAnimation.StatusTransitionTween;
		if (statusTransitionTween3 != null)
		{
			TweenExtensions.Kill(statusTransitionTween3, false);
		}
		backButtonAnimation.StatusTransitionTween = (Tween)(object)TweenExtensions.SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(backButtonRect, backButtonAnimation.StatusTwo, backButtonAnimation.TransitionDuration, false), backButtonAnimation.TransitionEase), "BackButtonUnfoldTween", (Component)(object)this);
		backButtonAnimation.InStatusOne = false;
		if (InputManager.IsLastControllerJoystick)
		{
			Object.FindObjectOfType<JoystickHighlight>().ToggleAlwaysFollow(state: true);
			Tween statusTransitionTween4 = configurationPanelAnimation.StatusTransitionTween;
			object obj = _003C_003Ec._003C_003E9__53_0;
			if (obj == null)
			{
				TweenCallback val = delegate
				{
					Object.FindObjectOfType<JoystickHighlight>().ToggleAlwaysFollow(state: false);
				};
				_003C_003Ec._003C_003E9__53_0 = val;
				obj = (object)val;
			}
			TweenSettingsExtensions.OnComplete<Tween>(statusTransitionTween4, (TweenCallback)obj);
			JoystickSelectPanel();
		}
		isFolded = false;
	}

	private void ClearSelection()
	{
		apocalypseLineGroup.NotifyToggleOn(-1);
	}

	private void ContinueAnimations()
	{
		for (int i = 0; i < ApocalypseLines.Count; i++)
		{
			ApocalypseLines[i].ContinueAnimations();
		}
	}

	private void FillWithApocalypseLines()
	{
		if (!ApocalypseManager.IsApocalypseUnlocked)
		{
			((Component)apocalypseLinesContainer).gameObject.SetActive(false);
			return;
		}
		ApocalypsesDefinition apocalypsesDefinition = ApocalypseDatabase.ApocalypsesDefinition;
		while (ApocalypseLines.Count <= TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable + 1 && ApocalypseLines.Count < apocalypsesDefinition.ApocalypseDefinitions.Count)
		{
			ApocalypseView apocalypseView = Object.Instantiate<ApocalypseView>(apocalypseLinePrefab, apocalypseLineContainer);
			apocalypseView.ApocalypseDefinition = apocalypsesDefinition.ApocalypseDefinitions[ApocalypseLines.Count];
			apocalypseView.Init(apocalypseLineGroup);
			ApocalypseLines.Add(apocalypseView);
		}
		for (int i = 0; i < ApocalypseLines.Count && i <= TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable + 1; i++)
		{
			((Component)ApocalypseLines[i]).gameObject.SetActive(true);
			ApocalypseLines[i].Refresh();
		}
		for (int j = TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable + 2; j < ApocalypseLines.Count; j++)
		{
			if (((Component)ApocalypseLines[j]).gameObject.activeSelf)
			{
				ApocalypseLines[j].Refresh();
				((Component)ApocalypseLines[j]).gameObject.SetActive(false);
			}
		}
	}

	private void PauseAnimations()
	{
		for (int i = 0; i < ApocalypseLines.Count; i++)
		{
			ApocalypseLines[i].PauseAnimations();
		}
	}

	private void RefreshJoystickNavigation()
	{
		PreviewedGlyphDisplay firstPreviewedGlyphDisplay = GlyphSelectionPreview.FirstPreviewedGlyphDisplay;
		Selectable val = ((firstPreviewedGlyphDisplay != null) ? ((Component)firstPreviewedGlyphDisplay).GetComponent<Selectable>() : null);
		for (int i = 0; i < ApocalypseLines.Count; i++)
		{
			ApocalypseView apocalypseView = ApocalypseLines[i];
			SelectableExtensions.SetMode((Selectable)(object)apocalypseView.JoystickSelectable, (Mode)4);
			if (i < ApocalypseLines.Count - 1)
			{
				SelectableExtensions.SetSelectOnDown((Selectable)(object)apocalypseView.JoystickSelectable, (Selectable)(object)ApocalypseLines[i + 1].JoystickSelectable);
			}
			if (i == 0)
			{
				SelectableExtensions.SetSelectOnUp((Selectable)(object)apocalypseView.JoystickSelectable, (Selectable)(((Object)(object)val != (Object)null) ? ((object)val) : ((object)GlyphSelectionPreview.EditButton)));
			}
			else
			{
				SelectableExtensions.SetSelectOnUp((Selectable)(object)apocalypseView.JoystickSelectable, (Selectable)(object)ApocalypseLines[i - 1].JoystickSelectable);
			}
		}
		SelectableExtensions.SetMode((Selectable)(object)GlyphSelectionPreview.EditButton, (Mode)4);
		SelectableExtensions.SetSelectOnDown((Selectable)(object)GlyphSelectionPreview.EditButton, (Selectable)(object)(((Object)(object)val != (Object)null) ? ((JoystickSelectable)(object)val) : ((ApocalypseLines.Count > 0) ? ApocalypseLines[0].JoystickSelectable : null)));
	}

	private void Start()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		((UnityEvent)((Button)previousCityButton).onClick).AddListener(new UnityAction(TPSingleton<WorldMapCityManager>.Instance.SelectPreviousCity));
		((UnityEvent)((Button)nextCityButton).onClick).AddListener(new UnityAction(TPSingleton<WorldMapCityManager>.Instance.SelectNextCity));
		((UnityEvent)((Button)closeButton).onClick).AddListener(new UnityAction(OnCloseButtonClicked));
		((UnityEvent)((Button)backButton).onClick).AddListener(new UnityAction(OnBackButtonClicked));
		FillWithApocalypseLines();
		PauseAnimations();
		RefreshJoystickNavigation();
		scrollBar.value = 1f;
	}
}
