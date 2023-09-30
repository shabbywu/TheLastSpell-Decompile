using System;
using System.Collections;
using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class ReloadGameState : State
{
	public const string Name = "ReloadGame";

	public override string GetName()
	{
		return "ReloadGame";
	}

	public override void OnStateEnter()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		if (((Scene)(ref activeScene)).name != ScenesManager.LoadLevelSceneName)
		{
			((MonoBehaviour)TPSingleton<ApplicationManager>.Instance).StartCoroutine(WaitForSavesCompletionThenLoadGame());
		}
	}

	private IEnumerator WaitForSavesCompletionThenLoadGame()
	{
		yield return (object)new WaitUntil((Func<bool>)(() => SaverLoader.AreSavesCompleted()));
		SceneManager.LoadScene(ScenesManager.LoadLevelSceneName);
	}
}
