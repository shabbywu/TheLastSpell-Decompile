using System.Collections;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.View.HUD;

public class JoystickHighlighter : MonoBehaviour
{
	[SerializeField]
	private bool waitNextFrame;

	[SerializeField]
	private E_GamepadButtonType gamepadButtonTypes;

	[SerializeField]
	public bool HideButtons;

	[SerializeField]
	private RectTransform overrideTargetRect;

	[SerializeField]
	private UnityEvent onHighlight = new UnityEvent();

	[SerializeField]
	private UnityEvent onUnhighlight = new UnityEvent();

	public RectTransform RectTransform
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)overrideTargetRect))
			{
				Transform transform = ((Component)this).transform;
				return (RectTransform)(object)((transform is RectTransform) ? transform : null);
			}
			return overrideTargetRect;
		}
	}

	public E_GamepadButtonType GamepadButtonTypes
	{
		get
		{
			if (!HideButtons)
			{
				return gamepadButtonTypes;
			}
			return E_GamepadButtonType.NONE;
		}
	}

	public void OnHighlight()
	{
		if (InputManager.IsLastControllerJoystick && TPSingleton<SettingsManager>.Instance.Settings.InputDeviceType != SettingsManager.E_InputDeviceType.MouseKeyboard)
		{
			if (waitNextFrame)
			{
				((MonoBehaviour)TPSingleton<UIManager>.Instance).StartCoroutine(HighlightEndOfFrame());
			}
			else
			{
				Highlight();
			}
		}
	}

	public void OnUnhighlight()
	{
		onUnhighlight.Invoke();
	}

	private void Highlight()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.FollowHighlighter(this);
		onHighlight.Invoke();
	}

	private IEnumerator HighlightEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		Highlight();
	}
}
