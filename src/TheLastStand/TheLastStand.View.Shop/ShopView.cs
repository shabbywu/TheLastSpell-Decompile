using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Item;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Shop;

public class ShopView : TPSingleton<ShopView>, IOverlayUser
{
	private static class Constants
	{
		public const string ItemCategoryResourcePathFormat = "View/Sprites/UI/ShopFilters/ShopFilters_{0}_On";

		public const string RerollAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_Shop";
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static TweenCallback _003C_003E9__92_0;

		public static Func<ShopSlot, JoystickSelectable> _003C_003E9__96_0;

		public static Func<JoystickSelectable, int> _003C_003E9__96_1;

		public static Func<JoystickSelectable, bool> _003C_003E9__96_2;

		public static Comparison<ShopSlot> _003C_003E9__121_0;

		public static Comparison<ShopSlot> _003C_003E9__121_1;

		public static Comparison<ShopSlot> _003C_003E9__121_2;

		internal void _003COpen_003Eb__92_0()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}

		internal JoystickSelectable _003CRefreshJoystickNavigation_003Eb__96_0(ShopSlot o)
		{
			return o.ShopSlotView.JoystickSelectable;
		}

		internal int _003CRefreshJoystickNavigation_003Eb__96_1(JoystickSelectable o)
		{
			return ((Component)o).transform.GetSiblingIndex();
		}

		internal bool _003CRefreshJoystickNavigation_003Eb__96_2(JoystickSelectable o)
		{
			return ((Component)o).gameObject.activeInHierarchy;
		}

		internal int _003CSort_003Eb__121_0(ShopSlot a, ShopSlot b)
		{
			return a.Item.Rarity.CompareTo(b.Item.Rarity);
		}

		internal int _003CSort_003Eb__121_1(ShopSlot a, ShopSlot b)
		{
			return a.Item.Level.CompareTo(b.Item.Level);
		}

		internal int _003CSort_003Eb__121_2(ShopSlot a, ShopSlot b)
		{
			return (a.Item.HasBeenSoldBefore ? a.Item.SellingPrice : a.Item.FinalPrice).CompareTo(b.Item.HasBeenSoldBefore ? b.Item.SellingPrice : b.Item.FinalPrice);
		}
	}

	[SerializeField]
	private CanvasGroup shopCanvasGroup;

	[SerializeField]
	private RectTransform shopShelvesParent;

	[SerializeField]
	private ShopSellRectView shopSellRectView;

	[SerializeField]
	private ShopSlotView shopSlotPrefab;

	[SerializeField]
	private GameObject shopShelfPrefab;

	[SerializeField]
	private UnitDropdownPanel unitDropdown;

	[SerializeField]
	private SimpleFontLocalizedParent fontLocalizedParent;

	[SerializeField]
	private Transform shopInventoryItemsPanelTransform;

	[SerializeField]
	private Scrollbar shopInventoryScrollbar;

	[SerializeField]
	private Transform shopItemsPanelTransform;

	[SerializeField]
	private ScrollRect inventoryScrollRect;

	[SerializeField]
	private RectTransform inventoryViewport;

	[SerializeField]
	private ScrollRect shelvesScrollRect;

	[SerializeField]
	private Scrollbar shopScrollbar;

	[SerializeField]
	private RectTransform scrollViewContentRectTransform;

	[SerializeField]
	private RectTransform shopSlotsScrollViewport;

	[SerializeField]
	private BetterButton rerollButton;

	[SerializeField]
	private TextMeshProUGUI rerollPrice;

	[SerializeField]
	private DataColor validPriceColor;

	[SerializeField]
	private DataColor invalidPriceColor;

	[SerializeField]
	private RectTransform filtersContainer;

	[SerializeField]
	private TMP_Dropdown categoryFilterDropdown;

	[SerializeField]
	private RectTransform categoryFilterScrollRect;

	[SerializeField]
	private RectTransform categoryFilterItem;

	[SerializeField]
	private TMP_Dropdown sortTypeDropdown;

	[SerializeField]
	private RectTransform sortTypeScrollRect;

	[SerializeField]
	private RectTransform sortItem;

	[SerializeField]
	private float dropdownRectsAdditionalHeight = 14f;

	[SerializeField]
	private float disabledRerollDropdownsWidth = 339f;

	[SerializeField]
	private ItemDefinition.E_Category[] displayedCategories;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	[SerializeField]
	private AudioClip[] openClips;

	[SerializeField]
	private AudioClip[] sellClips;

	[SerializeField]
	private AudioClip[] buyClips;

	[SerializeField]
	private bool playCloseSound = true;

	[SerializeField]
	private AudioClip closeClip;

	[SerializeField]
	private HUDJoystickTarget joystickTarget;

	[SerializeField]
	private HUDJoystickSimpleTarget shelvesJoystickTarget;

	[SerializeField]
	private LayoutNavigationInitializer shelvesNavigationInitializer;

	[SerializeField]
	private ShopInventoryToSlotsNavigation inventoryToSlotsNavigation;

	private int activeShelvesCount;

	private bool initialized;

	private float disabledRerollDropdownsWidthBase;

	private Tween fadeTween;

	private Canvas canvas;

	private List<GameObject> shopShelves = new List<GameObject>();

	private List<ShopSlot> orderedSlots = new List<ShopSlot>();

	private List<ShopSlot> soldOutSlots = new List<ShopSlot>();

	private string[] filtersKeys;

	private List<string> sortKeys;

	private int nextSoldItemClipIndex;

	private int nextBoughtItemClipIndex;

	private AudioClip[] rerollAudioClips;

	public bool HasActiveFilter => categoryFilterDropdown.value != 0;

	public bool HasActiveSort => sortTypeDropdown.value != 0;

	public DataColor InvalidPriceColor => invalidPriceColor;

	public Transform InventoryItemsPanelTransform => shopInventoryItemsPanelTransform;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public TheLastStand.Model.Building.Shop Shop { get; set; }

	public ShopSellRectView ShopSellRectView => shopSellRectView;

	public UnitDropdownPanel UnitDropdown => unitDropdown;

	public DataColor ValidPriceColor => validPriceColor;

	public HUDJoystickTarget JoystickTarget => joystickTarget;

	public static void Init()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!TPSingleton<ShopView>.Instance.initialized)
		{
			TPSingleton<ShopView>.Instance.canvas = ((Component)TPSingleton<ShopView>.Instance).GetComponent<Canvas>();
			((Behaviour)TPSingleton<ShopView>.Instance.canvas).enabled = false;
			TPSingleton<ShopView>.Instance.InitFilterDropdown();
			TPSingleton<ShopView>.Instance.InitSortDropdown();
			((UnityEvent<float>)(object)TPSingleton<ShopView>.Instance.shopScrollbar.onValueChanged).AddListener((UnityAction<float>)TPSingleton<ShopView>.Instance.ResetShopScrollbar);
			TPSingleton<ShopView>.Instance.disabledRerollDropdownsWidthBase = TPSingleton<ShopView>.Instance.filtersContainer.sizeDelta.x;
			TPSingleton<ShopView>.Instance.rerollAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_Shop", failSilently: false);
			TPSingleton<ShopView>.Instance.initialized = true;
		}
	}

	public void OnItemBought(ShopSlot shopSlot)
	{
		if (HasActiveSort)
		{
			((Component)shopSlot.ShopSlotView).transform.SetAsLastSibling();
		}
		if (HasActiveFilter)
		{
			((Component)shopSlot.ShopSlotView).gameObject.SetActive(false);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			((MonoBehaviour)this).StartCoroutine(RefreshJoystickNavigationEndOfFrame());
			if (HasActiveFilter)
			{
				((MonoBehaviour)this).StartCoroutine(RedirectSlotSelectionEndOfFrame(shopSlot));
			}
		}
		PlayBoughtItemSound();
	}

	public void OnItemSold(ShopSlot shopSlot)
	{
		if (HasActiveSort)
		{
			(TheLastStand.Model.Building.Shop.E_SortType sortType, int sortDirection) tuple = ParseSortDropdownData();
			TheLastStand.Model.Building.Shop.E_SortType item = tuple.sortType;
			int item2 = tuple.sortDirection;
			bool flag = false;
			orderedSlots.Remove(shopSlot);
			for (int i = 0; i < orderedSlots.Count; i++)
			{
				ShopSlot shopSlot2 = orderedSlots[(item2 > 0) ? i : (orderedSlots.Count - 1 - i)];
				if (shopSlot2.Item != null)
				{
					flag = item switch
					{
						TheLastStand.Model.Building.Shop.E_SortType.Rarity => !shopSlot2.IsSoldOut && shopSlot.Item.Rarity.CompareTo(shopSlot2.Item.Rarity) * item2 < 0, 
						TheLastStand.Model.Building.Shop.E_SortType.Level => !shopSlot2.IsSoldOut && shopSlot.Item.Level.CompareTo(shopSlot2.Item.Level) * item2 < 0, 
						TheLastStand.Model.Building.Shop.E_SortType.Price => !shopSlot2.IsSoldOut && (shopSlot.Item.HasBeenSoldBefore ? shopSlot.Item.SellingPrice : shopSlot.Item.FinalPrice).CompareTo(shopSlot2.Item.HasBeenSoldBefore ? shopSlot2.Item.SellingPrice : shopSlot2.Item.FinalPrice) * item2 < 0, 
						_ => flag, 
					};
					if (flag)
					{
						orderedSlots.Insert(i, shopSlot);
						((Component)shopSlot.ShopSlotView).transform.SetSiblingIndex(((Component)shopSlot2.ShopSlotView).transform.GetSiblingIndex());
						break;
					}
				}
			}
		}
		if (HasActiveFilter)
		{
			ItemDefinition.E_Category e_Category = ParseFilterDropdownCategory();
			((Component)shopSlot.ShopSlotView).gameObject.SetActive(e_Category.HasFlag(shopSlot.Item.ItemDefinition.Category));
		}
		if (InputManager.IsLastControllerJoystick)
		{
			((MonoBehaviour)this).StartCoroutine(RefreshJoystickNavigationEndOfFrame());
		}
		PlaySoldItemSound();
	}

	public ShopSlotView AddNewSlotView()
	{
		ShopSlotView shopSlotView = Object.Instantiate<ShopSlotView>(shopSlotPrefab, shopItemsPanelTransform);
		((Component)shopSlotView).gameObject.SetActive(true);
		inventoryToSlotsNavigation.ShelvesSlots.Add((Selectable)(object)shopSlotView.JoystickSelectable);
		return shopSlotView;
	}

	public void CheckShelves()
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			if (Shop.ShopSlots[i].ShopSlotView.ShopSlot != null)
			{
				num++;
			}
		}
		num += 2;
		int num2 = Mathf.FloorToInt((float)num / 3f);
		if (activeShelvesCount < num2)
		{
			GameObject val = FindFreeShelf();
			if ((Object)(object)val == (Object)null)
			{
				val = Object.Instantiate<GameObject>(shopShelfPrefab, (Transform)(object)shopShelvesParent);
			}
			else
			{
				val.SetActive(true);
			}
			shopShelves.Add(val);
			val.transform.SetSiblingIndex(num2);
			activeShelvesCount++;
			LayoutRebuilder.ForceRebuildLayoutImmediate(shopShelvesParent);
			scrollViewContentRectTransform.sizeDelta = new Vector2(scrollViewContentRectTransform.sizeDelta.x, shopShelvesParent.sizeDelta.y);
		}
	}

	public void ClearShelves()
	{
		for (int i = 0; i < shopShelves.Count; i++)
		{
			shopShelves[i].SetActive(false);
		}
		activeShelvesCount = 0;
	}

	public void Close(bool toAnotherPopup = false)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		CLoggerManager.Log((object)"ShopView closed", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
		CameraView.AttenuateWorldForPopupFocus(null);
		Tween obj = TPSingleton<ShopView>.Instance.fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		InventoryManager.InventoryView.DraggableItem.Reset();
		if (toAnotherPopup)
		{
			TPSingleton<ShopView>.Instance.shopCanvasGroup.alpha = 0f;
			DeactivateView();
		}
		else
		{
			TPSingleton<ShopView>.Instance.fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(TPSingleton<ShopView>.Instance.shopCanvasGroup, 0f, 0.4f), (Ease)9), new TweenCallback(DeactivateView)).SetFullId<TweenerCore<float, float, FloatOptions>>("ShopFadeOut", (Component)(object)this);
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OnPopupExitToWorld();
			}
		}
		TPSingleton<ShopView>.Instance.shopCanvasGroup.blocksRaycasts = false;
		TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.Hide();
		PlayCloseSound();
	}

	public void OnBotButtonClick()
	{
		shopScrollbar.value = Mathf.Clamp01(shopScrollbar.value - scrollButtonsSensitivity);
	}

	public void OnTopButtonClick()
	{
		shopScrollbar.value = Mathf.Clamp01(shopScrollbar.value + scrollButtonsSensitivity);
	}

	public void OnCloseButtonClick()
	{
		Shop.ShopController.CloseShopPanel();
	}

	public void OnGoldChanged(int gold)
	{
		RefreshPrices();
		RefreshRerollButton();
	}

	public void OnInventoryBotButtonClick()
	{
		Scrollbar obj = shopInventoryScrollbar;
		obj.value -= InventoryManager.InventoryView.ScrollButtonsSensitivity;
	}

	public void OnInventoryButtonClick()
	{
		if (TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory())
		{
			Shop.ShopController.CloseShopPanel(toAnotherPopup: true);
			TileObjectSelectionManager.EnsureUnitSelection();
			CharacterSheetManager.OpenCharacterSheetPanel(fromAnotherPopup: true, Shop.UnitToCompareIndex);
			TPSingleton<CharacterSheetPanel>.Instance.OpenInventory();
		}
	}

	public void OnInventoryTopButtonClick()
	{
		Scrollbar obj = shopInventoryScrollbar;
		obj.value += InventoryManager.InventoryView.ScrollButtonsSensitivity;
	}

	public void OnRerollButtonClick()
	{
		if (Shop.ShopController.TryToPayReroll())
		{
			SoundManager.PlayAudioClip(rerollAudioClips.PickRandom());
			OnShopReroll();
		}
	}

	public void OnShelvesItemSelected(RectTransform itemRectTransform)
	{
		if (InputManager.IsLastControllerJoystick)
		{
			GUIHelpers.AdjustScrollViewToFocusedItem(itemRectTransform, shopSlotsScrollViewport, shopScrollbar, 0.01f, 0.01f);
		}
	}

	public void OnInventoryItemSelected(RectTransform itemRectTransform)
	{
		if (InputManager.IsLastControllerJoystick)
		{
			GUIHelpers.AdjustScrollViewToFocusedItem(itemRectTransform, inventoryViewport, shopInventoryScrollbar, 0.01f, 0.01f);
		}
	}

	public void Open(bool instant = false)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		CLoggerManager.Log((object)"ShopView opened", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)this);
		Tween obj = fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		if ((Object)(object)fontLocalizedParent != (Object)null)
		{
			((FontLocalizedParent)fontLocalizedParent).RefreshChilds();
		}
		shopInventoryScrollbar.value = 1f;
		RefreshPrices();
		RefreshRerollButton();
		LayoutRebuilder.ForceRebuildLayoutImmediate(shopShelvesParent);
		scrollViewContentRectTransform.sizeDelta = new Vector2(scrollViewContentRectTransform.sizeDelta.x, shopShelvesParent.sizeDelta.y);
		if (instant)
		{
			shopCanvasGroup.alpha = 1f;
		}
		else
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
			TweenerCore<float, float, FloatOptions> obj2 = TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(shopCanvasGroup, 1f, 0.4f), (Ease)9).SetFullId<TweenerCore<float, float, FloatOptions>>("ShopFadeIn", (Component)(object)this);
			object obj3 = _003C_003Ec._003C_003E9__92_0;
			if (obj3 == null)
			{
				TweenCallback val = delegate
				{
					TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
				};
				_003C_003Ec._003C_003E9__92_0 = val;
				obj3 = (object)val;
			}
			fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(obj2, (TweenCallback)obj3);
		}
		ActivateAllSlots();
		((Behaviour)canvas).enabled = true;
		ToggleScrollRects(enable: true);
		shopScrollbar.value = 1f;
		shopCanvasGroup.blocksRaycasts = true;
		unitDropdown.ResetDropdown(Shop.UnitToCompareIndex);
		UIManager.GenericTooltip.Hide();
		PlayOpenSound();
		if (InputManager.IsLastControllerJoystick)
		{
			RefreshJoystickNavigation();
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(JoystickTarget.GetSelectionInfo());
		}
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnShopOpen);
	}

	public void RefreshPrices()
	{
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			Shop.ShopSlots[i].ShopSlotView.RefreshPriceColor();
			Shop.ShopSlots[i].ShopSlotView.RefreshLocalizedFonts();
		}
	}

	public void ResetSort()
	{
		orderedSlots.Clear();
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			orderedSlots.Add(Shop.ShopSlots[i]);
			((Component)Shop.ShopSlots[i].ShopSlotView).transform.SetAsLastSibling();
		}
	}

	public void RefreshRerollButton()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (!MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockShopRerollMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated) || effects.Length == 0)
		{
			((Component)rerollButton).gameObject.SetActive(false);
			filtersContainer.sizeDelta = new Vector2(disabledRerollDropdownsWidth, filtersContainer.sizeDelta.y);
			return;
		}
		((Component)rerollButton).gameObject.SetActive(true);
		filtersContainer.sizeDelta = new Vector2(disabledRerollDropdownsWidthBase, filtersContainer.sizeDelta.y);
		bool flag = (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)rerollButton).gameObject;
		int shopRerollPrice = Shop.ShopRerollPrice;
		((TMP_Text)rerollPrice).text = shopRerollPrice.ToString();
		bool flag2 = TPSingleton<ResourceManager>.Instance.Gold >= shopRerollPrice;
		((Graphic)rerollPrice).color = (flag2 ? ValidPriceColor._Color : InvalidPriceColor._Color);
		((Selectable)rerollButton).interactable = flag2;
		if (!flag2 && flag)
		{
			EventSystem.current.SetSelectedGameObject((GameObject)null);
			EventSystem.current.SetSelectedGameObject(((Component)rerollButton).gameObject);
		}
	}

	public void RefreshJoystickNavigation()
	{
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			((Selectable)(object)Shop.ShopSlots[i].ShopSlotView.JoystickSelectable).ClearNavigation();
		}
		shelvesNavigationInitializer.InitNavigation();
		List<JoystickSelectable> list = (from o in Shop.ShopSlots
			select o.ShopSlotView.JoystickSelectable into o
			orderby ((Component)o).transform.GetSiblingIndex()
			select o).ToList();
		List<JoystickSelectable> list2 = list.Where((JoystickSelectable o) => ((Component)o).gameObject.activeInHierarchy).ToList();
		JoystickSelectable selectOnDown = ((list2.Count > 0) ? list2[0] : null);
		((Selectable)(object)sortTypeDropdown).SetSelectOnDown((Selectable)(object)selectOnDown);
		((Selectable)(object)rerollButton).SetSelectOnDown((Selectable)(object)selectOnDown);
		((Selectable)(object)sortTypeDropdown).SetSelectOnLeft((Selectable)(object)(((Component)rerollButton).gameObject.activeSelf ? rerollButton : null));
		((Selectable)(object)categoryFilterDropdown).SetSelectOnLeft((Selectable)(object)(((Component)rerollButton).gameObject.activeSelf ? rerollButton : null));
		for (int j = 0; j < Mathf.Min(3, list2.Count); j++)
		{
			((Selectable)(object)list2[j]).SetSelectOnUp((Selectable)(object)sortTypeDropdown);
		}
		JoystickSelectable joystickSelectable = ((Component)InventoryItemsPanelTransform.GetChild(0)).GetComponent<ShopInventorySlotView>().JoystickSelectable;
		for (int k = 0; k < list2.Count; k++)
		{
			Navigation navigation = ((Selectable)list2[k]).navigation;
			if ((Object)(object)((Navigation)(ref navigation)).selectOnRight == (Object)null)
			{
				((Selectable)(object)list2[k]).SetSelectOnRight((Selectable)(object)joystickSelectable);
			}
		}
		shelvesJoystickTarget.ClearSelectables();
		shelvesJoystickTarget.AddSelectables((IEnumerable<Selectable>)list);
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void ActivateAllSlots()
	{
		foreach (ShopSlot shopSlot in Shop.ShopSlots)
		{
			((Behaviour)shopSlot.ShopSlotView).enabled = true;
			shopSlot.ShopSlotView.Toggle(toggle: true);
		}
		ToggleAllShopInventorySlots(toggle: true);
	}

	private void ClearFilters()
	{
		categoryFilterDropdown.value = 0;
		sortTypeDropdown.value = 0;
	}

	private void DeactivateAllSlots()
	{
		foreach (ShopSlot shopSlot in Shop.ShopSlots)
		{
			shopSlot.ShopSlotView.OnPointerExit(null);
			((Behaviour)shopSlot.ShopSlotView).enabled = false;
			shopSlot.ShopSlotView.Toggle(toggle: false);
		}
		ToggleAllShopInventorySlots(toggle: false);
	}

	private void DeactivateView()
	{
		((Behaviour)canvas).enabled = false;
		ToggleScrollRects(enable: false);
		ClearFilters();
		DeactivateAllSlots();
	}

	private void FilterByCategory(ItemDefinition.E_Category category)
	{
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			ShopSlot shopSlot = Shop.ShopSlots[i];
			bool active = shopSlot.Item != null && (category == ItemDefinition.E_Category.All || (!shopSlot.IsSoldOut && category.HasFlag(shopSlot.Item.ItemDefinition.Category)));
			((Component)shopSlot.ShopSlotView).gameObject.SetActive(active);
		}
	}

	private GameObject FindFreeShelf()
	{
		for (int i = 0; i < shopShelves.Count; i++)
		{
			if (!shopShelves[i].activeSelf)
			{
				return shopShelves[i];
			}
		}
		return null;
	}

	private void InitFilterDropdown()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		List<OptionData> list = new List<OptionData>();
		filtersKeys = new string[displayedCategories.Length];
		for (int i = 0; i < displayedCategories.Length; i++)
		{
			string localizationKey = displayedCategories[i].GetLocalizationKey();
			filtersKeys[i] = localizationKey;
			OptionData item = new OptionData
			{
				text = Localizer.Get(localizationKey),
				image = ((displayedCategories[i] != ItemDefinition.E_Category.All && displayedCategories[i] != 0) ? ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/ShopFilters/ShopFilters_{displayedCategories[i]}_On", failSilently: false) : null)
			};
			list.Add(item);
		}
		RectTransform obj = categoryFilterScrollRect;
		float x = categoryFilterScrollRect.sizeDelta.x;
		Rect rect = categoryFilterItem.rect;
		obj.sizeDelta = new Vector2(x, ((Rect)(ref rect)).height * (float)list.Count + dropdownRectsAdditionalHeight);
		categoryFilterDropdown.options = list;
		((UnityEvent<int>)(object)categoryFilterDropdown.onValueChanged).AddListener((UnityAction<int>)OnCategoryFilterDropdownValueChanged);
	}

	private void InitInventorySlots()
	{
		for (int i = 0; i < InventoryItemsPanelTransform.childCount; i++)
		{
			((Component)InventoryItemsPanelTransform.GetChild(i)).GetComponent<ShopInventorySlotView>().ItemIndex = i;
		}
	}

	private void InitSortDropdown()
	{
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		List<OptionData> list = new List<OptionData>();
		TheLastStand.Model.Building.Shop.E_SortType[] array = (TheLastStand.Model.Building.Shop.E_SortType[])Enum.GetValues(typeof(TheLastStand.Model.Building.Shop.E_SortType));
		sortKeys = new List<string>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == TheLastStand.Model.Building.Shop.E_SortType.None)
			{
				string text = string.Format("{0}{1}", "Shop_Sort_", array[i]);
				sortKeys.Add(text);
				list.Add(new OptionData
				{
					text = Localizer.Get(text)
				});
				continue;
			}
			string text2 = string.Format("{0}{1}_{2}", "Shop_Sort_", array[i], "Ascending");
			string text3 = string.Format("{0}{1}_{2}", "Shop_Sort_", array[i], "Descending");
			sortKeys.Add(text2);
			sortKeys.Add(text3);
			OptionData item = new OptionData
			{
				text = Localizer.Get(text2),
				image = ResourcePooler.LoadOnce<Sprite>(string.Format("View/Sprites/UI/ShopFilters/ShopFilters_{0}_On", string.Format("{0}{1}", array[i], "Ascending")), failSilently: false)
			};
			OptionData item2 = new OptionData
			{
				text = Localizer.Get(text3),
				image = ResourcePooler.LoadOnce<Sprite>(string.Format("View/Sprites/UI/ShopFilters/ShopFilters_{0}_On", string.Format("{0}{1}", array[i], "Descending")), failSilently: false)
			};
			list.Add(item);
			list.Add(item2);
		}
		RectTransform obj = sortTypeScrollRect;
		float x = sortTypeScrollRect.sizeDelta.x;
		Rect rect = sortItem.rect;
		obj.sizeDelta = new Vector2(x, ((Rect)(ref rect)).height * (float)list.Count + dropdownRectsAdditionalHeight);
		sortTypeDropdown.options = list;
		((UnityEvent<int>)(object)sortTypeDropdown.onValueChanged).AddListener((UnityAction<int>)OnSortDropdownValueChanged);
	}

	private void OnCategoryFilterDropdownValueChanged(int value)
	{
		ItemDefinition.E_Category category = ParseFilterDropdownCategory();
		FilterByCategory(category);
		if (!HasActiveFilter)
		{
			categoryFilterDropdown.captionText.text = Localizer.Get("Shop_Filter_Title");
		}
		if (InputManager.IsLastControllerJoystick)
		{
			((MonoBehaviour)this).StartCoroutine(RefreshJoystickNavigationEndOfFrame());
		}
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		((UnityEvent<int>)(object)categoryFilterDropdown.onValueChanged).RemoveListener((UnityAction<int>)OnCategoryFilterDropdownValueChanged);
		((UnityEvent<int>)(object)sortTypeDropdown.onValueChanged).RemoveListener((UnityAction<int>)OnSortDropdownValueChanged);
	}

	private void OnLocalize()
	{
		for (int i = 0; i < filtersKeys.Length; i++)
		{
			categoryFilterDropdown.options[i].text = Localizer.Get(filtersKeys[i]);
		}
		for (int j = 0; j < sortKeys.Count; j++)
		{
			sortTypeDropdown.options[j].text = Localizer.Get(sortKeys[j]);
		}
		sortTypeDropdown.RefreshShownValue();
		categoryFilterDropdown.RefreshShownValue();
		if (!HasActiveSort)
		{
			sortTypeDropdown.captionText.text = Localizer.Get("Shop_Sort_Title");
		}
		if (!HasActiveFilter)
		{
			categoryFilterDropdown.captionText.text = Localizer.Get("Shop_Filter_Title");
		}
	}

	private void OnShopReroll()
	{
		if (HasActiveFilter)
		{
			ItemDefinition.E_Category category = ParseFilterDropdownCategory();
			FilterByCategory(category);
		}
		if (HasActiveSort)
		{
			var (sortType, sortDirection) = ParseSortDropdownData();
			Sort(sortType, sortDirection);
		}
		if (!InputManager.IsLastControllerJoystick)
		{
			return;
		}
		((MonoBehaviour)this).StartCoroutine(RefreshJoystickNavigationEndOfFrame());
		if (EventSystem.current.currentSelectedGameObject.activeInHierarchy)
		{
			return;
		}
		for (int num = shopItemsPanelTransform.childCount - 1; num >= 0; num--)
		{
			GameObject gameObject = ((Component)shopItemsPanelTransform.GetChild(num)).gameObject;
			if (gameObject.activeInHierarchy)
			{
				EventSystem.current.SetSelectedGameObject(gameObject);
				break;
			}
		}
	}

	private void OnSortDropdownValueChanged(int value)
	{
		var (sortType, sortDirection) = ParseSortDropdownData();
		Sort(sortType, sortDirection);
		if (!HasActiveSort)
		{
			sortTypeDropdown.captionText.text = Localizer.Get("Shop_Sort_Title");
		}
		if (InputManager.IsLastControllerJoystick)
		{
			((MonoBehaviour)this).StartCoroutine(RefreshJoystickNavigationEndOfFrame());
		}
	}

	private ItemDefinition.E_Category ParseFilterDropdownCategory()
	{
		return displayedCategories[categoryFilterDropdown.value];
	}

	private (TheLastStand.Model.Building.Shop.E_SortType sortType, int sortDirection) ParseSortDropdownData()
	{
		int value = sortTypeDropdown.value;
		TheLastStand.Model.Building.Shop.E_SortType item = (TheLastStand.Model.Building.Shop.E_SortType)Mathf.CeilToInt((float)value / 2f);
		int item2 = ((value % 2 != 0) ? 1 : (-1));
		return (sortType: item, sortDirection: item2);
	}

	private void PlaySoldItemSound()
	{
		AudioClip audioClip = sellClips[nextSoldItemClipIndex];
		int num = nextSoldItemClipIndex;
		do
		{
			nextSoldItemClipIndex = Random.Range(0, sellClips.Length);
		}
		while (nextSoldItemClipIndex == num);
		SoundManager.PlayAudioClip(audioClip);
	}

	private void PlayBoughtItemSound()
	{
		AudioClip audioClip = buyClips[nextBoughtItemClipIndex];
		int num = nextBoughtItemClipIndex;
		do
		{
			nextBoughtItemClipIndex = Random.Range(0, buyClips.Length);
		}
		while (nextBoughtItemClipIndex == num);
		SoundManager.PlayAudioClip(audioClip);
	}

	private void PlayOpenSound()
	{
		SoundManager.PlayAudioClip(openClips.PickRandom());
	}

	private void PlayCloseSound()
	{
		if (playCloseSound)
		{
			SoundManager.PlayAudioClip(closeClip);
		}
	}

	private IEnumerator RefreshJoystickNavigationEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		RefreshJoystickNavigation();
	}

	private IEnumerator RedirectSlotSelectionEndOfFrame(ShopSlot slot)
	{
		yield return SharedYields.WaitForEndOfFrame;
		TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(JoystickTarget.GetSelectionInfo());
	}

	private void ResetShopScrollbar(float value)
	{
		((UnityEvent<float>)(object)shopScrollbar.onValueChanged).RemoveListener((UnityAction<float>)ResetShopScrollbar);
		shopScrollbar.value = 1f;
	}

	private void Sort(TheLastStand.Model.Building.Shop.E_SortType sortType, int sortDirection)
	{
		if (sortType == TheLastStand.Model.Building.Shop.E_SortType.None)
		{
			ResetSort();
			return;
		}
		orderedSlots.Clear();
		soldOutSlots.Clear();
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			ShopSlot shopSlot = Shop.ShopSlots[i];
			if (shopSlot.Item != null)
			{
				if (shopSlot.IsSoldOut)
				{
					soldOutSlots.Add(Shop.ShopSlots[i]);
				}
				else
				{
					orderedSlots.Add(Shop.ShopSlots[i]);
				}
			}
		}
		switch (sortType)
		{
		case TheLastStand.Model.Building.Shop.E_SortType.Rarity:
			orderedSlots.Sort((ShopSlot a, ShopSlot b) => a.Item.Rarity.CompareTo(b.Item.Rarity));
			break;
		case TheLastStand.Model.Building.Shop.E_SortType.Level:
			orderedSlots.Sort((ShopSlot a, ShopSlot b) => a.Item.Level.CompareTo(b.Item.Level));
			break;
		case TheLastStand.Model.Building.Shop.E_SortType.Price:
			orderedSlots.Sort((ShopSlot a, ShopSlot b) => (a.Item.HasBeenSoldBefore ? a.Item.SellingPrice : a.Item.FinalPrice).CompareTo(b.Item.HasBeenSoldBefore ? b.Item.SellingPrice : b.Item.FinalPrice));
			break;
		}
		for (int j = 0; j < orderedSlots.Count; j++)
		{
			if (sortDirection == 1)
			{
				((Component)orderedSlots[j].ShopSlotView).transform.SetAsLastSibling();
			}
			else
			{
				((Component)orderedSlots[j].ShopSlotView).transform.SetAsFirstSibling();
			}
		}
		for (int k = 0; k < soldOutSlots.Count; k++)
		{
			((Component)soldOutSlots[k].ShopSlotView).transform.SetAsLastSibling();
		}
	}

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		InitInventorySlots();
		categoryFilterDropdown.captionText.text = Localizer.Get("Shop_Filter_Title");
		sortTypeDropdown.captionText.text = Localizer.Get("Shop_Sort_Title");
	}

	private void ToggleAllShopInventorySlots(bool toggle)
	{
		foreach (ShopInventorySlot shopInventorySlot in Shop.ShopInventorySlots)
		{
			shopInventorySlot.ShopInventorySlotView.Toggle(toggle);
		}
	}

	private void ToggleScrollRects(bool enable)
	{
		((Behaviour)shelvesScrollRect).enabled = enable;
		((Behaviour)inventoryScrollRect).enabled = enable;
	}

	private void Update()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping)
		{
			if (InputManager.GetButtonDown(29))
			{
				Shop.ShopController.CloseShopPanel();
			}
			else if (InputManager.GetButtonDown(101) && Shop.ShopController.TryToPayReroll())
			{
				OnShopReroll();
			}
		}
	}
}
