using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Skill;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Serialization.Item;
using UnityEngine;

namespace TheLastStand.Controller.Item;

public class ItemController
{
	public TheLastStand.Model.Item.Item Item { get; }

	public ItemController(SerializedItem container, ItemSlot itemSlot)
	{
		Item = new TheLastStand.Model.Item.Item(container, this, itemSlot);
	}

	public ItemController(ItemDefinition itemDefinition, int level, ItemDefinition.E_Rarity rarity)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		TheLastStand.Model.Item.Item obj = new TheLastStand.Model.Item.Item(itemDefinition, this)
		{
			Level = level,
			Rarity = rarity
		};
		Vector2Int resistance = itemDefinition.Resistance;
		int x = ((Vector2Int)(ref resistance)).x;
		resistance = itemDefinition.Resistance;
		int resistance2;
		if (x != ((Vector2Int)(ref resistance)).y)
		{
			resistance = itemDefinition.Resistance;
			int x2 = ((Vector2Int)(ref resistance)).x;
			resistance = itemDefinition.Resistance;
			resistance2 = RandomManager.GetRandomRange(this, x2, ((Vector2Int)(ref resistance)).y + 1);
		}
		else
		{
			resistance = itemDefinition.Resistance;
			resistance2 = ((Vector2Int)(ref resistance)).x;
		}
		obj.Resistance = resistance2;
		Item = obj;
		if (Item.SkillsOverallUses == null)
		{
			return;
		}
		foreach (KeyValuePair<string, int> skillsOverallUse in Item.SkillsOverallUses)
		{
			if (SkillDatabase.SkillDefinitions.ContainsKey(skillsOverallUse.Key))
			{
				Item.Skills.Add(new SkillController(SkillDatabase.SkillDefinitions[skillsOverallUse.Key], Item, skillsOverallUse.Value, SkillDatabase.SkillDefinitions[skillsOverallUse.Key].UsesPerTurnCount).Skill);
			}
		}
	}

	public ItemController(TheLastStand.Model.Item.Item itemToCopy)
	{
		Item = new TheLastStand.Model.Item.Item(itemToCopy.ItemDefinition, this)
		{
			Level = itemToCopy.Level,
			Rarity = itemToCopy.Rarity,
			AdditionalAffixes = itemToCopy.AdditionalAffixes,
			AdditionalAffixesMalus = itemToCopy.AdditionalAffixesMalus,
			Resistance = itemToCopy.Resistance
		};
		if (Item.SkillsOverallUses == null)
		{
			return;
		}
		foreach (KeyValuePair<string, int> skillsOverallUse in Item.SkillsOverallUses)
		{
			if (SkillDatabase.SkillDefinitions.ContainsKey(skillsOverallUse.Key))
			{
				Item.Skills.Add(new SkillController(SkillDatabase.SkillDefinitions[skillsOverallUse.Key], Item, skillsOverallUse.Value, SkillDatabase.SkillDefinitions[skillsOverallUse.Key].UsesPerTurnCount).Skill);
			}
		}
	}

	public Dictionary<UnitStatDefinition.E_Stat, float> MergeAffixes(IEnumerable<IAffix> affixes)
	{
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = new Dictionary<UnitStatDefinition.E_Stat, float>();
		foreach (IAffix affix in affixes)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> finalStatModifier in affix.GetFinalStatModifiers())
			{
				if (dictionary.ContainsKey(finalStatModifier.Key))
				{
					dictionary[finalStatModifier.Key] += finalStatModifier.Value;
				}
				else
				{
					dictionary.Add(finalStatModifier.Key, finalStatModifier.Value);
				}
			}
		}
		return dictionary;
	}

	public Dictionary<UnitStatDefinition.E_Stat, float> MergeAllAffixes()
	{
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = MergeAffixes(Item.AdditionalAffixes);
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item in MergeAffixes(Item.AdditionalAffixesMalus))
		{
			if (dictionary.ContainsKey(item.Key))
			{
				dictionary[item.Key] -= item.Value;
			}
			else
			{
				dictionary.Add(item.Key, 0f - item.Value);
			}
		}
		return dictionary;
	}

	public void RefillOverallUses()
	{
		for (int num = Item.Skills.Count - 1; num >= 0; num--)
		{
			if (Item.Skills[num].OverallUsesRemaining != -1)
			{
				Item.Skills[num].OverallUsesRemaining = Item.Skills[num].ComputeTotalUses(Item.Holder);
			}
		}
	}

	public void StartTurn()
	{
		if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			RefillOverallUses();
		}
		for (int num = Item.Skills.Count - 1; num >= 0; num--)
		{
			if (Item.Skills[num].SkillDefinition.UsesPerTurnCount != -1)
			{
				Item.Skills[num].UsesPerTurnRemaining = Item.Skills[num].UsesPerTurn;
			}
		}
	}
}
