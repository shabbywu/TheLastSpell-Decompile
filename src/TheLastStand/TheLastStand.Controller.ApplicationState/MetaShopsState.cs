using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class MetaShopsState : State
{
	public const string Name = "MetaShops";

	public override string GetName()
	{
		return "MetaShops";
	}

	public override void OnStateEnter()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		if (((Scene)(ref activeScene)).name != ScenesManager.MetaShopSceneName)
		{
			MetaNarrationsManager.NarrationDoneThisDay = false;
			SceneManager.LoadScene(ScenesManager.MetaShopSceneName);
		}
	}
}
