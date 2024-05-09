using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.ItemRestriction;

public class WeaponRestrictionsCategoryPanel : MonoBehaviour
{
	[SerializeField]
	private float scrollSensitivity = 0.1f;

	[SerializeField]
	private RectTransform weaponFamilyDisplaysContainer;

	[SerializeField]
	private RectTransform weaponFamiliesViewport;

	[SerializeField]
	private Scrollbar weaponFamiliesScrollbar;

	[SerializeField]
	private GridLayoutGroup gridLayoutGroup;

	[SerializeField]
	private WeaponFamilyDisplay weaponFamilyDisplayPrefab;

	[SerializeField]
	private LayoutNavigationInitializer weaponFamiliesGridNavigationInitializer;

	public List<WeaponFamilyDisplay> WeaponFamilyDisplays { get; } = new List<WeaponFamilyDisplay>();


	public ItemDefinition.E_Category CurrentItemCategory { get; private set; }

	public GridLayoutGroup GridLayoutGroup => gridLayoutGroup;

	public void Init(ItemDefinition.E_Category itemCategory)
	{
		CurrentItemCategory = itemCategory;
		InstantiateWeaponFamilyDisplays();
		InitWeaponFamilyDisplays();
	}

	public bool TryAddNewWeaponFamilyDisplays()
	{
		int unlockedFamiliesNb = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.GetUnlockedFamiliesNb(CurrentItemCategory);
		if (unlockedFamiliesNb > WeaponFamilyDisplays.Count)
		{
			InstantiateWeaponFamilyDisplays(unlockedFamiliesNb);
			InitWeaponFamilyDisplays();
			return true;
		}
		return false;
	}

	public void OnGlyphsTopButtonClick()
	{
		Scrollbar obj = weaponFamiliesScrollbar;
		obj.value -= scrollSensitivity;
	}

	public void OnGlyphsBotButtonClick()
	{
		Scrollbar obj = weaponFamiliesScrollbar;
		obj.value += scrollSensitivity;
	}

	public void Refresh()
	{
		foreach (WeaponFamilyDisplay weaponFamilyDisplay in WeaponFamilyDisplays)
		{
			weaponFamilyDisplay.Refresh();
		}
	}

	public WeaponFamilyDisplay GetClosestWeaponFamilyDisplayFromRowIndex(int rowIndex, bool getClosestFromLeft)
	{
		int count = WeaponFamilyDisplays.Count;
		while (rowIndex >= 0)
		{
			int num = 0;
			int num2 = rowIndex * gridLayoutGroup.constraintCount;
			if (getClosestFromLeft)
			{
				num = num2;
			}
			else
			{
				int num3 = rowIndex * gridLayoutGroup.constraintCount + (gridLayoutGroup.constraintCount - 1);
				while (num3 >= count && num3 != num2)
				{
					num3--;
					if (num3 < count && ((Component)WeaponFamilyDisplays[num3]).gameObject.activeSelf)
					{
						return WeaponFamilyDisplays[num3];
					}
				}
				num = num3;
			}
			if (num < count && ((Component)WeaponFamilyDisplays[num]).gameObject.activeSelf)
			{
				return WeaponFamilyDisplays[num];
			}
			rowIndex--;
		}
		return null;
	}

	private void InitWeaponFamilyDisplays()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		if (!TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamiliesByItemCategory.TryGetValue(CurrentItemCategory, out var value))
		{
			return;
		}
		int count = WeaponFamilyDisplays.Count;
		List<ItemRestrictionFamily> list = value.FindAll((ItemRestrictionFamily anItemFamily) => anItemFamily.HasUnlockedItems);
		int count2 = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (i < count2)
			{
				((Component)WeaponFamilyDisplays[i]).gameObject.SetActive(true);
				WeaponFamilyDisplays[i].Init(list[i], TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories);
			}
			else
			{
				((Component)WeaponFamilyDisplays[i]).gameObject.SetActive(false);
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)((Component)weaponFamiliesGridNavigationInitializer).transform);
		weaponFamiliesGridNavigationInitializer.InitNavigation(reset: true);
	}

	private void InstantiateWeaponFamilyDisplays(int unlockedWeaponFamilies = 0)
	{
		if (unlockedWeaponFamilies == 0)
		{
			unlockedWeaponFamilies = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.GetUnlockedFamiliesNb(CurrentItemCategory);
		}
		int num = unlockedWeaponFamilies - WeaponFamilyDisplays.Count;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				CreatedWeaponFamilyDisplay();
			}
		}
	}

	private void CreatedWeaponFamilyDisplay()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		WeaponFamilyDisplay weaponFamilyDisplay = Object.Instantiate<WeaponFamilyDisplay>(weaponFamilyDisplayPrefab, (Transform)(object)weaponFamilyDisplaysContainer);
		((Component)weaponFamilyDisplay).GetComponent<JoystickSelectable>().AddListenerOnSelect((UnityAction)delegate
		{
			WeaponRestrictionsCategoryPanel weaponRestrictionsCategoryPanel = this;
			Transform transform = ((Component)weaponFamilyDisplay).transform;
			weaponRestrictionsCategoryPanel.OnJoystickSelect((RectTransform)(object)((transform is RectTransform) ? transform : null));
		});
		WeaponFamilyDisplays.Add(weaponFamilyDisplay);
	}

	private void OnJoystickSelect(RectTransform source)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(source, weaponFamiliesViewport, weaponFamiliesScrollbar, 0.02f, 0f, 0.1f);
	}
}
