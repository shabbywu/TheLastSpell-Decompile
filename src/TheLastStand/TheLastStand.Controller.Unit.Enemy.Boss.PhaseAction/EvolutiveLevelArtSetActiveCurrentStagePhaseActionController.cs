using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Model.Unit.Boss;
using TheLastStand.View;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class EvolutiveLevelArtSetActiveCurrentStagePhaseActionController : ABossPhaseActionController
{
	public EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition => base.ABossPhaseActionDefinition as EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition;

	public EvolutiveLevelArtSetActiveCurrentStagePhaseActionController(ABossPhaseActionDefinition aBossPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(aBossPhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		if (!TPSingleton<EvolutiveLevelArtObject>.Exist())
		{
			CLoggerManager.Log((object)"Tried to execute a EvolutiveLevelArtSetActiveCurrentStage but there is no EvolutiveLevelArtObject in the scene.", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		else
		{
			TPSingleton<EvolutiveLevelArtObject>.Instance.SetActiveCurrentStageObject(EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition.Value);
		}
		yield break;
	}
}
