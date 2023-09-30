using TheLastStand.Manager;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Recruitment;

public abstract class RecruitDisplay : MonoBehaviour
{
	[SerializeField]
	protected GamepadInputDisplay previousUnitInput;

	[SerializeField]
	protected GamepadInputDisplay nextUnitInput;

	[SerializeField]
	public bool HidePreviousUnitInput;

	[SerializeField]
	public bool HideNextUnitInput;

	public abstract Toggle Toggle { get; }

	private void Awake()
	{
		((UnityEvent<bool>)(object)Toggle.onValueChanged).AddListener((UnityAction<bool>)OnUnitToggleValueChanged);
	}

	private void OnDestroy()
	{
		((UnityEvent<bool>)(object)Toggle.onValueChanged).RemoveListener((UnityAction<bool>)OnUnitToggleValueChanged);
	}

	protected virtual void OnUnitToggleValueChanged(bool value)
	{
		if (InputManager.IsLastControllerJoystick)
		{
			if (value)
			{
				previousUnitInput.Display(!HidePreviousUnitInput);
				nextUnitInput.Display(!HideNextUnitInput);
			}
			else
			{
				previousUnitInput.Display(show: false);
				nextUnitInput.Display(show: false);
			}
		}
	}
}
