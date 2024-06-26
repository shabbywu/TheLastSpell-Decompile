using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Definition.Meta.Glyphs.GlyphEffects;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs;

public class GlyphDefinition : LocalizableDefinition
{
	private static class Constants
	{
		public const string GlyphTitleLocaPrefix = "GlyphName_";

		public const string GlyphDescriptionLocaPrefix = "GlyphDescription_";
	}

	public int Cost { get; private set; }

	public List<GlyphEffectDefinition> GlyphEffectDefinitions { get; private set; }

	public string Id { get; private set; }

	public bool IsCustom { get; private set; }

	public PerkDefinition PerkToShow { get; private set; }

	public GlyphDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public static void AssertIsTrue(bool value, string message)
	{
		if (!value)
		{
			CLoggerManager.Log((object)message, (LogType)1, (CLogLevel)2, true, "GlyphDefinition", false);
		}
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		AssertIsTrue(val != null, "GlyphDefinition received null element in Deserialize.");
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		AssertIsTrue(val2 != null, "Id attribute is missing in GlyphDefinition.");
		Id = val2.Value;
		DeserializeTokenVariables(((XContainer)val).Element(XName.op_Implicit("TokenVariables")));
		base.Deserialize((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("LocArguments")));
		XAttribute val3 = val.Attribute(XName.op_Implicit("Cost"));
		AssertIsTrue(val3 != null, "Cost attribute is missing in GlyphDefinition.");
		AssertIsTrue(int.TryParse(val3.Value, out var result), "Cost attribute can't be parsed into an int : \"" + val3.Value + "\".");
		Cost = result;
		XAttribute val4 = val.Attribute(XName.op_Implicit("IsCustom"));
		if (val4 != null && bool.TryParse(val4.Value, out var result2))
		{
			IsCustom = result2;
		}
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("GlyphEffects"));
		AssertIsTrue(obj != null, "GlyphEffects element is missing in GlyphDefinition (" + Id + ")");
		IEnumerable<XElement> enumerable = ((XContainer)obj).Elements();
		GlyphEffectDefinitions = new List<GlyphEffectDefinition>(enumerable.Count());
		foreach (XElement item in enumerable)
		{
			AddGlyphEffect(item);
		}
	}

	public string GetDescription(InterpreterContext interpreterContext)
	{
		if (base.LocArguments == null)
		{
			return Localizer.Get("GlyphDescription_" + Id);
		}
		return Localizer.Format("GlyphDescription_" + Id, GetArguments(interpreterContext));
	}

	public string GetName()
	{
		return Localizer.Get("GlyphName_" + Id);
	}

	private void AddGlyphEffect(XElement xGlyphEffect)
	{
		GlyphEffectDefinition glyphEffectDefinitionFromName = GetGlyphEffectDefinitionFromName(xGlyphEffect.Name.LocalName, xGlyphEffect);
		if (glyphEffectDefinitionFromName != null)
		{
			if (glyphEffectDefinitionFromName is GlyphNativePerkEffectDefinition { ForceHideTooltip: false } glyphNativePerkEffectDefinition)
			{
				PerkToShow = glyphNativePerkEffectDefinition.PerkDefinition;
			}
			GlyphEffectDefinitions.Add(glyphEffectDefinitionFromName);
		}
		else
		{
			CLoggerManager.Log((object)("GlyphEffectDefinition " + xGlyphEffect.Name.LocalName + " not found."), (LogType)2, (CLogLevel)2, true, "GlyphManager", false);
		}
	}

	private GlyphEffectDefinition GetGlyphEffectDefinitionFromName(string name, XElement xGlyphEffect)
	{
		return name switch
		{
			"AddBuildingPassive" => new GlyphAddBuildingPassiveEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"AdditionalRewardReroll" => new GlyphAdditionalRewardRerollEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"BonusUnits" => new GlyphBonusUnitsEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"BonusSellingRatio" => new GlyphBonusSellingRatioEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"DamnedSoulsPercentageModifier" => new GlyphDamnedSoulsPercentageModifierEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"DamnedSoulsScavengingPercentageModifier" => new GlyphDamnedSoulsScavengingPercentageModifierEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"DecreaseEnemiesCount" => new GlyphDecreaseEnemiesCountEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"FreeTrapUsageChances" => new GlyphFreeTrapUsageChancesEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"GoldScavengingPercentageModifier" => new GlyphGoldScavengingPercentageModifierEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"IncreaseBuildingHealth" => new GlyphIncreaseBuildingHealthEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"IncreaseDefensesDamages" => new GlyphIncreaseDefensesDamagesEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"IncreaseLevelupRerolls" => new GlyphIncreaseLevelupRerollsEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"IncreaseStartingGearLevel" => new GlyphIncreaseStartingGearLevelEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"IncreaseStartingResources" => new GlyphIncreaseStartingResourcesEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"MaterialScavengingPercentageModifier" => new GlyphMaterialScavengingPercentageModifierEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"ModifyBuildingActionsCost" => new GlyphModifyBuildingActionsCostEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"ModifyBuildLimit" => new GlyphModifyBuildLimitEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"ModifyCosts" => new GlyphModifyCostsEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"ModifyItemRarity" => new GlyphModifyItemRarityEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"ModifyLevelProbabilityTree" => new GlyphModifyLevelProbabilityTreeEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"ModifyPlayableUnitsStats" => new GlyphModifyPlayableUnitsStatsEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"ModifyRarityProbabilityTree" => new GlyphModifyRarityProbabilityTreeEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"ModifyRewardsCount" => new GlyphModifyRewardsCountEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"MultiplyItemWeight" => new GlyphMultiplyItemWeightEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"NativePerk" => new GlyphNativePerkEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"NativePerkPointsBonus" => new GlyphNativePerkPointsBonusEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"SetFogCap" => new GlyphSetFogCapEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			"ToggleSkillProgressionFlag" => new GlyphToggleSkillProgressionFlagEffectDefinition((XContainer)(object)xGlyphEffect, base.TokenVariables), 
			_ => null, 
		};
	}
}
