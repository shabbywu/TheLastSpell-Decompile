using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Helpers;
using TheLastStand.Manager;
using TheLastStand.View.HUD;
using TheLastStand.View.MetaNarration;
using TheLastStand.View.MetaShops.JoystickNavigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public class MetaShopView : MonoBehaviour
{
	public static class Constants
	{
		public const float CycleThroughNewEntriesAvailableAlpha = 1f;

		public const float CycleThroughNewEntriesUnavailableAlpha = 0.15f;
	}

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasScaler canvasScaler;

	[SerializeField]
	private Button backButton;

	[SerializeField]
	private GameObject backButtonParent;

	[SerializeField]
	private GoddessView goddessView;

	[SerializeField]
	private NarrationView narrationView;

	[SerializeField]
	private List<MetaShopTab> tabs;

	[SerializeField]
	private MetaShopSorter sorter;

	[SerializeField]
	private TextMeshProUGUI noUpgradeText;

	[SerializeField]
	private GameObject exitText;

	[SerializeField]
	private RectTransform shopPositionContainer;

	[SerializeField]
	private RectTransform shopPositionTarget;

	[SerializeField]
	protected TextMeshProUGUI unlockProgressionText;

	[SerializeField]
	private Animator fxAnimator;

	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	protected ScrollRect scrollRect;

	[SerializeField]
	protected ContentSizeFitter scrollViewContentSizeFitter;

	[SerializeField]
	protected VerticalLayoutGroup scrollViewLayoutGroup;

	[SerializeField]
	protected RectTransform scrollViewport;

	[SerializeField]
	private List<MetaShopFilter> filters;

	[SerializeField]
	private MetaShopFilterLink filtersLink;

	[SerializeField]
	private HUDJoystickSimpleTarget upgradesJoystickSimpleTarget;

	[SerializeField]
	private HUDJoystickDynamicTarget joystickDynamicTarget;

	[SerializeField]
	private AFiltersToUpgradeNavigation filtersToUpgradeNavigation;

	public Canvas Canvas => canvas;

	public CanvasGroup CanvasGroup => canvasGroup;

	public GameObject ExitText => exitText;

	public List<MetaShopFilter> Filters => filters;

	public List<MetaUpgradeLineView> Lines { get; private set; } = new List<MetaUpgradeLineView>();


	public Animator FxAnimator => fxAnimator;

	public RectTransform FxTransform
	{
		get
		{
			Transform transform = ((Component)FxAnimator).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	public GoddessView GoddessView => goddessView;

	public RectTransform LayoutGroupContainer
	{
		get
		{
			Transform transform = ((Component)scrollViewLayoutGroup).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	public NarrationView NarrationView => narrationView;

	public ScrollRect ScrollRect => scrollRect;

	public MetaShopSorter Sorter => sorter;

	public List<MetaShopTab> Tabs => tabs;

	public HUDJoystickDynamicTarget JoystickDynamicTarget => joystickDynamicTarget;

	public void AddLine(MetaUpgradeLineView newLine)
	{
		Lines.Add(newLine);
		upgradesJoystickSimpleTarget?.AddSelectable((Selectable)(object)newLine.JoystickSelectable);
	}

	public void ResetView()
	{
		goddessView.FadeInContainer.SetActive(false);
		goddessView.IdleContainer.SetActive(false);
		if ((Object)(object)CanvasGroup != (Object)null)
		{
			CanvasGroup.alpha = 0f;
			CanvasGroup.interactable = false;
		}
		backButtonParent.gameObject.SetActive(false);
		narrationView.Hide();
	}

	public void OnSlotViewJoystickSelect(RectTransform item)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(item, scrollViewport, scrollbar, 0.01f, 0.01f);
	}

	public void ResetScrollbar(bool forceRefresh = false)
	{
		UpdateAllLinesDisplay(forceRefresh);
		scrollbar.value = 1f;
	}

	public void EnableCanvas(bool enable)
	{
		((Behaviour)Canvas).enabled = enable;
		CanvasHelper.ScaleCanvas(canvasScaler, allowDecimals: false);
	}

	public void AddBackButtonListener(UnityAction action)
	{
		((UnityEvent)backButton.onClick).AddListener(action);
	}

	public void RefreshFilterView(MetaUpgradeDefinition.E_MetaUpgradeFilter currentFilter)
	{
		foreach (MetaShopFilter filter in filters)
		{
			filter.Toggle((filter.Filter & currentFilter) != 0);
		}
		filtersLink.Refresh();
	}

	public void RemoveBackButtonListener(UnityAction action)
	{
		((UnityEvent)backButton.onClick).RemoveListener(action);
	}

	public void ShowBackButton(bool show)
	{
		backButtonParent.gameObject.SetActive(show);
	}

	public void SnapShopPosition()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((Transform)shopPositionContainer).position = ((Transform)shopPositionTarget).position;
	}

	public void ToggleFiltersUpNavigation(bool toggle)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		foreach (MetaShopFilter filter in filters)
		{
			Navigation navigation;
			if (toggle)
			{
				navigation = filter.Selectable.navigation;
				if ((Object)(object)((Navigation)(ref navigation)).selectOnUp == (Object)null)
				{
					filter.Selectable.SetSelectOnUp((Selectable)(object)filtersToUpgradeNavigation);
				}
			}
			else
			{
				navigation = filter.Selectable.navigation;
				if ((Object)(object)((Navigation)(ref navigation)).selectOnUp == (Object)(object)filtersToUpgradeNavigation)
				{
					filter.Selectable.SetSelectOnUp(null);
				}
			}
		}
	}

	public void ToggleLayout(bool toggle)
	{
		((Behaviour)scrollViewContentSizeFitter).enabled = toggle;
		((Behaviour)scrollViewLayoutGroup).enabled = toggle;
	}

	public void ToggleNoUpgradeFeedback(bool toggle)
	{
		((Component)noUpgradeText).gameObject.SetActive(toggle);
	}

	public void UpdateAllLinesDisplay(Vector2 newValue)
	{
		UpdateAllLinesDisplay();
	}

	public void UpdateAllLinesDisplay(bool forceRefresh = false)
	{
		for (int num = Lines.Count - 1; num >= 0; num--)
		{
			Lines[num].UpdateDisplay(forceRefresh);
		}
	}

	public void UpdateAllLinesDisplayAfterAFrame(bool refreshSelection = true)
	{
		((MonoBehaviour)this).StartCoroutine(UpdateAllLinesDisplayAfterAFrameCoroutine(refreshSelection));
	}

	public void UpdateProgressionText(string text)
	{
		((TMP_Text)unlockProgressionText).text = text;
	}

	private IEnumerator UpdateAllLinesDisplayAfterAFrameCoroutine(bool refreshSelection = true)
	{
		yield return null;
		UpdateAllLinesDisplay();
		if (refreshSelection && InputManager.IsLastControllerJoystick && !TPSingleton<OraculumView>.Instance.TransitionRunning)
		{
			if (TPSingleton<OraculumView>.Instance.IsInDarkShop)
			{
				((MonoBehaviour)this).StartCoroutine(TPSingleton<DarkShopManager>.Instance.SelectFirstActiveChildEndOfFrame());
				TPSingleton<DarkShopManager>.Instance.RefreshShopJoystickNavigation();
			}
			else if (TPSingleton<OraculumView>.Instance.IsInLightShop)
			{
				((MonoBehaviour)this).StartCoroutine(TPSingleton<LightShopManager>.Instance.SelectFirstActiveChildEndOfFrame());
				TPSingleton<LightShopManager>.Instance.RefreshShopJoystickNavigation();
			}
		}
	}

	private void Start()
	{
		if ((Object)(object)CanvasGroup != (Object)null)
		{
			CanvasGroup.alpha = 0f;
		}
		ShowBackButton(show: false);
		NarrationView.Hide();
	}
}
