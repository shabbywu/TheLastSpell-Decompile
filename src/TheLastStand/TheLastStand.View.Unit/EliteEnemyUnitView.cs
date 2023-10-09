using System;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.View.Unit;

public class EliteEnemyUnitView : EnemyUnitView
{
	public new static class Constants
	{
		public const string EliteAffixDefaultSuffix = "Default";

		public const string EliteAffixFXPath = "Animation/EliteAffixes/";

		public const string EliteAffixPrefix = "FX_EliteAffixes_";

		public const string SpritesFolderName = "EliteEnemyUnits";
	}

	[SerializeField]
	private Animator affixAnimator;

	public EliteEnemyUnit EliteEnemyUnit => base.EnemyUnit as EliteEnemyUnit;

	protected override E_EnemyHudType GetBestFittingHUD()
	{
		float finalClamped = EliteEnemyUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthTotal).FinalClamped;
		E_GaugeSize e_GaugeSize = E_GaugeSize.Small;
		bool flag = false;
		for (int i = 0; i < EnemyUnitDatabase.EliteGaugeSizeThresholds.Count; i++)
		{
			int num = EnemyUnitDatabase.EliteGaugeSizeThresholds[i];
			if (finalClamped < (float)num)
			{
				break;
			}
			if (!Enum.IsDefined(typeof(E_GaugeSize), i + 1))
			{
				flag = true;
				break;
			}
			e_GaugeSize = (E_GaugeSize)(i + 1);
		}
		if (flag || finalClamped >= (float)EnemyUnitDatabase.EliteGaugeSizeThresholds[EnemyUnitDatabase.EliteGaugeSizeThresholds.Count - 1])
		{
			return E_EnemyHudType.BossLarge;
		}
		if (e_GaugeSize != 0)
		{
			return E_EnemyHudType.Large;
		}
		return E_EnemyHudType.Small;
	}

	protected override void InitDefaultSprites(string unitId)
	{
		base.EnemyUnit.DefaultSpriteFront = ResourcePooler.LoadOnce<Sprite>("View/Sprites/Units/" + GetDefaultSpritesFolder() + "/DefaultSprites/" + unitId + "/" + base.EnemyUnit.VariantId + "/" + unitId + "_" + base.EnemyUnit.VariantId + "_Idle_Front_00", failSilently: false);
		Sprite val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/Units/" + GetDefaultSpritesFolder() + "/DefaultSprites/" + unitId + "/" + base.EnemyUnit.VariantId + "/" + unitId + "_" + base.EnemyUnit.VariantId + "_Idle_Back_00", failSilently: false);
		base.EnemyUnit.DefaultSpriteBack = val ?? base.EnemyUnit.DefaultSpriteFront;
	}

	public override void InitVisuals(bool playSpawnAnim)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		base.InitVisuals(playSpawnAnim);
		AnimationClip val = ResourcePooler.LoadOnce<AnimationClip>("Animation/EliteAffixes/FX_EliteAffixes_" + EliteEnemyUnit.Affixes[0].EnemyAffixDefinition.EnemyAffixEffectDefinition.EnemyAffixEffect, failSilently: false);
		if ((Object)(object)val != (Object)null)
		{
			AnimatorOverrideController val2 = new AnimatorOverrideController(affixAnimator.runtimeAnimatorController);
			val2["FX_EliteAffixes_Default"] = val;
			affixAnimator.runtimeAnimatorController = (RuntimeAnimatorController)(object)val2;
		}
	}

	public override string GetDefaultSpritesFolder()
	{
		return "EliteEnemyUnits";
	}
}
