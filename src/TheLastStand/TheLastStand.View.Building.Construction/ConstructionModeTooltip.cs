using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Building.Construction;

public class ConstructionModeTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI actionName;

	[SerializeField]
	private TextMeshProUGUI actionDescription;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private GameObject unusableCausePanel;

	[SerializeField]
	private TextMeshProUGUI unusableCauseText;

	protected override bool CanBeDisplayed()
	{
		if (!((Object)(object)TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton != (Object)null))
		{
			return (Object)(object)TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton != (Object)null;
		}
		return true;
	}

	protected override void RefreshContent()
	{
		TheLastStand.Model.Building.Construction.E_UnusableActionCause e_UnusableActionCause = TheLastStand.Model.Building.Construction.E_UnusableActionCause.None;
		if ((Object)(object)TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton != (Object)null)
		{
			ConstructionModeButton hoveredConstructionModeButton = TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton;
			if (!(hoveredConstructionModeButton is RepairModeButton))
			{
				if (hoveredConstructionModeButton is DestroyModeButton)
				{
					RefreshDestroyMode();
				}
			}
			else
			{
				RefreshRepairMode();
			}
			e_UnusableActionCause = TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton.UnusableActionCause;
		}
		else if ((Object)(object)TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton != (Object)null)
		{
			RefreshRepairCategory();
			unusableCausePanel.SetActive(false);
			e_UnusableActionCause = TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton.UnusableActionCause;
		}
		if (e_UnusableActionCause == TheLastStand.Model.Building.Construction.E_UnusableActionCause.None)
		{
			unusableCausePanel.SetActive(false);
			return;
		}
		((TMP_Text)unusableCauseText).text = Localizer.Get("Construction_UnusableActionCause_" + e_UnusableActionCause);
		unusableCausePanel.SetActive(true);
	}

	private void RefreshRepairMode()
	{
		if (TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton is RepairModeButton repairModeButton)
		{
			((TMP_Text)actionName).text = Localizer.Get("RepairModeName_" + repairModeButton.RepairMode);
			((TMP_Text)actionDescription).text = Localizer.Get("RepairModeDescription_" + repairModeButton.RepairMode);
			((Behaviour)costText).enabled = false;
		}
	}

	private void RefreshRepairCategory()
	{
		RepairCategoryButton hoveredRepairCategoryButton = TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton;
		((TMP_Text)actionName).text = Localizer.Get("RepairCategoryName_" + hoveredRepairCategoryButton.Id);
		((TMP_Text)actionDescription).text = Localizer.Get("RepairCategoryDescription_" + hoveredRepairCategoryButton.Id);
		((Behaviour)costText).enabled = true;
		string arg = (hoveredRepairCategoryButton.IsGold ? "Gold" : "Materials");
		((TMP_Text)costText).text = $"<style={arg}><style=Number>{hoveredRepairCategoryButton.Cost}</style></style>";
	}

	private void RefreshDestroyMode()
	{
		if (TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton is DestroyModeButton destroyModeButton)
		{
			string text = "DestroyModeDescription_" + destroyModeButton.DestroyMode;
			string text2 = default(string);
			string text3 = ((InputManager.IsLastControllerJoystick && Localizer.TryGet(text + "_Gamepad", ref text2)) ? text2 : Localizer.Get(text));
			((TMP_Text)actionDescription).text = text3;
			((TMP_Text)actionName).text = Localizer.Get("DestroyModeName_" + destroyModeButton.DestroyMode);
			((Behaviour)costText).enabled = false;
		}
	}
}
