using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class GameLobbyState : State
{
	public const string Name = "GameLobby";

	public override string GetName()
	{
		return "GameLobby";
	}

	public override void OnStateEnter()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		if (!(((Scene)(ref activeScene)).name == ScenesManager.MainMenuSceneName) && ((Scene)(ref activeScene)).name != ScenesManager.SplashSceneName)
		{
			SceneManager.LoadScene(ScenesManager.MainMenuSceneName);
		}
	}
}
