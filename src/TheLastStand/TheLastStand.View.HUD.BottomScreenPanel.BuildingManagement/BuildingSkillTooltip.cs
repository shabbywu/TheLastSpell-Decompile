using System;
using TMPro;
using TPLib.Localization;
using TheLastStand.Model.Building;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingSkillTooltip : TooltipBase
{
	public static class Consts
	{
		public const string BuildingSkillTooltipNamePrefix = "BuildingSkillTooltipName_";

		public const string BuildingSkillTooltipDescriptionPrefix = "BuildingSkillTooltipDescription_";
	}

	[SerializeField]
	private TextMeshProUGUI buildingSkillIdText;

	[SerializeField]
	private TextMeshProUGUI buildingSkillCostText;

	[SerializeField]
	private TextMeshProUGUI buildingSkillDescriptionText;

	[SerializeField]
	private LayoutElement descriptionSpacingLayoutElement;

	private TheLastStand.Model.Building.Building building;

	private string buildingSkillId = string.Empty;

	public Transform FollowTarget
	{
		set
		{
			base.FollowElement.ChangeTarget(value);
		}
	}

	public void SetContent(string buildingSkillId, TheLastStand.Model.Building.Building building = null)
	{
		this.buildingSkillId = buildingSkillId;
		this.building = building;
		RefreshLocalizedTexts();
		if (this.building != null)
		{
			string text = (this.building.ConstructionModule.CostsMaterials ? "Materials" : "Gold");
			if (string.IsNullOrEmpty(this.building.BuildingView.GetBuildingSkillCostString(this.buildingSkillId)))
			{
				((Behaviour)buildingSkillCostText).enabled = false;
				return;
			}
			((Behaviour)buildingSkillCostText).enabled = true;
			((TMP_Text)buildingSkillCostText).text = "<style=\"" + text + "\"><style=\"Number\">" + this.building.BuildingView.GetBuildingSkillCostString(this.buildingSkillId) + "</style></style>";
		}
	}

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.Displayed)
		{
			RefreshLocalizedTexts();
		}
	}

	private void RefreshLocalizedTexts()
	{
		if (buildingSkillId != string.Empty)
		{
			((TMP_Text)buildingSkillIdText).text = Localizer.Get("BuildingSkillTooltipName_" + buildingSkillId);
			string text = default(string);
			if (Localizer.TryGet("BuildingSkillTooltipDescription_" + buildingSkillId, ref text) && !string.IsNullOrEmpty(text))
			{
				((TMP_Text)buildingSkillDescriptionText).text = text;
				descriptionSpacingLayoutElement.ignoreLayout = false;
			}
			else
			{
				((TMP_Text)buildingSkillDescriptionText).text = string.Empty;
				descriptionSpacingLayoutElement.ignoreLayout = true;
			}
		}
	}
}
