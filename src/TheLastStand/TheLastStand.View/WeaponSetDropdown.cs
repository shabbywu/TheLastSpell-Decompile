using System;
using System.Collections.Generic;
using TheLastStand.Framework.UI.TMPro;
using TheLastStand.Model.Unit;
using TheLastStand.View.HUD.UnitManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View;

public class WeaponSetDropdown : TMP_BetterDropdown
{
	[Serializable]
	public class WeaponSetOptionData : OptionData
	{
		public PlayableUnit PlayableUnit { get; private set; }

		public WeaponSetOptionData(string text, Sprite image, PlayableUnit playableUnit)
			: base(text, image)
		{
			PlayableUnit = playableUnit;
		}
	}

	protected class WeaponSetDropdownItem : DropdownItem
	{
		public WeaponSetView WeaponSetView;
	}

	[SerializeField]
	private WeaponSetView mainWeaponSet;

	[SerializeField]
	private WeaponSetView secondaryWeaponSet;

	[SerializeField]
	protected WeaponSetView itemWeaponSet;

	protected override DropdownItem AddItem(OptionData data, bool selected, DropdownItem itemTemplate, List<DropdownItem> items)
	{
		DropdownItem val = ((TMP_BetterDropdown)this).AddItem(data, selected, itemTemplate, items);
		if (data is WeaponSetOptionData weaponSetOptionData && val is WeaponSetDropdownItem weaponSetDropdownItem && (Object)(object)weaponSetDropdownItem.WeaponSetView != (Object)null)
		{
			weaponSetDropdownItem.WeaponSetView.RefreshEquipmentSlots(weaponSetOptionData.PlayableUnit, useEquippedSlots: true);
		}
		return val;
	}

	public override void RefreshShownValue()
	{
		((TMP_BetterDropdown)this).RefreshShownValue();
		if (((((TMP_BetterDropdown)this).options.Count > 0) ? ((TMP_BetterDropdown)this).options[Mathf.Clamp(((TMP_BetterDropdown)this).value, 0, ((TMP_BetterDropdown)this).options.Count - 1)] : null) is WeaponSetOptionData weaponSetOptionData)
		{
			if ((Object)(object)mainWeaponSet != (Object)null)
			{
				mainWeaponSet.RefreshEquipmentSlots(weaponSetOptionData.PlayableUnit, useEquippedSlots: true);
			}
			if ((Object)(object)secondaryWeaponSet != (Object)null)
			{
				secondaryWeaponSet.RefreshEquipmentSlots(weaponSetOptionData.PlayableUnit, useEquippedSlots: false);
			}
		}
		foreach (WeaponSetDropdownItem item in base.m_Items)
		{
			item.WeaponSetView.RefreshEquipmentSlots();
		}
	}

	protected override void AddDropdownItemComponent(Toggle itemToggle)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		WeaponSetDropdownItem weaponSetDropdownItem = ((Component)itemToggle).gameObject.AddComponent<WeaponSetDropdownItem>();
		((DropdownItem)weaponSetDropdownItem).text = base.m_ItemText;
		((DropdownItem)weaponSetDropdownItem).image = base.m_ItemImage;
		((DropdownItem)weaponSetDropdownItem).toggle = itemToggle;
		((DropdownItem)weaponSetDropdownItem).rectTransform = (RectTransform)((Component)itemToggle).transform;
		weaponSetDropdownItem.WeaponSetView = itemWeaponSet;
	}
}
