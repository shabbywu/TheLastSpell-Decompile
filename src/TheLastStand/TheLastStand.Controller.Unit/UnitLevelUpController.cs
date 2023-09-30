using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Unit;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Controller.Unit;

public class UnitLevelUpController
{
	public static bool CanOpenUnitLevelUpView
	{
		get
		{
			if ((TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.ConsentPopup || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BlockingPopup || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitCustomisation) && TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				return TileObjectSelectionManager.SelectedPlayableUnit.StatsPoints > 0;
			}
			return false;
		}
	}

	public static bool CanCloseUnitLevelUpView => TPSingleton<UnitLevelUpView>.Instance.IsOpened;

	public UnitLevelUp UnitLevelUp { get; private set; }

	public UnitLevelUpController(UnitLevelUpDefinition definition, SerializedLevelUpBonuses container)
	{
		UnitLevelUp = new UnitLevelUp(definition, container, this);
	}

	public UnitLevelUpController(UnitLevelUpDefinition definition, UnitLevelUpView view)
	{
		UnitLevelUp = new UnitLevelUp(definition, this);
	}

	public bool CanReroll()
	{
		return UnitLevelUp.CommonNbReroll > 0;
	}

	public void DrawAvailableMainStats()
	{
		if (CanReroll())
		{
			UnitLevelUp.MainNbReroll++;
			UnitLevelUp.CommonNbReroll--;
			DrawAvailableStats();
		}
	}

	public void DrawAvailableSecondaryStats()
	{
		if (CanReroll())
		{
			UnitLevelUp.SecondaryNbReroll++;
			UnitLevelUp.CommonNbReroll--;
			DrawAvailableStats(isDrawingMainStat: false);
		}
	}

	public void DrawAvailableStats(bool isDrawingMainStat = true)
	{
		List<UnitLevelUp.SelectedStatToLevelUp> selectedStatToLevelUps;
		Dictionary<UnitStatDefinition.E_Stat, UnitLevelUpStatDefinition> dictionary;
		int num;
		List<int> list;
		if (isDrawingMainStat)
		{
			selectedStatToLevelUps = UnitLevelUp.AvailableMainStats;
			dictionary = PlayableUnitDatabase.UnitLevelUpMainStatDefinitions;
			num = UnitLevelUp.MainNbReroll;
			list = UnitLevelUp.UnitLevelUpDefinition.MainStatDraws;
		}
		else
		{
			selectedStatToLevelUps = UnitLevelUp.AvailableSecondaryStats;
			dictionary = PlayableUnitDatabase.UnitLevelUpSecondaryStatDefinitions;
			num = UnitLevelUp.SecondaryNbReroll;
			list = UnitLevelUp.UnitLevelUpDefinition.SecondaryStatDraws;
		}
		selectedStatToLevelUps.Clear();
		DeselectStat();
		int num2 = list[Mathf.Min(num, list.Count - 1)];
		while (num2 != selectedStatToLevelUps.Count)
		{
			List<UnitLevelUpStatDefinition> list2 = new List<UnitLevelUpStatDefinition>(dictionary.Values);
			list2.RemoveAll((UnitLevelUpStatDefinition stat) => selectedStatToLevelUps.Any((UnitLevelUp.SelectedStatToLevelUp y) => y.Definition.Stat == stat.Stat) || UnitLevelUp.PlayableUnit.UnitStatsController.GetStat(stat.Stat).Base >= UnitLevelUp.PlayableUnit.UnitStatsController.GetStat(stat.Stat).Boundaries.y);
			float num3 = 0f;
			int count = list2.Count;
			if (count == 0)
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogWarning((object)"No available stat found on unit to level up -> Returning a random one. This can be perfectly fine if you used debug commands to buff a hero.", (CLogLevel)1, true, false);
				UnitLevelUp.SelectedStatToLevelUp item = new UnitLevelUp.SelectedStatToLevelUp(ListExtensions.PickRandom<UnitLevelUpStatDefinition>((IEnumerable<UnitLevelUpStatDefinition>)dictionary.Values), UnitLevelUp.E_StatLevelUpRarity.SmallRarity);
				selectedStatToLevelUps.Add(item);
				break;
			}
			for (int i = 0; i < count; i++)
			{
				num3 += list2[i].Weight;
			}
			float randomRange = RandomManager.GetRandomRange(this, 0f, num3);
			float num4 = 0f;
			for (int j = 0; j < count; j++)
			{
				num4 += list2[j].Weight;
				if (!(randomRange <= num4))
				{
					continue;
				}
				HashSet<int> hashSet = new HashSet<int>();
				foreach (KeyValuePair<UnitLevelUp.E_StatLevelUpRarity, int> bonuse in list2[j].Bonuses)
				{
					hashSet.Add((int)bonuse.Key);
				}
				int num5 = (int)(RarityProbabilitiesTreeController.GenerateRarity(UnitLevelUp.UnitLevelUpDefinition.RaritiesList) - 1);
				if (hashSet.Contains(num5))
				{
					UnitLevelUp.SelectedStatToLevelUp item2 = new UnitLevelUp.SelectedStatToLevelUp(list2[j], (UnitLevelUp.E_StatLevelUpRarity)num5);
					selectedStatToLevelUps.Add(item2);
					break;
				}
			}
		}
	}

	public void DeselectStat()
	{
		UnitLevelUp.HasSelectedStat = false;
	}

	public void SelectStat(UnitLevelUp.SelectedStatToLevelUp selectedStat)
	{
		UnitLevelUp.SelectedStat = selectedStat;
		UnitLevelUp.HasSelectedStat = true;
	}

	public void ValidateStatToIncrease(bool isValidatingMainStat)
	{
		UnitLevelUp.PlayableUnit.PlayableUnitStatsController.OnLevelUpStatValidated(UnitLevelUp);
		if (isValidatingMainStat)
		{
			UnitLevelUpPoint unitLevelUpPoint = UnitLevelUp.PlayableUnit.UnitLevelUpPoints.FirstOrDefault((UnitLevelUpPoint o) => o.HasMainStatPoint);
			if (unitLevelUpPoint != null)
			{
				unitLevelUpPoint.HasMainStatPoint = false;
			}
		}
		else
		{
			UnitLevelUpPoint unitLevelUpPoint2 = UnitLevelUp.PlayableUnit.UnitLevelUpPoints.FirstOrDefault((UnitLevelUpPoint o) => o.HasSecondaryStatPoint);
			if (unitLevelUpPoint2 != null)
			{
				unitLevelUpPoint2.HasSecondaryStatPoint = false;
			}
		}
		if (!UnitLevelUp.PlayableUnit.UnitLevelUpPoints[0].HasAnyStatPoint)
		{
			UnitLevelUp.PlayableUnit.UnitLevelUpPoints.RemoveAt(0);
		}
		if (UnitLevelUp.SelectedStat.Definition.Stat == UnitStatDefinition.E_Stat.Health || UnitLevelUp.SelectedStat.Definition.Stat == UnitStatDefinition.E_Stat.HealthTotal)
		{
			UnitLevelUp.PlayableUnit.PlayableUnitController.UpdateInjuryStage();
		}
		TPSingleton<MetaConditionManager>.Instance.RefreshMaxHeroStatReached(UnitLevelUp.SelectedStat.Definition.Stat, UnitLevelUp.PlayableUnit.GetClampedStatValue(UnitLevelUp.SelectedStat.Definition.Stat));
		if (TPSingleton<UnitLevelUpView>.Instance.CurrentLevelUpShownStat == UnitLevelUpView.E_LevelUpShownStat.Main)
		{
			UnitLevelUp.AvailableMainStats.Clear();
		}
		else if (TPSingleton<UnitLevelUpView>.Instance.CurrentLevelUpShownStat == UnitLevelUpView.E_LevelUpShownStat.Secondary)
		{
			UnitLevelUp.AvailableSecondaryStats.Clear();
		}
		if (isValidatingMainStat)
		{
			UnitLevelUp.MainNbReroll = 0;
		}
		else
		{
			UnitLevelUp.SecondaryNbReroll = 0;
		}
		TPSingleton<ToDoListView>.Instance.RefreshUnitLevelUpNotification();
	}
}
