using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.View.Tooltip.Tooltip.Compendium;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip.Compendium;

public class AttackTypeTooltip : CompendiumEntryTooltip
{
	[SerializeField]
	private DataSpriteDictionary damageTypeIcons;

	[SerializeField]
	private DataColorDictionary damageTypeColors;

	public AttackSkillActionDefinition.E_AttackType AttackType { get; set; }

	protected override bool CanBeDisplayed()
	{
		return AttackType != AttackSkillActionDefinition.E_AttackType.None;
	}

	protected override void OnHide()
	{
		base.OnHide();
		AttackType = AttackSkillActionDefinition.E_AttackType.None;
	}

	protected override void RefreshContent()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		icon.sprite = damageTypeIcons.GetSpriteById(AttackType.ToString());
		((TMP_Text)title).text = Localizer.Get(string.Format("{0}{1}", "DamageTypeName_", AttackType));
		((Graphic)title).color = damageTypeColors.GetColorById(AttackType.ToString()).Value;
		((TMP_Text)description).text = Localizer.Get(string.Format("{0}{1}", "DamageTypeDescription_", AttackType));
	}
}
