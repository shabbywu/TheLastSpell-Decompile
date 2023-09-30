using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Building.Construction;
using UnityEngine;

namespace TheLastStand.View.Building.UI;

public class RepairBuildingFeedback : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI repairTipText;

	public static bool IsDirty { get; set; }

	private void Awake()
	{
		Toggle(toggle: false);
	}

	private void Update()
	{
		if (((Behaviour)repairTipText).enabled && TPSingleton<ConstructionManager>.Instance.Construction.State == TheLastStand.Model.Building.Construction.E_State.None)
		{
			((Behaviour)repairTipText).enabled = false;
		}
		else
		{
			if (!TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged && !IsDirty)
			{
				return;
			}
			Tile previousTile = TPSingleton<GameManager>.Instance.Game.Cursor.PreviousTile;
			Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
			if (previousTile != null && tile == null)
			{
				((Behaviour)repairTipText).enabled = false;
			}
			if ((Object)(object)TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton == (Object)null && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction)
			{
				ConstructionView.ClearRepairTilesFeedback();
			}
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction && TPSingleton<ConstructionManager>.Instance.Construction.State == TheLastStand.Model.Building.Construction.E_State.Repair)
			{
				if (TPSingleton<ConstructionManager>.Instance.Construction.RepairMode != 0)
				{
					if (tile?.Building != null)
					{
						if (tile.Building.ConstructionModule.ConstructionModuleDefinition.IsRepairable)
						{
							switch (TPSingleton<ConstructionManager>.Instance.Construction.RepairMode)
							{
							case TheLastStand.Model.Building.Construction.E_RepairMode.Target:
								UpdateForRepairTarget(tile.Building);
								break;
							case TheLastStand.Model.Building.Construction.E_RepairMode.Id:
								UpdateForRepairId(tile.Building);
								break;
							}
						}
						else
						{
							((TMP_Text)repairTipText).text = Localizer.Format("ConstructionPanel_NotRepairable", new object[1] { tile.Building.BuildingDefinition.Name });
						}
						((Behaviour)repairTipText).enabled = true;
					}
					else
					{
						((Behaviour)repairTipText).enabled = false;
					}
				}
				else
				{
					((Behaviour)repairTipText).enabled = false;
				}
			}
			IsDirty = false;
		}
	}

	private void UpdateForRepairTarget(TheLastStand.Model.Building.Building building)
	{
		if (building.ConstructionModule.NeedRepair)
		{
			if (building.ConstructionModule.CostsGold)
			{
				int repairCost = building.ConstructionModule.RepairCost;
				((TMP_Text)repairTipText).text = (ConstructionManager.CanRepairBuilding(building.ConstructionModule) ? Localizer.Format("ConstructionPanel_RepairTipGold", new object[2]
				{
					building.BuildingDefinition.Name,
					repairCost
				}) : Localizer.Format("ConstructionPanel_CantRepairTipGold", new object[2]
				{
					building.BuildingDefinition.Name,
					repairCost
				}));
			}
			else if (building.ConstructionModule.CostsMaterials)
			{
				int repairCost2 = building.ConstructionModule.RepairCost;
				((TMP_Text)repairTipText).text = (ConstructionManager.CanRepairBuilding(building.ConstructionModule) ? Localizer.Format("ConstructionPanel_RepairTipMaterial", new object[2]
				{
					building.BuildingDefinition.Name,
					repairCost2
				}) : Localizer.Format("ConstructionPanel_CantRepairTipMaterial", new object[2]
				{
					building.BuildingDefinition.Name,
					repairCost2
				}));
			}
		}
		else
		{
			((TMP_Text)repairTipText).text = Localizer.Format("ConstructionPanel_CantRepairNotDamaged", new object[1] { building.BuildingDefinition.Name });
		}
	}

	private void UpdateForRepairId(TheLastStand.Model.Building.Building building)
	{
		List<TheLastStand.Model.Building.Building> buildingsById = BuildingManager.GetBuildingsById(building.BuildingDefinition.Id);
		int num = 0;
		int num2 = 0;
		foreach (TheLastStand.Model.Building.Building item in buildingsById)
		{
			if (item.ConstructionModule.NeedRepair)
			{
				num2++;
				num += item.ConstructionModule.RepairCost;
			}
		}
		if (num == 0 && num2 == 0)
		{
			((TMP_Text)repairTipText).text = Localizer.Format("ConstructionPanel_CantRepairNotDamaged", new object[1] { building.BuildingDefinition.Name });
			return;
		}
		if (building.ConstructionModule.CostsGold)
		{
			((TMP_Text)repairTipText).text = ((TPSingleton<ResourceManager>.Instance.Gold >= num) ? Localizer.Format("ConstructionPanel_RepairAllTipGold", new object[3]
			{
				building.BuildingDefinition.Name,
				num2,
				num
			}) : Localizer.Format("ConstructionPanel_CantRepairAllTipGold", new object[3]
			{
				building.BuildingDefinition.Name,
				num2,
				num
			}));
		}
		else if (building.ConstructionModule.CostsMaterials)
		{
			((TMP_Text)repairTipText).text = ((TPSingleton<ResourceManager>.Instance.Materials >= num) ? Localizer.Format("ConstructionPanel_RepairAllTipMaterial", new object[3]
			{
				building.BuildingDefinition.Name,
				num2,
				num
			}) : Localizer.Format("ConstructionPanel_CantRepairAllTipMaterial", new object[3]
			{
				building.BuildingDefinition.Name,
				num2,
				num
			}));
		}
		foreach (TheLastStand.Model.Building.Building item2 in buildingsById)
		{
			if (item2.ConstructionModule.NeedRepair)
			{
				ConstructionView.DisplayTilesFeedback(item2);
			}
		}
	}

	private void Toggle(bool toggle)
	{
		((Behaviour)repairTipText).enabled = toggle;
	}
}
