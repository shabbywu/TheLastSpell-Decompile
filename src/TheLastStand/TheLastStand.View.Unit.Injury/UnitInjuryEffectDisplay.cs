using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class UnitInjuryEffectDisplay : MonoBehaviour
{
	[SerializeField]
	private Image injuryIcon;

	[SerializeField]
	private DataSpriteTable injuryIcons;

	[SerializeField]
	private TextMeshProUGUI titleText;

	private UnitStatDefinition.E_Stat stat;

	private string preventedSkill;

	private float modifier;

	public virtual void Init(UnitStatDefinition.E_Stat stat, float modifier, int injuryStage)
	{
		this.stat = stat;
		this.modifier = modifier;
		injuryIcon.sprite = injuryIcons.GetSpriteAt(injuryStage - 1);
		RefreshTitle();
	}

	public virtual void Init(string preventedSkill, int injuryStage)
	{
		this.preventedSkill = preventedSkill;
		injuryIcon.sprite = injuryIcons.GetSpriteAt(injuryStage - 1);
		RefreshTitle();
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshTitle();
		}
	}

	private void RefreshTitle()
	{
		if (string.IsNullOrEmpty(preventedSkill))
		{
			((TMP_Text)titleText).text = string.Format("{0}{1}{2} <sprite name={3}>{4}", (modifier >= 0f) ? "+" : string.Empty, modifier, stat.ShownAsPercentage() ? "%" : string.Empty, stat, UnitDatabase.UnitStatDefinitions[stat].Name);
		}
		else
		{
			((TMP_Text)titleText).text = string.Format(Localizer.Get("Injury_PreventSkill_UnitTooltip"), "<style=Skill>" + Localizer.Get("SkillName_" + preventedSkill) + "</style>");
		}
	}
}
