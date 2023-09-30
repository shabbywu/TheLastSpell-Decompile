using TMPro;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip;

public class UnitHUDStatusesAndInjuriesTooltip : TooltipBase
{
	[SerializeField]
	private Image titleBG;

	[SerializeField]
	private Sprite baseBG;

	[SerializeField]
	private Sprite injuriesBG;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI linesText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	public string TitleLocalized { get; set; }

	public string LinesLocalized { get; set; }

	public string DescriptionLocalized { get; set; }

	public bool UseInjuryBox { get; set; }

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
		((TMP_Text)titleText).text = TitleLocalized;
		titleBG.sprite = (UseInjuryBox ? injuriesBG : baseBG);
		((TMP_Text)linesText).text = LinesLocalized;
		((TMP_Text)descriptionText).text = DescriptionLocalized;
	}

	protected override void RefreshLayout(bool showInstantly = false)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)titleBG).rectTransform.sizeDelta = new Vector2(Mathf.Min(base.RectTransform.sizeDelta.x - ((TMP_Text)titleText).rectTransform.anchoredPosition.x, (float)Mathf.RoundToInt(((TMP_Text)titleText).rectTransform.anchoredPosition.x + ((TMP_Text)titleText).rectTransform.sizeDelta.x + 20f)), ((Graphic)titleBG).rectTransform.sizeDelta.y);
		base.RefreshLayout(showInstantly);
	}
}
