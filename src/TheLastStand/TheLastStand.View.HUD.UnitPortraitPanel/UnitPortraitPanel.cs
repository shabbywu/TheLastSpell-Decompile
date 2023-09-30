using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitPortraitPanel;

public class UnitPortraitPanel : UnitPortraitView
{
	[SerializeField]
	private UnitPortraitStatGauge actionPointsGauge;

	[SerializeField]
	private UnitPortraitStatGauge movePointsGauge;

	[SerializeField]
	private UnitPortraitStatGauge manaPointsGauge;

	[SerializeField]
	private RectTransform containerRectTransform;

	[SerializeField]
	private Vector2 containerDisablePosition = new Vector2(0f, 14f);

	[SerializeField]
	private Image box;

	[SerializeField]
	private Sprite defaultBox;

	[SerializeField]
	private Sprite hoveredBox;

	public override void DisplayUnitPortraitBoxHovered(bool value)
	{
		box.sprite = (value ? hoveredBox : defaultBox);
		actionPointsGauge.Hover(value);
		movePointsGauge.Hover(value);
		manaPointsGauge.Hover(value);
		TPSingleton<TileObjectSelectionManager>.Instance.UpdateUnitInfoPanel(value ? PlayableUnit : null);
	}

	public void ToggleSkillTargeting(bool value)
	{
		if (value)
		{
			UnityEvent onShow = skillTargetingMark.OnShow;
			if (onShow != null)
			{
				onShow.Invoke();
			}
		}
		else
		{
			UnityEvent onHide = skillTargetingMark.OnHide;
			if (onHide != null)
			{
				onHide.Invoke();
			}
		}
	}

	public override void RefreshStats()
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		float clampedStatValue = PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ActionPoints);
		float clampedStatValue2 = PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.MovePoints);
		actionPointsGauge.Refresh(clampedStatValue, PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ActionPointsTotal));
		movePointsGauge.Refresh(clampedStatValue2, PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.MovePointsTotal));
		manaPointsGauge.Refresh(PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana), PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal));
		if (clampedStatValue == 0f && clampedStatValue2 == 0f)
		{
			containerRectTransform.anchoredPosition = containerDisablePosition;
			Color white = Color.white;
			Color color = PlayableUnit.PortraitColor._Color;
			Color color2 = white * ((Color)(ref color)).grayscale;
			color2.a = 1f;
			((Graphic)unitPortraitBGImage).color = color2;
		}
		else
		{
			containerRectTransform.anchoredPosition = Vector2.zero;
			((Graphic)unitPortraitBGImage).color = PlayableUnit.PortraitColor._Color;
		}
	}

	private void OnDestroy()
	{
		((UnityEventBase)base.UnitPortraitToggle.OnPointerClickEvent).RemoveAllListeners();
		((UnityEventBase)base.UnitPortraitToggle.OnPointerEnterEvent).RemoveAllListeners();
	}
}
