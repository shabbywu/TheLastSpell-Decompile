using System;
using TPLib;
using TPLib.Localization;
using TheLastStand.Framework.UI.TMPro;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.ProductionReport;
using UnityEngine;

namespace TheLastStand.View.Unit.UI;

public class UnitDropdownPanel : MonoBehaviour
{
	[SerializeField]
	private WeaponSetDropdown unitDropdown;

	[SerializeField]
	private UnitPortraitView unitToComparePortrait;

	[SerializeField]
	private WeaponSetView equippedWeaponSet;

	[SerializeField]
	private WeaponSetView otherWeaponSet;

	public event Action<int> OnUnitToCompareChanged = delegate
	{
	};

	public void OnChangeUnitToCompare()
	{
		int num = ((TMP_BetterDropdown)unitDropdown).value - 1;
		PlayableUnit playableUnit = ((num != -1) ? TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num] : null);
		unitToComparePortrait.PlayableUnit = playableUnit;
		unitToComparePortrait.RefreshPortrait();
		((TMP_BetterDropdown)unitDropdown).RefreshShownValue();
		this.OnUnitToCompareChanged(num);
	}

	public void ResetDropdown(int unitSelected = 0)
	{
		((TMP_BetterDropdown)unitDropdown).options.Clear();
		((TMP_BetterDropdown)unitDropdown).options.Add((OptionData)(object)new WeaponSetDropdown.WeaponSetOptionData(Localizer.Get("Shop_HeroesDropdown_None"), null, null));
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			((TMP_BetterDropdown)unitDropdown).options.Add((OptionData)(object)new WeaponSetDropdown.WeaponSetOptionData(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].Name, null, TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i]));
		}
		((TMP_BetterDropdown)unitDropdown).value = unitSelected;
		OnChangeUnitToCompare();
	}

	public void SelectNextUnit()
	{
		PlayableUnitManager.SelectNextUnit();
		int newUnitIndex = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.Shopping:
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.ChangeUnitToCompareAndResetDropdown(newUnitIndex);
			break;
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
			TPSingleton<ChooseRewardPanel>.Instance.ChangeUnitToCompareAndResetDropdown(newUnitIndex);
			break;
		}
	}

	public void SelectPreviousUnit()
	{
		PlayableUnitManager.SelectPreviousUnit();
		int newUnitIndex = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.Shopping:
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.ChangeUnitToCompareAndResetDropdown(newUnitIndex);
			break;
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
			TPSingleton<ChooseRewardPanel>.Instance.ChangeUnitToCompareAndResetDropdown(newUnitIndex);
			break;
		}
	}
}
