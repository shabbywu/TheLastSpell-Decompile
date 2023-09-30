using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip.Tooltip.Compendium;
using UnityEngine;

namespace TheLastStand.View.Item;

public class ItemTooltipSkillCycle : MonoBehaviour
{
	[SerializeField]
	private ItemTooltip[] itemTooltips;

	[SerializeField]
	private CompendiumPanel compendiumPanel;

	[SerializeField]
	private FollowElement followElement;

	private int tooltipsCount;

	public int SkillTabIndex { get; private set; }

	public bool CompendiumFollowRight { get; set; } = true;


	public CompendiumPanel CompendiumPanel => compendiumPanel;

	public void Reset()
	{
		SkillTabIndex = 0;
		for (int i = 0; i < tooltipsCount; i++)
		{
			if (itemTooltips[i].Displayed)
			{
				itemTooltips[i].Refresh();
			}
		}
	}

	public void UpdateCompendium()
	{
		CompendiumPanel.Clear();
		List<ItemTooltip> list = new List<ItemTooltip>();
		for (int i = 0; i < tooltipsCount; i++)
		{
			if (itemTooltips[i].Displayed && itemTooltips[i].Item != null && itemTooltips[i].Item.Skills != null)
			{
				list.Add(itemTooltips[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].AddAttackTypeEntries();
		}
		for (int k = 0; k < list.Count; k++)
		{
			list[k].AddSkillEffectEntries();
		}
		bool flag = compendiumPanel.CompendiumEntries.Count > 0;
		if (flag && !TPSingleton<SettingsManager>.Instance.Settings.HideCompendium)
		{
			compendiumPanel.Display();
		}
		((Behaviour)followElement).enabled = flag;
	}

	private void Awake()
	{
		tooltipsCount = itemTooltips.Length;
		for (int i = 0; i < tooltipsCount; i++)
		{
			itemTooltips[i].ItemTooltipSkillCycle = this;
		}
	}

	private void Update()
	{
		bool flag = false;
		if (InputManager.GetButtonDown(48))
		{
			for (int i = 0; i < tooltipsCount; i++)
			{
				if (itemTooltips[i].Item != null && itemTooltips[i].Item.Skills != null)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			int skillTabIndex = SkillTabIndex;
			SkillTabIndex++;
			bool flag2 = true;
			for (int j = 0; j < tooltipsCount; j++)
			{
				if (itemTooltips[j].Displayed && itemTooltips[j].Item != null && itemTooltips[j].Item.Skills != null && SkillTabIndex < itemTooltips[j].Item.Skills.Count)
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				SkillTabIndex = 0;
			}
			if (SkillTabIndex != skillTabIndex)
			{
				CompendiumPanel.Clear();
			}
			for (int k = 0; k < tooltipsCount; k++)
			{
				if (itemTooltips[k].Displayed)
				{
					itemTooltips[k].Refresh();
				}
			}
		}
		if (SkillTabIndex == 0)
		{
			return;
		}
		bool flag3 = true;
		for (int l = 0; l < tooltipsCount; l++)
		{
			if (itemTooltips[l].Displayed)
			{
				flag3 = false;
				break;
			}
		}
		if (flag3)
		{
			SkillTabIndex = 0;
			CompendiumPanel.Clear();
		}
	}
}
