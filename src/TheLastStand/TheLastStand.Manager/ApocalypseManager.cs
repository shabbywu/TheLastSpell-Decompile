using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Apocalypse;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization;
using TheLastStand.View.WorldMap;
using UnityEngine;

namespace TheLastStand.Manager;

public sealed class ApocalypseManager : Manager<ApocalypseManager>, ISerializable, IDeserializable
{
	public static class Constants
	{
		public const string Percentage = "Percentage";
	}

	[SerializeField]
	[Min(0f)]
	private int defaultApocalypseIndex;

	public static Apocalypse CurrentApocalypse { get; private set; }

	public static int CurrentApocalypseIndex { get; private set; }

	public static bool IsApocalypseUnlocked => TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable > 0;

	public bool FreshlyUnlockedApocalypse { get; set; }

	public int MaxApocalypseIndexAvailable { get; set; }

	public uint DamnedSoulsPercentageModifier => (uint)CurrentApocalypseIndex * ApocalypseDatabase.ConfigurationDefinition.DamnedSoulsPercentagePerLevel;

	public static bool IsThisStatIncreasedByPercentage(UnitStatDefinition.E_Stat stat)
	{
		if (ApocalypseDatabase.ConfigurationDefinition.StatWithModifierTypes.TryGetValue(stat, out var value))
		{
			return value == "Percentage";
		}
		((CLogger<ApocalypseManager>)TPSingleton<ApocalypseManager>.Instance).LogError((object)$"This stat hasn't an Apocalypse type modifier : {stat}", (CLogLevel)2, true, true);
		return false;
	}

	public static void SetApocalypse(int id)
	{
		if (ApocalypseDatabase.DoesApocalypseExist(id))
		{
			CurrentApocalypseIndex = id;
			CurrentApocalypse = new Apocalypse(ApocalypseDatabase.ComputeApocalypses(id), id);
			LogApocalypseData();
			return;
		}
		int maxValue = 0;
		for (int i = 0; i < ApocalypseDatabase.ApocalypsesDefinition.ApocalypseDefinitions.Count; i++)
		{
			if (maxValue < ApocalypseDatabase.ApocalypsesDefinition.ApocalypseDefinitions[i].Id)
			{
				maxValue = ApocalypseDatabase.ApocalypsesDefinition.ApocalypseDefinitions[i].Id;
			}
		}
		CurrentApocalypseIndex = maxValue;
		CurrentApocalypse = new Apocalypse(ApocalypseDatabase.ComputeApocalypses(maxValue), maxValue);
		TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable = maxValue;
		((CLogger<ApocalypseManager>)TPSingleton<ApocalypseManager>.Instance).Log((object)$"There is no Apocalypse with this id : {id} ! We are going to clamp the value to the current max apocalypse level ({maxValue}) ! This might be caused by a bug players encountered making them play with empty apocalypses", (CLogLevel)2, true, false);
		LogApocalypseData();
		TPSingleton<WorldMapCityManager>.Instance.Cities.ForEach(delegate(WorldMapCity x)
		{
			if (x.MaxApocalypsePassed > maxValue)
			{
				x.MaxApocalypsePassed = maxValue;
			}
		});
	}

	public static void TryIncreaseMaxApocalypseIndexAvailable()
	{
		if ((ApocalypseDatabase.ConfigurationDefinition.ApocalypseUnlockConditions.All((ApocalypseUnlockCondition condition) => condition.IsValid) || TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable > 0) && CurrentApocalypseIndex == TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable && ApocalypseDatabase.DoesApocalypseExist(CurrentApocalypseIndex + 1))
		{
			TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable++;
			TPSingleton<ApocalypseManager>.Instance.FreshlyUnlockedApocalypse = true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (((TPSingleton<ApocalypseManager>)(object)this)._IsValid)
		{
			SetApocalypse(defaultApocalypseIndex);
		}
	}

	private static void LogApocalypseData()
	{
		((CLogger<ApocalypseManager>)TPSingleton<ApocalypseManager>.Instance).Log((object)CurrentApocalypse.ToString(), (CLogLevel)1, false, false);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedApocalypse serializedApocalypse)
		{
			SetApocalypse(serializedApocalypse.ApocalypseIndex);
		}
	}

	public void GlobalDeserialize(ISerializedData container = null)
	{
		if (container != null)
		{
			int num = 0;
			if (!(container is SerializedGlobalApocalypse serializedGlobalApocalypse))
			{
				return;
			}
			for (int i = 0; i < ApocalypseDatabase.ApocalypsesDefinition.ApocalypseDefinitions.Count; i++)
			{
				if (num < ApocalypseDatabase.ApocalypsesDefinition.ApocalypseDefinitions[i].Id)
				{
					num = ApocalypseDatabase.ApocalypsesDefinition.ApocalypseDefinitions[i].Id;
				}
			}
			MaxApocalypseIndexAvailable = Mathf.Clamp(serializedGlobalApocalypse.MaxAvailableApocalypseIndex, 0, num);
		}
		else
		{
			MaxApocalypseIndexAvailable = 0;
		}
	}

	public ISerializedData GlobalSerialize()
	{
		return new SerializedGlobalApocalypse
		{
			MaxAvailableApocalypseIndex = MaxApocalypseIndexAvailable
		};
	}

	public ISerializedData Serialize()
	{
		return new SerializedApocalypse
		{
			ApocalypseIndex = CurrentApocalypseIndex
		};
	}

	[DevConsoleCommand("ChangeApocalypse")]
	public static void ChangeApocalypse(int id)
	{
		SetApocalypse(id);
	}

	[DevConsoleCommand("SetMaxApocalypseAvailableTo")]
	public static void SetMaxApocalypseAvailableTo(int apocalypseId)
	{
		TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable = apocalypseId;
		TPSingleton<GameConfigurationsView>.Instance.Refresh();
	}

	[DevConsoleCommand("SetMaxApocalypsePassedTo")]
	public static void SetMaxApocalypsePassedTo(string cityId, int apocalypseId)
	{
		if (apocalypseId > TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable)
		{
			TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable = apocalypseId;
		}
		WorldMapCity worldMapCity = TPSingleton<WorldMapCityManager>.Instance.Cities.FirstOrDefault((WorldMapCity x) => x.CityDefinition.Id == cityId);
		if (worldMapCity != null)
		{
			worldMapCity.MaxApocalypsePassed = apocalypseId;
		}
		TPSingleton<GameConfigurationsView>.Instance.Refresh();
	}
}
