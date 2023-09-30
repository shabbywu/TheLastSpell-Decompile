using System.Text;
using TMPro;
using TPLib.Localization;
using TheLastStand.Definition.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class SkillWithoutCompendiumTooltip : TooltipBase
{
	[SerializeField]
	protected RectTransform skillDetailsRect;

	[SerializeField]
	private RectTransform bgRect;

	[SerializeField]
	protected SkillDisplay skillDisplay;

	[SerializeField]
	private RectTransform skillEffectsRect;

	[SerializeField]
	private GameObject invalidPhasePanel;

	[SerializeField]
	private GameObject invalidInjuryPanel;

	[SerializeField]
	private TextMeshProUGUI invalidPhaseText;

	[SerializeField]
	private TextMeshProUGUI invalidInjuryText;

	public RectTransform TooltipPanel => tooltipPanel;

	public bool DisplayInvalidityPanel { get; set; }

	public void RefreshOnTileChanged()
	{
		if (base.Displayed)
		{
			skillDisplay.RefreshEffects();
		}
	}

	public void SetContent(TheLastStand.Model.Skill.Skill skill, ISkillCaster skillOwner = null)
	{
		skillDisplay.Skill = skill;
		skillDisplay.SkillOwner = skillOwner;
	}

	protected override bool CanBeDisplayed()
	{
		return skillDisplay.Skill != null;
	}

	protected override void OnHide()
	{
		base.OnHide();
		DisplayInvalidityPanel = false;
	}

	protected override void RefreshContent()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		skillDisplay.Refresh();
		float num = 0f - ((Transform)skillDetailsRect).localPosition.y + skillDetailsRect.sizeDelta.y + skillEffectsRect.sizeDelta.y;
		tooltipPanel.sizeDelta = new Vector2(tooltipPanel.sizeDelta.x, num);
		bgRect.sizeDelta = new Vector2(bgRect.sizeDelta.x, num + ((Transform)bgRect).localPosition.y);
		((Transform)skillEffectsRect).localPosition = Vector2.op_Implicit(new Vector2(((Transform)skillEffectsRect).localPosition.x, ((Transform)skillDetailsRect).localPosition.y - skillDetailsRect.sizeDelta.y));
		RefreshInvalidityPanel();
	}

	private void RefreshInvalidityPanel()
	{
		if (!DisplayInvalidityPanel)
		{
			invalidPhasePanel.SetActive(false);
			invalidInjuryPanel.SetActive(false);
			return;
		}
		if (skillDisplay.Skill.SkillController.CheckPhaseAllowed() || skillDisplay.SkillOwner is EnemyUnit)
		{
			invalidPhasePanel.SetActive(false);
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(Localizer.Get("SkillTooltip_InvalidPhase_Title"));
			if (!skillDisplay.Skill.SkillDefinition.AllowDuringPhase.HasFlag(SkillDefinition.E_Phase.Production))
			{
				stringBuilder.AppendLine(Localizer.Get(string.Format("{0}{1}", "SkillTooltip_InvalidPhase_", Game.E_DayTurn.Production)));
			}
			if (!skillDisplay.Skill.SkillDefinition.AllowDuringPhase.HasFlag(SkillDefinition.E_Phase.Deployment))
			{
				stringBuilder.AppendLine(Localizer.Get(string.Format("{0}{1}", "SkillTooltip_InvalidPhase_", Game.E_DayTurn.Deployment)));
			}
			if (!skillDisplay.Skill.SkillDefinition.AllowDuringPhase.HasFlag(SkillDefinition.E_Phase.Night))
			{
				stringBuilder.AppendLine(Localizer.Get(string.Format("{0}{1}", "SkillTooltip_InvalidPhase_", Game.E_Cycle.Night)));
			}
			((TMP_Text)invalidPhaseText).text = stringBuilder.ToString();
			invalidPhasePanel.SetActive(true);
		}
		bool flag = skillDisplay.SkillOwner.PreventedSkillsIds.Contains(skillDisplay.Skill.SkillDefinition.Id);
		if (!flag)
		{
			invalidInjuryPanel.SetActive(false);
			return;
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine(Localizer.Get("SkillTooltip_InvalidInjury_Title"));
		if (flag && skillDisplay.SkillOwner is TheLastStand.Model.Unit.Unit unit)
		{
			int num = 0;
			for (int i = 0; i < unit.UnitTemplateDefinition.InjuryDefinitions.Count; i++)
			{
				num++;
				if (unit.UnitTemplateDefinition.InjuryDefinitions[i].PreventedSkillsIds.Contains(skillDisplay.Skill.SkillDefinition.Id))
				{
					break;
				}
			}
			stringBuilder2.AppendLine(Localizer.Get(string.Format("{0}{1}", "SkillTooltip_Injuried_", num)));
			Debug.Log((object)num);
		}
		((TMP_Text)invalidInjuryText).text = stringBuilder2.ToString();
		invalidInjuryPanel.SetActive(true);
	}
}
