using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class ScavengeBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	private int gainDamnedSouls;

	private int gainGold;

	private int gainMaterials;

	public List<CreateItemDefinition> CreateItemDefinitions { get; } = new List<CreateItemDefinition>();


	public int Damage { get; private set; }

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

	public ScavengeBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("GainGold"));
		if (!XDocumentExtensions.IsNullOrEmpty(val2))
		{
			if (!int.TryParse(val2.Value, out var result))
			{
				Debug.LogError((object)"A ScavengeGold Building ActionEffect must have a valid GainGold (int)");
				return;
			}
			gainGold = result;
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("GainMaterials"));
		if (!XDocumentExtensions.IsNullOrEmpty(val3))
		{
			if (!int.TryParse(val3.Value, out var result2))
			{
				Debug.LogError((object)"A ScavengeMaterials Building ActionEffect must have a valid GainMaterials (int)");
				return;
			}
			gainMaterials = result2;
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("GainDamnedSouls"));
		if (!XDocumentExtensions.IsNullOrEmpty(val4))
		{
			if (!int.TryParse(val4.Value, out var result3))
			{
				Debug.LogError((object)"A ScavengeDamnedSouls Building ActionEffect must have a valid GainDamnedSouls (int)");
				return;
			}
			gainDamnedSouls = result3;
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("CreateItem")))
		{
			CreateItemDefinitions.Add(new CreateItemDefinition((XContainer)(object)item));
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("Damage"));
		int result4;
		if (XDocumentExtensions.IsNullOrEmpty(val5))
		{
			Debug.LogError((object)"A ScavengeGold Building ActionEffect must have a Damage element");
		}
		else if (!int.TryParse(val5.Value, out result4))
		{
			Debug.LogError((object)"A ScavengeGold Building ActionEffect must have a valid Damage (int)");
		}
		else
		{
			Damage = result4;
		}
	}
}
