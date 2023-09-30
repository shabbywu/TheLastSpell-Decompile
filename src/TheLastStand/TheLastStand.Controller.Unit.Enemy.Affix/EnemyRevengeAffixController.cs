using TheLastStand.Controller.Status;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Model;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyRevengeAffixController : EnemyAffixController
{
	public EnemyRevengeAffix EnemyRevengeAffix => base.EnemyAffix as EnemyRevengeAffix;

	public EnemyRevengeAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyRevengeAffix(this, enemyAffixDefinition, enemyUnit);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
		if (effectTime == E_EffectTime.OnHitTaken)
		{
			ISkillCaster attacker = data as ISkillCaster;
			ApplyAlteration(attacker);
		}
	}

	private void ApplyAlteration(ISkillCaster attacker)
	{
		if (!(attacker is PlayableUnit playableUnit))
		{
			return;
		}
		StatusEffectDefinition statusEffectDefinition = EnemyRevengeAffix.EnemyRevengeAffixEffectDefinition.StatusEffectDefinition;
		if (!(statusEffectDefinition is DebuffEffectDefinition debuffEffectDefinition))
		{
			if (!(statusEffectDefinition is StunEffectDefinition stunEffectDefinition))
			{
				if (statusEffectDefinition is PoisonEffectDefinition poisonEffectDefinition)
				{
					StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
					statusCreationInfo.Source = base.EnemyAffix.EnemyUnit;
					statusCreationInfo.TurnsCount = poisonEffectDefinition.TurnsCount;
					statusCreationInfo.Value = poisonEffectDefinition.DamagePerTurn;
					statusCreationInfo.IsFromInjury = false;
					StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
					playableUnit.UnitController.AddStatus(new PoisonStatusController(base.EnemyAffix.EnemyUnit, statusCreationInfo2).Status);
					playableUnit.UnitView.UnitHUD.RefreshHealth();
				}
			}
			else
			{
				StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
				statusCreationInfo.Source = base.EnemyAffix.EnemyUnit;
				statusCreationInfo.TurnsCount = stunEffectDefinition.TurnsCount;
				statusCreationInfo.IsFromInjury = false;
				StatusCreationInfo statusCreationInfo3 = statusCreationInfo;
				playableUnit.UnitController.AddStatus(new StunStatusController(base.EnemyAffix.EnemyUnit, statusCreationInfo3).Status);
				playableUnit.UnitView.RefreshStatus();
			}
		}
		else
		{
			StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
			statusCreationInfo.Source = base.EnemyAffix.EnemyUnit;
			statusCreationInfo.TurnsCount = debuffEffectDefinition.TurnsCount;
			statusCreationInfo.Value = debuffEffectDefinition.ModifierValue;
			statusCreationInfo.Stat = debuffEffectDefinition.Stat;
			statusCreationInfo.IsFromInjury = false;
			StatusCreationInfo statusCreationInfo4 = statusCreationInfo;
			playableUnit.UnitController.AddStatus(new DebuffStatusController(base.EnemyAffix.EnemyUnit, statusCreationInfo4).Status);
			playableUnit.UnitView.RefreshStatus();
		}
	}
}
