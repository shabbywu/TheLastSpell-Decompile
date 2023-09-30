using Rewired;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Menus;

public class MainMenuFeedbackButton : MonoBehaviour
{
	[SerializeField]
	private Button button;

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		((Selectable)button).interactable = (int)controllerType != 2;
	}

	private void Awake()
	{
		TPSingleton<InputManager>.Instance.LastActiveControllerChanged += OnLastActiveControllerChanged;
	}

	private void OnDestroy()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		}
	}
}
