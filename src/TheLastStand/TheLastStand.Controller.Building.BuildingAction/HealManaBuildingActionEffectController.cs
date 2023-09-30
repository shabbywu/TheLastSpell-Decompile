using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingAction;

public class HealManaBuildingActionEffectController : BuildingActionEffectController
{
	public HealManaBuildingActionEffect HealManaBuildingActionEffect => base.BuildingActionEffect as HealManaBuildingActionEffect;

	public HealManaBuildingActionEffectController(HealManaBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new HealManaBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return HealManaBuildingActionEffect.HealManaBuildingActionDefinition.BuildingActionTargeting switch
		{
			BuildingActionEffectDefinition.E_BuildingActionTargeting.All => true, 
			BuildingActionEffectDefinition.E_BuildingActionTargeting.Single => tile.Unit is PlayableUnit playableUnit && playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana) < playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal), 
			_ => false, 
		};
	}

	public override void ExecuteActionEffect()
	{
		switch (HealManaBuildingActionEffect.HealManaBuildingActionDefinition.BuildingActionTargeting)
		{
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.All:
		{
			foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				HealMana(playableUnit2, HealManaBuildingActionEffect.HealManaBuildingActionDefinition.Amount);
			}
			break;
		}
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.Single:
			if (base.BuildingActionEffect.Target.Unit is PlayableUnit playableUnit)
			{
				HealMana(playableUnit, HealManaBuildingActionEffect.HealManaBuildingActionDefinition.Amount);
			}
			else
			{
				((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)"Selected Unit is not a playable.", (CLogLevel)1, true, true);
			}
			break;
		}
	}

	private void HealMana(PlayableUnit playableUnit, int amount)
	{
		((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)$"Adding {amount} mana to {playableUnit.Id}.", (CLogLevel)1, false, false);
		float num = playableUnit.PlayableUnitController.GainMana(amount);
		if (!(num <= 0f))
		{
			RestoreStatDisplay pooledComponent = ObjectPooler.GetPooledComponent<RestoreStatDisplay>("RestoreStatDisplay", ResourcePooler.LoadOnce<RestoreStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/RestoreStatDisplay", false), EffectManager.EffectDisplaysParent, false);
			pooledComponent.Init(UnitStatDefinition.E_Stat.Mana, (int)num);
			playableUnit.PlayableUnitController.AddEffectDisplay(pooledComponent);
			GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraitsStats();
		}
	}
}
