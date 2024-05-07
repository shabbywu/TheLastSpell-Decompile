using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Unit.Race;

public class RaceTooltip : TooltipBase
{
	[SerializeField]
	private UnitRaceDisplay unitRaceDisplay;

	[SerializeField]
	private RectTransform tooltipRectTransform;

	[SerializeField]
	private GameObject racePanel;

	public UnitRaceDisplay LinkedUnitRaceDisplay { get; private set; }

	public RectTransform TooltipPanel => tooltipPanel;

	public RectTransform TooltipRectTransform => tooltipRectTransform;

	public void SetContent(UnitRaceDisplay linkedUnitRaceDisplay)
	{
		LinkedUnitRaceDisplay = linkedUnitRaceDisplay;
		RefreshRaceDefinition();
	}

	public void UpdateAnchors(bool displayTowardsRight, bool displayTop = false)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = ((!displayTowardsRight) ? Vector2.one : Vector2.up);
		if (displayTop)
		{
			val.y = 0f;
		}
		TooltipRectTransform.anchorMin = val;
		TooltipRectTransform.anchorMax = val;
		TooltipRectTransform.pivot = val;
	}

	protected override void Awake()
	{
		racePanel.SetActive(true);
		base.Awake();
	}

	protected override bool CanBeDisplayed()
	{
		return unitRaceDisplay.RaceDefinition != null;
	}

	protected override void RefreshContent()
	{
		RefreshRaceDefinition();
		unitRaceDisplay.Refresh();
	}

	private void RefreshRaceDefinition()
	{
		if ((Object)(object)LinkedUnitRaceDisplay != (Object)null && LinkedUnitRaceDisplay.RaceDefinition?.Id != unitRaceDisplay.RaceDefinition?.Id)
		{
			unitRaceDisplay.SetContent(LinkedUnitRaceDisplay.RaceDefinition);
		}
	}
}
