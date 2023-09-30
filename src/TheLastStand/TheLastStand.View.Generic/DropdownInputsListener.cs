using TPLib;
using TheLastStand.Framework.UI.TMPro;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.View.Generic;

[RequireComponent(typeof(TMP_BetterDropdown))]
public class DropdownInputsListener : MonoBehaviour
{
	private TMP_BetterDropdown dropdown;

	public TMP_BetterDropdown Dropdown
	{
		get
		{
			if ((Object)(object)dropdown == (Object)null)
			{
				dropdown = ((Component)this).GetComponent<TMP_BetterDropdown>();
			}
			return dropdown;
		}
	}

	private void Hide()
	{
		Dropdown.Hide();
		if (TPSingleton<InputManager>.Exist())
		{
			InputManager.OnDropdownClose();
		}
	}

	private void OnDisable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		Dropdown.OnDropDownOpened.RemoveListener(new UnityAction(OnDropdownOpen));
		Dropdown.OnDropDownClosed.RemoveListener(new UnityAction(OnDropdownClose));
	}

	private void OnEnable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		Dropdown.OnDropDownOpened.AddListener(new UnityAction(OnDropdownOpen));
		Dropdown.OnDropDownClosed.AddListener(new UnityAction(OnDropdownClose));
	}

	private void OnDropdownOpen()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			InputManager.OnDropdownOpen();
		}
	}

	private void OnDropdownClose()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			InputManager.OnDropdownClose();
		}
	}

	private void Update()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			if (InputManager.GetButtonDown(63) && Dropdown.Displayed)
			{
				Hide();
			}
			if (InputManager.GetButtonDown(64) && Dropdown.Displayed)
			{
				Dropdown.SelectHoveredItem();
				Hide();
			}
		}
	}
}
