using TMPro;
using TheLastStand.Framework;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class EliteAffixTooltip : TooltipBase
{
	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image titleBG;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private GameObject descriptionSeparator;

	[SerializeField]
	private TextMeshProUGUI additionalDescriptionText;

	public EnemyAffix Affix { get; set; }

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
		((TMP_Text)titleText).text = Affix.EnemyAffixDefinition.GetTitle();
		((TMP_Text)descriptionText).text = Affix.EnemyAffixDefinition.GetDescription(Affix.Interpreter);
		iconImage.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Units/EnemiesAffixes/Icons/EnemyAffix_Icon_{Affix.EnemyAffixDefinition.EnemyAffixEffectDefinition.EnemyAffixEffect.ToString()}", failSilently: false);
		string additionalDescription = Affix.EnemyAffixDefinition.GetAdditionalDescription(Affix.Interpreter);
		bool flag = additionalDescription != null;
		descriptionSeparator.SetActive(flag);
		((Component)additionalDescriptionText).gameObject.SetActive(flag);
		if (flag)
		{
			((TMP_Text)additionalDescriptionText).text = additionalDescription;
		}
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
