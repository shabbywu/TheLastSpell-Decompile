using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Model.Extensions;

public static class E_SkillUnitAffectExtensions
{
	public static bool AffectsUnitType(this AffectingUnitSkillEffectDefinition.E_SkillUnitAffect affectedUnits, AffectingUnitSkillEffectDefinition.E_SkillUnitAffect unitType)
	{
		return affectedUnits.HasFlag(unitType);
	}

	public static void Deserialize(this ref AffectingUnitSkillEffectDefinition.E_SkillUnitAffect affectedUnits, XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("CanAffectPlayableUnits"));
		if (val != null)
		{
			if (bool.TryParse(val.Value, out var result))
			{
				affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.PlayableUnit;
				if (result)
				{
					affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.PlayableUnit;
				}
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse CanAffectPlayableUnits into a bool : " + val.Value), (Object)(object)TPSingleton<SkillManager>.Instance, (LogType)0, (CLogLevel)2, true, "SkillManager", false);
			}
		}
		XElement val2 = obj.Element(XName.op_Implicit("CanAffectEnemyUnits"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result2))
			{
				affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit;
				if (result2)
				{
					affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit;
				}
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse CanAffectEnemyUnits into a bool : " + val2.Value), (Object)(object)TPSingleton<SkillManager>.Instance, (LogType)0, (CLogLevel)2, true, "SkillManager", false);
			}
		}
		XElement val3 = obj.Element(XName.op_Implicit("CanAffectBossUnits"));
		if (val3 != null)
		{
			if (bool.TryParse(val3.Value, out var result3))
			{
				affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
				if (result3)
				{
					affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
				}
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse CanAffectBossUnits into a bool : " + val3.Value), (Object)(object)TPSingleton<SkillManager>.Instance, (LogType)0, (CLogLevel)2, true, "SkillManager", false);
				affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
				if (affectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit))
				{
					affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
				}
			}
		}
		else
		{
			affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
			if (affectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit))
			{
				affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
			}
		}
		XElement val4 = obj.Element(XName.op_Implicit("CanAffectCaster"));
		if (val4 != null)
		{
			if (bool.TryParse(val4.Value, out var result4))
			{
				affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster;
				if (result4)
				{
					affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster;
				}
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse CanAffectCaster into a bool : " + val4.Value), (Object)(object)TPSingleton<SkillManager>.Instance, (LogType)0, (CLogLevel)2, true, "SkillManager", false);
			}
		}
		XElement val5 = obj.Element(XName.op_Implicit("CanAffectBuildings"));
		if (val5 == null)
		{
			return;
		}
		if (bool.TryParse(val5.Value, out var result5))
		{
			affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Building;
			if (result5)
			{
				affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Building;
			}
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse CanAffectBuildings into a bool : " + val5.Value), (Object)(object)TPSingleton<SkillManager>.Instance, (LogType)0, (CLogLevel)2, true, "SkillManager", false);
		}
	}

	public static bool ShouldDamageableBeAffected(this AffectingUnitSkillEffectDefinition.E_SkillUnitAffect affectedUnits, ISkillCaster caster, IDamageable damageable)
	{
		if (damageable is EnemyUnit enemyUnit)
		{
			EnemyHigherPlaneAffixController.TargetingValidity targetingValidity = new EnemyHigherPlaneAffixController.TargetingValidity(caster, newValidity: true);
			enemyUnit.EnemyUnitController.TriggerAffixes(E_EffectTime.OnTargetingComputation, targetingValidity);
			if (!targetingValidity.validity)
			{
				return false;
			}
		}
		DamageableModule damageableModule;
		if (damageable is ISkillCaster skillCaster)
		{
			if (skillCaster == caster)
			{
				return affectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster);
			}
			damageableModule = damageable as DamageableModule;
			if (damageableModule != null)
			{
				goto IL_0078;
			}
			if (damageable is PlayableUnit)
			{
				return affectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.PlayableUnit);
			}
			if (damageable is BossUnit)
			{
				return affectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit);
			}
			if (damageable is EnemyUnit)
			{
				return affectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit);
			}
		}
		else
		{
			damageableModule = damageable as DamageableModule;
			if (damageableModule != null)
			{
				goto IL_0078;
			}
			if (damageable == null)
			{
				return false;
			}
		}
		return true;
		IL_0078:
		if (caster != null && caster == damageableModule.BuildingParent?.BattleModule)
		{
			return affectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster);
		}
		return affectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Building);
	}
}
