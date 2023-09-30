using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.BuildingPassive;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class BuildingUpgradeDefinition : Definition
{
	public class LeveledBuildingUpgradeDefinition : Definition
	{
		private int goldCost;

		private int materialCost;

		public List<BuildingUpgradeEffectDefinition> BuildingUpgradeEffectDefinitions { get; private set; }

		public int DefaultGoldCost => goldCost;

		public int DefaultMaterialsCost => materialCost;

		public int GoldCost
		{
			get
			{
				int num = ResourceManager.ComputeExtraPercentageForCost(ResourceManager.E_PriceModifierType.BuildingUpgrades);
				return goldCost + Mathf.RoundToInt((float)(goldCost * num) / 100f);
			}
			private set
			{
				goldCost = value;
			}
		}

		public int MaterialCost
		{
			get
			{
				int num = ResourceManager.ComputeExtraPercentageForCost(ResourceManager.E_PriceModifierType.BuildingUpgrades);
				return materialCost + Mathf.RoundToInt((float)(materialCost * num) / 100f);
			}
			private set
			{
				materialCost = value;
			}
		}

		public CastFxDefinition OverrideCastFxDefinition { get; set; }

		public LeveledBuildingUpgradeDefinition(XContainer container)
			: base(container, (Dictionary<string, string>)null)
		{
		}

		public override void Deserialize(XContainer container)
		{
			XElement val = (XElement)(object)((container is XElement) ? container : null);
			XElement val2 = ((XContainer)val).Element(XName.op_Implicit("GoldCost"));
			if (!XDocumentExtensions.IsNullOrEmpty(val2))
			{
				if (!int.TryParse(val2.Value, out var result))
				{
					TPDebug.LogError((object)"BuildingUpgradeDefinition UpgradeLevel must have a valid GoldCost (int)", (Object)null);
					return;
				}
				GoldCost = result;
			}
			XElement val3 = ((XContainer)val).Element(XName.op_Implicit("MaterialCost"));
			if (!XDocumentExtensions.IsNullOrEmpty(val3))
			{
				if (!int.TryParse(val3.Value, out var result2))
				{
					TPDebug.LogError((object)"BuildingUpgradeDefinition UpgradeLevel must have a valid MaterialCost (int)", (Object)null);
					return;
				}
				MaterialCost = result2;
			}
			XElement val4 = ((XContainer)val).Element(XName.op_Implicit("UpgradeEffects"));
			if (val4 == null)
			{
				TPDebug.LogError((object)"BuildingUpgradeDefinition UpgradeLevel must have a UpgradeEffects", (Object)null);
				return;
			}
			BuildingUpgradeEffectDefinitions = new List<BuildingUpgradeEffectDefinition>();
			foreach (XElement item12 in ((XContainer)val4).Elements(XName.op_Implicit("UpgradeEffect")))
			{
				XAttribute val5 = item12.Attribute(XName.op_Implicit("Id"));
				if (XDocumentExtensions.IsNullOrEmpty(val5))
				{
					TPDebug.LogError((object)"BuildingUpgradeEffectDefinition must have an Id", (Object)null);
					return;
				}
				switch (val5.Value)
				{
				case "UnlockAction":
				{
					UnlockActionDefinition item11 = new UnlockActionDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item11);
					break;
				}
				case "SwapAction":
				{
					SwapActionDefinition item10 = new SwapActionDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item10);
					break;
				}
				case "ImproveGaugeEffect":
				{
					ImproveGaugeEffectDefinition item9 = new ImproveGaugeEffectDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item9);
					break;
				}
				case "ImproveGuessWho":
				{
					ImproveGuessWhoDefinition item8 = new ImproveGuessWhoDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item8);
					break;
				}
				case "ImprovePassive":
				{
					ImprovePassiveDefinition item7 = new ImprovePassiveDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item7);
					break;
				}
				case "ImproveLevel":
				{
					ImproveLevelDefinition item6 = new ImproveLevelDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item6);
					break;
				}
				case "ImproveUnitLimit":
				{
					ImproveUnitLimitDefinition item5 = new ImproveUnitLimitDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item5);
					break;
				}
				case "ImproveSellRatio":
				{
					ImproveSellRatioDefinition item4 = new ImproveSellRatioDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item4);
					break;
				}
				case "ReplaceBuilding":
				{
					ReplaceBuildingDefinition item3 = new ReplaceBuildingDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item3);
					break;
				}
				case "SwapSkill":
				{
					SwapSkillDefinition item2 = new SwapSkillDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item2);
					break;
				}
				case "IncreasePlaysPerTurn":
				{
					IncreasePlaysPerTurnDefinition item = new IncreasePlaysPerTurnDefinition((XContainer)(object)item12);
					BuildingUpgradeEffectDefinitions.Add(item);
					break;
				}
				}
			}
			XElement val6 = ((XContainer)val).Element(XName.op_Implicit("CastFXs"));
			if (val6 != null)
			{
				OverrideCastFxDefinition = new CastFxDefinition((XContainer)(object)val6);
			}
		}
	}

	public CastFxDefinition CastFxDefinition { get; set; }

	public string Id { get; private set; }

	public bool IsGlobal { get; private set; }

	public List<string> LinkedUpgradesIds { get; private set; }

	public List<LeveledBuildingUpgradeDefinition> LeveledBuildingUpgradeDefinitions { get; private set; }

	public string LoreDescription => string.Empty;

	public BuildingUpgradeDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			TPDebug.LogError((object)"BuildingUpgradeDefinition must have an Id", (Object)null);
			return;
		}
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("IsGlobal"));
		if (!XDocumentExtensions.IsNullOrEmpty(val3))
		{
			IsGlobal = bool.Parse(val3.Value);
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("UpgradeLevels"));
		if (val4 == null)
		{
			TPDebug.LogError((object)("BuildingUpgradeDefinition " + Id + " must have a UpgradeLevels"), (Object)null);
			return;
		}
		LeveledBuildingUpgradeDefinitions = new List<LeveledBuildingUpgradeDefinition>();
		foreach (XElement item2 in ((XContainer)val4).Elements(XName.op_Implicit("UpgradeLevel")))
		{
			LeveledBuildingUpgradeDefinition item = new LeveledBuildingUpgradeDefinition((XContainer)(object)item2);
			LeveledBuildingUpgradeDefinitions.Add(item);
		}
		LinkedUpgradesIds = new List<string>();
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("LinkedUpgrades"));
		if (val5 != null)
		{
			foreach (XElement item3 in ((XContainer)val5).Elements(XName.op_Implicit("LinkedUpgrade")))
			{
				if (XDocumentExtensions.IsNullOrEmpty(item3.Attribute(XName.op_Implicit("Id"))))
				{
					TPDebug.LogError((object)("BuildingUpgradeDefinition " + Id + " LinkedUpgrades must all have an Id"), (Object)null);
					return;
				}
				LinkedUpgradesIds.Add(item3.Attribute(XName.op_Implicit("Id")).Value);
			}
		}
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("CastFXs"));
		if (val6 != null)
		{
			CastFxDefinition = new CastFxDefinition((XContainer)(object)val6);
		}
	}

	public string GetDescriptionAtLevel(int level, TheLastStand.Model.Building.Building building, bool isAlreadyActive)
	{
		LeveledBuildingUpgradeDefinition leveledBuildingUpgradeDefinition = LeveledBuildingUpgradeDefinitions[level];
		if (building == null)
		{
			return Localizer.Get(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipDescription_", Id, level));
		}
		int num = 0;
		switch (leveledBuildingUpgradeDefinition.BuildingUpgradeEffectDefinitions[0].Id)
		{
		case "ImprovePassive":
		{
			ImprovePassiveDefinition improvePassiveDefinition = leveledBuildingUpgradeDefinition.BuildingUpgradeEffectDefinitions[0] as ImprovePassiveDefinition;
			if (building.PassivesModule?.BuildingPassives == null)
			{
				break;
			}
			FillEffectGauge fillEffectGauge = null;
			for (int i = 0; i < building.PassivesModule.BuildingPassives.Count; i++)
			{
				int num3 = 0;
				FillEffectGauge fillEffectGauge2;
				while (num3 < building.PassivesModule.BuildingPassives[i].PassiveEffects.Count)
				{
					fillEffectGauge2 = building.PassivesModule.BuildingPassives[i].PassiveEffects[num3] as FillEffectGauge;
					if (fillEffectGauge2 == null)
					{
						num3++;
						continue;
					}
					goto IL_00d0;
				}
				continue;
				IL_00d0:
				fillEffectGauge = fillEffectGauge2;
				break;
			}
			if (fillEffectGauge == null)
			{
				return Localizer.Get(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipDescription_", Id, level));
			}
			num = improvePassiveDefinition.Value.EvalToInt();
			int num4 = (isAlreadyActive ? (fillEffectGauge.CurrentValue - num) : fillEffectGauge.CurrentValue);
			num4 /= building.ProductionModule.BuildingGaugeEffect.UnitsThreshold;
			int num5 = (isAlreadyActive ? fillEffectGauge.CurrentValue : (fillEffectGauge.CurrentValue + num));
			num5 /= building.ProductionModule.BuildingGaugeEffect.UnitsThreshold;
			return Localizer.Format(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipDescription_", Id, level), new object[2] { num4, num5 });
		}
		case "ImproveGaugeEffect":
		{
			ImproveGaugeEffectDefinition improveGaugeEffectDefinition = leveledBuildingUpgradeDefinition.BuildingUpgradeEffectDefinitions[0] as ImproveGaugeEffectDefinition;
			TheLastStand.Model.Building.BuildingGaugeEffect.BuildingGaugeEffect buildingGaugeEffect = building.ProductionModule?.BuildingGaugeEffect;
			if (buildingGaugeEffect != null)
			{
				num = improveGaugeEffectDefinition.Value;
				float num2 = 0f;
				switch (buildingGaugeEffect.BuildingGaugeEffectDefinition.Id)
				{
				case "GainGold":
				{
					int upgradedBonusValue2 = (building.ProductionModule.BuildingGaugeEffect as GainGold).UpgradedBonusValue;
					num2 = (buildingGaugeEffect.BuildingGaugeEffectDefinition as GainGoldDefinition).GoldGain.EvalToFloat((InterpreterContext)(object)new FormulaInterpreterContext());
					num2 += (float)upgradedBonusValue2;
					break;
				}
				case "GainMaterials":
				{
					int upgradedBonusValue = (building.ProductionModule.BuildingGaugeEffect as GainMaterials).UpgradedBonusValue;
					num2 = (buildingGaugeEffect.BuildingGaugeEffectDefinition as GainMaterialsDefinition).MaterialsGain.EvalToFloat();
					num2 += (float)upgradedBonusValue;
					break;
				}
				}
				return Localizer.Format(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipDescription_", Id, level), new object[2]
				{
					isAlreadyActive ? (num2 - (float)num) : num2,
					isAlreadyActive ? num2 : (num2 + (float)num)
				});
			}
			break;
		}
		}
		return Localizer.Get(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipDescription_", Id, level));
	}

	public string GetNameAtLevel(int level)
	{
		return Localizer.Get(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipName_", Id, level));
	}
}
