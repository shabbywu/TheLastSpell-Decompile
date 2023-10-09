using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.Manager;

public class HUDJoystickNavigationManager : Manager<HUDJoystickNavigationManager>
{
	[SerializeField]
	private HUDJoystickTarget firstHUDJoystickTarget;

	[SerializeField]
	private JoystickHighlight joystickHighlight;

	private Vector2 previousPanelsNavigationInput;

	private HUDJoystickTarget currentPanel;

	private GameObject previousSelection;

	[HideInInspector]
	public bool SlotSelectionToggleThisFrame;

	[HideInInspector]
	public InventorySlot InventorySlotToPlace;

	private Tween highlightPositionTween;

	private Tween highlightSizeTween;

	public bool HUDNavigationOn { get; private set; }

	public bool ShowTooltips { get; private set; }

	public Dictionary<Selectable, HUDJoystickTarget> SelectablesContainerHUDTargets { get; } = new Dictionary<Selectable, HUDJoystickTarget>();


	public JoystickHighlight JoystickHighlight => joystickHighlight;

	public event Action<bool> HUDNavigationToggled;

	public event Action<bool> TooltipsToggled;

	public bool CanOpenHUDNavigationMode()
	{
		if (ApplicationManager.Application.State.GetName() != "Game")
		{
			return false;
		}
		return TPSingleton<GameManager>.Instance.Game.State switch
		{
			Game.E_State.Management => TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits, 
			Game.E_State.Construction => true, 
			_ => false, 
		};
	}

	public bool CanExitHUDNavigationMode()
	{
		if (ApplicationManager.Application.State.GetName() == "Game")
		{
			return TPSingleton<GameManager>.Instance.Game.State switch
			{
				Game.E_State.Management => true, 
				Game.E_State.Construction => true, 
				_ => false, 
			};
		}
		if (ApplicationManager.Application.State.GetName() == "Settings" || ApplicationManager.Application.State.GetName() == "GameLobby")
		{
			return false;
		}
		return true;
	}

	public void OpenHUDNavigationMode(bool selectDefaultPanel = true)
	{
		if (!HUDNavigationOn)
		{
			if (selectDefaultPanel)
			{
				SelectHUDDefaultPanel();
			}
			HUDNavigationOn = true;
			this.HUDNavigationToggled?.Invoke(HUDNavigationOn);
		}
	}

	public bool ExitHUDNavigationMode()
	{
		if (!HUDNavigationOn)
		{
			return false;
		}
		if ((Object)(object)currentPanel != (Object)null)
		{
			currentPanel.RaiseDeselectEvent();
		}
		EventSystem.current.SetSelectedGameObject((GameObject)null);
		JoystickHighlight.Display(state: false);
		HUDNavigationOn = false;
		this.HUDNavigationToggled?.Invoke(HUDNavigationOn);
		return true;
	}

	public void SelectPanel(HUDJoystickTarget.SelectionInfo selectionInfo, bool updateSelection = true)
	{
		if ((Object)(object)currentPanel != (Object)null)
		{
			currentPanel.RaiseDeselectEvent();
		}
		currentPanel = selectionInfo.HUDTarget;
		currentPanel.RaiseSelectEvent();
		if (updateSelection && (Object)(object)selectionInfo.Selectable != (Object)null)
		{
			EventSystem.current.SetSelectedGameObject(((Component)selectionInfo.Selectable).gameObject);
		}
	}

	public void SelectHUDDefaultPanel()
	{
		SelectPanel(firstHUDJoystickTarget.GetSelectionInfo());
	}

	public void RegisterSelectable(Selectable selectable, HUDJoystickTarget container)
	{
		HUDJoystickTarget value;
		if ((Object)(object)selectable == (Object)null)
		{
			((CLogger<HUDJoystickNavigationManager>)TPSingleton<HUDJoystickNavigationManager>.Instance).LogError((object)("Trying to register a null selectable on " + ((Object)((Component)container).transform).name + "! Aborting."), (CLogLevel)1, true, true);
		}
		else if (SelectablesContainerHUDTargets.TryGetValue(selectable, out value))
		{
			if ((Object)(object)container != (Object)(object)value)
			{
				((CLogger<HUDJoystickNavigationManager>)this).LogWarning((object)("Selectable " + ((Object)((Component)selectable).transform).name + " has already been registered with a different container (trying to register using " + ((Object)((Component)container).transform).name + ", already registered with " + ((Object)((Component)value).transform).name + ")."), (CLogLevel)1, true, false);
			}
		}
		else
		{
			SelectablesContainerHUDTargets.Add(selectable, container);
		}
	}

	public void UnregisterSelectable(Selectable selectable)
	{
		if (SelectablesContainerHUDTargets.ContainsKey(selectable))
		{
			SelectablesContainerHUDTargets.Remove(selectable);
		}
	}

	public void OnPopupExitToWorld()
	{
		if (InputManager.JoystickConfig.HUDNavigation.StayInHUDOnPopupExit && (ApplicationManager.Application.State.GetName() != "Game" || TPSingleton<GameManager>.Instance.Game.State != Game.E_State.PlaceUnit))
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectHUDDefaultPanel();
		}
		else
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		}
	}

	private void UpdatePanelForCurrentSelection()
	{
		if (!((Object)(object)currentPanel == (Object)null))
		{
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			Selectable key = default(Selectable);
			if ((Object)(object)currentSelectedGameObject != (Object)null && (Object)(object)previousSelection != (Object)(object)currentSelectedGameObject && InventorySlotToPlace == null && currentSelectedGameObject.TryGetComponent<Selectable>(ref key) && SelectablesContainerHUDTargets.TryGetValue(key, out var value) && (Object)(object)value != (Object)(object)currentPanel)
			{
				currentPanel.RaiseDeselectEvent();
				currentPanel = value;
				currentPanel.RaiseSelectEvent();
			}
		}
	}

	private void UpdateHUDNavigation()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (InventorySlotToPlace != null)
		{
			return;
		}
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(InputManager.GetAxis(85), InputManager.GetAxis(86));
		if (((Vector2)(ref previousPanelsNavigationInput)).magnitude <= InputManager.JoystickConfig.DefaultDeadZone && ((Vector2)(ref val)).magnitude > InputManager.JoystickConfig.DefaultDeadZone)
		{
			HUDJoystickTarget hUDJoystickTarget = (((Object)(object)currentPanel != (Object)null) ? currentPanel.GetNextPanelForDirection(val) : firstHUDJoystickTarget);
			if ((Object)(object)hUDJoystickTarget != (Object)null)
			{
				HUDJoystickTarget.SelectionInfo selectionInfo = hUDJoystickTarget.GetSelectionInfo(val);
				if ((Object)(object)selectionInfo.HUDTarget != (Object)(object)currentPanel)
				{
					SelectPanel(selectionInfo);
				}
			}
		}
		previousPanelsNavigationInput = val;
	}

	protected override void Awake()
	{
		base.Awake();
		if ((Object)(object)firstHUDJoystickTarget == (Object)null && ApplicationManager.Application.State.GetName() == "Game")
		{
			((CLogger<HUDJoystickNavigationManager>)this).LogError((object)"Missing firstHUDJoystickTarget reference! Trying to get it dynamically even though it may be a wrong panel!", (Object)(object)((Component)this).gameObject, (CLogLevel)1, true, true);
			firstHUDJoystickTarget = Object.FindObjectOfType<HUDJoystickTarget>();
		}
		if (InputManager.JoystickConfig.HUDNavigation.TooltipsToggledInit)
		{
			ToggleTooltips();
		}
	}

	protected override void OnDestroy()
	{
		((CLogger<HUDJoystickNavigationManager>)this).OnDestroy();
		this.TooltipsToggled = null;
		this.HUDNavigationToggled = null;
	}

	private void ToggleTooltips()
	{
		ShowTooltips = !ShowTooltips;
		this.TooltipsToggled?.Invoke(ShowTooltips);
	}

	public IEnumerator ToggleSlotSelectionCoroutine()
	{
		SlotSelectionToggleThisFrame = true;
		yield return SharedYields.WaitForEndOfFrame;
		SlotSelectionToggleThisFrame = false;
	}

	private void Update()
	{
		if ((InputManager.GetButtonDown(84) && (InputManager.JoystickConfig.HUDNavigation.CanLeaveHUDUsingEnterInput || !HUDNavigationOn)) || (InputManager.GetButtonDown(80) && HUDNavigationOn))
		{
			if (!HUDNavigationOn)
			{
				if (CanOpenHUDNavigationMode())
				{
					OpenHUDNavigationMode();
				}
			}
			else if (CanExitHUDNavigationMode())
			{
				ExitHUDNavigationMode();
			}
		}
		if (HUDNavigationOn)
		{
			UpdatePanelForCurrentSelection();
			UpdateHUDNavigation();
			previousSelection = EventSystem.current.currentSelectedGameObject;
		}
		if (InputManager.GetButtonDown(87))
		{
			ToggleTooltips();
		}
	}
}
