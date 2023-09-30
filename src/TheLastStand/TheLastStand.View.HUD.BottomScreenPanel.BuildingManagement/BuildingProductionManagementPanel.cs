using TMPro;
using TheLastStand.Model.Building;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingProductionManagementPanel : MonoBehaviour
{
	[SerializeField]
	private Image buildingProductionEffectRewardIconImage;

	[SerializeField]
	private TextMeshProUGUI buildingProductionEffectRewardText;

	public void Refresh(TheLastStand.Model.Building.Building building)
	{
		if (building.ProductionModule?.BuildingGaugeEffect == null || building is MagicCircle)
		{
			Disable();
			return;
		}
		((Component)this).gameObject.SetActive(true);
		buildingProductionEffectRewardIconImage.sprite = building.ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectView.GetProductionRewardIconSpriteBig();
		((Behaviour)buildingProductionEffectRewardIconImage).enabled = (Object)(object)buildingProductionEffectRewardIconImage.sprite != (Object)null;
		((Behaviour)buildingProductionEffectRewardText).enabled = true;
		RefreshLocalizedTexts(building);
	}

	public void RefreshLocalizedTexts(TheLastStand.Model.Building.Building building)
	{
		if (building.ProductionModule?.BuildingGaugeEffect != null && !(building is MagicCircle))
		{
			((TMP_Text)buildingProductionEffectRewardText).text = building.ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectView.GetEffectRewardString();
		}
	}

	private void Disable()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
