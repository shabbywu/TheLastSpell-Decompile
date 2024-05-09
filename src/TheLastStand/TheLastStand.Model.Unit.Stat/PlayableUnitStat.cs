using System.Collections.Generic;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using UnityEngine;

namespace TheLastStand.Model.Unit.Stat;

public class PlayableUnitStat : UnitStat
{
	public static readonly UnitStatDefinition.E_Stat[] damageTypesByPriority = new UnitStatDefinition.E_Stat[3]
	{
		UnitStatDefinition.E_Stat.PhysicalDamage,
		UnitStatDefinition.E_Stat.MagicalDamage,
		UnitStatDefinition.E_Stat.RangedDamage
	};

	public static bool AllowPerkRefreshInjuries = true;

	private float perks;

	public PlayableUnitStatsController PlayableUnitStatsController => base.UnitStatsController as PlayableUnitStatsController;

	public float Equipment { get; set; }

	public float Perks
	{
		get
		{
			float num = ComputePerksBonus();
			bool flag = IsLocked();
			bool flag2 = false;
			if (!TPHelpers.IsApproxEqual(num, perks, 5E-05f) && base.ParentStat != null)
			{
				flag = true;
				if (base.StatId == UnitStatDefinition.E_Stat.Health)
				{
					flag2 = AllowPerkRefreshInjuries;
					if (Base + num <= 0f && Base + perks > 0f)
					{
						PlayableUnitStatsController.SetBaseStat(base.StatId, 0f - num + 1f);
					}
				}
				else if (Base + num < 0f)
				{
					PlayableUnitStatsController.SetBaseStat(base.StatId, 0f - num);
				}
			}
			perks = num;
			if (flag && (Object)(object)base.UnitStatsController.UnitStats.Unit.UnitView != (Object)null && (Object)(object)base.UnitStatsController.UnitStats.Unit.UnitView.UnitHUD != (Object)null)
			{
				base.UnitStatsController.UnitStats.Unit.UnitView.UnitHUD.RefreshStat(base.StatId);
			}
			if (flag2)
			{
				base.UnitStatsController.UnitStats.Unit.UnitController.UpdateInjuryStage();
			}
			return perks;
		}
	}

	public float Race { get; set; }

	public float Traits { get; set; }

	public List<StatModifierEffect> PerkStatModifierEffects { get; set; } = new List<StatModifierEffect>();


	public List<DynamicStatsModifierEffect> PerkDynamicStatsModifierEffects { get; set; } = new List<DynamicStatsModifierEffect>();


	public List<GaugeModule> PerkGaugeModules { get; set; } = new List<GaugeModule>();


	public int PerkLocksBuffer { get; set; }

	public override float Final
	{
		get
		{
			if (ShouldNullifyStat())
			{
				return 0f;
			}
			return base.Final + Equipment + Perks + Traits + Race;
		}
	}

	public override float FinalClamped
	{
		get
		{
			if (IsLocked())
			{
				return 0f;
			}
			return base.FinalClamped;
		}
	}

	public float FinalUnlockedClamped => base.FinalClamped;

	public PlayableUnitStat(UnitStatsController statsController, UnitStatDefinition.E_Stat id, Vector2 boundaries)
		: base(statsController, id, boundaries)
	{
	}//IL_0024: Unknown result type (might be due to invalid IL or missing references)


	protected override Vector2 ComputeBaseBoundaries()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		if (base.ParentStat == null)
		{
			return base.Boundaries;
		}
		Vector2 boundaries = base.Boundaries;
		boundaries.y = Mathf.Min(base.ParentStat.FinalClamped, boundaries.y);
		foreach (StatModifierEffect perkStatModifierEffect in PerkStatModifierEffects)
		{
			if (perkStatModifierEffect.APerkModule.IsActive && !perkStatModifierEffect.HasBeenUsed)
			{
				perkStatModifierEffect.HasBeenUsed = true;
				boundaries.x -= perkStatModifierEffect.Value;
				boundaries.y -= perkStatModifierEffect.Value;
				perkStatModifierEffect.HasBeenUsed = false;
			}
		}
		foreach (GaugeModule perkGaugeModule in PerkGaugeModules)
		{
			if (perkGaugeModule.IsActive && !perkGaugeModule.HasBeenUsed)
			{
				perkGaugeModule.HasBeenUsed = true;
				boundaries.y -= perkGaugeModule.GaugeMaxValue;
				perkGaugeModule.HasBeenUsed = false;
			}
		}
		Base = Mathf.Clamp(Base, boundaries.x, boundaries.y);
		return boundaries;
	}

	private float ComputePerksBonus()
	{
		float num = 0f;
		foreach (StatModifierEffect perkStatModifierEffect in PerkStatModifierEffects)
		{
			if (perkStatModifierEffect.APerkModule.IsActive && !perkStatModifierEffect.HasBeenUsed)
			{
				perkStatModifierEffect.HasBeenUsed = true;
				num += perkStatModifierEffect.Value;
				perkStatModifierEffect.HasBeenUsed = false;
			}
		}
		foreach (DynamicStatsModifierEffect perkDynamicStatsModifierEffect in PerkDynamicStatsModifierEffects)
		{
			if (!perkDynamicStatsModifierEffect.HasBeenUsed)
			{
				perkDynamicStatsModifierEffect.HasBeenUsed = true;
				PlayableUnitStat highestStat = perkDynamicStatsModifierEffect.GetHighestStat();
				if (highestStat != null && highestStat.StatId != base.StatId)
				{
					num += Mathf.Floor(highestStat.FinalClamped * (perkDynamicStatsModifierEffect.Value / 100f));
				}
				perkDynamicStatsModifierEffect.HasBeenUsed = false;
			}
		}
		foreach (GaugeModule perkGaugeModule in PerkGaugeModules)
		{
			if (perkGaugeModule.IsActive && !perkGaugeModule.HasBeenUsed)
			{
				perkGaugeModule.HasBeenUsed = true;
				num += (float)(base.IsParentStat ? perkGaugeModule.GaugeMaxValue : perkGaugeModule.GaugeValue);
				perkGaugeModule.HasBeenUsed = false;
			}
		}
		return num;
	}

	private bool IsLocked()
	{
		return PerkLocksBuffer > 0;
	}
}
