using System.Xml.Linq;
using TheLastStand.Controller.Unit.Enemy.Boss.PhaseCondition;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;

public static class BossPhaseConditionsFactory
{
	public static class Constants
	{
		public static class Conditions
		{
			public const string FinishWave = "FinishWave";

			public const string KilledActors = "KilledActors";

			public const string RemainingActors = "RemainingActors";

			public const string SurviveTurns = "SurviveTurns";

			public const string TurnCondition = "TurnCondition";
		}
	}

	public static ABossPhaseConditionController BossPhaseConditionControllerFromDefinition(IBossPhaseConditionDefinition bossPhaseConditionDefinition)
	{
		if (!(bossPhaseConditionDefinition is BossPhaseTurnConditionDefinition definition))
		{
			if (!(bossPhaseConditionDefinition is FinishWaveBossPhaseConditionDefinition bossPhaseConditionDefinition2))
			{
				if (!(bossPhaseConditionDefinition is KilledActorsAmountBossPhaseConditionDefinition bossPhaseConditionDefinition3))
				{
					if (!(bossPhaseConditionDefinition is RemainingActorsBossPhaseConditionDefinition remainingActorsBossPhaseConditionDefinition))
					{
						if (bossPhaseConditionDefinition is SurviveTurnsBossPhaseConditionDefinition phaseConditionDefinition)
						{
							return new SurviveTurnsBossPhaseConditionController(phaseConditionDefinition);
						}
						return null;
					}
					return new RemainingActorsBossPhaseConditionController(remainingActorsBossPhaseConditionDefinition);
				}
				return new KilledActorsAmountBossPhaseConditionController(bossPhaseConditionDefinition3);
			}
			return new FinishWaveBossPhaseConditionController(bossPhaseConditionDefinition2);
		}
		return new BossPhaseTurnConditionController(definition);
	}

	public static bool BossPhaseConditionControllerFromDefinition(IBossPhaseConditionDefinition bossPhaseConditionDefinition, out ABossPhaseConditionController bossPhaseConditionController)
	{
		bossPhaseConditionController = BossPhaseConditionControllerFromDefinition(bossPhaseConditionDefinition);
		return bossPhaseConditionController != null;
	}

	public static IBossPhaseConditionDefinition BossPhaseConditionDefinitionFromXElement(XElement conditionElement)
	{
		return conditionElement.Name.LocalName switch
		{
			"FinishWave" => new FinishWaveBossPhaseConditionDefinition((XContainer)(object)conditionElement), 
			"KilledActors" => new KilledActorsAmountBossPhaseConditionDefinition((XContainer)(object)conditionElement), 
			"RemainingActors" => new RemainingActorsBossPhaseConditionDefinition((XContainer)(object)conditionElement), 
			"SurviveTurns" => new SurviveTurnsBossPhaseConditionDefinition((XContainer)(object)conditionElement), 
			"TurnCondition" => new BossPhaseTurnConditionDefinition((XContainer)(object)conditionElement), 
			_ => null, 
		};
	}

	public static bool BossPhaseConditionDefinitionFromXElement(XElement conditionElement, out IBossPhaseConditionDefinition bossPhaseContentDefinition)
	{
		bossPhaseContentDefinition = BossPhaseConditionDefinitionFromXElement(conditionElement);
		return bossPhaseContentDefinition != null;
	}
}
