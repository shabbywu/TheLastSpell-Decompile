using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Animation;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap;

public class WorldMapApocalypseMaxLevelView : TPSingleton<WorldMapApocalypseMaxLevelView>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private ApocalypseEffectsTooltip apocalypseTooltip;

	[SerializeField]
	private FloatTweenAnimation foldAnimation;

	[SerializeField]
	private float foldDuration;

	[SerializeField]
	private float foldInitialDuration;

	[SerializeField]
	private Scrollbar scrollBar;

	[SerializeField]
	private RectTransform scrollViewRectTransform;

	[SerializeField]
	private RectTransform scrollRectRectTransform;

	[SerializeField]
	private BetterButton leftButton;

	[SerializeField]
	private BetterButton rightButton;

	[SerializeField]
	private LayoutGroup apocalypseLayoutGroup;

	[SerializeField]
	private ContentSizeFitter contentSizeFitter;

	[SerializeField]
	private RectTransform foldRectTransform;

	[SerializeField]
	private ApocalypseLevelView previousApocalypsesLevelPrefab;

	[SerializeField]
	private ApocalypseLevelView currentApocalypsesLevelPrefab;

	private List<ApocalypseLevelView> apocalypseLevelViews = new List<ApocalypseLevelView>();

	private bool isFolded = true;

	public void OnLeftButtonClick()
	{
		Scrollbar obj = scrollBar;
		obj.value -= scrollBar.size + 0.1f;
	}

	public void OnRightButtonClick()
	{
		Scrollbar obj = scrollBar;
		obj.value += scrollBar.size + 0.1f;
	}

	public void OnStateChange()
	{
		switch (TPSingleton<WorldMapStateManager>.Instance.CurrentState)
		{
		case WorldMapStateManager.WorldMapState.EXPLORATION:
			Unfold(foldDuration);
			break;
		case WorldMapStateManager.WorldMapState.FOCUSED:
			Fold(foldDuration);
			break;
		}
	}

	public IEnumerator PopulateApocalypses(int apocalypseLevelMax)
	{
		ApocalypseLevelView apocalypseLevelView;
		for (int i = 0; i < apocalypseLevelMax; i++)
		{
			apocalypseLevelView = Object.Instantiate<ApocalypseLevelView>(previousApocalypsesLevelPrefab, ((Component)scrollRectRectTransform).transform);
			apocalypseLevelView.Init(i);
			apocalypseLevelViews.Add(apocalypseLevelView);
		}
		apocalypseLevelView = Object.Instantiate<ApocalypseLevelView>(currentApocalypsesLevelPrefab, (Transform)(object)scrollRectRectTransform);
		apocalypseLevelView.Init(apocalypseLevelMax);
		apocalypseLevelViews.Add(apocalypseLevelView);
		yield return (object)new WaitForEndOfFrame();
		((Behaviour)contentSizeFitter).enabled = false;
		((Behaviour)apocalypseLayoutGroup).enabled = false;
		Refresh();
		yield return (object)new WaitForEndOfFrame();
		Unfold(foldInitialDuration);
	}

	public void Refresh()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		BetterButton betterButton = leftButton;
		Rect rect = scrollViewRectTransform.rect;
		float width = ((Rect)(ref rect)).width;
		rect = scrollRectRectTransform.rect;
		betterButton.Interactable = width < ((Rect)(ref rect)).width;
		BetterButton betterButton2 = rightButton;
		rect = scrollViewRectTransform.rect;
		float width2 = ((Rect)(ref rect)).width;
		rect = scrollRectRectTransform.rect;
		betterButton2.Interactable = width2 < ((Rect)(ref rect)).width;
	}

	private void Fold(float duration)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (!isFolded && ApocalypseManager.IsApocalypseUnlocked)
		{
			Tween statusTransitionTween = foldAnimation.StatusTransitionTween;
			if (statusTransitionTween != null)
			{
				TweenExtensions.Kill(statusTransitionTween, false);
			}
			foldAnimation.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(foldRectTransform, foldAnimation.StatusOne, foldAnimation.TransitionDuration, false), foldAnimation.TransitionEase).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("ApocalypseFold", (Component)(object)this);
			foldAnimation.InStatusOne = true;
			isFolded = true;
			UIOptimization(stopAnimations: true);
		}
	}

	private void UIOptimization(bool stopAnimations)
	{
		for (int i = 0; i < apocalypseLevelViews.Count; i++)
		{
			apocalypseLevelViews[i].StopAnimation(stopAnimations);
		}
	}

	private void Unfold(float duration)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (isFolded && ApocalypseManager.IsApocalypseUnlocked)
		{
			Tween statusTransitionTween = foldAnimation.StatusTransitionTween;
			if (statusTransitionTween != null)
			{
				TweenExtensions.Kill(statusTransitionTween, false);
			}
			foldAnimation.StatusTransitionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(foldRectTransform, foldAnimation.StatusTwo, foldAnimation.TransitionDuration, false), foldAnimation.TransitionEase).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("ApocalypseUnfold", (Component)(object)this);
			foldAnimation.InStatusOne = false;
			isFolded = false;
			UIOptimization(stopAnimations: false);
		}
	}

	private void Start()
	{
		if (ApocalypseManager.IsApocalypseUnlocked)
		{
			int num = TPSingleton<WorldMapCityManager>.Instance.Cities.Max((WorldMapCity o) => o.MaxApocalypsePassed);
			((MonoBehaviour)this).StartCoroutine(PopulateApocalypses(num));
			apocalypseTooltip.SetDamnedSoulsPercentageModifier((uint)num * ApocalypseDatabase.ConfigurationDefinition.DamnedSoulsPercentagePerLevel);
			apocalypseTooltip.SetApocalypsesToDisplay(num);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		apocalypseTooltip.Display();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		apocalypseTooltip.Hide();
	}
}
