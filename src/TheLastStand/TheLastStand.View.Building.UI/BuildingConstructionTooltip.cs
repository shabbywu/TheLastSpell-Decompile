using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Building;
using TheLastStand.Manager.Building;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class BuildingConstructionTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI buildingName;

	[SerializeField]
	private TextMeshProUGUI buildingDescription;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private TextMeshProUGUI buildLimitText;

	[SerializeField]
	private TextMeshProUGUI maxHealthText;

	[SerializeField]
	private TextMeshProUGUI chargesText;

	[SerializeField]
	private GameObject chargesIcon;

	private bool useDefaultValues;

	private BuildingDefinition buildingDefinition;

	private Color buildLimitTextDefaultColor;

	public void Init(BuildingDefinition newBuildingDefinition, bool newUseDefaultValues = false)
	{
		buildingDefinition = newBuildingDefinition;
		useDefaultValues = newUseDefaultValues;
	}

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		buildLimitTextDefaultColor = ((Graphic)buildLimitText).color;
	}

	protected override bool CanBeDisplayed()
	{
		return buildingDefinition != null;
	}

	protected override void RefreshContent()
	{
		RefreshText();
		RefreshLimitDisplay();
	}

	private void RefreshText()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)buildingName).text = buildingDefinition.Name;
		((TMP_Text)buildingDescription).text = buildingDefinition.Description;
		bool num = buildingDefinition.ConstructionModuleDefinition.NativeGoldCost > 0;
		int num2 = BuildingManager.ComputeBuildingCost(buildingDefinition.ConstructionModuleDefinition, useDefaultValues);
		((TMP_Text)buildLimitText).text = buildingDefinition.ConstructionModuleDefinition.GetLocalizedBuildLimit(useDefaultValues);
		((Graphic)buildLimitText).color = ((!TPSingleton<ConstructionManager>.Exist() || ConstructionManager.IsUnderBuildLimit(buildingDefinition.ConstructionModuleDefinition)) ? buildLimitTextDefaultColor : GameView.NegativeColor);
		string arg = (num ? "Gold" : "Materials");
		((TMP_Text)costText).text = $"<style=\"{arg}\"><style=\"Number\">{num2}</style></style>";
		if (buildingDefinition.DamageableModuleDefinition == null || BuildingManager.ComputeBuildingTotalHealth(buildingDefinition.DamageableModuleDefinition, useDefaultValues) == 0f)
		{
			((Component)maxHealthText).gameObject.SetActive(false);
			if (buildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Trap) && buildingDefinition.BattleModuleDefinition != null)
			{
				chargesIcon.SetActive(true);
				((Component)chargesText).gameObject.SetActive(true);
				((TMP_Text)chargesText).text = $"<style=Number><style=RemainingCharges>{buildingDefinition.BattleModuleDefinition.MaximumTrapCharges}</style></style>";
			}
			else
			{
				chargesIcon.SetActive(false);
				((Component)chargesText).gameObject.SetActive(false);
			}
		}
		else
		{
			((Component)maxHealthText).gameObject.SetActive(true);
			chargesIcon.SetActive(false);
			((Component)chargesText).gameObject.SetActive(false);
			((TMP_Text)maxHealthText).text = $"<sprite name=\"Health\"><style=\"Number\">{BuildingManager.ComputeBuildingTotalHealth(buildingDefinition.DamageableModuleDefinition, useDefaultValues)}</style>";
		}
	}

	private void RefreshLimitDisplay()
	{
		((Component)buildLimitText).gameObject.SetActive(!buildingDefinition.ConstructionModuleDefinition.IsUnlimited(useDefaultValues));
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
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshText();
		}
	}
}
