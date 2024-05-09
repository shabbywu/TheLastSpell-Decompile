using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.View.Skill;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Item;

public class ItemCarouselEntryIconDisplay : MonoBehaviour
{
	[SerializeField]
	private RectTransform displayRectTransform;

	[SerializeField]
	private GameObject perkContainer;

	[SerializeField]
	private Image perkIcon;

	[SerializeField]
	private Vector2 perkDisplaySize;

	[SerializeField]
	private GameObject skillContainer;

	[SerializeField]
	private Image skillIcon;

	[SerializeField]
	private Vector2 skillDisplaySize;

	[SerializeField]
	private GameObject nextTextsSkillContainer;

	[SerializeField]
	private GameObject nextTextsPerkContainer;

	public PerkDefinition PerkDefinition { get; private set; }

	public SkillDefinition SkillDefinition { get; private set; }

	public void Refresh()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		perkContainer.SetActive(PerkDefinition != null);
		skillContainer.SetActive(SkillDefinition != null);
		if (PerkDefinition != null && (Object)(object)perkIcon != (Object)null)
		{
			perkIcon.sprite = PerkDefinition.PerkSprite;
			if ((Object)(object)displayRectTransform != (Object)null)
			{
				displayRectTransform.sizeDelta = perkDisplaySize;
			}
		}
		if (SkillDefinition != null && (Object)(object)skillIcon != (Object)null)
		{
			skillIcon.sprite = SkillView.GetIconSprite(SkillDefinition.ArtId);
			if ((Object)(object)displayRectTransform != (Object)null)
			{
				displayRectTransform.sizeDelta = skillDisplaySize;
			}
		}
	}

	public void SetContent(PerkDefinition perkDefinition)
	{
		PerkDefinition = perkDefinition;
		SkillDefinition = null;
	}

	public void SetContent(SkillDefinition skillDefinition)
	{
		SkillDefinition = skillDefinition;
		PerkDefinition = null;
	}

	public void ToggleNextElementLabel(bool mustDisplay, bool isNextEntryASkill)
	{
		if ((Object)(object)nextTextsSkillContainer != (Object)null && (Object)(object)nextTextsPerkContainer != (Object)null)
		{
			nextTextsSkillContainer.SetActive(mustDisplay && isNextEntryASkill);
			nextTextsPerkContainer.SetActive(mustDisplay && !isNextEntryASkill);
		}
	}
}
