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
		DropdownItem dropdownItem = base.AddItem(data, selected, itemTemplate, items);
		if (data is WeaponSetOptionData weaponSetOptionData && dropdownItem is WeaponSetDropdownItem weaponSetDropdownItem && (Object)(object)weaponSetDropdownItem.WeaponSetView != (Object)null)
		{
			weaponSetDropdownItem.WeaponSetView.RefreshEquipmentSlots(weaponSetOptionData.PlayableUnit, useEquippedSlots: true);
		}
		return dropdownItem;
	}

	public override void RefreshShownValue()
	{
		base.RefreshShownValue();
		if (((base.options.Count > 0) ? base.options[Mathf.Clamp(base.value, 0, base.options.Count - 1)] : null) is WeaponSetOptionData weaponSetOptionData)
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
		foreach (WeaponSetDropdownItem item in m_Items)
		{
			item.WeaponSetView.RefreshEquipmentSlots();
		}
	}

	protected override void AddDropdownItemComponent(Toggle itemToggle)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		WeaponSetDropdownItem weaponSetDropdownItem = ((Component)itemToggle).gameObject.AddComponent<WeaponSetDropdownItem>();
		weaponSetDropdownItem.text = m_ItemText;
		weaponSetDropdownItem.image = m_ItemImage;
		weaponSetDropdownItem.toggle = itemToggle;
		weaponSetDropdownItem.rectTransform = (RectTransform)((Component)itemToggle).transform;
		weaponSetDropdownItem.WeaponSetView = itemWeaponSet;
	}
}
