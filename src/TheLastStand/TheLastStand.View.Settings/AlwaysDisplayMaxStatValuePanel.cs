using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class AlwaysDisplayMaxStatValuePanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle alwaysDisplayMaxStatValueToggle;

	public void OnValueChanged()
	{
		SettingsController.SetAlwaysDisplayMaxStatValue(((Toggle)alwaysDisplayMaxStatValueToggle).isOn);
	}

	public void Refresh()
	{
		((Toggle)alwaysDisplayMaxStatValueToggle).isOn = TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayMaxStatValue;
	}
}
