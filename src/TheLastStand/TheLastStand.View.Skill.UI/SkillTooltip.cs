using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.View.Tooltip.Tooltip.Compendium;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class SkillTooltip : SkillWithoutCompendiumTooltip
{
	[SerializeField]
	[Tooltip("If left to null, it will automatically use the one from SkillManager")]
	private CompendiumPanel compendiumPanel;

	[SerializeField]
	private Transform compendiumLeftBottomAnchor;

	[SerializeField]
	private Transform compendiumRightBottomAnchor;

	public bool CompendiumFollowRight { get; set; }

	protected override void Awake()
	{
		if ((Object)(object)compendiumPanel == (Object)null)
		{
			compendiumPanel = SkillManager.CompendiumPanel;
		}
		base.Awake();
	}

	protected override void OnHide()
	{
		base.OnHide();
		compendiumPanel.Clear();
		compendiumPanel.Hide();
	}

	protected override void RefreshContent()
	{
		base.RefreshContent();
		compendiumPanel.Clear();
		DisplayCompendiumPanel();
	}

	private void DisplayCompendiumPanel()
	{
		if (skillDisplay.Skill.SkillAction is AttackSkillAction attackSkillAction)
		{
			compendiumPanel.AddDamageType(attackSkillAction);
		}
		if (skillDisplay.Skill.SkillAction.SkillActionDefinition.SkillEffectDefinitions != null)
		{
			compendiumPanel.AddSkillEffectIds(skillDisplay.Skill.SkillAction.SkillActionDefinition.SkillEffectDefinitions);
		}
		if (compendiumPanel.CompendiumEntries.Count > 0 && !TPSingleton<SettingsManager>.Instance.Settings.HideCompendium)
		{
			RefreshCompendiumPanelFollowTarget();
			RefreshCompendiumPanelOffset();
			compendiumPanel.UpdateAnchor(CompendiumFollowRight ? CompendiumPanel.AnchorType.LeftBot : CompendiumPanel.AnchorType.RightBot);
			compendiumPanel.Display();
		}
	}

	private void RefreshCompendiumPanelFollowTarget()
	{
		compendiumPanel.FollowElement.ChangeTarget(CompendiumFollowRight ? compendiumRightBottomAnchor : compendiumLeftBottomAnchor);
	}

	private void RefreshCompendiumPanelOffset()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (skillDisplay.SkillAreaOfEffectGrid.Displayed)
		{
			skillDisplay.SkillAreaOfEffectGridPlacedEvent += UpdateSkillEffectTooltipsPanelOffset;
			return;
		}
		Vector3 offset = default(Vector3);
		((Vector3)(ref offset))._002Ector(0f, compendiumPanel.FollowElement.FollowElementDatas.Offset.y, compendiumPanel.FollowElement.FollowElementDatas.Offset.z);
		compendiumPanel.FollowElement.ChangeOffset(offset);
	}

	private void UpdateSkillEffectTooltipsPanelOffset()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		skillDisplay.SkillAreaOfEffectGridPlacedEvent -= UpdateSkillEffectTooltipsPanelOffset;
		Vector3 offset = default(Vector3);
		((Vector3)(ref offset))._002Ector(0f, compendiumPanel.FollowElement.FollowElementDatas.Offset.y, compendiumPanel.FollowElement.FollowElementDatas.Offset.z);
		if ((Object)(object)compendiumPanel.FollowElement.FollowElementDatas.FollowTarget == (Object)(object)compendiumRightBottomAnchor)
		{
			float num = ((Transform)skillDisplay.SkillAreaOfEffectGrid.RectTransform).localPosition.x + ((Transform)skillDisplay.SkillParametersContainer).localPosition.x + skillDisplay.SkillAreaOfEffectGrid.RectTransform.sizeDelta.x - base.RectTransform.sizeDelta.x;
			num = Mathf.Max(0f, num);
			((Vector3)(ref offset))._002Ector(num, compendiumPanel.FollowElement.FollowElementDatas.Offset.y, compendiumPanel.FollowElement.FollowElementDatas.Offset.z);
			compendiumPanel.FollowElement.ChangeOffset(offset);
		}
		else
		{
			compendiumPanel.FollowElement.ChangeOffset(offset);
		}
	}
}
