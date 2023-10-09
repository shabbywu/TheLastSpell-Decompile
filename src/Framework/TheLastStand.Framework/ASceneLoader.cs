using System.Collections;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Framework;

public abstract class ASceneLoader : MonoBehaviour
{
	[SerializeField]
	protected SceneField levelName;

	[SerializeField]
	[Range(0f, 5f)]
	protected float sceneLoadingLogGap = 0.5f;

	[SerializeField]
	protected bool showLogsInUnity;

	protected float startTime;

	protected int sceneLoadingLogCounter;

	public virtual string SceneName => levelName;

	public virtual void StartLoadScene()
	{
		startTime = Time.realtimeSinceStartup;
		CLoggerManager.Log((object)$"[{Time.realtimeSinceStartup}] SceneLoader Start", (LogType)3, (CLogLevel)1, true, "SceneLoader", false);
		((MonoBehaviour)this).StartCoroutine(LoadLevelAsyncCoroutine());
	}

	protected abstract IEnumerator LoadLevelAsyncCoroutine();

	protected abstract IEnumerator WaitEndOfLoading(AsyncOperation asyncOperation);
}
