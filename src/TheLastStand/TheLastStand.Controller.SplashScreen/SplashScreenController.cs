using TPLib;
using TPLib.Log;
using TheLastStand.Controller.SplashScreen.SplashScreenStates;
using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using TheLastStand.Model.SplashScreen;

namespace TheLastStand.Controller.SplashScreen;

public class SplashScreenController
{
	public delegate void SplashScreenStateChangeHandler(State state);

	public TheLastStand.Model.SplashScreen.SplashScreen SplashScreen { get; private set; }

	public event SplashScreenStateChangeHandler SplashScreenStateChangeEvent;

	public SplashScreenController()
	{
		SplashScreen = new TheLastStand.Model.SplashScreen.SplashScreen(this);
	}

	public void SetState(string stateName)
	{
		switch (stateName)
		{
		case "MainMenuLoading":
			SetState((State)(object)new MainMenuLoadingState());
			break;
		case "ModsLoading":
			SetState((State)(object)new ModsLoadingState());
			break;
		case "SettingsLoading":
			SetState((State)(object)new SettingsLoadingState());
			break;
		default:
			((CLogger<SplashScreenManager>)TPSingleton<SplashScreenManager>.Instance).LogError((object)("Trying to set an unknown State : " + stateName), (CLogLevel)1, true, true);
			break;
		}
	}

	public void SetState(State newState)
	{
		((CLogger<SplashScreenManager>)TPSingleton<SplashScreenManager>.Instance).Log((object)$"State transition : {((StateMachine)SplashScreen).State} -> {newState}", (CLogLevel)0, false, false);
		((StateMachine)SplashScreen).SetState(newState);
		SplashScreenManager.CurrentStateName = ((StateMachine)SplashScreen).State.GetName();
		this.SplashScreenStateChangeEvent?.Invoke(((StateMachine)SplashScreen).State);
	}
}
