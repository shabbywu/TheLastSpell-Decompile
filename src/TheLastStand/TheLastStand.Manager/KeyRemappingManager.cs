using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Settings;
using TheLastStand.View.Settings.KeyRemapping;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Manager;

public class KeyRemappingManager : Manager<KeyRemappingManager>
{
	public struct RemappingInfo
	{
		public KeyRemappingBindingLineView BindingView;

		public int ActionId;

		public ControllerMap ControllerMap;

		public ActionElementMap ActionElementMap;

		public AxisRange AxisRange;
	}

	[SerializeField]
	private KeyCode[] unmappableKeyboardKeys;

	[SerializeField]
	private bool allowModifierKeys;

	[SerializeField]
	private bool displayJoystickMaps;

	private KeyRemappingView keyRemappingView;

	private bool conflictResolved;

	private bool conflictConfirmed;

	private InputMapper inputMapper;

	private RemappingInfo? pendingRemappingInfo;

	public bool DisplayJoystickMaps => displayJoystickMaps;

	public bool RemappingInProgress { get; private set; }

	public bool ResetAllInProgress { get; private set; }

	public static void Initialize()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Invalid comparison between Unknown and I4
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView = KeyRemappingViewAccessor.KeyRemappingView;
		if (TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.Initialized)
		{
			TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.RefreshTexts();
			TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.RebuildLayout();
			return;
		}
		((UnityEventBase)((Button)TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ConflictConfirmButton).onClick).RemoveAllListeners();
		((UnityEventBase)((Button)TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ConflictCancelButton).onClick).RemoveAllListeners();
		((UnityEventBase)((Button)TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllButton).onClick).RemoveAllListeners();
		((UnityEventBase)((Button)TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllConfirmButton).onClick).RemoveAllListeners();
		((UnityEventBase)((Button)TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllCancelButton).onClick).RemoveAllListeners();
		((UnityEvent)((Button)TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ConflictConfirmButton).onClick).AddListener(new UnityAction(TPSingleton<KeyRemappingManager>.Instance.OnConfirmConflictButtonClicked));
		((UnityEvent)((Button)TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ConflictCancelButton).onClick).AddListener(new UnityAction(TPSingleton<KeyRemappingManager>.Instance.OnCancelConflictButtonClicked));
		((UnityEvent)((Button)TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllButton).onClick).AddListener(new UnityAction(TPSingleton<KeyRemappingManager>.Instance.OnResetAllButtonClicked));
		((UnityEvent)((Button)TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllConfirmButton).onClick).AddListener(new UnityAction(TPSingleton<KeyRemappingManager>.Instance.OnConfirmResetAllButtonClicked));
		((UnityEvent)((Button)TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.ResetAllCancelButton).onClick).AddListener(new UnityAction(TPSingleton<KeyRemappingManager>.Instance.OnCancelResetAllButtonClicked));
		List<InputCategory> list = ReInput.mapping.ActionCategories.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].userAssignable)
			{
				continue;
			}
			KeyRemappingCategoryView keyRemappingCategoryView = TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.InstantiateInputCategory(list[i]);
			List<InputAction> list2 = ReInput.mapping.ActionsInCategory(list[i].id, true).ToList();
			for (int j = 0; j < list2.Count; j++)
			{
				if (!list2[j].userAssignable)
				{
					continue;
				}
				KeyRemappingBindingLineView keyRemappingBindingLineView = keyRemappingCategoryView.InstantiateInputBindingLine();
				InputActionType type = list2[j].type;
				if ((int)type != 0)
				{
					if ((int)type == 1)
					{
						keyRemappingBindingLineView.Initialize(list2[j], list[i].id, (AxisRange)0);
						keyRemappingBindingLineView.ActionRemapButtonPressed += TPSingleton<KeyRemappingManager>.Instance.OnActionRemapButtonClicked;
					}
				}
				else
				{
					keyRemappingBindingLineView.Initialize(list2[j], list[i].id, (AxisRange)1, list2[j].positiveDescriptiveName);
					keyRemappingBindingLineView.ActionRemapButtonPressed += TPSingleton<KeyRemappingManager>.Instance.OnActionRemapButtonClicked;
					keyRemappingBindingLineView = keyRemappingCategoryView.InstantiateInputBindingLine();
					keyRemappingBindingLineView.Initialize(list2[j], list[i].id, (AxisRange)2, list2[j].negativeDescriptiveName);
					keyRemappingBindingLineView.ActionRemapButtonPressed += TPSingleton<KeyRemappingManager>.Instance.OnActionRemapButtonClicked;
				}
			}
		}
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.RefreshTexts();
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.RebuildLayout();
		TPSingleton<KeyRemappingManager>.Instance.keyRemappingView.Initialized = true;
	}

	private IEnumerator CancelRemapping()
	{
		while (true)
		{
			InputMapper val = inputMapper;
			if (val != null && (int)val.status == 1)
			{
				if (InputManager.GetButtonDown(23))
				{
					inputMapper.Stop();
					break;
				}
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator ConfirmResetAllInput()
	{
		while (ResetAllInProgress)
		{
			if (InputManager.GetButtonDown(23))
			{
				OnCancelResetAllButtonClicked();
				break;
			}
			if (InputManager.GetButtonDown(66))
			{
				OnConfirmResetAllButtonClicked();
				break;
			}
			yield return null;
		}
	}

	public ControllerMap GetControllerMap(ControllerType type)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return TPSingleton<InputManager>.Instance.Player.controllers.maps.GetFirstMapInCategory(type, 0, 0);
	}

	private IEnumerator HandleInputMapperConflict(ConflictFoundEventData conflictFoundEventData)
	{
		conflictResolved = false;
		string text = string.Empty;
		ElementAssignmentConflictInfo val = conflictFoundEventData.conflicts[0];
		InputActionType type = ((ElementAssignmentConflictInfo)(ref val)).action.type;
		if ((int)type != 0)
		{
			if ((int)type == 1)
			{
				val = conflictFoundEventData.conflicts[0];
				text = ((ElementAssignmentConflictInfo)(ref val)).action.name;
			}
		}
		else
		{
			val = conflictFoundEventData.conflicts[0];
			string text2;
			if ((int)((ElementAssignmentConflictInfo)(ref val)).elementMap.axisContribution != 0)
			{
				val = conflictFoundEventData.conflicts[0];
				text2 = ((ElementAssignmentConflictInfo)(ref val)).action.negativeDescriptiveName;
			}
			else
			{
				val = conflictFoundEventData.conflicts[0];
				text2 = ((ElementAssignmentConflictInfo)(ref val)).action.positiveDescriptiveName;
			}
			text = text2;
		}
		KeyRemappingView obj = keyRemappingView;
		string conflictedActionName = text;
		val = conflictFoundEventData.conflicts[0];
		obj.DisplayConflictSolver(show: true, conflictedActionName, ((ElementAssignmentConflictInfo)(ref val)).keyCode);
		if (conflictFoundEventData.isProtected)
		{
			conflictFoundEventData.responseCallback((ConflictResponse)0);
		}
		else
		{
			conflictConfirmed = false;
			yield return (object)new WaitUntil((Func<bool>)(() => conflictResolved || InputManager.GetButtonDown(23)));
			conflictFoundEventData.responseCallback((ConflictResponse)(conflictConfirmed ? 1 : 0));
		}
		keyRemappingView.DisplayConflictSolver(show: false);
		keyRemappingView.DisplayResetAllOption(show: true);
	}

	public void ResetAll()
	{
		((CLogger<KeyRemappingManager>)TPSingleton<KeyRemappingManager>.Instance).Log((object)"Resetting all mapping settings.", (CLogLevel)1, false, false);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadDefaultMaps((ControllerType)0);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadDefaultMaps((ControllerType)1);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadDefaultMaps((ControllerType)2);
		SettingsController.SetKeyboardLayout(SettingsManager.GetKeyboardLayoutFromSystemLanguage());
		InputManager.RefreshRewiredKeyboardLayout();
		int keyboardLayoutId = InputManager.GetKeyboardLayoutId();
		((CLogger<KeyRemappingManager>)TPSingleton<KeyRemappingManager>.Instance).Log((object)$"Loading Rewired categories Default/World using Keyboard Layout Id {keyboardLayoutId} (= {(SettingsManager.E_KeyboardLayout)keyboardLayoutId})", (CLogLevel)1, false, false);
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadMap((ControllerType)0, 0, 0, InputManager.GetKeyboardLayoutId());
		TPSingleton<InputManager>.Instance.Player.controllers.maps.LoadMap((ControllerType)0, 0, 3, InputManager.GetKeyboardLayoutId());
	}

	private void Refresh()
	{
		keyRemappingView.RefreshBindingViews();
	}

	private void StartRemapper(RemappingInfo info)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		pendingRemappingInfo = info;
		inputMapper = new InputMapper();
		inputMapper.StartedEvent += OnInputMapperStartedEvent;
		inputMapper.StoppedEvent += OnInputMapperStoppedEvent;
		inputMapper.ConflictFoundEvent += OnInputMapperConflictFoundEvent;
		inputMapper.options.allowKeyboardKeysWithModifiers = allowModifierKeys;
		inputMapper.options.allowAxes = false;
		inputMapper.options.allowButtons = true;
		inputMapper.options.timeout = 36000f;
		inputMapper.options.isElementAllowedCallback = OnIsElementAllowed;
		Context val = new Context
		{
			actionId = info.ActionId,
			actionElementMapToReplace = info.ActionElementMap,
			controllerMap = info.ControllerMap,
			actionRange = info.AxisRange
		};
		inputMapper.Start(val);
		((MonoBehaviour)this).StartCoroutine(CancelRemapping());
	}

	private void OnInputMapperStartedEvent(StartedEventData startedEventData)
	{
		RemappingInProgress = true;
		keyRemappingView.DisplayResetAllOption(show: false);
	}

	private void OnInputMapperStoppedEvent(StoppedEventData stoppedEventData)
	{
		RemappingInProgress = false;
		pendingRemappingInfo?.BindingView.HideRemappingFeedback();
		pendingRemappingInfo = null;
		keyRemappingView.DisplayResetAllOption(show: true);
		keyRemappingView.AllowPanelNavigation(state: true);
		Refresh();
	}

	private void OnInputMapperConflictFoundEvent(ConflictFoundEventData conflictFoundEventData)
	{
		((MonoBehaviour)this).StartCoroutine(HandleInputMapperConflict(conflictFoundEventData));
	}

	private bool OnIsElementAllowed(ControllerPollingInfo info)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if ((int)((ControllerPollingInfo)(ref info)).controllerType == 0)
		{
			return unmappableKeyboardKeys.Count((KeyCode o) => o == ((ControllerPollingInfo)(ref info)).keyboardKey) == 0;
		}
		return true;
	}

	private void OnActionRemapButtonClicked(RemappingInfo infos)
	{
		if (!RemappingInProgress)
		{
			StartRemapper(infos);
			keyRemappingView.AllowPanelNavigation(state: false);
		}
	}

	private void OnCancelResetAllButtonClicked()
	{
		keyRemappingView.DisplayResetAllPanel(show: false);
		ResetAllInProgress = false;
	}

	private void OnConfirmResetAllButtonClicked()
	{
		ResetAll();
		Refresh();
		keyRemappingView.DisplayResetAllPanel(show: false);
		ResetAllInProgress = false;
	}

	private void OnCancelConflictButtonClicked()
	{
		conflictConfirmed = false;
		conflictResolved = true;
	}

	private void OnConfirmConflictButtonClicked()
	{
		conflictConfirmed = true;
		conflictResolved = true;
	}

	public void OnResetActionButtonClicked(int actionId, AxisRange axisRange)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		List<ActionElementMap> list = new List<ActionElementMap>();
		for (int num = ReInput.mapping.MapCategories.Count - 1; num >= 0; num--)
		{
			List<ActionElementMap> list2 = new List<ActionElementMap>();
			((ControllerMap)ReInput.mapping.GetKeyboardMapInstanceSavedOrDefault(TPSingleton<InputManager>.Instance.Player.id, num, InputManager.GetKeyboardLayoutId())).GetElementMapsWithAction(actionId, list2);
			list.AddRange(list2);
			((ControllerMap)ReInput.mapping.GetKeyboardMapInstanceSavedOrDefault(TPSingleton<InputManager>.Instance.Player.id, num, 0)).GetElementMapsWithAction(actionId, list2);
			list.AddRange(list2);
		}
		list.RemoveAll((ActionElementMap o) => ((int)axisRange == 1 && (int)o.axisContribution == 1) || ((int)axisRange == 2 && (int)o.axisContribution == 0));
		list.Select((ActionElementMap o) => o.keyCode);
		GetControllerMap((ControllerType)0).DeleteElementMapsWithAction(actionId);
		GetControllerMap((ControllerType)1).DeleteElementMapsWithAction(actionId);
	}

	private void OnResetAllButtonClicked()
	{
		ResetAllInProgress = true;
		keyRemappingView.DisplayResetAllPanel(show: true);
		((MonoBehaviour)this).StartCoroutine(ConfirmResetAllInput());
	}

	[DevConsoleCommand("KeyRemappingResetAll")]
	public static void DebugResetAll()
	{
		TPSingleton<KeyRemappingManager>.Instance.ResetAll();
	}

	[DevConsoleCommand("KeyRemappingSave")]
	public static void DebugSave()
	{
		InputManager.SaveMap();
	}

	[DevConsoleCommand("KeyRemappingInit")]
	public static void DebugInit()
	{
		Initialize();
	}

	[DevConsoleCommand("KeyRemappingShowKeyCodes")]
	public static void DebugShowKeyCodes(bool show = true)
	{
		Object.FindObjectsOfType<KeyRemappingBindingView>().ToList().ForEach(delegate(KeyRemappingBindingView o)
		{
			o.DebugShowRawKeyCode(show);
		});
	}
}
