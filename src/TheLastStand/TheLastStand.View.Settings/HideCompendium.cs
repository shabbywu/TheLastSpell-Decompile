using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class HideCompendium : MonoBehaviour
{
	[SerializeField]
	private BetterToggle hideCompendiumToggle;

	public void OnValueChanged()
	{
		SettingsController.SetHideCompendium(((Toggle)hideCompendiumToggle).isOn);
	}

	public void Refresh()
	{
		((Toggle)hideCompendiumToggle).isOn = TPSingleton<SettingsManager>.Instance.Settings.HideCompendium;
	}
}
