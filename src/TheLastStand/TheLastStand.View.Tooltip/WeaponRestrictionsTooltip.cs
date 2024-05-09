using System;
using System.Collections.Generic;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Manager.Item;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip;

public class WeaponRestrictionsTooltip : TooltipBase
{
	[SerializeField]
	private List<WeaponRestrictionsTooltipCategoryPanel> weaponCategoryPanels;

	[SerializeField]
	private Image boundlessModeIcon;

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(RefreshText));
	}

	protected override bool CanBeDisplayed()
	{
		return !TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.AreAllUnlockedFamiliesSelected();
	}

	protected override void RefreshContent()
	{
		RefreshText();
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(RefreshText));
	}

	private void RefreshText()
	{
		int count = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.ItemCategoriesCollectionDefinition.itemCategoryDefinitions.Count;
		for (int i = 0; i < weaponCategoryPanels.Count; i++)
		{
			if (i < count)
			{
				((Component)weaponCategoryPanels[i]).gameObject.SetActive(true);
				ItemRestrictionCategoryDefinition itemRestrictionCategoryDefinition = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.ItemCategoriesCollectionDefinition.itemCategoryDefinitions[i];
				weaponCategoryPanels[i].SetContent(itemRestrictionCategoryDefinition.ItemCategory);
			}
			else
			{
				((Component)weaponCategoryPanels[i]).gameObject.SetActive(false);
			}
		}
	}

	private void Start()
	{
		((Behaviour)boundlessModeIcon).enabled = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive;
		RefreshText();
	}
}
