using TMPro;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Race;

public class UnitRaceDisplay : MonoBehaviour
{
	[SerializeField]
	private Image raceIcon;

	[SerializeField]
	private TextMeshProUGUI raceName;

	[SerializeField]
	private TextMeshProUGUI raceDescription;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	private bool isIconInHoveredState;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public RaceDefinition RaceDefinition { get; private set; }

	public void SetContent(RaceDefinition raceDefinition)
	{
		RaceDefinition = raceDefinition;
	}

	public void SetIconInHoveredState(bool isHovered)
	{
		isIconInHoveredState = isHovered;
	}

	public void Refresh()
	{
		if (RaceDefinition != null)
		{
			if ((Object)(object)raceIcon != (Object)null)
			{
				raceIcon.sprite = (isIconInHoveredState ? RaceDefinition.RaceHoveredSprite : RaceDefinition.RaceSprite);
			}
			if ((Object)(object)raceName != (Object)null)
			{
				((TMP_Text)raceName).text = RaceDefinition.Name;
			}
			if ((Object)(object)raceDescription != (Object)null)
			{
				((TMP_Text)raceDescription).text = RaceDefinition.GetDescription();
			}
		}
	}
}
