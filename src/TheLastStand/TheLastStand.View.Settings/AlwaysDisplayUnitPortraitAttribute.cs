using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class AlwaysDisplayUnitPortraitAttribute : MonoBehaviour
{
	[SerializeField]
	private BetterToggle alwaysDisplayUnitPortraitAttributeToggle;

	public void OnValueChanged()
	{
		SettingsController.SetAlwaysDisplayUnitPortraitAttribute(((Toggle)alwaysDisplayUnitPortraitAttributeToggle).isOn);
	}

	public void Refresh()
	{
		((Toggle)alwaysDisplayUnitPortraitAttributeToggle).isOn = TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayUnitPortraitAttribute;
	}
}
