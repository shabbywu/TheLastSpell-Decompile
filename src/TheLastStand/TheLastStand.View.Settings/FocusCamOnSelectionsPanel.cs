using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class FocusCamOnSelectionsPanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle focusCamOnSelectionsToggle;

	public void OnValueChanged()
	{
		SettingsController.SetFocusCamOnSelections(!((Toggle)focusCamOnSelectionsToggle).isOn);
	}

	public void Refresh()
	{
		((Toggle)focusCamOnSelectionsToggle).isOn = !TPSingleton<SettingsManager>.Instance.Settings.FocusCamOnSelections;
	}
}
