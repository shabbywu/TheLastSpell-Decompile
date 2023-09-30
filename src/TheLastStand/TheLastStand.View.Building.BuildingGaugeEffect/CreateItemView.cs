using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Manager.Building;

namespace TheLastStand.View.Building.BuildingGaugeEffect;

public class CreateItemView : BuildingGaugeEffectView
{
	private static class Constants
	{
		public const string MeleeItem = "MeleeItem";

		public const string RangedItem = "RangedItem";

		public const string MagicItem = "MagicItem";

		public const string DefensiveItem = "DefensiveItem";

		public const string Trinket = "Trinket";

		public const string Usable = "Usable";
	}

	public override string GetEffectRewardString()
	{
		string empty = string.Empty;
		switch (base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id)
		{
		case "Blacksmith":
			empty = "MeleeItem";
			break;
		case "Bowyer":
			empty = "RangedItem";
			break;
		case "MagicShop":
			empty = "MagicItem";
			break;
		case "ArmorMaker":
			empty = "DefensiveItem";
			break;
		case "TrinketMaker":
			empty = "Trinket";
			break;
		case "PotionMaker":
			empty = "Usable";
			break;
		default:
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)"No production reward icon id found", (CLogLevel)1, true, true);
			return empty;
		}
		return Localizer.Format("BuildingGaugeEffectReward_Create" + empty, new object[1] { base.BuildingGaugeEffect.GetProductionValue() });
	}

	protected override string GetProductionRewardIconId()
	{
		return base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id switch
		{
			"Blacksmith" => "MeleeItem", 
			"Bowyer" => "RangedItem", 
			"MagicShop" => "MagicItem", 
			"ArmorMaker" => "DefensiveItem", 
			"TrinketMaker" => "Trinket", 
			"PotionMaker" => "Usable", 
			_ => string.Empty, 
		};
	}
}
