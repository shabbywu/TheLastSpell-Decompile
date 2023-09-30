using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.BuildingUpgrade;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class SwapActionController : BuildingUpgradeEffectController
{
	public SwapAction SwapAction => base.BuildingUpgradeEffect as SwapAction;

	public SwapActionController(SwapActionDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new SwapAction(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		int num = 0;
		TheLastStand.Model.Building.BuildingAction.BuildingAction buildingAction2 = base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions.Find((TheLastStand.Model.Building.BuildingAction.BuildingAction buildingAction) => buildingAction.BuildingActionDefinition.Id == SwapAction.SwapActionDefinition.OldActionId);
		if (buildingAction2 != null)
		{
			num = base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions.IndexOf(buildingAction2);
			base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions.RemoveAt(num);
			BuildingActionDefinition buildingActionDefinition = null;
			if (BuildingDatabase.BuildingActionDefinitions.TryGetValue(SwapAction.SwapActionDefinition.NewActionId, out var value))
			{
				buildingActionDefinition = value.Clone();
				TheLastStand.Model.Building.BuildingAction.BuildingAction buildingAction3 = new BuildingActionController(buildingActionDefinition, base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule).BuildingAction;
				int num2 = buildingActionDefinition.UsesPerTurnCount - buildingAction2.BuildingActionDefinition.UsesPerTurnCount;
				buildingAction3.UsesPerTurnRemaining = Mathf.Clamp(buildingAction2.UsesPerTurnRemaining + num2, 0, buildingActionDefinition.UsesPerTurnCount);
				base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions.Insert(num, buildingAction3);
			}
			else
			{
				((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)("BuildingActionDefinition " + SwapAction.SwapActionDefinition.NewActionId + " not found"), (CLogLevel)2, true, true);
			}
		}
		else
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)("SwapActionController was not able to find an existing building action with the Id " + SwapAction.SwapActionDefinition.OldActionId + " => Abort upgrade effect"), (CLogLevel)2, true, true);
		}
	}
}
