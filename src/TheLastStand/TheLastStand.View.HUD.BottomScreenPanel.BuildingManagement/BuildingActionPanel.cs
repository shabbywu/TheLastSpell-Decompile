using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Unit;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingActionPanel : BuildingCapacityPanel
{
	[SerializeField]
	private TextMeshProUGUI workersCost;

	[SerializeField]
	private Canvas usesPerTurnCanvas;

	[SerializeField]
	private TextMeshProUGUI usesPerTurnText;

	[SerializeField]
	private Image actionIcon;

	[SerializeField]
	private SimpleFontLocalizedParent fontLocalizedParent;

	private Color workersCostColorInit = Color.white;

	public BuildingAction BuildingAction { get; set; }

	public static Sprite GetActionSprite(string buildingActionId)
	{
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Buildings/Actions/" + buildingActionId, failSilently: false);
	}

	public void Display(bool show)
	{
		((Component)base.BuildingCapacityRect).gameObject.SetActive(show);
		if (show)
		{
			SimpleFontLocalizedParent obj = fontLocalizedParent;
			if (obj != null)
			{
				((FontLocalizedParent)obj).RegisterChilds();
			}
		}
		else
		{
			SimpleFontLocalizedParent obj2 = fontLocalizedParent;
			if (obj2 != null)
			{
				((FontLocalizedParent)obj2).UnregisterChilds();
			}
		}
	}

	public override void OnSkillPanelHovered(bool hover)
	{
		BuildingManager.OnBuildingActionHovered(BuildingAction, hover);
		if (hover)
		{
			if (BuildingAction.BuildingActionDefinition.ContainsRepelFogEffect)
			{
				TileMapView.DebugShowFogMinMax();
			}
			BuildingManager.BuildingActionTooltip.Init(BuildingAction);
			BuildingManager.BuildingActionTooltip.FollowElement.ChangeTarget(((Component)this).transform);
			foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				playableUnit.UnitView.UnitHUD.AttackEstimationDisplay.DisplayHoveredBuildingActionEffects(BuildingAction, playableUnit);
			}
		}
		else
		{
			if (BuildingAction.BuildingActionDefinition.ContainsRepelFogEffect)
			{
				TileMapView.FogMinMaxTilemap.ClearAllTiles();
			}
			BuildingManager.BuildingActionTooltip.Init();
			foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				playableUnit2.UnitView.UnitHUD.AttackEstimationDisplay.Hide();
			}
		}
		BuildingManager.BuildingActionTooltip.Display();
	}

	public void OnBuildingActionClick()
	{
		((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)("Building action " + BuildingAction.BuildingActionDefinition.Id + " was clicked."), (CLogLevel)0, false, false);
		if (BuildingManager.SelectedBuildingAction != BuildingAction)
		{
			BuildingManager.SelectedBuildingAction = BuildingAction;
			buildingCapacitiesPanel.ChangeSelectedCapacityPanel(this);
			GameController.SetState(Game.E_State.BuildingPreparingAction);
		}
		else if (!InputManager.IsLastControllerJoystick)
		{
			BuildingManager.SelectedBuildingAction = null;
			buildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
			buildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
		}
	}

	public override void Refresh()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (BuildingAction != null)
		{
			int modifiedWorkersCost = ResourceManager.GetModifiedWorkersCost(BuildingAction.BuildingActionDefinition);
			((Graphic)workersCost).color = ((modifiedWorkersCost <= TPSingleton<ResourceManager>.Instance.Workers) ? workersCostColorInit : Color.red);
			((TMP_Text)workersCost).text = $"{modifiedWorkersCost}";
			button.Interactable = BuildingAction.BuildingActionController.CanExecuteAction();
			actionIcon.sprite = GetActionSprite(BuildingAction.BuildingActionDefinition.Id);
			((Behaviour)usesPerTurnCanvas).enabled = BuildingAction.BuildingActionDefinition.UsesPerTurnCount != -1;
			if (BuildingAction.BuildingActionDefinition.UsesPerTurnCount != -1)
			{
				((TMP_Text)usesPerTurnText).text = $"{BuildingAction.UsesPerTurnRemaining}/{BuildingAction.BuildingActionDefinition.UsesPerTurnCount}";
			}
		}
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		workersCostColorInit = ((Graphic)workersCost).color;
	}
}
