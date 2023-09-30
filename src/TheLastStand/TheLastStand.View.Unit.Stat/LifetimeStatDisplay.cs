using TMPro;
using TheLastStand.View.HUD;
using UnityEngine;

namespace TheLastStand.View.Unit.Stat;

public class LifetimeStatDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI lifetimeStatName;

	[SerializeField]
	private TextMeshProUGUI lifetimeStatValue;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public void Refresh(string statName, string statValue)
	{
		((TMP_Text)lifetimeStatName).text = statName;
		((TMP_Text)lifetimeStatValue).text = statValue;
	}
}
