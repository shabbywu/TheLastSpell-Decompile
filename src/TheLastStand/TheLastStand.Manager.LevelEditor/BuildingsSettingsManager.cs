using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.EventSystem;
using TheLastStand.Model.Building;
using TheLastStand.Model.Events;
using TheLastStand.Model.LevelEditor.Conversation;
using TheLastStand.View.LevelEditor;
using UnityEngine;

namespace TheLastStand.Manager.LevelEditor;

public class BuildingsSettingsManager : Manager<BuildingsSettingsManager>
{
	[SerializeField]
	private BuildingSettingsPanel buildingSettingsPanelPrefab;

	public Dictionary<TheLastStand.Model.Building.Building, BuildingSettingsPanel> BuildingSettingsPanels { get; private set; } = new Dictionary<TheLastStand.Model.Building.Building, BuildingSettingsPanel>();


	public void OnBuildingPlaced(TheLastStand.Model.Building.Building building, XContainer buildingContainer)
	{
		BuildingSettingsPanel buildingSettingsPanel = Object.Instantiate<BuildingSettingsPanel>(buildingSettingsPanelPrefab, ((Component)this).transform);
		buildingSettingsPanel.Init(building, buildingContainer);
		BuildingSettingsPanels.Add(building, buildingSettingsPanel);
	}

	protected override void Awake()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		base.Awake();
		TPSingleton<LevelEditorManager>.Instance.LevelEditorStateChanged += OnLevelEditorStateChanged;
		PlaceBuildingCommand.BuildingPlaced = (PlaceBuildingCommand.BuildingPlacedEventHandler)Delegate.Combine(PlaceBuildingCommand.BuildingPlaced, new PlaceBuildingCommand.BuildingPlacedEventHandler(OnBuildingPlaced));
		EventManager.AddListener(typeof(BuildingDestroyedEvent), new EventHandler(OnBuildingDestroyed), false);
	}

	protected override void OnDestroy()
	{
		((CLogger<BuildingsSettingsManager>)this).OnDestroy();
		if (TPSingleton<LevelEditorManager>.Exist())
		{
			TPSingleton<LevelEditorManager>.Instance.LevelEditorStateChanged -= OnLevelEditorStateChanged;
		}
		PlaceBuildingCommand.BuildingPlaced = (PlaceBuildingCommand.BuildingPlacedEventHandler)Delegate.Remove(PlaceBuildingCommand.BuildingPlaced, new PlaceBuildingCommand.BuildingPlacedEventHandler(OnBuildingPlaced));
	}

	private void DisplayAllSettingsPanelsButtons(bool show)
	{
		foreach (KeyValuePair<TheLastStand.Model.Building.Building, BuildingSettingsPanel> buildingSettingsPanel in BuildingSettingsPanels)
		{
			if (!show)
			{
				buildingSettingsPanel.Value.DisplayPanel(show: false);
			}
			((Component)buildingSettingsPanel.Value).gameObject.SetActive(show);
		}
	}

	private void OnBuildingDestroyed(Event raisedEvent)
	{
		BuildingDestroyedEvent buildingDestroyedEvent = raisedEvent as BuildingDestroyedEvent;
		if (BuildingSettingsPanels.ContainsKey(buildingDestroyedEvent.Building))
		{
			Object.Destroy((Object)(object)((Component)BuildingSettingsPanels[buildingDestroyedEvent.Building]).gameObject);
			BuildingSettingsPanels.Remove(buildingDestroyedEvent.Building);
		}
	}

	private void OnBuildingPlaced(TheLastStand.Model.Building.Building building)
	{
		if (ShouldCreateSettingsPanel(building))
		{
			BuildingSettingsPanel buildingSettingsPanel = Object.Instantiate<BuildingSettingsPanel>(buildingSettingsPanelPrefab, ((Component)this).transform);
			buildingSettingsPanel.Init(building);
			BuildingSettingsPanels.Add(building, buildingSettingsPanel);
		}
	}

	private void OnLevelEditorStateChanged(LevelEditorManager.E_State previousState, LevelEditorManager.E_State newState)
	{
		DisplayAllSettingsPanelsButtons(newState == LevelEditorManager.E_State.SelectBuilding);
	}

	private bool ShouldCreateSettingsPanel(TheLastStand.Model.Building.Building building)
	{
		if (building.BlueprintModule.IsIndestructible)
		{
			return building.UpgradeModule != null;
		}
		return true;
	}
}
