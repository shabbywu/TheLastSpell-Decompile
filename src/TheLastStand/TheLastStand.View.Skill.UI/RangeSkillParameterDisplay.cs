using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class RangeSkillParameterDisplay : SkillParameterDisplay
{
	[SerializeField]
	private Image cardinalDirectionOnlyDisplay;

	[SerializeField]
	private Image modifiableDisplay;

	[SerializeField]
	private HorizontalLayoutGroup layoutGroup;

	[SerializeField]
	private int rightPaddingWithoutIcons = 50;

	[SerializeField]
	private int rightPaddingWithIcons = 20;

	public void Refresh(string effectName, string effectValue, bool isCardinalDirectionOnly, bool isModifiable, string overrideSign = "")
	{
		Refresh(effectName, effectValue, overrideSign);
		((Component)cardinalDirectionOnlyDisplay).gameObject.SetActive(isCardinalDirectionOnly);
		((Component)modifiableDisplay).gameObject.SetActive(isModifiable);
		if (!isCardinalDirectionOnly && !isModifiable)
		{
			((LayoutGroup)layoutGroup).padding.right = rightPaddingWithoutIcons;
		}
		else
		{
			((LayoutGroup)layoutGroup).padding.right = rightPaddingWithIcons;
		}
	}
}
