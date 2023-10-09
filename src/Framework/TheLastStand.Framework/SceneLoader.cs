using System;
using System.Collections;
using TPLib.Log;
using TPLib.Yield;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TheLastStand.Framework;

public class SceneLoader : ASceneLoader
{
	[SerializeField]
	[Range(0f, 10f)]
	private float delay;

	[SerializeField]
	[Range(0f, 10f)]
	private float minDuration;

	[SerializeField]
	[Range(0f, 10f)]
	private float finalDelay;

	[SerializeField]
	protected UnityEvent onFinishLoading = new UnityEvent();

	[SerializeField]
	private bool loadOnStart = true;

	private void Start()
	{
		if (loadOnStart)
		{
			StartLoadScene();
		}
	}

	protected override IEnumerator LoadLevelAsyncCoroutine()
	{
		if (delay > 0f)
		{
			yield return (object)new WaitForSeconds(delay);
		}
		CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene load starting", (LogType)3, (CLogLevel)1, true, "SceneLoader", false);
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
		if (minDuration > 0f)
		{
			asyncLoad.allowSceneActivation = false;
			yield return WaitEndOfLoading(asyncLoad);
			onFinishLoading.Invoke();
			CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene ready", (LogType)3, (CLogLevel)1, true, "SceneLoader", false);
			if (finalDelay > 0f)
			{
				yield return SharedYields.WaitForSeconds(finalDelay);
			}
			Debug.Log((object)("Scene Loaded : " + Time.realtimeSinceStartup));
			asyncLoad.allowSceneActivation = true;
		}
		else
		{
			yield return (object)new WaitUntil((Func<bool>)(() => asyncLoad.isDone));
			onFinishLoading.Invoke();
			CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene ready", (LogType)3, (CLogLevel)1, true, "SceneLoader", false);
		}
	}

	protected override IEnumerator WaitEndOfLoading(AsyncOperation asyncOperation)
	{
		float timer = 0f;
		bool isLoading = true;
		while (!(asyncOperation.progress >= 0.9f) || !(Time.realtimeSinceStartup >= startTime + minDuration))
		{
			timer += Time.deltaTime;
			if (timer >= sceneLoadingLogGap)
			{
				timer = 0f;
				if (asyncOperation.progress < 0.9f)
				{
					CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene currently loading", (LogType)3, (CLogLevel)0, showLogsInUnity, "SceneLoader", false);
				}
				else if (Time.realtimeSinceStartup < startTime + minDuration)
				{
					CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene loaded, minDuration stalling", (LogType)3, (CLogLevel)0, showLogsInUnity, "SceneLoader", false);
				}
			}
			if (isLoading && asyncOperation.progress >= 0.9f && minDuration > 0f)
			{
				isLoading = false;
				CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene has just finished loading !", (LogType)3, (CLogLevel)0, showLogsInUnity, "SceneLoader", false);
			}
			yield return null;
		}
		if (minDuration == 0f)
		{
			CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] scene has just finished loading !", (LogType)3, (CLogLevel)0, showLogsInUnity, "SceneLoader", false);
		}
	}
}
