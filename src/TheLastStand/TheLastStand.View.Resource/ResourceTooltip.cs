using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Resource;

public class ResourceTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI resourceTitle;

	[SerializeField]
	private Image resourceIcon;

	[SerializeField]
	private DataSpriteDictionary resourceIcons;

	[SerializeField]
	private DataColorDictionary resourceColors;

	[SerializeField]
	private TextMeshProUGUI resourceDescription;

	private string resourceId = string.Empty;

	public void SetContent(string newResourceId)
	{
		resourceId = newResourceId;
	}

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		resourceIcon.sprite = resourceIcons.GetSpriteById(resourceId);
		Color? colorById = resourceColors.GetColorById(resourceId);
		if (colorById.HasValue)
		{
			((Graphic)resourceTitle).color = colorById.Value;
		}
		RefreshLocalizedText();
	}

	private void RefreshLocalizedText()
	{
		((TMP_Text)resourceTitle).text = Localizer.Get("Resources_Name_" + resourceId);
		((TMP_Text)resourceDescription).text = Localizer.Get("Resources_Description_" + resourceId);
	}
}
