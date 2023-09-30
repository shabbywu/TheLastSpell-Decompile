using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class ShowSkillsHotkeysPanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle showSkillsHotkeysToggle;

	public void OnValueChanged()
	{
		SettingsController.SetShowSkillsHotkeys(((Toggle)showSkillsHotkeysToggle).isOn);
	}

	public void Refresh()
	{
		((Toggle)showSkillsHotkeysToggle).isOn = TPSingleton<SettingsManager>.Instance.Settings.ShowSkillsHotkeys;
	}
}
