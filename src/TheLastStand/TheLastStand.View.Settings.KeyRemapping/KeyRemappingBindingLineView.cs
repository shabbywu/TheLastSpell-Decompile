using System;
using System.Collections.Generic;
using Rewired;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Settings.KeyRemapping;

public class KeyRemappingBindingLineView : MonoBehaviour
{
	private static class Constants
	{
		public const string GameObjectNamePrefix = "Action Panel - ";
	}

	[SerializeField]
	private TextMeshProUGUI actionNameText;

	[SerializeField]
	private LocalizedFont actionNameLocalizedFont;

	[SerializeField]
	private KeyRemappingBindingView[] bindingButtons;

	private readonly List<ActionElementMap> actionElementMaps = new List<ActionElementMap>();

	private readonly List<ControllerMap> controllerMaps = new List<ControllerMap>();

	private string overrideName;

	public AxisRange AxisRange { get; private set; }

	public int CategoryId { get; private set; }

	public InputAction InputAction { get; private set; }

	public event Action<KeyRemappingManager.RemappingInfo> ActionRemapButtonPressed;

	public void HideRemappingFeedback()
	{
		for (int num = bindingButtons.Length - 1; num >= 0; num--)
		{
			bindingButtons[num].DisplayRemappingFeedback(show: false);
			bindingButtons[num].Highlight(state: false);
		}
	}

	public void Initialize(InputAction action, int categoryId, AxisRange axisRange = 0, string overrideName = null)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		((Object)((Component)this).transform).name = "Action Panel - " + action.name;
		InputAction = action;
		CategoryId = categoryId;
		AxisRange = axisRange;
		this.overrideName = overrideName;
		Refresh();
	}

	public void Refresh()
	{
		actionElementMaps.Clear();
		controllerMaps.Clear();
		for (int i = 0; i < bindingButtons.Length; i++)
		{
			((UnityEventBase)((Button)bindingButtons[i].Button).onClick).RemoveAllListeners();
		}
		InitializeBindingsWithAllMaps();
		HideRemappingFeedback();
		RefreshTexts();
	}

	public void RefreshTexts()
	{
		((TMP_Text)actionNameText).SetText(Localizer.Get("KeyRemapping_ActionName_" + (overrideName ?? InputAction.name)), true);
		if ((Object)(object)actionNameLocalizedFont != (Object)null)
		{
			actionNameLocalizedFont.RefreshFont();
		}
		for (int i = 0; i < bindingButtons.Length; i++)
		{
			bindingButtons[i].RefreshLocalizedKeyCode();
		}
	}

	private ActionElementMap GetActionElementMap(int index)
	{
		if (index < actionElementMaps.Count)
		{
			return actionElementMaps[index];
		}
		return null;
	}

	private ControllerMap GetControllerMap(int index)
	{
		if (controllerMaps.Count != 0)
		{
			return controllerMaps[Mathf.Clamp(index, 0, controllerMaps.Count - 1)];
		}
		return TPSingleton<KeyRemappingManager>.Instance.GetControllerMap((ControllerType)0);
	}

	private void InitializeBindingsWithAllMaps()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Invalid comparison between Unknown and I4
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Invalid comparison between Unknown and I4
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Invalid comparison between Unknown and I4
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Invalid comparison between Unknown and I4
		List<ActionElementMap> list = new List<ActionElementMap>();
		foreach (ControllerMap allMap in TPSingleton<InputManager>.Instance.Player.controllers.maps.GetAllMaps())
		{
			if ((int)allMap.controllerType == 2 && !TPSingleton<KeyRemappingManager>.Instance.DisplayJoystickMaps)
			{
				continue;
			}
			List<ActionElementMap> list2 = new List<ActionElementMap>();
			allMap.GetButtonMapsWithAction(InputAction.id, list2);
			foreach (ActionElementMap item in list2)
			{
				if ((int)InputAction.type == 1 || ((int)AxisRange == 1 && (int)item.axisContribution == 0) || ((int)AxisRange == 2 && (int)item.axisContribution == 1))
				{
					list.Add(item);
					actionElementMaps.Add(item);
					controllerMaps.Add(item.controllerMap);
				}
			}
		}
		for (int i = 0; i < bindingButtons.Length; i++)
		{
			if (i < list.Count)
			{
				bindingButtons[i].SetKeyCode(list[i].keyCode);
				bindingButtons[i].RefreshText(list[i].elementIdentifierName);
			}
			else
			{
				bindingButtons[i].SetKeyCode((KeyCode)0);
				bindingButtons[i].RefreshText(string.Empty);
			}
			int elementMapId = i;
			KeyRemappingManager.RemappingInfo remappingInfos = new KeyRemappingManager.RemappingInfo
			{
				ActionId = InputAction.id,
				ActionElementMap = GetActionElementMap(elementMapId),
				ControllerMap = GetControllerMap(elementMapId),
				AxisRange = AxisRange,
				BindingView = this
			};
			((UnityEvent)((Button)bindingButtons[i].Button).onClick).AddListener((UnityAction)delegate
			{
				if (!TPSingleton<KeyRemappingManager>.Instance.RemappingInProgress)
				{
					this.ActionRemapButtonPressed?.Invoke(remappingInfos);
					bindingButtons[elementMapId].DisplayRemappingFeedback(show: true);
				}
			});
		}
	}

	[ContextMenu("Log Default Key")]
	private void LogDefaultKey()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<KeyRemappingManager>.Instance.OnResetActionButtonClicked(InputAction.id, AxisRange);
	}
}
