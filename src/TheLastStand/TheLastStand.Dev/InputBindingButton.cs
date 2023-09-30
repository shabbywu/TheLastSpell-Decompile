using System;
using System.Collections.Generic;
using Rewired;
using TMPro;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Dev;

[DisallowMultipleComponent]
public class InputBindingButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI actionName;

	[SerializeField]
	private BetterButton[] buttons;

	[SerializeField]
	private BetterButton resetActionButton;

	[SerializeField]
	private GameObject remappingFeedback;

	private List<ActionElementMap> actionElementMaps = new List<ActionElementMap>();

	private List<ControllerMap> maps = new List<ControllerMap>();

	public AxisRange AxisRange { get; private set; }

	public int CategoryId { get; private set; }

	public int ElementMapId { get; private set; }

	public InputAction InputAction { get; private set; }

	public event Action<KeyRemappingManager.RemappingInfo> ActionRemapButtonPressed;

	public event Action<int> ActionResetButtonPressed;

	public void DisplayRemappingFeedback(bool show)
	{
		remappingFeedback.SetActive(show);
	}

	public ActionElementMap GetActionElementMap(int index)
	{
		if (index >= actionElementMaps.Count)
		{
			return null;
		}
		return actionElementMaps[index];
	}

	public ControllerMap GetControllerMap(int index)
	{
		if (maps.Count == 0)
		{
			return TPSingleton<KeyRemappingManager>.Instance.GetControllerMap((ControllerType)0);
		}
		return maps[Mathf.Clamp(index, 0, maps.Count - 1)];
	}

	public void Init(InputAction action, int categoryId, AxisRange axisRange = 0, string overrideDescriptiveName = null)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		actionElementMaps.Clear();
		maps.Clear();
		InputAction = action;
		CategoryId = categoryId;
		AxisRange = axisRange;
		((TMP_Text)actionName).SetText("KeyRemapping_ActionName_" + (overrideDescriptiveName ?? InputAction.name), true);
		InitWithAllMaps();
		((UnityEvent)((Button)resetActionButton).onClick).AddListener((UnityAction)delegate
		{
			this.ActionResetButtonPressed?.Invoke(InputAction.id);
		});
	}

	private void InitWithAllMaps()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Invalid comparison between Unknown and I4
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Invalid comparison between Unknown and I4
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Invalid comparison between Unknown and I4
		List<ActionElementMap> list = new List<ActionElementMap>();
		foreach (ControllerMap allMap in TPSingleton<InputManager>.Instance.Player.controllers.maps.GetAllMaps())
		{
			List<ActionElementMap> list2 = new List<ActionElementMap>();
			allMap.GetButtonMapsWithAction(InputAction.id, list2);
			foreach (ActionElementMap item in list2)
			{
				if ((int)InputAction.type == 1 || ((int)AxisRange == 1 && (int)item.axisContribution == 0) || ((int)AxisRange == 2 && (int)item.axisContribution == 1))
				{
					list.Add(item);
					actionElementMaps.Add(item);
					maps.Add(item.controllerMap);
				}
			}
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].ChangeText(string.Empty);
			if (i < list.Count)
			{
				buttons[i].ChangeText(list[i].elementIdentifierName);
			}
			int index = i;
			KeyRemappingManager.RemappingInfo remappingInfos = new KeyRemappingManager.RemappingInfo
			{
				ActionId = InputAction.id,
				ActionElementMap = GetActionElementMap(index),
				ControllerMap = GetControllerMap(index),
				AxisRange = AxisRange,
				BindingView = null
			};
			((UnityEvent)((Button)buttons[i]).onClick).AddListener((UnityAction)delegate
			{
				this.ActionRemapButtonPressed?.Invoke(remappingInfos);
			});
		}
	}
}
