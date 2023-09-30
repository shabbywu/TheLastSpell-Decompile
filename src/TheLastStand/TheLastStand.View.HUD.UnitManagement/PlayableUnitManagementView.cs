using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Controller.Unit;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.Stat;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.HUD.UnitManagement;

public class PlayableUnitManagementView : UnitManagementView<PlayableUnitManagementView>
{
	[SerializeField]
	protected UnitPortraitView unitPortraitView;

	[SerializeField]
	protected UnitLevelDisplay unitLevelDisplay;

	[SerializeField]
	protected UnitStatWithRegenDisplay healthDisplay;

	[SerializeField]
	protected UnitStatWithRegenDisplay manaDisplay;

	[SerializeField]
	protected UnitStatDisplay actionPointsDisplay;

	[SerializeField]
	private ChangeEquipmentBoxView changeEquipmentBoxView;

	public static UnitPortraitView UnitPortraitView => TPSingleton<PlayableUnitManagementView>.Instance.unitPortraitView;

	public ChangeEquipmentBoxView ChangeEquipmentBoxView => changeEquipmentBoxView;

	public PlayableSkillBar PlayableSkillBar => TPSingleton<PlayableUnitManagementView>.Instance.SkillBar as PlayableSkillBar;

	public static void OnBuildingDestroyed()
	{
		if (TPSingleton<PlayableUnitManagementView>.Exist())
		{
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.Refresh();
		}
	}

	public static void OnGameStateChange(Game.E_State state)
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits)
		{
			return;
		}
		switch (state)
		{
		case Game.E_State.Management:
		case Game.E_State.CharacterSheet:
		case Game.E_State.UnitPreparingSkill:
		case Game.E_State.UnitExecutingSkill:
		case Game.E_State.Recruitment:
		case Game.E_State.Shopping:
		case Game.E_State.Wait:
		case Game.E_State.ProductionReport:
		case Game.E_State.Settings:
			UnitManagementView<PlayableUnitManagementView>.DisplayCanvas(TileObjectSelectionManager.HasPlayableUnitSelected);
			if (((Behaviour)TPSingleton<PlayableUnitManagementView>.Instance.canvas).enabled)
			{
				SimpleFontLocalizedParent obj2 = TPSingleton<PlayableUnitManagementView>.Instance.simpleFontLocalizedParent;
				if (obj2 != null)
				{
					((FontLocalizedParent)obj2).RegisterChilds();
				}
			}
			else
			{
				SimpleFontLocalizedParent obj3 = TPSingleton<PlayableUnitManagementView>.Instance.simpleFontLocalizedParent;
				if (obj3 != null)
				{
					((FontLocalizedParent)obj3).UnregisterChilds();
				}
			}
			SkillManager.SkillInfoPanel.Hide();
			break;
		default:
		{
			UnitManagementView<PlayableUnitManagementView>.DisplayCanvas(display: false);
			SimpleFontLocalizedParent obj = TPSingleton<PlayableUnitManagementView>.Instance.simpleFontLocalizedParent;
			if (obj != null)
			{
				((FontLocalizedParent)obj).UnregisterChilds();
			}
			SkillManager.SkillInfoPanel.Hide();
			break;
		}
		}
		if (state != Game.E_State.UnitPreparingSkill && state != Game.E_State.UnitExecutingSkill && TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManagementView>.Instance.skillBar.JoystickSkillBar.DeselectCurrentSkill();
		}
	}

	public static void OnSelectedBuildingChange()
	{
		if (TileObjectSelectionManager.SelectedBuilding != null)
		{
			UnitManagementView<PlayableUnitManagementView>.DisplayCanvas(display: false);
			SimpleFontLocalizedParent obj = TPSingleton<PlayableUnitManagementView>.Instance.simpleFontLocalizedParent;
			if (obj != null)
			{
				((FontLocalizedParent)obj).UnregisterChilds();
			}
		}
		else
		{
			UnitManagementView<PlayableUnitManagementView>.Refresh();
		}
	}

	public static void OnTurnStart()
	{
		Game.E_Cycle cycle = TPSingleton<GameManager>.Instance.Game.Cycle;
		if (cycle == Game.E_Cycle.Day || cycle != Game.E_Cycle.Night)
		{
			return;
		}
		switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
		{
		case Game.E_NightTurn.PlayableUnits:
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.ProductionReport)
			{
				UnitManagementView<PlayableUnitManagementView>.DisplayCanvas(TileObjectSelectionManager.HasUnitSelected);
				if (((Behaviour)TPSingleton<PlayableUnitManagementView>.Instance.canvas).enabled)
				{
					SimpleFontLocalizedParent obj2 = TPSingleton<PlayableUnitManagementView>.Instance.simpleFontLocalizedParent;
					if (obj2 != null)
					{
						((FontLocalizedParent)obj2).RegisterChilds();
					}
				}
				else
				{
					SimpleFontLocalizedParent obj3 = TPSingleton<PlayableUnitManagementView>.Instance.simpleFontLocalizedParent;
					if (obj3 != null)
					{
						((FontLocalizedParent)obj3).UnregisterChilds();
					}
				}
			}
			SkillManager.SkillInfoPanel.Hide();
			break;
		case Game.E_NightTurn.EnemyUnits:
		{
			UnitManagementView<PlayableUnitManagementView>.DisplayCanvas(display: false);
			SimpleFontLocalizedParent obj = TPSingleton<PlayableUnitManagementView>.Instance.simpleFontLocalizedParent;
			if (obj != null)
			{
				((FontLocalizedParent)obj).UnregisterChilds();
			}
			SkillManager.SkillInfoPanel.Hide();
			break;
		}
		}
	}

	public override void Init()
	{
		if (!initialized)
		{
			base.Init();
			healthDisplay.RegenStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthRegen];
			healthDisplay.Init();
			actionPointsDisplay.Init();
			manaDisplay.Init();
			healthDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
			healthDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthTotal];
			manaDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Mana];
			manaDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ManaTotal];
			manaDisplay.RegenStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ManaRegen];
			actionPointsDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPoints];
			actionPointsDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPointsTotal];
		}
	}

	public void OnChangeEquipmentButtonClick()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management)
		{
			TPSingleton<PlayableUnitManager>.Instance.ChangeEquipment();
		}
	}

	public void OnNextUnitButtonClick()
	{
		if (PlayableUnitManager.SelectedSkill != null)
		{
			PlayableUnitManager.SelectedSkill = null;
		}
		Game.E_State state = TPSingleton<GameManager>.Instance.Game.State;
		if (state == Game.E_State.Management || state == Game.E_State.UnitPreparingSkill)
		{
			PlayableUnitManager.SelectNextUnit();
		}
	}

	public void OnPreviousUnitButtonClick()
	{
		if (PlayableUnitManager.SelectedSkill != null)
		{
			PlayableUnitManager.SelectedSkill = null;
		}
		Game.E_State state = TPSingleton<GameManager>.Instance.Game.State;
		if (state == Game.E_State.Management || state == Game.E_State.UnitPreparingSkill)
		{
			PlayableUnitManager.SelectPreviousUnit();
		}
	}

	public void OnRecruitmentPanelButtonClick()
	{
		if (RecruitmentController.CanOpenRecruitmentPanel())
		{
			RecruitmentController.OpenRecruitmentPanel();
		}
	}

	public void OnUnitLevelButtonClick()
	{
		if (UnitLevelUpController.CanOpenUnitLevelUpView)
		{
			TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = TileObjectSelectionManager.SelectedPlayableUnit.LevelUp;
			TPSingleton<UnitLevelUpView>.Instance.Open();
		}
	}

	public bool IsChangeEquipmentBoxSelected()
	{
		return (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)changeEquipmentBoxView).gameObject;
	}

	public void RefreshEquipmentBoxContent()
	{
		changeEquipmentBoxView.RefreshEquipmentSlots();
	}

	public void RefreshEquipmentBoxSelectedSet(bool instantly = false)
	{
		changeEquipmentBoxView.RefreshChangeButton(TileObjectSelectionManager.SelectedPlayableUnit.EquippedWeaponSetIndex == 0, instantly);
	}

	public void RefreshModifiersLayoutView()
	{
		modifiersLayoutView.Refresh();
	}

	protected override void RefreshView()
	{
		if (TileObjectSelectionManager.SelectedUnit == null || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.NightReport || TileObjectSelectionManager.SelectedUnit is EnemyUnit)
		{
			UnitManagementView<PlayableUnitManagementView>.DisplayCanvas(display: false);
			SimpleFontLocalizedParent obj = TPSingleton<PlayableUnitManagementView>.Instance.simpleFontLocalizedParent;
			if (obj != null)
			{
				((FontLocalizedParent)obj).UnregisterChilds();
			}
			return;
		}
		base.RefreshView();
		healthDisplay.Refresh();
		PlayableSkillBar.Refresh();
		PlayableUnit playableUnit = TileObjectSelectionManager.SelectedUnit as PlayableUnit;
		((TMP_Text)TPSingleton<PlayableUnitManagementView>.Instance.unitName).text = playableUnit.Name;
		TPSingleton<PlayableUnitManagementView>.Instance.unitPortraitView.PlayableUnit = playableUnit;
		TPSingleton<PlayableUnitManagementView>.Instance.unitLevelDisplay.PlayableUnit = playableUnit;
		TPSingleton<PlayableUnitManagementView>.Instance.unitLevelDisplay.Refresh();
		TPSingleton<PlayableUnitManagementView>.Instance.manaDisplay.Refresh();
		TPSingleton<PlayableUnitManagementView>.Instance.actionPointsDisplay.Refresh();
		TPSingleton<PlayableUnitManagementView>.Instance.injuriesDisplay.Refresh(playableUnit);
		TPSingleton<PlayableUnitManagementView>.Instance.unitPortraitView.RefreshPortrait();
		TPSingleton<PlayableUnitManagementView>.Instance.RefreshEquipmentBoxContent();
		TPSingleton<PlayableUnitManagementView>.Instance.RefreshEquipmentBoxSelectedSet(instantly: true);
		GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraitsStats();
	}
}
