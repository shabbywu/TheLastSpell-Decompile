using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using TheLastStand.Serialization.Meta;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Dev.View;

public class MetaConditionsDebugView : TPSingleton<MetaConditionsDebugView>
{
	public enum E_MetaStatusFilter
	{
		None = 0,
		Activated = 1,
		Unlocked = 2,
		Locked = 4,
		ActivatedUnlocked = 3,
		ActivatedLocked = 5,
		UnlockedLocked = 6,
		All = 7
	}

	[Serializable]
	private class SerializedMetaState
	{
		public SerializedMetaConditions MetaConditions;

		public SerializedMetaUpgrades MetaUpgrades;
	}

	[SerializeField]
	private GameObject contentGameObject;

	[SerializeField]
	private GraphicRaycaster hitbox;

	[SerializeField]
	private TMP_InputField searchBar;

	[SerializeField]
	private MetaUpgradeDebugLineView metaUpgradeViewExample;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button unlockAllButton;

	[SerializeField]
	private Button lockAllButton;

	[SerializeField]
	private Button activateAllButton;

	[SerializeField]
	private Button saveStateButton;

	[SerializeField]
	private Button loadStateButton;

	[SerializeField]
	private Button sortByNameButton;

	[SerializeField]
	private Button sortByStatusButton;

	[SerializeField]
	private Button filterActivatedButton;

	[SerializeField]
	private Button filterUnlockedButton;

	[SerializeField]
	private Button filterLockedButton;

	[SerializeField]
	private Image filterActivatedIcon;

	[SerializeField]
	private Image filterUnlockedIcon;

	[SerializeField]
	private Image filterLockedIcon;

	[SerializeField]
	private Toggle refreshToggle;

	[SerializeField]
	private Button refreshStatesButton;

	[SerializeField]
	private TMP_Dropdown dropDown;

	private List<MetaUpgradeDebugLineView> metaUpgradeViews = new List<MetaUpgradeDebugLineView>();

	private bool nameSortReversed = true;

	private bool statusSortReversed = true;

	private IEnumerable<MetaUpgrade> AllUpgrades => TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Concat(TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades).Concat(TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades);

	public E_MetaStatusFilter Filter { get; private set; }

	public bool IsOpen { get; private set; }

	private string StatesExtension => "BMS";

	private string StatesFolderPath => Path.Combine(Application.persistentDataPath, "MetaStates");

	[DevConsoleCommand("MCDW")]
	public static void ToggleMetaConditionsDebugView()
	{
		TPSingleton<MetaConditionsDebugView>.Instance.IsOpen = !TPSingleton<MetaConditionsDebugView>.Instance.IsOpen;
		TPSingleton<MetaConditionsDebugView>.Instance.Refresh();
		InputManager.DebugOnMetaConditionDebugViewToggled(TPSingleton<MetaConditionsDebugView>.Instance.IsOpen);
		if (!TPSingleton<MetaConditionsDebugView>.Instance.IsOpen)
		{
			SaveManager.Save();
		}
	}

	public void ClearSearchBar()
	{
		searchBar.text = string.Empty;
	}

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)closeButton.onClick).AddListener(new UnityAction(ToggleMetaConditionsDebugView));
		((UnityEvent)unlockAllButton.onClick).AddListener(new UnityAction(UnlockAllUpgrades));
		((UnityEvent)lockAllButton.onClick).AddListener(new UnityAction(LockAllUpgrades));
		((UnityEvent)activateAllButton.onClick).AddListener(new UnityAction(ActivateAllUpgrades));
		((UnityEvent)sortByNameButton.onClick).AddListener(new UnityAction(SortByName));
		((UnityEvent)sortByStatusButton.onClick).AddListener(new UnityAction(SortByStatus));
		((UnityEvent)filterActivatedButton.onClick).AddListener((UnityAction)delegate
		{
			OnFilterValueChanged(E_MetaStatusFilter.Activated);
		});
		((UnityEvent)filterUnlockedButton.onClick).AddListener((UnityAction)delegate
		{
			OnFilterValueChanged(E_MetaStatusFilter.Unlocked);
		});
		((UnityEvent)filterLockedButton.onClick).AddListener((UnityAction)delegate
		{
			OnFilterValueChanged(E_MetaStatusFilter.Locked);
		});
		((UnityEvent<bool>)(object)refreshToggle.onValueChanged).AddListener((UnityAction<bool>)ToggleRefresh);
		((UnityEvent)loadStateButton.onClick).AddListener(new UnityAction(LoadState));
		((UnityEvent)saveStateButton.onClick).AddListener(new UnityAction(SaveState));
		RefreshAvailableStates();
	}

	[ContextMenu("Activate all upgrades")]
	private void ActivateAllUpgrades()
	{
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			metaUpgradeView.Activate();
		}
	}

	private void ApplySearchFilters()
	{
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			((Component)metaUpgradeView).gameObject.SetActive(Filter == E_MetaStatusFilter.All || Filter == E_MetaStatusFilter.None || ((Filter & E_MetaStatusFilter.Activated) == E_MetaStatusFilter.Activated && metaUpgradeView.MetaState == MetaUpgradesManager.E_MetaState.Activated) || ((Filter & E_MetaStatusFilter.Unlocked) == E_MetaStatusFilter.Unlocked && metaUpgradeView.MetaState == MetaUpgradesManager.E_MetaState.Unlocked) || ((Filter & E_MetaStatusFilter.Locked) == E_MetaStatusFilter.Locked && metaUpgradeView.MetaState == MetaUpgradesManager.E_MetaState.Locked));
		}
		foreach (MetaUpgradeDebugLineView metaUpgradeView2 in metaUpgradeViews)
		{
			if (((Component)metaUpgradeView2).gameObject.activeSelf)
			{
				string text = metaUpgradeView2.MetaUpgrade.MetaUpgradeDefinition.Id.ToLower();
				((Component)metaUpgradeView2).gameObject.SetActive(string.IsNullOrEmpty(searchBar.text) || text.Contains(searchBar.text.ToLower()));
			}
		}
	}

	private void ClearUpgrades()
	{
		Transform parent = ((Component)metaUpgradeViewExample).transform.parent;
		for (int num = parent.childCount - 1; num >= 0; num--)
		{
			if ((Object)(object)parent.GetChild(num) != (Object)(object)((Component)metaUpgradeViewExample).transform)
			{
				Object.Destroy((Object)(object)((Component)parent.GetChild(num)).gameObject);
			}
		}
	}

	private void CreateUpgrades()
	{
		((Component)metaUpgradeViewExample).gameObject.SetActive(true);
		metaUpgradeViews.Clear();
		foreach (MetaUpgrade allUpgrade in AllUpgrades)
		{
			MetaUpgradeDebugLineView component = Object.Instantiate<GameObject>(((Component)metaUpgradeViewExample).gameObject, ((Component)metaUpgradeViewExample).transform.parent).GetComponent<MetaUpgradeDebugLineView>();
			component.Set(allUpgrade);
			metaUpgradeViews.Add(component);
		}
		((Component)metaUpgradeViewExample).gameObject.SetActive(false);
	}

	private IReadOnlyList<string> ListStates()
	{
		if (!Directory.Exists(StatesFolderPath))
		{
			Directory.CreateDirectory(StatesFolderPath);
		}
		return (from o in Directory.GetFiles(StatesFolderPath)
			select Path.GetFileNameWithoutExtension(o)).ToList();
	}

	private void LoadState()
	{
		LoadState(dropDown.options[dropDown.value].text);
	}

	private void LoadState(string stateName)
	{
		string text = Path.Combine(StatesFolderPath, stateName + "." + StatesExtension);
		if (!File.Exists(text))
		{
			Debug.LogError((object)("??? " + text + " does not exist!"));
		}
		SerializedMetaState serializedMetaState;
		using (FileStream serializationStream = File.OpenRead(text))
		{
			serializedMetaState = (SerializedMetaState)new BinaryFormatter().Deserialize(serializationStream);
		}
		MetaConditionManager.DebugClearConditionControllers();
		TPSingleton<MetaUpgradesManager>.Instance.Deserialize((ISerializedData)(object)serializedMetaState.MetaUpgrades);
		TPSingleton<MetaConditionManager>.Instance.DeserializeFromAppSave((ISerializedData)(object)serializedMetaState.MetaConditions);
		ClearUpgrades();
		CreateUpgrades();
		Refresh();
	}

	[ContextMenu("Lock all upgrades")]
	private void LockAllUpgrades()
	{
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			metaUpgradeView.Lock();
		}
	}

	private void OnDestroy()
	{
		MetaConditionManager.OnConditionsRefreshed -= Refresh;
		((UnityEvent<string>)(object)searchBar.onValueChanged).RemoveListener((UnityAction<string>)OnSearchBarValueChanged);
	}

	private void OnFilterValueChanged(E_MetaStatusFilter filter)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		Filter ^= filter;
		Color color = ((Graphic)filterActivatedIcon).color;
		color.a = (((Filter & E_MetaStatusFilter.Activated) == E_MetaStatusFilter.Activated) ? 1f : 0.25f);
		((Graphic)filterActivatedIcon).color = color;
		color = ((Graphic)filterUnlockedIcon).color;
		color.a = (((Filter & E_MetaStatusFilter.Unlocked) == E_MetaStatusFilter.Unlocked) ? 1f : 0.25f);
		((Graphic)filterUnlockedIcon).color = color;
		color = ((Graphic)filterLockedIcon).color;
		color.a = (((Filter & E_MetaStatusFilter.Locked) == E_MetaStatusFilter.Locked) ? 1f : 0.25f);
		((Graphic)filterLockedIcon).color = color;
		ApplySearchFilters();
	}

	private void OnSearchBarValueChanged(string value)
	{
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			string text = metaUpgradeView.MetaUpgrade.MetaUpgradeDefinition.Id.ToLower();
			((Component)metaUpgradeView).gameObject.SetActive(string.IsNullOrEmpty(value) || text.Contains(value.ToLower()));
		}
		ApplySearchFilters();
	}

	private void Refresh()
	{
		contentGameObject.SetActive(IsOpen);
		((Behaviour)hitbox).enabled = IsOpen;
		if (!IsOpen)
		{
			return;
		}
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			metaUpgradeView.Refresh();
		}
		RefreshAvailableStates();
	}

	private void RefreshAvailableStates()
	{
		dropDown.options = ((IEnumerable<string>)ListStates()).Select((Func<string, OptionData>)((string o) => new OptionData(o))).ToList();
	}

	private void SaveState()
	{
		SerializedMetaState graph = new SerializedMetaState
		{
			MetaConditions = (TPSingleton<MetaConditionManager>.Instance.SerializeToAppSave() as SerializedMetaConditions),
			MetaUpgrades = (TPSingleton<MetaUpgradesManager>.Instance.Serialize() as SerializedMetaUpgrades)
		};
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		string text = Path.Combine(StatesFolderPath, Environment.UserName.Substring(0, 3) + "_" + DateTime.Now.ToString("dd-MM-yy HH.mm.ss") + "." + StatesExtension);
		using (FileStream serializationStream = File.OpenWrite(text))
		{
			binaryFormatter.Serialize(serializationStream, graph);
		}
		Debug.Log((object)("Saved Meta state under " + text));
		RefreshAvailableStates();
	}

	private void SortByName()
	{
		statusSortReversed = true;
		nameSortReversed = !nameSortReversed;
		metaUpgradeViews.Sort((MetaUpgradeDebugLineView lineA, MetaUpgradeDebugLineView lineB) => lineA.MetaUpgrade.MetaUpgradeDefinition.Id.CompareTo(lineB.MetaUpgrade.MetaUpgradeDefinition.Id));
		if (nameSortReversed)
		{
			metaUpgradeViews.Reverse();
		}
		for (int i = 0; i < metaUpgradeViews.Count; i++)
		{
			((Component)metaUpgradeViews[i]).transform.SetSiblingIndex(i);
		}
	}

	private void SortByStatus()
	{
		nameSortReversed = true;
		statusSortReversed = !statusSortReversed;
		metaUpgradeViews.Sort((MetaUpgradeDebugLineView lineA, MetaUpgradeDebugLineView lineB) => lineA.MetaState.CompareTo(lineB.MetaState));
		if (statusSortReversed)
		{
			metaUpgradeViews.Reverse();
		}
		for (int i = 0; i < metaUpgradeViews.Count; i++)
		{
			((Component)metaUpgradeViews[i]).transform.SetSiblingIndex(i);
		}
	}

	private void Start()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		MetaConditionManager.OnConditionsRefreshed += Refresh;
		((UnityEvent<string>)(object)searchBar.onValueChanged).AddListener((UnityAction<string>)OnSearchBarValueChanged);
		((UnityEvent)refreshStatesButton.onClick).AddListener(new UnityAction(RefreshAvailableStates));
		((Graphic)filterActivatedIcon).color = new Color(((Graphic)filterActivatedIcon).color.r, ((Graphic)filterActivatedIcon).color.g, ((Graphic)filterActivatedIcon).color.b, 0.25f);
		((Graphic)filterUnlockedIcon).color = new Color(((Graphic)filterUnlockedIcon).color.r, ((Graphic)filterUnlockedIcon).color.g, ((Graphic)filterUnlockedIcon).color.b, 0.25f);
		((Graphic)filterLockedIcon).color = new Color(((Graphic)filterLockedIcon).color.r, ((Graphic)filterLockedIcon).color.g, ((Graphic)filterLockedIcon).color.b, 0.25f);
		CreateUpgrades();
	}

	private void ToggleRefresh(bool isEnabled)
	{
		TPSingleton<MetaConditionManager>.Instance.DisableRefreshes = !isEnabled;
	}

	[ContextMenu("Unlock all upgrades")]
	private void UnlockAllUpgrades()
	{
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			metaUpgradeView.Unlock();
		}
	}
}
