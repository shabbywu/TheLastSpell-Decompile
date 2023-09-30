using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Controller.Meta;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class ConstructionModuleDefinition : BuildingModuleDefinition
{
	public int NativeBuildLimit { get; private set; } = -1;


	public string BuildLimitGroupId { get; private set; } = string.Empty;


	public int ConstructionAnimationFrameRate { get; private set; }

	public int ConstructionAnimationShockwaveFrame { get; private set; }

	public BuildingDefinition.E_ConstructionAnimationType ConstructionAnimationType { get; private set; } = BuildingDefinition.E_ConstructionAnimationType.Instantaneous;


	public BuildingDefinition.E_ConstructionAnimationType DestructionAnimationType { get; private set; } = BuildingDefinition.E_ConstructionAnimationType.Instantaneous;


	public List<GroundDefinition.E_GroundCategory> GroundCategories { get; private set; }

	public bool IsBuyable { get; private set; }

	public bool IsDemolishable { get; private set; }

	public bool IsRepairable { get; private set; }

	public int NativeGoldCost { get; private set; }

	public int NativeMaterialsCost { get; private set; }

	public BuildingDefinition.E_OccupationVolumeType OccupationVolumeType { get; private set; } = BuildingDefinition.E_OccupationVolumeType.Adjacent;


	public bool ShouldDisplayConstructionTileFeedback { get; private set; }

	public bool PlayConstructionSound { get; private set; }

	public bool PlayDestructionSound { get; private set; }

	public ConstructionModuleDefinition(BuildingDefinition buildingDefinition, XContainer constructionDefinition)
		: base(buildingDefinition, constructionDefinition)
	{
	}

	public bool IsUnlimited(bool useDefault = false)
	{
		return GetBuildLimit(useDefault) < 0;
	}

	public int GetBuildLimit(bool useDefault = false)
	{
		if (BuildingDatabase.BuildingLimitGroupDefinitions.TryGetValue(BuildLimitGroupId, out var value))
		{
			return value.GetBuildLimit(useDefault);
		}
		if (useDefault)
		{
			return NativeBuildLimit;
		}
		int num = 0;
		if (NativeBuildLimit != -1)
		{
			if (MetaUpgradeEffectsController.TryGetEffectsOfType<BuildingModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
			{
				for (int num2 = effects.Length - 1; num2 >= 0; num2--)
				{
					if (effects[num2].BuildingId == BuildingDefinition.Id && effects[num2].MaxCityInstancesBonus != -1)
					{
						num += effects[num2].MaxCityInstancesBonus;
					}
				}
			}
			num += DictionaryExtensions.GetValueOrDefault<string, int>(TPSingleton<GlyphManager>.Instance.BuildLimitModifiers, BuildingDefinition.Id);
		}
		return NativeBuildLimit + num;
	}

	public string GetLocalizedBuildLimit(bool useDefaultValues = false)
	{
		if (BuildingDatabase.BuildingLimitGroupDefinitions.TryGetValue(BuildLimitGroupId, out var value))
		{
			string text = string.Empty;
			foreach (string buildingId in value.BuildingIds)
			{
				if (BuildingDatabase.BuildingDefinitions.TryGetValue(buildingId, out var value2))
				{
					text += ((text == string.Empty) ? value2.Name : (Localizer.Get("Generic_EnumerableSeparator") + value2.Name));
				}
			}
			return Localizer.Format("ConstructionPanel_BuildLimitGroupTooltip", new object[3]
			{
				GetBuildLimit(useDefaultValues),
				(!useDefaultValues) ? TPSingleton<ConstructionManager>.Instance.GetBuildingCount(this) : 0,
				text
			});
		}
		return Localizer.Format("ConstructionPanel_BuildLimitTooltip", new object[2]
		{
			GetBuildLimit(useDefaultValues),
			(!useDefaultValues) ? TPSingleton<ConstructionManager>.Instance.GetBuildingCount(this) : 0
		});
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("ConstructionAnimationTypes"));
		XElement val3 = ((val2 != null) ? ((XContainer)val2).Element(XName.op_Implicit("ConstructionAnimationType")) : null);
		if (val3 != null)
		{
			if (!Enum.TryParse<BuildingDefinition.E_ConstructionAnimationType>(val3.Attribute(XName.op_Implicit("Value")).Value, out var result))
			{
				CLoggerManager.Log((object)("Building " + BuildingDefinition.Id + " must have a valid ConstructionAnimationType!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			ConstructionAnimationType = result;
			if (ConstructionAnimationType == BuildingDefinition.E_ConstructionAnimationType.Animated)
			{
				XElement val4 = ((XContainer)val2).Element(XName.op_Implicit("ConstructionAnimationFrameRate"));
				if (val4 != null)
				{
					XAttribute val5 = val4.Attribute(XName.op_Implicit("Value"));
					if (val5 != null)
					{
						if (int.TryParse(val5.Value, out var result2))
						{
							ConstructionAnimationFrameRate = Mathf.Clamp(result2, 1, int.MaxValue);
						}
						else
						{
							CLoggerManager.Log((object)("BuildingDefinition " + BuildingDefinition.Id + " ConstructionAnimationFrameRate has an invalid value, setting it to 20."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
							ConstructionAnimationFrameRate = 20;
						}
					}
				}
				else
				{
					CLoggerManager.Log((object)("BuildingDefinition " + BuildingDefinition.Id + " is set to animated but ConstructionAnimationFrameRate is not defined, setting it to 20."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
					ConstructionAnimationFrameRate = 20;
				}
				XElement val6 = ((XContainer)val2).Element(XName.op_Implicit("ConstructionAnimationShockwaveFrame"));
				if (val6 != null)
				{
					XAttribute val7 = val6.Attribute(XName.op_Implicit("Value"));
					if (val7 != null)
					{
						if (int.TryParse(val7.Value, out var result3))
						{
							ConstructionAnimationShockwaveFrame = Mathf.Clamp(result3, 0, int.MaxValue);
						}
						else
						{
							CLoggerManager.Log((object)("BuildingDefinition " + BuildingDefinition.Id + " ConstructionAnimationShockwaveFrame has an invalid value, setting it to -1 (no shockwave)."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
							ConstructionAnimationShockwaveFrame = -1;
						}
					}
				}
				else
				{
					ConstructionAnimationShockwaveFrame = -1;
				}
			}
		}
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("DestructionAnimationTypes"));
		XElement val8 = ((obj != null) ? ((XContainer)obj).Element(XName.op_Implicit("DestructionAnimationType")) : null);
		if (val8 != null)
		{
			if (!Enum.TryParse<BuildingDefinition.E_ConstructionAnimationType>(val8.Attribute(XName.op_Implicit("Value")).Value, out var result4))
			{
				CLoggerManager.Log((object)("Building " + BuildingDefinition.Id + " must have a valid DestructionAnimationType!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			DestructionAnimationType = result4;
		}
		XElement val9 = ((XContainer)val).Element(XName.op_Implicit("OccupationVolumeType"));
		if (val9 != null)
		{
			if (!Enum.TryParse<BuildingDefinition.E_OccupationVolumeType>(val9.Value, out var result5))
			{
				CLoggerManager.Log((object)("BuildingDefinition " + BuildingDefinition.Id + " must have a valid OccupationVolumeType!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			OccupationVolumeType = result5;
		}
		GroundCategories = new List<GroundDefinition.E_GroundCategory>();
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("GroundCategories"))).Elements(XName.op_Implicit("GroundCategory")))
		{
			if (Enum.TryParse<GroundDefinition.E_GroundCategory>(item.Value, out var result6))
			{
				GroundCategories.Add(result6);
				continue;
			}
			CLoggerManager.Log((object)("Error while parsing GroundCategory for " + BuildingDefinition.Id + " : " + item.Value + " is not a valid GroundType"), (LogType)0, (CLogLevel)1, true, ((object)this).GetType().Name, false);
		}
		ShouldDisplayConstructionTileFeedback = OccupationVolumeType != BuildingDefinition.E_OccupationVolumeType.Ignore || (GroundCategories.Count == 1 && GroundCategories[0] == GroundDefinition.E_GroundCategory.City);
		XElement val10 = ((XContainer)val).Element(XName.op_Implicit("MaterialsCost"));
		if (val10 != null)
		{
			if (int.TryParse(val10.Value, out var result7))
			{
				NativeMaterialsCost = result7;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse the MaterialsCost element in " + BuildingDefinition.Id + " into an int : " + val10.Value + "."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		else
		{
			NativeMaterialsCost = 0;
		}
		XElement val11 = ((XContainer)val).Element(XName.op_Implicit("GoldCost"));
		if (val11 != null)
		{
			if (int.TryParse(val11.Value, out var result8))
			{
				NativeGoldCost = result8;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse the GoldCost element in " + BuildingDefinition.Id + " into an int : " + val11.Value + "."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		else
		{
			NativeGoldCost = 0;
		}
		XElement val12 = ((XContainer)val).Element(XName.op_Implicit("IsBuyable"));
		IsBuyable = ((val12 != null) ? bool.Parse(val12.Value) : (NativeGoldCost > 0 || NativeMaterialsCost > 0));
		XElement val13 = ((XContainer)val).Element(XName.op_Implicit("IsRepairable"));
		IsRepairable = ((val13 != null) ? bool.Parse(val13.Value) : (NativeGoldCost > 0 || NativeMaterialsCost > 0));
		XElement val14 = ((XContainer)val).Element(XName.op_Implicit("IsDemolishable"));
		IsDemolishable = ((val14 != null) ? bool.Parse(val14.Value) : IsBuyable);
		XElement val15 = ((XContainer)val).Element(XName.op_Implicit("BuildLimit"));
		if (!XDocumentExtensions.IsNullOrEmpty(val15))
		{
			BuildingLimitGroupDefinition value;
			if (sbyte.TryParse(val15.Value, out var result9))
			{
				if (result9 == 0)
				{
					CLoggerManager.Log((object)("BuildingDefinition " + BuildingDefinition.Id + " has an invalid BuildLimit (must be in the range -1 to 127, excluding 0)!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					NativeBuildLimit = result9;
				}
			}
			else if (BuildingDatabase.BuildingLimitGroupDefinitions.TryGetValue(val15.Value, out value))
			{
				BuildLimitGroupId = value.Id;
				value.BuildingIds.Add(BuildingDefinition.Id);
			}
			else
			{
				CLoggerManager.Log((object)("BuildingDefinition " + BuildingDefinition.Id + " has an invalid BuildLimit (must be in the range -1 to 127 excluding 0 OR an existing BuildingLimitGroup Id)!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		PlayConstructionSound = ((XContainer)val).Element(XName.op_Implicit("MuteConstructionSound")) == null;
		PlayDestructionSound = ((XContainer)val).Element(XName.op_Implicit("MuteDestructionSound")) == null;
	}
}
