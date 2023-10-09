using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller;

public class MainMenuSceneLoader : ASceneLoader
{
	protected override IEnumerator LoadLevelAsyncCoroutine()
	{
		CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene load starting", (LogType)3, (CLogLevel)1, showLogsInUnity, "MainMenuSceneLoader", false);
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
		asyncLoad.allowSceneActivation = false;
		yield return WaitEndOfLoading(asyncLoad);
		CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene has just finished loading !", (LogType)3, (CLogLevel)0, showLogsInUnity, "MainMenuSceneLoader", false);
		float timer = 0f;
		while (!TPSingleton<SplashScreenManager>.Instance.SplashScreenIsOver)
		{
			timer += Time.deltaTime;
			if (timer >= sceneLoadingLogGap)
			{
				timer = 0f;
				CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene loaded, minDuration stalling", (LogType)3, (CLogLevel)0, showLogsInUnity, "MainMenuSceneLoader", false);
			}
			yield return null;
		}
		ApplicationManager.Application.ApplicationController.SetState("GameLobby");
		CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene ready", (LogType)3, (CLogLevel)1, showLogsInUnity, "MainMenuSceneLoader", false);
		asyncLoad.allowSceneActivation = true;
	}

	protected override IEnumerator WaitEndOfLoading(AsyncOperation asyncOperation)
	{
		float timer = 0f;
		while (!(asyncOperation.progress >= 0.9f))
		{
			timer += Time.deltaTime;
			if (timer >= sceneLoadingLogGap)
			{
				timer = 0f;
				CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene currently loading", (LogType)3, (CLogLevel)0, showLogsInUnity, "MainMenuSceneLoader", false);
			}
			yield return null;
		}
	}
}
