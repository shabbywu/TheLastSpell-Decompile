using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class PlayableUnitTooltip : UnitTooltip
{
	[SerializeField]
	private UnitPortraitView portrait;

	[SerializeField]
	private UnitStatDisplay actionPointsDisplay;

	[SerializeField]
	private UnitStatDisplay manaDisplay;

	[SerializeField]
	private UnitStatDisplay overallDamageDisplay;

	[SerializeField]
	private UnitStatDisplay resistanceReductionDisplay;

	[SerializeField]
	private UnitStatDisplay accuracyDisplay;

	[SerializeField]
	private UnitStatDisplay criticalDamageDisplay;

	[SerializeField]
	private WeaponSetView weaponSetView;

	public PlayableUnit PlayableUnit => base.Unit as PlayableUnit;

	public void SetContent(PlayableUnit playableUnit)
	{
		base.Unit = playableUnit;
	}

	protected override bool CanBeDisplayed()
	{
		if (base.Unit != null)
		{
			return TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.FinalBossDeath;
		}
		return false;
	}

	protected override void RefreshContent()
	{
		base.RefreshContent();
		if ((Object)(object)portrait != (Object)null)
		{
			portrait.PlayableUnit = PlayableUnit;
			portrait.RefreshPortrait();
		}
		if ((Object)(object)actionPointsDisplay != (Object)null)
		{
			actionPointsDisplay.TargetUnit = PlayableUnit;
			actionPointsDisplay.Refresh();
		}
		if ((Object)(object)manaDisplay != (Object)null)
		{
			manaDisplay.TargetUnit = PlayableUnit;
			manaDisplay.Refresh();
		}
		if ((Object)(object)overallDamageDisplay != (Object)null)
		{
			overallDamageDisplay.TargetUnit = PlayableUnit;
			overallDamageDisplay.Refresh();
		}
		if ((Object)(object)resistanceReductionDisplay != (Object)null)
		{
			resistanceReductionDisplay.TargetUnit = PlayableUnit;
			resistanceReductionDisplay.Refresh();
		}
		if ((Object)(object)accuracyDisplay != (Object)null)
		{
			accuracyDisplay.TargetUnit = PlayableUnit;
			accuracyDisplay.Refresh();
		}
		if ((Object)(object)criticalDamageDisplay != (Object)null)
		{
			criticalDamageDisplay.TargetUnit = PlayableUnit;
			criticalDamageDisplay.Refresh();
		}
		RefreshEquipmentSlots();
		LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
		RefreshBackgroundSize();
	}

	public void RefreshEquipmentSlots()
	{
		weaponSetView.RefreshEquipmentSlots(PlayableUnit, useEquippedSlots: true);
	}
}
