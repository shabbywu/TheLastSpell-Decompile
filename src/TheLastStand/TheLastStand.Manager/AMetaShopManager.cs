using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Database;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using TheLastStand.View.HUD;
using TheLastStand.View.MetaShops;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace TheLastStand.Manager;

public abstract class AMetaShopManager<T> : Manager<T> where T : SerializedMonoBehaviour
{
	[SerializeField]
	protected Canvas shopCanvas;

	[SerializeField]
	protected MetaShopView shopView;

	[SerializeField]
	protected MetaUpgradeLineView metaUpgradeLinePrefab;

	[SerializeField]
	protected SimpleFontLocalizedParent fontLocalizedParent;

	[SerializeField]
	protected LayoutNavigationInitializer navigationInitializer;

	private MetaUpgrade currentFocusedMetaUpgrade;

	private bool hasAnActiveLine;

	private MetaUpgradeLineTooltipPackage metaUpgradeLineTooltipPackage;

	private Dictionary<MetaUpgrade, MetaUpgradeLineView> newMetaUpgrades = new Dictionary<MetaUpgrade, MetaUpgradeLineView>();

	private Tween scrollTween;

	private int siblingIndex;

	private HashSet<MetaShopTab> selectedTabs = new HashSet<MetaShopTab>();

	private bool sorted;

	protected abstract string ActivatedNumberLocalizationKey { get; }

	public Canvas Canvas => shopCanvas;

	public MetaShopView MetaShopView => shopView;

	public Dictionary<MetaUpgrade, MetaUpgradeLineView> Lines { get; } = new Dictionary<MetaUpgrade, MetaUpgradeLineView>();


	public List<MetaUpgradeLineView> SortedLines { get; protected set; }

	public Dictionary<MetaUpgrade, MetaUpgradeLineView> GetNewMetaUpgrades()
	{
		return (from x in Lines
			where TPSingleton<MetaShopsManager>.Instance.IsANewUpgrade(x.Key) && x.Value.State == MetaUpgradeLineView.E_State.Unlocked
			orderby ((Component)x.Value).transform.GetSiblingIndex()
			select x).ToDictionary((KeyValuePair<MetaUpgrade, MetaUpgradeLineView> o) => o.Key, (KeyValuePair<MetaUpgrade, MetaUpgradeLineView> o) => o.Value);
	}

	public void Init()
	{
		RefreshShop();
	}

	public void InitTooltips(MetaUpgradeLineTooltipPackage newMetaUpgradeLineTooltipPackage)
	{
		metaUpgradeLineTooltipPackage = newMetaUpgradeLineTooltipPackage;
	}

	public void RefreshActivatedLines()
	{
		RefreshActivatedLines(TPSingleton<OraculumView>.Instance.CurrentCategory, TPSingleton<MetaShopsManager>.Instance.CurrentFilter);
	}

	public void RefreshFilterView(MetaUpgradeDefinition.E_MetaUpgradeFilter filter)
	{
		MetaShopView.RefreshFilterView(filter);
	}

	public virtual void RefreshShop()
	{
		MetaUpgradesManager.RefreshFulfilledUpgrades();
		if (Lines.Count > 0)
		{
			UpdateNewMetaUpgrades();
		}
		siblingIndex = 0;
	}

	public virtual void RefreshShopView()
	{
		UpdateMetaTabs();
		UpdateActivatedMetasText();
		UpdateNewMetaUpgrades();
		RefreshSorting();
		SimpleFontLocalizedParent obj = fontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).RefreshChilds();
		}
	}

	public virtual void RefreshTexts()
	{
	}

	public void RefreshShopJoystickNavigation()
	{
		navigationInitializer.InitNavigation(reset: true);
	}

	public void SelectCategory(MetaUpgradeDefinition.E_MetaUpgradeCategory category, bool isSelectingFirstTab = false, bool updateLastSelectedTab = false)
	{
		MetaShopView.ScrollRect.verticalScrollbar.value = 1f;
		selectedTabs.Clear();
		bool flag = false;
		foreach (MetaShopTab tab in MetaShopView.Tabs)
		{
			bool flag2 = tab.Enabled && category == tab.Category;
			tab.Toggle(flag2);
			if (flag2)
			{
				selectedTabs.Add(tab);
				flag = true;
			}
		}
		if (!flag)
		{
			if (isSelectingFirstTab)
			{
				((CLogger<T>)this).LogError((object)"Tried to select the first tab available but none was found !", (CLogLevel)1, true, true);
			}
			else
			{
				SelectFirstTab(updateLastSelectedTab);
			}
			return;
		}
		RefreshActivatedLines(category, TPSingleton<MetaShopsManager>.Instance.CurrentFilter);
		if (updateLastSelectedTab)
		{
			TPSingleton<OraculumView>.Instance.CurrentCategory = category;
		}
		if (InputManager.IsLastControllerJoystick && IsRelatedShopOpen())
		{
			((MonoBehaviour)this).StartCoroutine(SelectFirstActiveChildEndOfFrame());
			RefreshShopJoystickNavigation();
		}
	}

	public void SelectFirstActiveChild()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
		if (hasAnActiveLine)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(shopView.JoystickDynamicTarget.GetSelectionInfo(), updateSelection: false);
			MetaUpgradeLineView metaUpgradeLineView = Lines.Values.OrderBy((MetaUpgradeLineView line) => ((Component)line).transform.GetSiblingIndex()).FirstOrDefault((MetaUpgradeLineView line) => line.IsActive == true);
			EventSystem.current.SetSelectedGameObject(((Object)(object)metaUpgradeLineView != (Object)null) ? ((Component)metaUpgradeLineView).gameObject : null);
		}
		else
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(shopView.JoystickDynamicTarget.GetSelectionInfo());
		}
	}

	public IEnumerator SelectFirstActiveChildEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		SelectFirstActiveChild();
	}

	public void SelectFirstTab(bool updateLastSelectedTab = false)
	{
		foreach (MetaShopTab tab in MetaShopView.Tabs)
		{
			if (tab.Enabled)
			{
				SelectCategory(tab.Category, isSelectingFirstTab: true, updateLastSelectedTab);
				break;
			}
		}
	}

	public void SelectNextTab()
	{
		List<MetaShopTab> list = (from o in MetaShopView.Tabs
			where ((Component)o).gameObject.activeSelf
			orderby ((Component)o).transform.GetSiblingIndex()
			select o).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Category == TPSingleton<OraculumView>.Instance.CurrentCategory)
			{
				SelectCategory(list[(i + 1) % list.Count].Category, isSelectingFirstTab: false, updateLastSelectedTab: true);
				break;
			}
		}
	}

	public void SelectPreviousTab()
	{
		List<MetaShopTab> list = (from o in MetaShopView.Tabs
			where ((Component)o).gameObject.activeSelf
			orderby ((Component)o).transform.GetSiblingIndex()
			select o).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Category == TPSingleton<OraculumView>.Instance.CurrentCategory)
			{
				SelectCategory(list[IntExtensions.Mod(i - 1, list.Count)].Category, isSelectingFirstTab: false, updateLastSelectedTab: true);
				break;
			}
		}
	}

	public void SetActive(bool toggle)
	{
		((Component)MetaShopView.LayoutGroupContainer).gameObject.SetActive(toggle);
		((Component)this).gameObject.SetActive(toggle);
	}

	public void UpdateActivatedMetasText()
	{
		float num = GetNumberOfActivatedMetas();
		MetaShopView.UpdateProgressionText(Localizer.Format(ActivatedNumberLocalizationKey, new object[3]
		{
			num,
			Lines.Count,
			Mathf.FloorToInt(num / (float)Lines.Count * 100f)
		}));
	}

	public void UpdateNewMetaUpgrades()
	{
		newMetaUpgrades = GetNewMetaUpgrades();
	}

	protected abstract void ApplySpecificSorting();

	protected abstract int GetDefaultSortingStartingIndex();

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	protected int GetNumberOfActivatedMetas()
	{
		return Lines.Count((KeyValuePair<MetaUpgrade, MetaUpgradeLineView> x) => TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Contains(x.Key));
	}

	protected abstract bool IsRelatedShopOpen();

	protected abstract bool IsValidUpgradeForShop(MetaUpgrade metaUpgrade);

	protected override void OnDestroy()
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		((CLogger<T>)this).OnDestroy();
		foreach (MetaShopTab tab in MetaShopView.Tabs)
		{
			tab.OnHover -= OnTabHovered;
			tab.OnClicked -= OnTabClicked;
		}
		foreach (MetaShopFilter filter in MetaShopView.Filters)
		{
			filter.OnToggled -= OnFilterToggled;
		}
		shopView.Sorter.OnToggled += OnSorterToggled;
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		((UnityEvent<Vector2>)(object)MetaShopView.ScrollRect.onValueChanged).RemoveListener((UnityAction<Vector2>)MetaShopView.UpdateAllLinesDisplay);
	}

	protected void RefreshUpgradesByState(List<MetaUpgrade> metaUpgrades, MetaUpgradeLineView.E_State stateToApply)
	{
		int i = 0;
		for (int count = metaUpgrades.Count; i < count; i++)
		{
			RefreshUpgrade(metaUpgrades[i], stateToApply);
		}
	}

	protected void RefreshUpgrade(MetaUpgrade metaUpgrade, MetaUpgradeLineView.E_State stateToApply)
	{
		if (!IsValidUpgradeForShop(metaUpgrade))
		{
			return;
		}
		if (Lines.TryGetValue(metaUpgrade, out var value))
		{
			if (value.State != stateToApply)
			{
				value.ChangeState(stateToApply);
			}
			else
			{
				value.RefreshData();
			}
		}
		else
		{
			value = Object.Instantiate<MetaUpgradeLineView>(metaUpgradeLinePrefab, (Transform)(object)MetaShopView.LayoutGroupContainer);
			value.Init(metaUpgrade, stateToApply, metaUpgradeLineTooltipPackage);
			value.SetActive(metaUpgrade.IsValidated(TPSingleton<OraculumView>.Instance.CurrentCategory, TPSingleton<MetaShopsManager>.Instance.CurrentFilter, newMetaUpgrades));
			Lines.Add(metaUpgrade, value);
			MetaShopView.AddLine(value);
		}
		((Component)value).transform.SetSiblingIndex(siblingIndex);
		siblingIndex++;
	}

	protected virtual void Start()
	{
		((UnityEvent<Vector2>)(object)MetaShopView.ScrollRect.onValueChanged).AddListener((UnityAction<Vector2>)MetaShopView.UpdateAllLinesDisplay);
		RefreshTexts();
		foreach (MetaShopTab tab in shopView.Tabs)
		{
			tab.OnHover += OnTabHovered;
		}
		foreach (MetaShopFilter filter in shopView.Filters)
		{
			filter.OnToggled += OnFilterToggled;
		}
		shopView.Sorter.OnToggled += OnSorterToggled;
	}

	private void ApplyDefaultSorting()
	{
		int defaultSortingStartingIndex = GetDefaultSortingStartingIndex();
		foreach (KeyValuePair<string, MetaUpgradeDefinition> metaUpgradesDefinition in MetaDatabase.MetaUpgradesDefinitions)
		{
			foreach (KeyValuePair<MetaUpgrade, MetaUpgradeLineView> line in Lines)
			{
				if (line.Key.MetaUpgradeDefinition == metaUpgradesDefinition.Value && TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Contains(line.Key))
				{
					((Component)line.Value).transform.SetSiblingIndex(defaultSortingStartingIndex++);
					break;
				}
			}
		}
		SortedLines.Sort((MetaUpgradeLineView a, MetaUpgradeLineView b) => ((Component)a).transform.GetSiblingIndex().CompareTo(((Component)b).transform.GetSiblingIndex()));
	}

	private void OnFilterToggled(MetaUpgradeDefinition.E_MetaUpgradeFilter metaUpgradeFilter, bool toggle)
	{
		ToggleFilter(metaUpgradeFilter, toggle);
	}

	private void OnSorterToggled(bool toggle)
	{
		ToggleSorting(toggle);
	}

	private void OnLocalize()
	{
		RefreshTexts();
	}

	private void OnTabClicked(MetaShopTab metaShopTab)
	{
		SelectCategory(metaShopTab.Category, isSelectingFirstTab: false, updateLastSelectedTab: true);
	}

	private void OnTabHovered(MetaShopTab metaShopTab, bool isHovering)
	{
		foreach (MetaShopTab selectedTab in selectedTabs)
		{
			selectedTab.ToggleCategoryTitle(!isHovering);
		}
	}

	private void RefreshActivatedLines(MetaUpgradeDefinition.E_MetaUpgradeCategory category, MetaUpgradeDefinition.E_MetaUpgradeFilter filter, bool refreshSelection = true)
	{
		hasAnActiveLine = false;
		foreach (KeyValuePair<MetaUpgrade, MetaUpgradeLineView> line in Lines)
		{
			bool flag = line.Key.IsValidated(category, filter, newMetaUpgrades);
			line.Value.SetActive(flag);
			if (flag)
			{
				hasAnActiveLine = true;
			}
		}
		shopView.ToggleFiltersUpNavigation(hasAnActiveLine);
		shopView.ToggleNoUpgradeFeedback(!hasAnActiveLine);
		MetaShopView.UpdateAllLinesDisplayAfterAFrame(refreshSelection);
	}

	private void RefreshSorting()
	{
		if (sorted)
		{
			ApplySpecificSorting();
		}
		else
		{
			ApplyDefaultSorting();
		}
		shopView.UpdateAllLinesDisplayAfterAFrame();
	}

	private void ToggleFilter(MetaUpgradeDefinition.E_MetaUpgradeFilter filter, bool toggle)
	{
		if (toggle)
		{
			TPSingleton<MetaShopsManager>.Instance.CurrentFilter |= filter;
		}
		else
		{
			TPSingleton<MetaShopsManager>.Instance.CurrentFilter &= ~filter;
		}
		RefreshActivatedLines(TPSingleton<OraculumView>.Instance.CurrentCategory, TPSingleton<MetaShopsManager>.Instance.CurrentFilter, refreshSelection: false);
		if (InputManager.IsLastControllerJoystick)
		{
			RefreshShopJoystickNavigation();
		}
	}

	private void ToggleSorting(bool toggle)
	{
		MetaShopView.ScrollRect.verticalScrollbar.value = 1f;
		sorted = toggle;
		RefreshSorting();
	}

	private void UpdateMetaTabs()
	{
		List<MetaUpgradeDefinition.E_MetaUpgradeCategory> list = new List<MetaUpgradeDefinition.E_MetaUpgradeCategory>(MetaShopView.Tabs.Select((MetaShopTab tab) => tab.Category));
		int num = list.Count - 1;
		foreach (KeyValuePair<MetaUpgrade, MetaUpgradeLineView> line in Lines)
		{
			if (num < 0)
			{
				break;
			}
			for (int num2 = num; num2 >= 0; num2--)
			{
				if ((list[num2] & line.Key.MetaUpgradeDefinition.Category) != 0)
				{
					list.RemoveAt(num2);
					num--;
				}
			}
		}
		foreach (MetaShopTab tab in MetaShopView.Tabs)
		{
			tab.OnClicked -= OnTabClicked;
			if (list.Contains(tab.Category))
			{
				tab.Enable(enable: false);
				continue;
			}
			tab.Enable(enable: true);
			tab.OnClicked += OnTabClicked;
		}
	}
}
