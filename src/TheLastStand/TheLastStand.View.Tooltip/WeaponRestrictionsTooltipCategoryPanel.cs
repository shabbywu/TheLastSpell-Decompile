using System;
using System.Text;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item.ItemRestriction;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip;

public class WeaponRestrictionsTooltipCategoryPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI itemCategoryTitle;

	[SerializeField]
	private Image itemCategoryIcon;

	[SerializeField]
	private TextMeshProUGUI selectedFamiliesDescription;

	private ItemDefinition.E_Category currentItemCategory;

	public void SetContent(ItemDefinition.E_Category itemCategory)
	{
		currentItemCategory = itemCategory;
		RefreshIcon();
		RefreshText();
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(RefreshText));
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(RefreshText));
	}

	private void RefreshIcon()
	{
		if (currentItemCategory != 0)
		{
			switch (currentItemCategory)
			{
			case ItemDefinition.E_Category.MagicWeapon:
				itemCategoryIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_MagicalDamage", failSilently: false);
				break;
			case ItemDefinition.E_Category.MeleeWeapon:
				itemCategoryIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_PhysicalDamage", failSilently: false);
				break;
			case ItemDefinition.E_Category.RangeWeapon:
				itemCategoryIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_RangedDamage", failSilently: false);
				break;
			case ItemDefinition.E_Category.MeleeWeapon | ItemDefinition.E_Category.RangeWeapon:
				break;
			}
		}
	}

	private void RefreshText()
	{
		if (currentItemCategory == ItemDefinition.E_Category.None)
		{
			return;
		}
		((TMP_Text)itemCategoryTitle).text = Localizer.Get(string.Format("{0}{1}", "ItemRestrictionCategoryTooltipName_", currentItemCategory));
		StringBuilder stringBuilder = new StringBuilder();
		if (TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamiliesByItemCategory.TryGetValue(currentItemCategory, out var value))
		{
			foreach (ItemRestrictionFamily item in value)
			{
				if (item.IsActive)
				{
					stringBuilder.Append("<style=DarkShopKW>• " + item.LocalizedName + "</style>\r\n");
				}
			}
		}
		((TMP_Text)selectedFamiliesDescription).text = stringBuilder.ToString();
	}
}
