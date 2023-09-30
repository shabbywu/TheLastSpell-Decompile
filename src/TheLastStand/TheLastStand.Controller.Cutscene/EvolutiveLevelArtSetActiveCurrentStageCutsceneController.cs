using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Cutscene;
using TheLastStand.View;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class EvolutiveLevelArtSetActiveCurrentStageCutsceneController : CutsceneController
{
	public EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition => base.CutsceneDefinition as EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition;

	public EvolutiveLevelArtSetActiveCurrentStageCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (!TPSingleton<EvolutiveLevelArtObject>.Exist())
		{
			CLoggerManager.Log((object)"Tried to execute a EvolutiveLevelArtSetActiveCurrentStage but there is no EvolutiveLevelArtObject in the scene.", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		else
		{
			TPSingleton<EvolutiveLevelArtObject>.Instance.SetActiveCurrentStageObject(EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition.Value);
		}
		yield break;
	}
}
