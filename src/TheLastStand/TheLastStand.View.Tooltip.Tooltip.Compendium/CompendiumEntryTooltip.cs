using TMPro;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip.Tooltip.Compendium;

public abstract class CompendiumEntryTooltip : TooltipBase
{
	[SerializeField]
	protected RectTransform layoutRectTransform;

	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected TextMeshProUGUI title;

	[SerializeField]
	protected Image titleBG;

	[SerializeField]
	protected TextMeshProUGUI description;

	public RectTransform TooltipPanel => tooltipPanel;

	protected override void RefreshLayout(bool showInstantly = false)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)titleBG).rectTransform.sizeDelta = new Vector2(Mathf.Min(rectTransform.sizeDelta.x - ((TMP_Text)title).rectTransform.anchoredPosition.x, (float)Mathf.RoundToInt(((TMP_Text)title).rectTransform.anchoredPosition.x + ((TMP_Text)title).rectTransform.sizeDelta.x + 20f)), ((Graphic)titleBG).rectTransform.sizeDelta.y);
		tooltipPanel.sizeDelta = new Vector2(tooltipPanel.sizeDelta.x, layoutRectTransform.sizeDelta.y);
		base.RefreshLayout(showInstantly);
	}
}
