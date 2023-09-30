using Rewired;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View;

public class AnimatedSceneSkipText : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI keyboardText;

	[SerializeField]
	private TextMeshProUGUI controllerText;

	[SerializeField]
	private string keyboardLocalizationKey = string.Empty;

	[SerializeField]
	private string controllerLocalizationKey = string.Empty;

	private void Awake()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<InputManager>.Instance.LastActiveControllerChanged += OnLastActiveControllerChanged;
		OnLastActiveControllerChanged(InputManager.GetLastControllerType());
	}

	private void OnDestroy()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		}
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		((Component)keyboardText).gameObject.SetActive((int)controllerType != 2);
		((Component)controllerText).gameObject.SetActive((int)controllerType == 2);
		if ((int)controllerType == 2)
		{
			((TMP_Text)controllerText).text = Localizer.Get(controllerLocalizationKey);
		}
		else
		{
			((TMP_Text)keyboardText).text = Localizer.Get(keyboardLocalizationKey);
		}
	}
}
