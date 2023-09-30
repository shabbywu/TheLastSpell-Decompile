using TMPro;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingSkillRepairPanel : BuildingCapacityPanel
{
	private const string RepairSkillId = "Repair";

	[SerializeField]
	private Image skillCostIconImage;

	[SerializeField]
	private TextMeshProUGUI skillCostText;

	public TheLastStand.Model.Building.Building Building { get; set; }

	public override void OnSkillPanelHovered(bool hover)
	{
		if (hover)
		{
			BuildingManager.BuildingSkillTooltip.SetContent("Repair", Building);
			BuildingManager.BuildingSkillTooltip.FollowTarget = ((Component)confirmButton).transform;
			BuildingManager.BuildingSkillTooltip.Display();
		}
		else
		{
			BuildingManager.BuildingSkillTooltip.Hide();
		}
	}

	public void OnRepairButtonClick()
	{
		buildingCapacitiesPanel.ChangeSelectedCapacityPanel(this);
	}

	public void OnRepairConfirmButtonClick(BetterButton button)
	{
		((Selectable)button).interactable = false;
		OnConfirmButtonClick();
		buildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
		ConstructionManager.RepairBuilding(Building);
		BuildingManager.BuildingSkillTooltip.Hide();
		GameView.BottomScreenPanel.BuildingManagementPanel.Refresh();
		buildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
	}

	public override void Refresh()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		bool interactable = ConstructionManager.CanRepairBuilding(Building.ConstructionModule);
		button.Interactable = interactable;
		skillCostIconImage.sprite = Building.BuildingView.GetBuildingCostIconSprite();
		((TMP_Text)skillCostText).text = Building.BuildingView.GetBuildingSkillCostString("Repair");
		((Graphic)skillCostText).color = TPSingleton<ResourceManager>.Instance.GetResourceColor(Building.ConstructionModule.CostsMaterials ? "Materials" : "Gold");
	}
}
