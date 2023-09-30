using TMPro;
using TPLib.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class GenericTitledTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI description;

	private Color baseColor;

	private string descriptionLocaKey;

	private string descriptionLocalized;

	private Sprite iconSprite;

	private Color titleColor;

	private string titleLocaKey;

	private string titleLocalized;

	public void SetTitleColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		titleColor = color;
	}

	public void SetContent(string titleLocaKey, string descriptionLocaKey, Sprite iconSprite = null)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		titleLocalized = string.Empty;
		this.titleLocaKey = titleLocaKey;
		this.descriptionLocaKey = descriptionLocaKey;
		this.iconSprite = iconSprite;
		titleColor = baseColor;
	}

	public void SetContentLocalized(string title, string description, Sprite iconSprite = null)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		titleLocalized = title;
		descriptionLocalized = description;
		titleLocaKey = string.Empty;
		descriptionLocaKey = string.Empty;
		this.iconSprite = iconSprite;
		titleColor = baseColor;
	}

	protected override void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		baseColor = ((Graphic)title).color;
	}

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		((Component)icon).gameObject.SetActive((Object)(object)iconSprite != (Object)null);
		if ((Object)(object)iconSprite != (Object)null)
		{
			icon.sprite = iconSprite;
			RectTransform obj = ((Graphic)icon).rectTransform;
			Rect rect = iconSprite.rect;
			obj.sizeDelta = ((Rect)(ref rect)).size;
		}
		RefreshLocalizedText();
		((Graphic)title).color = titleColor;
	}

	private void RefreshLocalizedText()
	{
		((TMP_Text)title).text = (string.IsNullOrEmpty(titleLocaKey) ? titleLocalized : Localizer.Get(titleLocaKey));
		((TMP_Text)description).text = (string.IsNullOrEmpty(descriptionLocaKey) ? descriptionLocalized : Localizer.Get(descriptionLocaKey));
	}
}
