using TMPro;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager.Skill;
using UnityEngine;

namespace TheLastStand.View.Tooltip.Tooltip.Compendium;

public class SkillEffectTooltip : CompendiumEntryTooltip
{
	[SerializeField]
	private DataColorDictionary skillEffectColors;

	[SerializeField]
	private DataSpriteDictionary skillEffectIcons;

	[SerializeField]
	protected Sprite titleBGNormalSprite;

	[SerializeField]
	protected Sprite titleBGSurroundingSprite;

	[SerializeField]
	protected Sprite titleBGCasterEffectSprite;

	public string SkillEffectId { get; set; }

	public string ColorSkillEffectName(string skillEffectId, string name)
	{
		if ((Object)(object)skillEffectColors == (Object)null)
		{
			CLoggerManager.Log((object)"Missing reference on skillEffectColors dictionary, returning SkillEffect name without coloring.", (LogType)0, (CLogLevel)2, true, "SkillEffectTooltip", false);
			return name;
		}
		string colorHexCodeById = skillEffectColors.GetColorHexCodeById(skillEffectId);
		if (colorHexCodeById == null)
		{
			return name;
		}
		return "<color=#" + colorHexCodeById + ">" + name + "</color>";
	}

	protected override bool CanBeDisplayed()
	{
		if (SkillEffectId != null)
		{
			return SkillEffectId != string.Empty;
		}
		return false;
	}

	protected override void OnHide()
	{
		base.OnHide();
		SkillEffectId = null;
	}

	protected override void RefreshContent()
	{
		switch (SkillEffectId)
		{
		case "SurroundingEffect":
			titleBG.sprite = titleBGSurroundingSprite;
			break;
		case "CasterEffect":
			titleBG.sprite = titleBGCasterEffectSprite;
			break;
		default:
			titleBG.sprite = titleBGNormalSprite;
			break;
		}
		icon.sprite = skillEffectIcons.GetSpriteById(SkillEffectId);
		((TMP_Text)title).text = ColorSkillEffectName(SkillEffectId, SkillManager.GetSkillEffectName(SkillEffectId));
		((TMP_Text)description).text = SkillManager.GetSkillEffectDescription(SkillEffectId);
	}
}
