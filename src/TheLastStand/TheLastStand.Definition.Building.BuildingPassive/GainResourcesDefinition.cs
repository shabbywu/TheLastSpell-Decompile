using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class GainResourcesDefinition : BuildingPassiveEffectDefinition
{
	private int gainDamnedSouls;

	private int gainGold;

	private int gainMaterials;

	public int GainGold
	{
		get
		{
			int num = 0;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num += TPSingleton<GlyphManager>.Instance.GoldScavengingPercentageModifier;
			}
			float num2 = 1f + (float)num / 100f;
			return (int)((float)gainGold * num2);
		}
	}

	public int GainDamnedSouls
	{
		get
		{
			uint num = TPSingleton<ApocalypseManager>.Instance.DamnedSoulsPercentageModifier;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num += (uint)TPSingleton<GlyphManager>.Instance.DamnedSoulsScavengingPercentageModifier;
				num += TPSingleton<GlyphManager>.Instance.DamnedSoulsPercentageModifier;
			}
			float num2 = 1f + (float)num / 100f;
			return (int)((float)gainDamnedSouls * num2);
		}
	}

	public int GainMaterials
	{
		get
		{
			int num = 0;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num += TPSingleton<GlyphManager>.Instance.MaterialScavengingPercentageModifier;
			}
			float num2 = 1f + (float)num / 100f;
			return (int)((float)gainMaterials * num2);
		}
	}

	public GainResourcesDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Gold"));
		if (!string.IsNullOrEmpty((val2 != null) ? val2.Value : null))
		{
			if (!int.TryParse(val2.Value, out var result))
			{
				CLoggerManager.Log((object)"A GainResources BuildingPassiveEffect has an incorrect gold value", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
				return;
			}
			gainGold = result;
		}
		XAttribute val3 = val.Attribute(XName.op_Implicit("Materials"));
		if (!string.IsNullOrEmpty((val3 != null) ? val3.Value : null))
		{
			if (!int.TryParse(val3.Value, out var result2))
			{
				CLoggerManager.Log((object)"A GainResources BuildingPassiveEffect has an incorrect Materials value", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
				return;
			}
			gainMaterials = result2;
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("DamnedSouls"));
		if (!string.IsNullOrEmpty((val4 != null) ? val4.Value : null))
		{
			if (!int.TryParse(val4.Value, out var result3))
			{
				CLoggerManager.Log((object)"A GainResources BuildingPassiveEffect has an incorrect DamnedSouls value", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
			else
			{
				gainDamnedSouls = result3;
			}
		}
	}
}
