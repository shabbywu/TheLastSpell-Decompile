using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Manager;

public class ScenesManager : Manager<ScenesManager>
{
	[SerializeField]
	private SceneField splashScene;

	[SerializeField]
	private SceneField mainMenuScene;

	[SerializeField]
	private SceneField creditsScene;

	[SerializeField]
	private SceneField animatedCutsceneScene;

	[SerializeField]
	private SceneField loadLevelScene;

	[SerializeField]
	private SceneField loadWorldMapScene;

	[SerializeField]
	private SceneField levelScene;

	[SerializeField]
	private bool overrideLevelScene;

	[SerializeField]
	private SceneField levelSceneOverride;

	[SerializeField]
	private SceneField worldMapScene;

	[SerializeField]
	private bool overrideWorldMapScene;

	[SerializeField]
	private SceneField worldMapSceneOverride;

	[SerializeField]
	private SceneField metaShopScene;

	[SerializeField]
	private bool overrideMetaShopScene;

	[SerializeField]
	private SceneField metaShopSceneOverride;

	public static string CreditsSceneName => TPSingleton<ScenesManager>.Instance.creditsScene;

	public static string AnimatedCutsceneSceneName => TPSingleton<ScenesManager>.Instance.animatedCutsceneScene;

	public static string LevelSceneName
	{
		get
		{
			if (TPSingleton<ScenesManager>.Instance.overrideLevelScene)
			{
				((CLogger<ScenesManager>)TPSingleton<ScenesManager>.Instance).Log((object)"Loading overridden level scene.", (CLogLevel)1, false, false);
				return TPSingleton<ScenesManager>.Instance.levelSceneOverride;
			}
			return TPSingleton<ScenesManager>.Instance.levelScene;
		}
	}

	public static string LoadLevelSceneName => TPSingleton<ScenesManager>.Instance.loadLevelScene;

	public static string LoadWorldMapSceneName => TPSingleton<ScenesManager>.Instance.loadWorldMapScene;

	public static string MainMenuSceneName => TPSingleton<ScenesManager>.Instance.mainMenuScene;

	public static string MetaShopSceneName
	{
		get
		{
			if (TPSingleton<ScenesManager>.Instance.overrideMetaShopScene)
			{
				((CLogger<ScenesManager>)TPSingleton<ScenesManager>.Instance).Log((object)"Loading overridden meta shop scene.", (CLogLevel)1, false, false);
				return TPSingleton<ScenesManager>.Instance.metaShopSceneOverride;
			}
			return TPSingleton<ScenesManager>.Instance.metaShopScene;
		}
	}

	public static string SplashSceneName => TPSingleton<ScenesManager>.Instance.splashScene;

	public static string WorldMapSceneName
	{
		get
		{
			if (TPSingleton<ScenesManager>.Instance.overrideWorldMapScene)
			{
				((CLogger<ScenesManager>)TPSingleton<ScenesManager>.Instance).Log((object)"Loading overridden world map scene.", (CLogLevel)1, false, false);
				return TPSingleton<ScenesManager>.Instance.worldMapSceneOverride;
			}
			return TPSingleton<ScenesManager>.Instance.worldMapScene;
		}
	}

	public static bool IsActiveSceneLevel()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		string name = ((Scene)(ref activeScene)).name;
		if (TPSingleton<ScenesManager>.Instance.overrideLevelScene)
		{
			return name == TPSingleton<ScenesManager>.Instance.levelSceneOverride;
		}
		return name == TPSingleton<ScenesManager>.Instance.levelScene;
	}

	public static bool IsActiveSceneWorldMap()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		string name = ((Scene)(ref activeScene)).name;
		if (TPSingleton<ScenesManager>.Instance.overrideWorldMapScene)
		{
			return name == TPSingleton<ScenesManager>.Instance.worldMapSceneOverride;
		}
		return name == TPSingleton<ScenesManager>.Instance.worldMapScene;
	}

	public static bool IsActiveSceneMetaShop()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		string name = ((Scene)(ref activeScene)).name;
		if (TPSingleton<ScenesManager>.Instance.overrideMetaShopScene)
		{
			return name == TPSingleton<ScenesManager>.Instance.metaShopSceneOverride;
		}
		return name == TPSingleton<ScenesManager>.Instance.metaShopScene;
	}

	public static bool IsSceneActive(string sceneName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		string name = ((Scene)(ref activeScene)).name;
		if (TPSingleton<ScenesManager>.Instance.overrideMetaShopScene)
		{
			return name == TPSingleton<ScenesManager>.Instance.metaShopSceneOverride;
		}
		return name == sceneName;
	}
}
