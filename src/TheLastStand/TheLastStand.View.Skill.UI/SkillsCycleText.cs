using Rewired;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class SkillsCycleText : MonoBehaviour
{
	[SerializeField]
	private GameObject keyboardTextContainer;

	[SerializeField]
	private GameObject controllerTextContainer;

	private void OnEnable()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<InputManager>.Instance.LastActiveControllerChanged += OnLastActiveControllerChanged;
		OnLastActiveControllerChanged(InputManager.GetLastControllerType());
	}

	private void OnDisable()
	{
		TPSingleton<InputManager>.Instance.LastActiveControllerChanged -= OnLastActiveControllerChanged;
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
