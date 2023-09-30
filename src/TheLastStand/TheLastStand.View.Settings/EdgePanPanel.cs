using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class EdgePanPanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle edgePanToggle;

	public void OnValueChanged()
	{
		SettingsController.SetEdgePan(((Toggle)edgePanToggle).isOn);
	}

	public void Refresh()
	{
		((Toggle)edgePanToggle).isOn = TPSingleton<SettingsManager>.Instance.Settings.EdgePan;
	}
}
