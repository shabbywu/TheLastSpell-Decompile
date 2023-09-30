using System;
using System.Collections;
using System.Collections.Generic;
using AsusAuraWrapper;
using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Manager.SDK;

public class LightningSDKManager : Manager<LightningSDKManager>
{
	public enum SDKEvent
	{
		MAIN_MENU,
		WORLDMAP,
		PRODUCTION,
		DEPLOYMENT,
		PLAYER_TURN,
		ENEMY_TURN,
		DARK_SHOP,
		LIGHT_SHOP,
		HUB_SHOP
	}

	[Serializable]
	public struct ColorEventPair
	{
		public SDKEvent Event;

		public DataColor Color;
	}

	private AsusAuraService asusAuraService;

	private Color32 currentColor = Color32.op_Implicit(Color.black);

	private Coroutine currentCoroutine;

	[SerializeField]
	private List<ColorEventPair> colorPerEvent = new List<ColorEventPair>();

	[SerializeField]
	private AnimationCurve flashCurve;

	[SerializeField]
	[Range(0.05f, 1f)]
	private float flashDuration = 0.1f;

	[SerializeField]
	[Range(0.05f, 2f)]
	private float transitionDuration = 0.5f;

	public void HandleApplicationStateColor(State state, float? duration = null)
	{
		switch (state.GetName())
		{
		case "GameLobby":
			TryTransitionToColorEvent(SDKEvent.MAIN_MENU, duration);
			break;
		case "WorldMap":
			TryTransitionToColorEvent(SDKEvent.WORLDMAP, duration);
			break;
		case "MetaShops":
			break;
		}
	}

	public void HandleGameCycleColor(float? duration = null)
	{
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			switch (TPSingleton<GameManager>.Instance.Game.DayTurn)
			{
			case Game.E_DayTurn.Production:
				TryTransitionToColorEvent(SDKEvent.PRODUCTION, duration);
				break;
			case Game.E_DayTurn.Deployment:
				TryTransitionToColorEvent(SDKEvent.DEPLOYMENT, duration);
				break;
			}
			break;
		case Game.E_Cycle.Night:
			switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
			{
			case Game.E_NightTurn.EnemyUnits:
				TryTransitionToColorEvent(SDKEvent.ENEMY_TURN, duration);
				break;
			case Game.E_NightTurn.PlayableUnits:
				TryTransitionToColorEvent(SDKEvent.PLAYER_TURN, duration);
				break;
			}
			break;
		}
	}

	public void HandleMetaShopTransition(SDKEvent metaShopTarget, float? duration = null)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		switch (metaShopTarget)
		{
		case SDKEvent.DARK_SHOP:
		case SDKEvent.LIGHT_SHOP:
			TryTransitionToColorEvent(metaShopTarget, duration);
			break;
		case SDKEvent.HUB_SHOP:
			TransitionToColor(Color32.op_Implicit(Color.black), duration);
			break;
		}
	}

	public void OnApplicationStateChange(State state)
	{
		HandleApplicationStateColor(state);
	}

	public void SetAllLightsToColor(uint r, uint g, uint b, uint a)
	{
		AsusAuraService obj = asusAuraService;
		if (obj != null)
		{
			obj.SetAllLightsToColor(r, g, b, a);
		}
	}

	public void SetAllLightsToColor(Color32 color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		SetAllLightsToColor(color.r, color.g, color.b, color.a);
	}

	public void TransitionToColor(Color32 targetColor, float? duration = null)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		StopCurrentCoroutineIfNeeded();
		currentCoroutine = ((MonoBehaviour)this).StartCoroutine(ColorTransitionCoroutine(currentColor, targetColor, duration ?? transitionDuration));
	}

	public void TriggerFlashEffect(bool backToCurrentColorOnEnd = true)
	{
		StopCurrentCoroutineIfNeeded();
		currentCoroutine = ((MonoBehaviour)this).StartCoroutine(FlashCoroutine(backToCurrentColorOnEnd));
	}

	public bool TryTransitionToColorEvent(SDKEvent SDKEventTriggered, float? duration = null)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		ColorEventPair colorEventPair = default(ColorEventPair);
		if (ListExtensions.TryFind<ColorEventPair>(colorPerEvent, (Predicate<ColorEventPair>)((ColorEventPair x) => x.Event == SDKEventTriggered), ref colorEventPair))
		{
			TransitionToColor(Color32.op_Implicit(colorEventPair.Color._Color), duration);
			return true;
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		if (((TPSingleton<LightningSDKManager>)(object)this)._IsValid)
		{
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
			HandleApplicationStateColor(((StateMachine)ApplicationManager.Application).State);
		}
	}

	private IEnumerator ColorTransitionCoroutine(Color32 initialColor, Color32 targetColor, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			currentColor = Color32.op_Implicit(Color.Lerp(Color32.op_Implicit(initialColor), Color32.op_Implicit(targetColor), elapsedTime / duration));
			SetAllLightsToColor(currentColor);
			yield return null;
		}
		currentCoroutine = null;
	}

	private IEnumerator FlashCoroutine(bool backToCurrentColorOnEnd = true)
	{
		float elapsedTime = 0f;
		Color32 currentFlashColor = Color32.op_Implicit(Color.black);
		while (elapsedTime < flashDuration)
		{
			elapsedTime += Time.deltaTime;
			currentFlashColor = Color32.op_Implicit(Color.white * flashCurve.Evaluate(elapsedTime / flashDuration));
			SetAllLightsToColor(currentFlashColor);
			yield return null;
		}
		if (backToCurrentColorOnEnd)
		{
			yield return ColorTransitionCoroutine(currentFlashColor, currentColor, transitionDuration);
		}
		else
		{
			currentColor = currentFlashColor;
		}
	}

	private void OnApplicationQuit()
	{
		AsusAuraService obj = asusAuraService;
		if (obj != null)
		{
			obj.ExitSDKMode();
		}
	}

	private void StopCurrentCoroutineIfNeeded()
	{
		if (currentCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(currentCoroutine);
		}
	}
}
