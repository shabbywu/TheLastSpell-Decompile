using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class LoadWorldMapState : State
{
	public const string Name = "LoadWorldMap";

	public override string GetName()
	{
		return "LoadWorldMap";
	}

	public override void OnStateEnter()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (!ScenesManager.IsActiveSceneWorldMap())
		{
			Scene activeScene = SceneManager.GetActiveScene();
			if (((Scene)(ref activeScene)).name != ScenesManager.LoadWorldMapSceneName)
			{
				SceneManager.LoadScene(ScenesManager.LoadWorldMapSceneName);
			}
		}
	}
}
