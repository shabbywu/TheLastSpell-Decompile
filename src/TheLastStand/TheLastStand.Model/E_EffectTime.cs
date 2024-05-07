namespace TheLastStand.Model;

public enum E_EffectTime
{
	None,
	Permanent,
	OnDeath,
	OnCreation,
	OnTileCrossed,
	OnMovementEnd,
	OnHitTaken,
	OnDodge,
	OnTargetHit,
	OnTargetDodge,
	OnTargetKilled,
	OnDefenseTargetKilled,
	OnAttackDataComputed,
	OnSkillNextHit,
	OnSkillCastBegin,
	OnSkillCastEnd,
	OnSkillStatusApplied,
	OnStatusApplied,
	OnEnemyMovementEnd,
	OnPoisonProc,
	OnPoisonKill,
	OnBehaviorClusterExecutionEnd,
	OnBuildingDestroyed,
	OnBuildingConstructed,
	OnStartTurn,
	OnStartNightTurnPlayable,
	OnStartNightTurnEnemy,
	OnStartProductionTurn,
	OnStartDeploymentTurn,
	OnEndTurn,
	OnEndNightTurnPlayable,
	OnNightEnd,
	OnEndNightTurnEnemy,
	OnEndProductionTurn,
	OnEndDeploymentTurn,
	OnCreationAfterViewInitialized,
	OnSkillUndo,
	OnExtinguish,
	OnStatsLoadStart,
	OnStatusImmunityComputation,
	OnTargetingComputation,
	OnDealDamageTargetHit,
	OnDealDamageTargetKill,
	OnDealDamageExecutionEnd,
	OnPerkApplyStatusEnd
}
