using System.Collections.Generic;
using Sirenix.OdinInspector;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Perk;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Item;

public class ItemCarouselEntryListDisplay : SerializedMonoBehaviour
{
	[SerializeField]
	private ItemCarouselEntryIconDisplay carouselEntryIconDisplayPrefab;

	[SerializeField]
	private HorizontalLayoutGroup layout;

	private List<ItemCarouselEntryIconDisplay> carouselEntriesDisplays = new List<ItemCarouselEntryIconDisplay>();

	public void DisplayElement(int elementIndex, bool show)
	{
		((Component)carouselEntriesDisplays[elementIndex]).gameObject.SetActive(show);
	}

	public void SetContent(List<TheLastStand.Model.Skill.Skill> skills, List<Perk> perks)
	{
		int i = 0;
		int num = 0;
		int num2 = 0;
		if (skills != null)
		{
			num = skills.Count;
		}
		if (perks != null)
		{
			num2 = perks.Count;
		}
		int num3 = num + num2;
		if (num3 > 0)
		{
			for (; i < num3; i++)
			{
				while (carouselEntriesDisplays.Count <= i)
				{
					ItemCarouselEntryIconDisplay item = Object.Instantiate<ItemCarouselEntryIconDisplay>(carouselEntryIconDisplayPrefab, ((Component)layout).transform);
					carouselEntriesDisplays.Add(item);
				}
				if (i < num)
				{
					carouselEntriesDisplays[i].SetContent(skills[i].SkillDefinition);
				}
				else
				{
					int index = 0;
					if (num > 0)
					{
						index = i - num;
					}
					carouselEntriesDisplays[i].SetContent(perks[index].PerkDefinition);
				}
				carouselEntriesDisplays[i].Refresh();
				((Component)carouselEntriesDisplays[i]).gameObject.SetActive(true);
			}
		}
		for (; i < carouselEntriesDisplays.Count; i++)
		{
			((Component)carouselEntriesDisplays[i]).gameObject.SetActive(false);
		}
	}

	private void Awake()
	{
		TPHelpers.DestroyChildren(((Component)layout).transform);
	}
}
