using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingAction;

public class GainGoldBuildingActionEffectController : BuildingActionEffectController
{
	public GainGoldBuildingActionEffect GainGoldBuildingActionEffect => base.BuildingActionEffect as GainGoldBuildingActionEffect;

	public GainGoldBuildingActionEffectController(GainGoldBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new GainGoldBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	public override void ExecuteActionEffect()
	{
		if (base.BuildingActionEffect.ProductionBuilding.BuildingParent.BlueprintModule.IsIndestructible || !base.BuildingActionEffect.ProductionBuilding.BuildingParent.DamageableModule.IsDead)
		{
			TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + GainGoldBuildingActionEffect.GainGoldBuildingActionDefinition.GainGold);
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)$"Gaining {GainGoldBuildingActionEffect.GainGoldBuildingActionDefinition.GainGold} Gold from GainGold building action effect.", (CLogLevel)1, false, false);
			GainGoldDisplay pooledComponent = ObjectPooler.GetPooledComponent<GainGoldDisplay>("GainGoldDisplay", ResourcePooler.LoadOnce<GainGoldDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainGoldDisplay", false), EffectManager.EffectDisplaysParent, false);
			pooledComponent.Init(GainGoldBuildingActionEffect.GainGoldBuildingActionDefinition.GainGold);
			base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		}
	}
}
