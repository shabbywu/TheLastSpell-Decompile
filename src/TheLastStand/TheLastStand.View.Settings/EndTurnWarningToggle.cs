using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class EndTurnWarningToggle : MonoBehaviour
{
	[SerializeField]
	private BetterToggle toggle;

	[SerializeField]
	private SettingsManager.E_EndTurnWarning warningType;

	public void OnValueChanged()
	{
		SettingsController.ToggleTurnEndWarning(warningType, ((Toggle)toggle).isOn);
	}

	public void Refresh()
	{
		((Toggle)toggle).isOn = TPSingleton<SettingsManager>.Instance.Settings.EndTurnWarnings[(int)warningType];
	}
}
