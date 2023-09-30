using System.Collections.Generic;
using Sirenix.OdinInspector;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class SkillListDisplay : SerializedMonoBehaviour
{
	[SerializeField]
	private SkillDisplay skillDisplayPrefab;

	[SerializeField]
	private HorizontalLayoutGroup layout;

	private List<SkillDisplay> skillDisplays = new List<SkillDisplay>();

	public void DisplaySkill(int skillIndex, bool show)
	{
		((Component)skillDisplays[skillIndex]).gameObject.SetActive(show);
	}

	public void SetSkills(List<TheLastStand.Model.Skill.Skill> skills, TheLastStand.Model.Unit.Unit ownerUnit, SkillTooltip skillTooltip = null)
	{
		int i = 0;
		if (skills != null)
		{
			for (; i < skills.Count; i++)
			{
				while (skillDisplays.Count <= i)
				{
					SkillDisplay item = Object.Instantiate<SkillDisplay>(skillDisplayPrefab, ((Component)layout).transform);
					skillDisplays.Add(item);
				}
				skillDisplays[i].Init(skillTooltip);
				skillDisplays[i].Skill = skills[i];
				skillDisplays[i].SkillOwner = ownerUnit;
				skillDisplays[i].Refresh();
				((Component)skillDisplays[i]).gameObject.SetActive(true);
			}
		}
		for (; i < skillDisplays.Count; i++)
		{
			((Component)skillDisplays[i]).gameObject.SetActive(false);
		}
	}

	private void Awake()
	{
		TPHelpers.DestroyChildren(((Component)layout).transform);
	}
}
