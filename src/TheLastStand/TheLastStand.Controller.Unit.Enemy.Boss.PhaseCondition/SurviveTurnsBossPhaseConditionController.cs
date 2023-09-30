using TPLib;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseCondition;

public class SurviveTurnsBossPhaseConditionController : ABossPhaseConditionController
{
	public SurviveTurnsBossPhaseConditionDefinition SurviveTurnsBossPhaseConditionDefinition => base.BossPhaseConditionDefinition as SurviveTurnsBossPhaseConditionDefinition;

	public SurviveTurnsBossPhaseConditionController(SurviveTurnsBossPhaseConditionDefinition phaseConditionDefinition)
		: base(phaseConditionDefinition)
	{
	}

	public override bool IsValid()
	{
		return TPSingleton<BossManager>.Instance.CurrentBossPhase.PhaseStartedAtTurn + SurviveTurnsBossPhaseConditionDefinition.Expression.EvalToInt((InterpreterContext)(object)TPSingleton<BossManager>.Instance.CurrentBossPhase) <= TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
	}
}
