using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public class BossPhaseHandlerDefinition : Definition
{
	public static class Constants
	{
		public static class Actions
		{
			public const string SetPhase = "SetPhase";

			public const string PauseWave = "PauseWave";

			public const string SetHandlerLock = "SetHandlerLock";

			public const string DestroyActor = "DestroyActor";

			public const string SpawnActor = "SpawnActor";

			public const string SetNightProgression = "SetNightProgression";

			public const string EvolutiveLevelArtSetStage = "EvolutiveLevelArtSetStage";

			public const string EvolutiveLevelArtSetActiveCurrentStage = "EvolutiveLevelArtSetActiveCurrentStage";

			public const string ReplaceActors = "ReplaceActors";

			public const string PlayCutscene = "PlayCutscene";

			public const string UnlockSpawnerBossAchievement = "UnlockSpawnerBossAchievement";
		}

		public const string Name = "PhaseHandler";
	}

	public List<ABossPhaseActionDefinition> ActionsDefinitions { get; } = new List<ABossPhaseActionDefinition>();


	public List<IBossPhaseConditionDefinition> ConditionsDefinitions { get; } = new List<IBossPhaseConditionDefinition>();


	public string Id { get; private set; }

	public bool DefaultLockValue { get; private set; }

	public BossPhaseHandlerDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Conditions"));
		if (val2 != null)
		{
			foreach (XElement item in ((XContainer)val2).Elements())
			{
				if (BossPhaseConditionsFactory.BossPhaseConditionDefinitionFromXElement(item, out var bossPhaseContentDefinition))
				{
					ConditionsDefinitions.Add(bossPhaseContentDefinition);
				}
			}
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Actions"));
		if (val3 != null)
		{
			foreach (XElement item2 in ((XContainer)val3).Elements())
			{
				switch (item2.Name.LocalName)
				{
				case "DestroyActor":
					ActionsDefinitions.Add(new DestroyActorPhaseActionDefinition((XContainer)(object)item2));
					break;
				case "EvolutiveLevelArtSetStage":
					ActionsDefinitions.Add(new EvolutiveLevelArtSetStagePhaseActionDefinition((XContainer)(object)item2));
					break;
				case "EvolutiveLevelArtSetActiveCurrentStage":
					ActionsDefinitions.Add(new EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition((XContainer)(object)item2));
					break;
				case "PauseWave":
					ActionsDefinitions.Add(new PauseWavePhaseActionDefinition((XContainer)(object)item2));
					break;
				case "PlayCutscene":
					ActionsDefinitions.Add(new PlayCutscenePhaseActionDefinition((XContainer)(object)item2));
					break;
				case "ReplaceActors":
					ActionsDefinitions.Add(new ReplaceActorsPhaseActionDefinition((XContainer)(object)item2));
					break;
				case "SetHandlerLock":
					ActionsDefinitions.Add(new SetPhaseHandlerLockPhaseActionDefinition((XContainer)(object)item2));
					break;
				case "SetNightProgression":
					ActionsDefinitions.Add(new SetNightProgressionPhaseActionDefinition((XContainer)(object)item2));
					break;
				case "SetPhase":
					ActionsDefinitions.Add(new SetPhasePhaseActionDefinition((XContainer)(object)item2));
					break;
				case "SpawnActor":
					ActionsDefinitions.Add(new SpawnActorPhaseActionDefinition((XContainer)(object)item2));
					break;
				case "UnlockSpawnerBossAchievement":
					ActionsDefinitions.Add(new UnlockSpawnerBossAchievementPhaseActionDefinition((XContainer)(object)item2));
					break;
				}
			}
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("Id"));
		Id = val4.Value;
		XAttribute val5 = val.Attribute(XName.op_Implicit("IsLockedByDefault"));
		if (val5 != null)
		{
			if (bool.TryParse(val5.Value, out var result))
			{
				DefaultLockValue = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val5.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
	}
}
