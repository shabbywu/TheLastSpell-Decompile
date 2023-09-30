using System;
using System.Collections;
using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class LoadGameState : State
{
	public const string Name = "LoadGame";

	public override string GetName()
	{
		return "LoadGame";
	}

	public override void OnStateEnter()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (!ScenesManager.IsActiveSceneLevel())
		{
			Scene activeScene = SceneManager.GetActiveScene();
			if (((Scene)(ref activeScene)).name != ScenesManager.LoadLevelSceneName)
			{
				((MonoBehaviour)TPSingleton<ApplicationManager>.Instance).StartCoroutine(WaitForSavesCompletionThenLoadGame());
			}
		}
	}

	private IEnumerator WaitForSavesCompletionThenLoadGame()
	{
		yield return (object)new WaitUntil((Func<bool>)(() => SaverLoader.AreSavesCompleted()));
		SceneManager.LoadScene(ScenesManager.LoadLevelSceneName);
	}
}
