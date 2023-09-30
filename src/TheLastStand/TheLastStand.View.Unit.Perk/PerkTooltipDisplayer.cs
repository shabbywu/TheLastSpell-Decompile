using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Unit.Perk;

public class PerkTooltipDisplayer : TooltipDisplayer
{
	[SerializeField]
	private UnitPerkDisplay unitPerkDisplay;

	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	[SerializeField]
	private RectTransform perkRectTransform;

	private float xOffset;

	public override void DisplayTooltip()
	{
		DisplayTooltip(display: true);
	}

	public override void HideTooltip()
	{
		DisplayTooltip(display: false);
	}

	public void DisplayTooltip(bool display)
	{
		if (!display || unitPerkDisplay.Perk != null || unitPerkDisplay.PerkDefinition != null)
		{
			PerkTooltip perkTooltip = PlayableUnitManager.PerkTooltip;
			if (display)
			{
				perkTooltip.SetContent(unitPerkDisplay.Perk);
				perkTooltip.Display();
				PlaceTooltip();
			}
			else
			{
				perkTooltip.Hide();
			}
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		UnitPerkTreeView.HoveredPerkTooltipDisplayer = this;
		if (unitPerkDisplay.Perk != null)
		{
			DisplayTooltip(display: true);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if ((Object)(object)UnitPerkTreeView.HoveredPerkTooltipDisplayer == (Object)(object)this)
		{
			UnitPerkTreeView.HoveredPerkTooltipDisplayer = null;
		}
		if (unitPerkDisplay.Perk != null)
		{
			DisplayTooltip(display: false);
		}
	}

	private void Awake()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)unitPerkDisplay == (Object)null)
		{
			unitPerkDisplay = ((Component)this).GetComponent<UnitPerkDisplay>();
		}
		xOffset = followDatas.Offset.x;
	}

	private void PlaceTooltip()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		PerkTooltip perkTooltip = PlayableUnitManager.PerkTooltip;
		Rect rect = perkTooltip.TooltipPanel.rect;
		float num = ((Rect)(ref rect)).size.x;
		if (perkTooltip.CompendiumPanel.CompendiumEntries.Count > 0 && !TPSingleton<SettingsManager>.Instance.Settings.HideCompendium)
		{
			float num2 = num;
			rect = perkTooltip.CompendiumPanel.RectTransform.rect;
			num = num2 + ((Rect)(ref rect)).size.x;
		}
		num *= TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale;
		bool flag = ACameraView.MainCam.ScreenToViewportPoint(new Vector3(((Transform)perkRectTransform).position.x + num, 0f, 0f)).x <= 1f;
		followDatas.Offset = (flag ? new Vector3(xOffset, followDatas.Offset.y, followDatas.Offset.z) : new Vector3(0f - xOffset, followDatas.Offset.y, followDatas.Offset.z));
		perkTooltip.UpdateAnchors(flag);
		perkTooltip.FollowElement.ChangeFollowDatas(followDatas);
	}
}
