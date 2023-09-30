using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class EdgePanOverUIPanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle edgePanOverUIToggle;

	public void OnValueChanged()
	{
		SettingsController.SetEdgePanOverUI(((Toggle)edgePanOverUIToggle).isOn);
	}

	public void Refresh()
	{
		((Toggle)edgePanOverUIToggle).isOn = TPSingleton<SettingsManager>.Instance.Settings.EdgePanOverUI;
	}
}
