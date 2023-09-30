using System;
using System.Collections;
using DG.Tweening;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Controller;

public class ApplicationController
{
	public delegate void ApplicationStateChangeHandler(State state);

	public Application Application { get; private set; }

	public event ApplicationStateChangeHandler ApplicationStateChangeEvent;

	public ApplicationController()
	{
		Application = new Application(this);
	}

	public void BackToPreviousState()
	{
		if (((PushdownAutomata)Application).PreviousStatesStack.Count == 0)
		{
			((CLogger<ApplicationManager>)TPSingleton<ApplicationManager>.Instance).LogError((object)$"Trying to back to a previous state (currently {((StateMachine)Application).State}) but no previous state found!", (CLogLevel)2, true, true);
		}
		else
		{
			((StateMachine)Application).SetState(((PushdownAutomata)Application).PreviousStatesStack.Pop());
		}
	}

	public void SetState(string stateName)
	{
		if (!Application.StatesPool.TryGetValue(stateName, out var value))
		{
			switch (stateName)
			{
			case "ExitApp":
				value = (State)(object)new ExitAppState();
				break;
			case "Game":
				value = (State)(object)new GameState();
				break;
			case "GameLobby":
				value = (State)(object)new GameLobbyState();
				break;
			case "LoadGame":
				value = (State)(object)new LoadGameState();
				break;
			case "ReloadGame":
				value = (State)(object)new ReloadGameState();
				break;
			case "LoadWorldMap":
				value = (State)(object)new LoadWorldMapState();
				break;
			case "WorldMap":
				value = (State)(object)new WorldMapState();
				break;
			case "MetaShops":
				value = (State)(object)new MetaShopsState();
				break;
			case "NewGame":
				value = (State)(object)new NewGameState();
				break;
			case "Settings":
				value = (State)(object)new SettingsState();
				break;
			case "LevelEditor":
				value = (State)(object)new LevelEditorState();
				break;
			case "Credits":
				value = (State)(object)new CreditsState();
				break;
			case "AnimatedCutscene":
				value = (State)(object)new AnimatedCutsceneState();
				break;
			case "ModList":
				value = (State)(object)new ModListState();
				break;
			case "SplashScreen":
				value = (State)(object)new TheLastStand.Controller.ApplicationState.SplashScreen();
				break;
			default:
				((CLogger<ApplicationManager>)TPSingleton<ApplicationManager>.Instance).LogError((object)("Unknown state " + stateName), (CLogLevel)2, true, true);
				return;
			}
			Application.StatesPool.Add(stateName, value);
		}
		switch (stateName)
		{
		case "GameLobby":
			if (ScenesManager.IsSceneActive(ScenesManager.SplashSceneName))
			{
				break;
			}
			goto case "Credits";
		case "Credits":
		case "Game":
		case "AnimatedCutscene":
		case "LoadGame":
		case "LoadWorldMap":
		case "MetaShops":
		case "NewGame":
		case "WorldMap":
			if (((StateMachine)Application).State == null)
			{
				SetState(value);
			}
			else
			{
				((MonoBehaviour)TPSingleton<ApplicationManager>.Instance).StartCoroutine(SetStateCoroutine(value));
			}
			return;
		}
		SetState(value);
	}

	public void SetState(State newState, params object[] args)
	{
		((CLogger<ApplicationManager>)TPSingleton<ApplicationManager>.Instance).Log((object)$"State transition : {((StateMachine)Application).State} -> {newState}", (CLogLevel)0, false, false);
		((StateMachine)Application).SetState(newState);
		ApplicationManager.CurrentStateName = ((StateMachine)Application).State.GetName();
		this.ApplicationStateChangeEvent?.Invoke(((StateMachine)Application).State);
	}

	public IEnumerator SetStateCoroutine(State newState)
	{
		CanvasFadeManager.FadeIn(-1f, -1, (Ease)0);
		yield return (object)new WaitUntil((Func<bool>)(() => TPSingleton<CanvasFadeManager>.Instance.FadeIsOver));
		SetState(newState);
	}
}
