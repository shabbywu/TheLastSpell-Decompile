using Rewired;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Unit.Perk;

public class PerkBookmarkInfo : MonoBehaviour
{
	[SerializeField]
	private GameObject keyboardTextContainer;

	[SerializeField]
	private GameObject controllerTextContainer;

	private void OnEnable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		OnLastActiveControllerChanged(InputManager.GetLastControllerType());
	}

	private void OnDisable()
	{
		InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		controllerTextContainer.SetActive((int)controllerType == 2);
		keyboardTextContainer.SetActive((int)controllerType != 2);
	}
}
