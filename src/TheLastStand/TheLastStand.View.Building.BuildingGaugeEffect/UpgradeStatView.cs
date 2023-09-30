using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Manager.Building;

namespace TheLastStand.View.Building.BuildingGaugeEffect;

public class UpgradeStatView : BuildingGaugeEffectView
{
	private static class Constants
	{
		public const string Temple = "Temple";

		public const string ManaWell = "ManaWell";
	}

	public override string GetEffectRewardString()
	{
		string text = base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id switch
		{
			"Temple" => "Temple", 
			"ManaWell" => "ManaWell", 
			_ => string.Empty, 
		};
		if (string.IsNullOrEmpty(text))
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)"No production reward icon id found", (CLogLevel)1, true, true);
		}
		return Localizer.Format("BuildingGaugeEffectReward_" + text, new object[1] { base.BuildingGaugeEffect.GetProductionValue() });
	}

	protected override string GetProductionRewardIconId()
	{
		return base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id switch
		{
			"Temple" => "Temple", 
			"ManaWell" => "ManaWell", 
			_ => string.Empty, 
		};
	}
}
