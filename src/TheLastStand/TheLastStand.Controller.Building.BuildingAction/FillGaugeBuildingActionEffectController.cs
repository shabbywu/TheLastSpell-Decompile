using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller.Building.BuildingAction;

public class FillGaugeBuildingActionEffectController : BuildingActionEffectController
{
	public FillGaugeBuildingActionEffect FillGaugeBuildingActionEffect => base.BuildingActionEffect as FillGaugeBuildingActionEffect;

	public FillGaugeBuildingActionEffectController(FillGaugeBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new FillGaugeBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	public override void ExecuteActionEffect()
	{
		((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)$"Filling building gauge for {base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id} by {FillGaugeBuildingActionEffect.FillGaugeBuildingActionDefinition.Amount} units.", (CLogLevel)1, false, false);
		base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingController.ProductionModuleController.AddProductionUnits(FillGaugeBuildingActionEffect.FillGaugeBuildingActionDefinition.Amount);
	}
}
