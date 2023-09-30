using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Definition.Unit;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip.Tooltip.Compendium;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Stat;

public class StatTooltip : TooltipBase
{
	[SerializeField]
	private UnitStatDisplay tooltipUnitStatDisplay;

	[SerializeField]
	private Image regenIcon;

	[SerializeField]
	private UnitStatBreakdownDisplay regenTooltipStatDisplay;

	[SerializeField]
	private GameObject regenTooltipUnitStatDisplayGameObject;

	[SerializeField]
	private GameObject regenSeparator;

	[SerializeField]
	private UnitStatBreakdownDisplay additionalTooltipStatDisplay;

	[SerializeField]
	private GameObject additionalUnitStatDisplayGameObject;

	[SerializeField]
	private GameObject additionalStatSeparator;

	[SerializeField]
	private CompendiumPanel compendiumPanel;

	private AdditionalUnitStatDisplay additionalUnitStatDisplay;

	private UnitStatDefinition regenStatDefinition;

	public UnitStatDisplay TooltipUnitStatDisplay => tooltipUnitStatDisplay;

	public void SetContent(UnitStatDisplay statDisplay)
	{
		tooltipUnitStatDisplay.StatDefinition = statDisplay.StatDefinition;
		tooltipUnitStatDisplay.SecondaryStatDefinition = statDisplay.SecondaryStatDefinition;
		if (statDisplay is UnitStatWithRegenDisplay unitStatWithRegenDisplay && tooltipUnitStatDisplay.TargetUnit != null)
		{
			regenStatDefinition = unitStatWithRegenDisplay.RegenStatDefinition;
		}
		else
		{
			regenStatDefinition = null;
		}
		if (statDisplay is AdditionalUnitStatDisplay additionalUnitStatDisplay && tooltipUnitStatDisplay.TargetUnit != null)
		{
			this.additionalUnitStatDisplay = additionalUnitStatDisplay;
		}
		else
		{
			this.additionalUnitStatDisplay = null;
		}
		RefreshRegen();
	}

	public void UpdateAnchor(bool displayOnRight)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = (displayOnRight ? Vector2.up : Vector2.one);
		rectTransform.pivot = val;
		rectTransform.anchorMin = val;
		rectTransform.anchorMax = val;
		if (displayOnRight)
		{
			((Component)compendiumPanel).transform.SetAsLastSibling();
		}
		else
		{
			((Component)compendiumPanel).transform.SetAsFirstSibling();
		}
		compendiumPanel.UpdateAnchor((!displayOnRight) ? CompendiumPanel.AnchorType.RightTop : CompendiumPanel.AnchorType.LeftTop);
	}

	protected override bool CanBeDisplayed()
	{
		return tooltipUnitStatDisplay.StatDefinition != null;
	}

	protected override void OnHide()
	{
		base.OnHide();
		compendiumPanel.Clear();
		compendiumPanel.Hide();
	}

	protected override void RefreshContent()
	{
		tooltipUnitStatDisplay.Refresh();
		RefreshRegen();
		DisplayCompendiumPanel();
	}

	private void DisplayCompendiumPanel()
	{
		compendiumPanel.Clear();
		if (tooltipUnitStatDisplay.StatDefinition == null || tooltipUnitStatDisplay.StatDefinition.CompendiumEntries.Count <= 0)
		{
			return;
		}
		foreach (CompendiumEntryDefinition compendiumEntry in tooltipUnitStatDisplay.StatDefinition.CompendiumEntries)
		{
			compendiumPanel.AddCompendiumEntry(compendiumEntry.Id, compendiumEntry.DisplayLinkedEntries);
		}
		compendiumPanel.Display();
	}

	private void RefreshRegen()
	{
		if (regenStatDefinition != null && tooltipUnitStatDisplay.TargetUnit != null)
		{
			regenTooltipUnitStatDisplayGameObject.SetActive(true);
			regenSeparator.SetActive(true);
			string valueString = regenTooltipStatDisplay.FormatStatValue(tooltipUnitStatDisplay.TargetUnit.GetClampedStatValue(regenStatDefinition.Id), statIsPercentage: false);
			regenTooltipStatDisplay.Refresh(valueString, "UnitStat_ShortName_" + regenStatDefinition.Id);
			regenIcon.sprite = UnitStatDisplay.GetStatIconSprite(regenStatDefinition.Id, UnitStatDisplay.E_IconSize.Small);
		}
		else
		{
			regenStatDefinition = null;
			regenTooltipUnitStatDisplayGameObject.SetActive(false);
			regenSeparator.SetActive(false);
		}
		if ((Object)(object)additionalUnitStatDisplay != (Object)null && tooltipUnitStatDisplay.TargetUnit != null)
		{
			additionalUnitStatDisplayGameObject.SetActive(true);
			additionalStatSeparator.SetActive(true);
			additionalTooltipStatDisplay.RefreshAdditionalStat(additionalUnitStatDisplay);
		}
		else
		{
			additionalUnitStatDisplay = null;
			additionalUnitStatDisplayGameObject.SetActive(false);
			additionalStatSeparator.SetActive(false);
		}
	}
}
