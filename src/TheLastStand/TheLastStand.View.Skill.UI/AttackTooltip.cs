using System;
using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class AttackTooltip : SkillActionTooltip
{
	[SerializeField]
	private RectTransform skillDetailsRectTransform;

	[SerializeField]
	private RectTransform skillDetailsParent;

	[SerializeField]
	private RectTransform skillEffectsRectTransform;

	[SerializeField]
	private RectTransform skillEffectsParent;

	[SerializeField]
	private SkillParameterDisplay attackValueDisplay;

	[SerializeField]
	private SkillParameterDisplay resistanceDisplay;

	[SerializeField]
	private SkillParameterDisplay blockDisplay;

	[SerializeField]
	private SkillParameterDisplay isolatedDisplay;

	[SerializeField]
	private SkillParameterDisplay momentumDisplay;

	[SerializeField]
	private SkillParameterDisplay opportunityDisplay;

	[SerializeField]
	private SkillParameterDisplay perkDamageDisplay;

	[SerializeField]
	private SkillParameterDisplay finalValueDisplay;

	[SerializeField]
	private FormatSkillParameterDisplay criticalValueDisplay;

	[SerializeField]
	private DataColorDictionary damageTypeColor;

	[SerializeField]
	private GameObject noBlockFeedback;

	[SerializeField]
	private GameObject dodgeResistancePanel;

	[SerializeField]
	private UnitStatDisplay resistanceStatDisplay;

	[SerializeField]
	private UnitStatDisplay dodgeStatDisplay;

	public void RefreshAttackData()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_072a: Unknown result type (might be due to invalid IL or missing references)
		//IL_075d: Unknown result type (might be due to invalid IL or missing references)
		//IL_076d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0777: Unknown result type (might be due to invalid IL or missing references)
		//IL_078d: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_07be: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07de: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0803: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_082d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0838: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		RefreshStats();
		skillEffectsRectTransform.sizeDelta = new Vector2(skillEffectsRectTransform.sizeDelta.x, skillEffectsParent.sizeDelta.y);
		AttackSkillAction attackSkillAction = skillDisplay.Skill.SkillAction as AttackSkillAction;
		skillDisplay.Skill.SkillAction.SkillActionController.EnsurePerkData(base.TargetTile, base.TargetTile?.Damageable);
		DamageRangeData damageRangeData = attackSkillAction.AttackSkillActionController.ComputeFinalDamageRange(base.TargetTile, skillDisplay.SkillOwner);
		int num = 0;
		float num2 = 0f;
		TheLastStand.Model.Unit.Unit unit = skillDisplay.SkillOwner as TheLastStand.Model.Unit.Unit;
		PlayableUnit playableUnit = skillDisplay.SkillOwner as PlayableUnit;
		if (base.TargetTile != null && base.TargetUnit != null && base.TargetUnit != skillDisplay.SkillOwner)
		{
			num = Mathf.CeilToInt(base.TargetUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Block));
			if (unit != null)
			{
				float clampedStatValue = unit.GetClampedStatValue(UnitStatDefinition.E_Stat.ResistanceReduction);
				float num3 = unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).ClampStatValue(unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).FinalClamped + ((attackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Magical) ? UnitDatabase.MagicDamagePercentageResistanceReduction : 0f));
				float num4 = 0f;
				float num5 = 0f;
				if (playableUnit != null)
				{
					num4 = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.FlatResistanceReduction, skillDisplay.Skill.SkillAction.PerkDataContainer);
					num5 = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.PercentageResistanceReduction, skillDisplay.Skill.SkillAction.PerkDataContainer);
				}
				num2 = base.TargetUnit.GetReducedResistance(clampedStatValue + num4, num3 + num5);
			}
		}
		float num6 = 0f;
		if (unit != null)
		{
			num6 += unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Critical);
			if (playableUnit != null)
			{
				num6 += playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.Critical, attackSkillAction.PerkDataContainer);
			}
		}
		bool flag = num6 > 0f;
		if (flag)
		{
			float num7 = playableUnit?.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.CriticalPower, skillDisplay.Skill.SkillAction.PerkDataContainer) ?? 0f;
			Vector2Int finalDamageRange = damageRangeData.FinalDamageRange;
			float num8 = ((Vector2Int)(ref finalDamageRange)).x;
			finalDamageRange = damageRangeData.FinalDamageRange;
			Vector2Int val = VectorExtensions.RoundToInt(new Vector2(num8, (float)((Vector2Int)(ref finalDamageRange)).y) * (unit.GetClampedStatValue(UnitStatDefinition.E_Stat.CriticalPower) + num7) / 100f);
			criticalValueDisplay.Refresh("AttackTooltip_Critical", VectorExtensions.GetSimplifiedRange(val, "-", (Func<int, int>)null), "", num6);
		}
		criticalValueDisplay.Display(flag);
		if ((float)num > 0f || ((Vector2Int)(ref damageRangeData.ResistanceReductionRange)).magnitude != 0f || ((Vector2Int)(ref damageRangeData.IsolatedDamageRange)).magnitude != 0f || ((Vector2Int)(ref damageRangeData.MomentumDamageRange)).magnitude != 0f || ((Vector2Int)(ref damageRangeData.OpportunismDamageRange)).magnitude != 0f || ((Vector2Int)(ref damageRangeData.PerksDamageRange)).magnitude != 0f)
		{
			attackValueDisplay.Refresh("AttackTooltip_AttackValue", VectorExtensions.GetSimplifiedRange(damageRangeData.BaseDamageRange, "-", (Func<int, int>)null));
			attackValueDisplay.Display(show: true);
			bool flag2 = ((Vector2Int)(ref damageRangeData.IsolatedDamageRange)).magnitude != 0f;
			if (flag2)
			{
				Vector2Int isolatedDamageRange = damageRangeData.IsolatedDamageRange;
				string overrideSign = (((float)((Vector2Int)(ref isolatedDamageRange)).x < 0f && (float)((Vector2Int)(ref isolatedDamageRange)).y < 0f) ? "-" : "+");
				isolatedDisplay.Refresh("AttackTooltip_Isolated", VectorExtensions.GetSimplifiedRange(isolatedDamageRange, "-", (Func<int, int>)Mathf.Abs) ?? "", overrideSign);
			}
			bool flag3 = ((Vector2Int)(ref damageRangeData.OpportunismDamageRange)).magnitude != 0f;
			if (flag3)
			{
				Vector2Int opportunismDamageRange = damageRangeData.OpportunismDamageRange;
				string overrideSign2 = (((float)((Vector2Int)(ref opportunismDamageRange)).x < 0f && (float)((Vector2Int)(ref opportunismDamageRange)).y < 0f) ? "-" : "+");
				opportunityDisplay.Refresh("AttackTooltip_Opportunistic", VectorExtensions.GetSimplifiedRange(opportunismDamageRange, "-", (Func<int, int>)Mathf.Abs) ?? "", overrideSign2);
			}
			bool flag4 = ((Vector2Int)(ref damageRangeData.MomentumDamageRange)).magnitude > 0f;
			if (flag4)
			{
				Vector2Int momentumDamageRange = damageRangeData.MomentumDamageRange;
				momentumDisplay.Refresh("AttackTooltip_Momentum", VectorExtensions.GetSimplifiedRange(momentumDamageRange, "-", (Func<int, int>)null) ?? "");
			}
			bool flag5 = ((Vector2Int)(ref damageRangeData.PerksDamageRange)).magnitude != 0f;
			if (flag5)
			{
				Vector2Int perksDamageRange = damageRangeData.PerksDamageRange;
				string overrideSign3 = "+";
				if ((float)((Vector2Int)(ref perksDamageRange)).x < 0f && (float)((Vector2Int)(ref perksDamageRange)).y < 0f)
				{
					overrideSign3 = "-";
					((Vector2Int)(ref perksDamageRange))._002Ector(Mathf.Abs(((Vector2Int)(ref perksDamageRange)).x), Mathf.Abs(((Vector2Int)(ref perksDamageRange)).y));
				}
				perkDamageDisplay.Refresh("AttackTooltip_Perks", VectorExtensions.GetSimplifiedRange(perksDamageRange, "-", (Func<int, int>)null) ?? "", overrideSign3);
			}
			bool flag6 = ((Vector2Int)(ref damageRangeData.ResistanceReductionRange)).magnitude != 0f;
			if (flag6)
			{
				Vector2Int resistanceReductionRange = damageRangeData.ResistanceReductionRange;
				resistanceDisplay.Refresh("AttackTooltip_Resistance", VectorExtensions.GetSimplifiedRange(resistanceReductionRange, "-", (Func<int, int>)Mathf.Abs) ?? "", (num2 < 0f) ? "+" : "-");
			}
			bool flag7 = num > 0;
			if (flag7)
			{
				if (!attackSkillAction.HasEffect("NoBlock"))
				{
					noBlockFeedback.SetActive(false);
					blockDisplay.Refresh("AttackTooltip_Block", num.ToString());
				}
				else
				{
					noBlockFeedback.SetActive(true);
					blockDisplay.Refresh("AttackTooltip_Block", "<style=\"Outline\"><style=\"Bad\">0</style></style>");
				}
			}
			blockDisplay.Display(flag7);
			isolatedDisplay.Display(flag2);
			momentumDisplay.Display(flag4);
			opportunityDisplay.Display(flag3);
			perkDamageDisplay.Display(flag5);
			resistanceDisplay.Display(flag6);
		}
		else
		{
			attackValueDisplay.Display(show: false);
			blockDisplay.Display(show: false);
			isolatedDisplay.Display(show: false);
			momentumDisplay.Display(show: false);
			opportunityDisplay.Display(show: false);
			perkDamageDisplay.Display(show: false);
			resistanceDisplay.Display(show: false);
		}
		DataColorDictionary obj = damageTypeColor;
		Color? valueColor = ((obj != null) ? new Color?(obj.GetColorById(attackSkillAction.AttackType.ToString()).Value) : null);
		finalValueDisplay.Refresh("AttackTooltip_FinalDamage", VectorExtensions.GetSimplifiedRange(damageRangeData.FinalDamageRange, "-", (Func<int, int>)null), valueColor);
		LayoutRebuilder.ForceRebuildLayoutImmediate(skillDetailsParent);
		skillDetailsRectTransform.sizeDelta = new Vector2(skillDetailsRectTransform.sizeDelta.x, skillDetailsParent.sizeDelta.y);
		((Transform)skillEffectsRectTransform).localPosition = new Vector3(((Transform)skillEffectsRectTransform).localPosition.x, ((Transform)skillDetailsRectTransform).localPosition.y - skillDetailsRectTransform.sizeDelta.y, ((Transform)skillEffectsRectTransform).localPosition.z);
		tooltipPanel.sizeDelta = new Vector2(tooltipPanel.sizeDelta.x, Mathf.Abs(((Transform)skillDetailsRectTransform).localPosition.y) + skillDetailsRectTransform.sizeDelta.y + (((Component)skillEffectsRectTransform).gameObject.activeInHierarchy ? skillEffectsRectTransform.sizeDelta.y : 0f));
	}

	protected override void RefreshContent()
	{
		base.RefreshContent();
		RefreshAttackData();
	}

	private void RefreshStats()
	{
		dodgeResistancePanel.SetActive(base.TargetUnit != null);
		if (base.TargetUnit != null)
		{
			AttackSkillAction attackSkillAction = skillDisplay.Skill.SkillAction as AttackSkillAction;
			dodgeStatDisplay.TargetUnit = base.TargetUnit;
			dodgeStatDisplay.IsDisabled = attackSkillAction.HasEffect("NoDodge");
			dodgeStatDisplay.Refresh();
			resistanceStatDisplay.TargetUnit = base.TargetUnit;
			resistanceStatDisplay.Refresh();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		dodgeStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Dodge];
		resistanceStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Resistance];
	}
}
