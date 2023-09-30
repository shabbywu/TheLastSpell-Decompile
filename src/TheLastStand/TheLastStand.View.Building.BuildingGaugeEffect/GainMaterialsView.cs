using TPLib.Localization;
using TheLastStand.Model.Building.BuildingGaugeEffect;

namespace TheLastStand.View.Building.BuildingGaugeEffect;

public class GainMaterialsView : BuildingGaugeEffectView
{
	public GainMaterials GainMaterials => base.BuildingGaugeEffect as GainMaterials;

	public override string GetEffectRewardString()
	{
		return Localizer.Format("BuildingGaugeEffectReward_GainMaterials", new object[1] { base.BuildingGaugeEffect.GetProductionValue() });
	}

	protected override string GetProductionRewardIconId()
	{
		return "Material";
	}
}
