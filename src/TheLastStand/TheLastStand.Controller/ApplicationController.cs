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
		if (Application.PreviousStatesStack.Count == 0)
		{
			((CLogger<ApplicationManager>)TPSingleton<ApplicationManager>.Instance).LogError((object)$"Trying to back to a previous state (currently {Application.State}) but no previous state found!", (CLogLevel)2, true, true);
		}
		else
		{
			Application.SetState(Application.PreviousStatesStack.Pop());
		}
	}

	public void SetState(string stateName)
	{
		if (!Application.StatesPool.TryGetValue(stateName, out var value))
		{
			switch (stateName)
			{
			case "ExitApp":
				value = new ExitAppState();
				break;
			case "Game":
				value = new GameState();
				break;
			case "GameLobby":
				value = new GameLobbyState();
				break;
			case "LoadGame":
				value = new LoadGameState();
				break;
			case "ReloadGame":
				value = new ReloadGameState();
				break;
			case "LoadWorldMap":
				value = new LoadWorldMapState();
				break;
			case "WorldMap":
				value = new WorldMapState();
				break;
			case "MetaShops":
				value = new MetaShopsState();
				break;
			case "NewGame":
				value = new NewGameState();
				break;
			case "Settings":
				value = new SettingsState();
				break;
			case "LevelEditor":
				value = new LevelEditorState();
				break;
			case "Credits":
				value = new CreditsState();
				break;
			case "AnimatedCutscene":
				value = new AnimatedCutsceneState();
				break;
			case "ModList":
				value = new ModListState();
				break;
			case "SplashScreen":
				value = new TheLastStand.Controller.ApplicationState.SplashScreen();
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
			if (Application.State == null)
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
		((CLogger<ApplicationManager>)TPSingleton<ApplicationManager>.Instance).Log((object)$"State transition : {Application.State} -> {newState}", (CLogLevel)0, false, false);
		Application.SetState(newState);
		ApplicationManager.CurrentStateName = Application.State.GetName();
		this.ApplicationStateChangeEvent?.Invoke(Application.State);
	}

	public IEnumerator SetStateCoroutine(State newState)
	{
		CanvasFadeManager.FadeIn(-1f, -1, (Ease)0);
		yield return (object)new WaitUntil((Func<bool>)(() => TPSingleton<CanvasFadeManager>.Instance.FadeIsOver));
		SetState(newState);
	}
}
