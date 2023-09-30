using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.CastFx;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Definition.CastFx;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.Model.CastFx;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class BuildingUpgradeController
{
	public TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade BuildingUpgrade { get; protected set; }

	public BuildingUpgradeController(SerializedUpgrade container, TheLastStand.Model.Building.Building building)
	{
		BuildingUpgradeDefinition definition = BuildingDatabase.BuildingUpgradeDefinitions[container.Id];
		BuildingUpgrade = CreateModel(container, definition, this, building);
		InitializeCastFxs();
	}

	public BuildingUpgradeController(BuildingUpgradeDefinition definition, TheLastStand.Model.Building.Building building)
	{
		BuildingUpgrade = CreateModel(definition, this, building);
		InitializeCastFxs();
	}

	public void UnlockUpgrade(bool freeUpgrade = false, bool playFx = true)
	{
		if ((!freeUpgrade && TPSingleton<ResourceManager>.Instance.Gold < BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[BuildingUpgrade.UpgradeLevel + 1].GoldCost) || TPSingleton<ResourceManager>.Instance.Materials < BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[BuildingUpgrade.UpgradeLevel + 1].MaterialCost)
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)"Tried to unlock upgrade but the player does not have enough resources.", (CLogLevel)1, false, false);
			return;
		}
		BuildingUpgrade.UpgradeLevel++;
		if (!freeUpgrade)
		{
			TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold - BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[BuildingUpgrade.UpgradeLevel].GoldCost);
			TPSingleton<ResourceManager>.Instance.Materials -= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[BuildingUpgrade.UpgradeLevel].MaterialCost;
		}
		int i = 0;
		for (int count = BuildingUpgrade.BuildingUpgradeLevels[BuildingUpgrade.UpgradeLevel].Effects.Count; i < count; i++)
		{
			BuildingUpgrade.BuildingUpgradeLevels[BuildingUpgrade.UpgradeLevel].Effects[i].BuildingUpgradeEffectController.TriggerEffect();
		}
		if (playFx)
		{
			PlayFx();
		}
	}

	protected virtual void PlayFx()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		(BuildingUpgrade.BuildingUpgradeLevels[BuildingUpgrade.UpgradeLevel].OverrideCastFx ?? BuildingUpgrade.CastFx)?.CastFxController.PlayCastFxs(TileObjectSelectionManager.E_Orientation.NONE, default(Vector2), BuildingUpgrade.Building);
	}

	private TheLastStand.Model.CastFx.CastFx InitializeCastFx(CastFxDefinition castFxDefinition)
	{
		TheLastStand.Model.CastFx.CastFx castFx = new CastFxController(castFxDefinition).CastFx;
		castFx.TargetTile = BuildingUpgrade.Building.OriginTile;
		castFx.SourceTile = BuildingUpgrade.Building.OriginTile;
		castFx.CastFXInterpreterContext = new CastFXInterpreterContext(castFx);
		castFx.AffectedTiles.Add(new List<Tile>(1) { castFx.TargetTile });
		return castFx;
	}

	protected virtual TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade CreateModel(SerializedUpgrade container, BuildingUpgradeDefinition definition, BuildingUpgradeController controller, TheLastStand.Model.Building.Building building)
	{
		return new TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade(container, definition, controller, building);
	}

	protected virtual TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade CreateModel(BuildingUpgradeDefinition definition, BuildingUpgradeController controller, TheLastStand.Model.Building.Building building)
	{
		return new TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade(definition, controller, building);
	}

	private void InitializeCastFxs()
	{
		if (BuildingUpgrade.BuildingUpgradeDefinition.CastFxDefinition != null)
		{
			BuildingUpgrade.CastFx = InitializeCastFx(BuildingUpgrade.BuildingUpgradeDefinition.CastFxDefinition);
		}
		int i = 0;
		for (int count = BuildingUpgrade.BuildingUpgradeLevels.Count; i < count; i++)
		{
			if (BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[i].OverrideCastFxDefinition != null)
			{
				BuildingUpgrade.BuildingUpgradeLevels[i].OverrideCastFx = InitializeCastFx(BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[i].OverrideCastFxDefinition);
			}
		}
	}
}
