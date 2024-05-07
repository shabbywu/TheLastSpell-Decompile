using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Serialization.Perk;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.Unit.Perk;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk;

public class UnitPerkTreeController
{
	public UnitPerkTree UnitPerkTree { get; private set; }

	public UnitPerkTreeController(UnitPerkTreeView view, PlayableUnit playableUnit)
	{
		UnitPerkTree = new UnitPerkTree(this, view, playableUnit);
		if (!TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.Inited)
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.Init();
		}
		if (UnitPerkTree.UnitPerkTreeView.UnitPerkTierViews.Count < PlayableUnitDatabase.UnitPerkTemplateDefinition.TierCount)
		{
			((CLogger<CharacterSheetManager>)TPSingleton<CharacterSheetManager>.Instance).LogError((object)"UnitPerkTreeView must have as many UnitPerkTierViews as in PlayableUnitDatabase.UnitPerkTierDefinitions --> Have you lost a reference on UnitPerkTierViews prefab?", (CLogLevel)1, true, true);
		}
	}

	public void BuyPerk()
	{
		if (UnitPerkTree.CanBuyPerk())
		{
			UnitPerkTree.UnitPerkTreeView.SelectedPerk.Perk.PerkController.Unlock(UnitPerkTree.PlayableUnit);
			UnitPerkTree.PlayableUnit.PerksPoints--;
			UnitPerkTree.UnitPerkTreeView.RefreshSelectedPerk(UnitPerkTree.UnitPerkTreeView.SelectedPerk);
			OnSetNewPerk(UnitPerkTree.UnitPerkTreeView.SelectedPerk.PerkDefinition.Id, UnitPerkTree.UnitPerkTreeView.SelectedPerk.Perk);
			UpdateTiersAvailability();
			UnitPerkTree.UnitPerkTreeView.RefreshPerkPoints();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshPerkAvailableNotif(TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshOpenedPage();
		}
	}

	public void GeneratePerkTree(PlayableUnit owner)
	{
		List<UnitPerkCollectionDefinition> list = PickRandomCollections();
		UnitPerkTree.SetCollectionIds(list);
		for (int i = 0; i < PlayableUnitDatabase.UnitPerkTemplateDefinition.TierCount; i++)
		{
			if (!PlayableUnitDatabase.UnitPerkTemplateDefinition.RequiredPerksCountPerTier.TryGetValue(i + 1, out var value))
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)$"Missing tier for the requiredPerksCount in UnitPerkTemplateDefinition. Tier missing : \"{i}\". Skip.", (CLogLevel)1, true, true);
				continue;
			}
			UnitPerkTier unitPerkTier = new UnitPerkTierController(UnitPerkTree.UnitPerkTreeView.UnitPerkTierViews[i], UnitPerkTree, value, i).UnitPerkTier;
			UnitPerkTree.UnitPerkTiers.Add(unitPerkTier);
			for (int j = 0; j < list.Count; j++)
			{
				PerkDefinition perkDefinition = PickRandomPerkDefinition(list, i, j);
				UnitPerkDisplay perkView = TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTierViews[i].PerkDisplays[j];
				TheLastStand.Model.Unit.Perk.Perk perk = null;
				if (perkDefinition != null)
				{
					perk = new PerkController(perkDefinition, perkView, owner, unitPerkTier, list[j].Id, isNative: false, isFromRace: false).Perk;
					owner.PlayableUnitPerksController.TryAddPerk(perk);
				}
				unitPerkTier.Perks.Add((perkDefinition == null) ? null : perk);
			}
			if (i == 0)
			{
				unitPerkTier.UnitPerkTierController.Unlock();
			}
		}
	}

	public void GeneratePerkTree(List<SerializedPerkCollection> perkCollections, PlayableUnit owner, bool isOwnerDead)
	{
		FixHumanPerkCollectionNotBeingSet(perkCollections, owner);
		UnitPerkTree.SetCollectionIds(perkCollections.Select((SerializedPerkCollection perkCollection) => perkCollection.Id).ToList());
		for (int i = 0; i < PlayableUnitDatabase.UnitPerkTemplateDefinition.TierCount; i++)
		{
			if (!PlayableUnitDatabase.UnitPerkTemplateDefinition.RequiredPerksCountPerTier.TryGetValue(i + 1, out var value))
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)$"Missing tier for the requiredPerksCount in UnitPerkTemplateDefinition. Tier missing : \"{i}\". Skip.", (CLogLevel)1, true, true);
				continue;
			}
			UnitPerkTier unitPerkTier = new UnitPerkTierController(UnitPerkTree.UnitPerkTreeView.UnitPerkTierViews[i], UnitPerkTree, value, i).UnitPerkTier;
			UnitPerkTree.UnitPerkTiers.Add(unitPerkTier);
			for (int j = 0; j < perkCollections.Count; j++)
			{
				SerializedPerk serializedPerk = perkCollections[j].Perks[i];
				if (perkCollections[j].Perks.Count > i && PlayableUnitDatabase.PerkDefinitions.ContainsKey(serializedPerk.Id))
				{
					PerkDefinition perkDefinition = PlayableUnitDatabase.PerkDefinitions[perkCollections[j].Perks[i].Id];
					UnitPerkDisplay perkView = TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTierViews[i].PerkDisplays[j];
					unitPerkTier.Perks.Add(new PerkController(serializedPerk, perkDefinition, perkView, owner, unitPerkTier, perkCollections[j].Id, isOwnerDead, isNative: false, isFromRace: false).Perk);
				}
				else
				{
					unitPerkTier.Perks.Add(null);
				}
			}
			UpdateTiersAvailability(0, refreshView: false);
		}
	}

	public bool IsSkillLockedByPerks(TheLastStand.Model.Skill.Skill skill)
	{
		if (!skill.SkillDefinition.IsLockedByPerk)
		{
			return false;
		}
		return !UnitPerkTree.PlayableUnit.ContextualSkills.Contains(skill);
	}

	public void SelectPerk(UnitPerkDisplay selectedPerkDisplay)
	{
		if (!((Object)(object)UnitPerkTree.UnitPerkTreeView.SelectedPerk == (Object)(object)selectedPerkDisplay) && (!((Object)(object)selectedPerkDisplay != (Object)null) || selectedPerkDisplay.Perk != null))
		{
			UnitPerkTree.UnitPerkTreeView.RefreshSelectedPerk(selectedPerkDisplay);
		}
	}

	public void UnlockPerk(string perkDefinitionId)
	{
		for (int num = UnitPerkTree.UnitPerkTiers.Count - 1; num >= 0; num--)
		{
			for (int num2 = UnitPerkTree.UnitPerkTiers[num].Perks.Count - 1; num2 >= 0; num2--)
			{
				TheLastStand.Model.Unit.Perk.Perk perk = UnitPerkTree.UnitPerkTiers[num].Perks[num2];
				if (perk != null && perk.PerkDefinition.Id == perkDefinitionId)
				{
					perk.PerkController.Unlock(UnitPerkTree.PlayableUnit);
					TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
					TPSingleton<CharacterSheetPanel>.Instance.RefreshOpenedPage();
					UnitPerkTree.UnitPerkTreeView.RefreshSelectedPerk(UnitPerkTree.UnitPerkTreeView.SelectedPerk);
				}
			}
		}
		UpdateTiersAvailability(1, refreshView: false);
	}

	public void LockPerk(string perkDefinitionId)
	{
		for (int num = UnitPerkTree.UnitPerkTiers.Count - 1; num >= 0; num--)
		{
			for (int num2 = UnitPerkTree.UnitPerkTiers[num].Perks.Count - 1; num2 >= 0; num2--)
			{
				TheLastStand.Model.Unit.Perk.Perk perk = UnitPerkTree.UnitPerkTiers[num].Perks[num2];
				if (perk != null && perk.PerkDefinition.Id == perkDefinitionId)
				{
					perk.PerkController.Lock(UnitPerkTree.PlayableUnit);
					TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
					TPSingleton<CharacterSheetPanel>.Instance.RefreshOpenedPage();
					UnitPerkTree.UnitPerkTreeView.RefreshSelectedPerk(UnitPerkTree.UnitPerkTreeView.SelectedPerk);
				}
			}
		}
		UpdateTiersAvailability(1, refreshView: false);
	}

	private bool HasPerkAlready(PerkDefinition perkDefinition)
	{
		if (!TPSingleton<GlyphManager>.Instance.NativePerksToUnlock.ContainsKey(perkDefinition.Id))
		{
			return UnitPerkTree.UnitPerkTiers.Any((UnitPerkTier perkTier) => perkTier.Perks.Any((TheLastStand.Model.Unit.Perk.Perk perk) => perk != null && perk.PerkDefinition == perkDefinition));
		}
		return true;
	}

	private void OnSetNewPerk(string id, TheLastStand.Model.Unit.Perk.Perk unitPerk, bool shouldRefreshHud = true)
	{
		TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.Refresh(fullRefresh: true);
	}

	private List<UnitPerkCollectionDefinition> PickRandomCollections()
	{
		List<UnitPerkCollectionDefinition> list = new List<UnitPerkCollectionDefinition>();
		List<Tuple<UnitPerkCollectionDefinition, int, string>> list2 = new List<Tuple<UnitPerkCollectionDefinition, int, string>>();
		HashSet<UnitPerkCollectionDefinition> hashSet = new HashSet<UnitPerkCollectionDefinition>();
		for (int i = 0; i < PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions.Count; i++)
		{
			int num = 0;
			list2.Clear();
			foreach (Tuple<UnitPerkCollectionDefinition, int, string> tupleCollectionWeight in PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions[i].CollectionsPerWeight)
			{
				if (IsPerkCollectionAvailableToRace(tupleCollectionWeight.Item3) && (tupleCollectionWeight.Item1.MultipleAllowed || !hashSet.Any((UnitPerkCollectionDefinition collection) => collection == tupleCollectionWeight.Item1)))
				{
					num += tupleCollectionWeight.Item2;
					list2.Add(tupleCollectionWeight);
				}
			}
			if (list2.Count == 0)
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)$"Something went wrong on the perk tree generation : there are no potential collections for slot \"{i + 1}\". Use default instead.", (CLogLevel)1, true, true);
				list.Add(null);
				continue;
			}
			int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, num);
			foreach (Tuple<UnitPerkCollectionDefinition, int, string> item in list2)
			{
				num -= item.Item2;
				if (randomRange >= num)
				{
					list.Add(item.Item1);
					hashSet.Add(item.Item1);
					break;
				}
			}
		}
		return list;
	}

	private bool IsPerkCollectionAvailableToRace(string perkCollectionRaceId)
	{
		if (UnitPerkTree.PlayableUnit == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(perkCollectionRaceId))
		{
			return perkCollectionRaceId == UnitPerkTree.PlayableUnit.RaceDefinition.Id;
		}
		return true;
	}

	private PerkDefinition PickRandomPerkDefinition(List<UnitPerkCollectionDefinition> collections, int perkTierIndex, int perkIndex)
	{
		List<Tuple<PerkDefinition, int>> list = new List<Tuple<PerkDefinition, int>>();
		int num = 0;
		if (collections[perkIndex] != null)
		{
			foreach (Tuple<PerkDefinition, int> item in collections[perkIndex].PerksFromTier[perkTierIndex + 1])
			{
				if (!HasPerkAlready(item.Item1))
				{
					list.Add(item);
					num += item.Item2;
				}
			}
		}
		if (list.Count == 0)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)("Something went wrong on the perk tree generation :\n" + $"There are no potential perks for slot : {perkIndex + 1} ; tier : {perkTierIndex + 1} ; Collection : {collections[perkIndex]?.Id}. Use default instead."), (CLogLevel)1, true, true);
			return null;
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, num);
		foreach (Tuple<PerkDefinition, int> item2 in list)
		{
			num -= item2.Item2;
			if (randomRange >= num)
			{
				return item2.Item1;
			}
		}
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Something went wrong with the weight algorithm.", (CLogLevel)2, true, true);
		return null;
	}

	private void UpdateTiersAvailability(int startIndex = 1, bool refreshView = true)
	{
		int i = startIndex;
		bool flag = true;
		for (; i < UnitPerkTree.UnitPerkTiers.Count; i++)
		{
			if (UnitPerkTree.UnitPerkTiers[i].RequiredPerksCount <= UnitPerkTree.PlayableUnit.UnlockedPerksCount)
			{
				UnitPerkTree.UnitPerkTiers[i].UnitPerkTierController.Unlock();
			}
			if (refreshView)
			{
				UnitPerkTree.UnitPerkTiers[i].UnitPerkTierView.RefreshAvailability(flag && !UnitPerkTree.UnitPerkTiers[i].Available);
			}
			flag = UnitPerkTree.UnitPerkTiers[i].Available;
		}
	}

	private void FixHumanPerkCollectionNotBeingSet(List<SerializedPerkCollection> perkCollections, PlayableUnit owner)
	{
		if (owner.RaceDefinition?.Id != "Human" || perkCollections.Any((SerializedPerkCollection perkCollection) => perkCollection.Id == "Human"))
		{
			return;
		}
		string text = "Human";
		int num = -1;
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions.Count; i++)
		{
			foreach (Tuple<UnitPerkCollectionDefinition, int, string> item in PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions[i].CollectionsPerWeight)
			{
				if (item.Item3 == text)
				{
					num = i;
					break;
				}
			}
		}
		if (num == -1)
		{
			return;
		}
		foreach (Tuple<UnitPerkCollectionDefinition, int, string> item2 in PlayableUnitDatabase.UnitPerkTemplateDefinition.UnitPerkCollectionSetDefinitions[num].CollectionsPerWeight)
		{
			hashSet.Add(item2.Item3);
		}
		if (num < perkCollections.Count && hashSet.Contains(text) && !hashSet.Contains(perkCollections[num].Id) && perkCollections[num].Id == "Misc")
		{
			perkCollections[num].Id = text;
		}
	}

	public void DebugUpdateAllTiersAvailability()
	{
		foreach (UnitPerkTier unitPerkTier in UnitPerkTree.UnitPerkTiers)
		{
			unitPerkTier.UnitPerkTierController.Unlock();
		}
		UnitPerkTree.UnitPerkTreeView.Refresh();
	}
}
