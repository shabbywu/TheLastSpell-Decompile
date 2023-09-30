using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UnitLevelUpStatDefinition : Definition
{
	public Dictionary<UnitLevelUp.E_StatLevelUpRarity, int> Bonuses { get; private set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public float Weight { get; private set; }

	public UnitLevelUpStatDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Stat"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			Debug.LogError((object)"UnitLevelUpStat must have a Stat");
			return;
		}
		if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val2.Value, out var result))
		{
			Debug.LogError((object)"UnitLevelUpStat must have a valid Stat");
			return;
		}
		Stat = result;
		XAttribute val3 = val.Attribute(XName.op_Implicit("Weight"));
		if (XDocumentExtensions.IsNullOrEmpty(val3))
		{
			TPDebug.LogError((object)"UnitLevelUpStat must have a Weight", (Object)null);
			return;
		}
		if (!float.TryParse(val3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			TPDebug.LogError((object)"UnitLevelUpStat Weight must be a valid float", (Object)null);
			return;
		}
		Weight = result2;
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Bonuses"));
		if (XDocumentExtensions.IsNullOrEmpty(val4))
		{
			TPDebug.LogError((object)"UnitLevelUpStat must have a Bonuses", (Object)null);
			return;
		}
		Bonuses = new Dictionary<UnitLevelUp.E_StatLevelUpRarity, int>();
		XElement val5 = ((XContainer)val4).Element(XName.op_Implicit("BigBonus"));
		XElement val6 = ((XContainer)val4).Element(XName.op_Implicit("MediumBonus"));
		XElement val7 = ((XContainer)val4).Element(XName.op_Implicit("SmallBonus"));
		if (val5 != null)
		{
			if (!int.TryParse(val5.Value, out var result3))
			{
				TPDebug.LogError((object)"UnitLevelUpStat BigBonus must be a valid integer", (Object)null);
				return;
			}
			Bonuses.Add(UnitLevelUp.E_StatLevelUpRarity.BigRarity, result3);
		}
		if (val6 != null)
		{
			if (!int.TryParse(val6.Value, out var result4))
			{
				TPDebug.LogError((object)"UnitLevelUpStat MediumBonus must be a valid integer", (Object)null);
				return;
			}
			Bonuses.Add(UnitLevelUp.E_StatLevelUpRarity.MediumRarity, result4);
		}
		if (val7 != null)
		{
			if (!int.TryParse(val7.Value, out var result5))
			{
				TPDebug.LogError((object)"UnitLevelUpStat SmallBonus must be a valid integer", (Object)null);
			}
			else
			{
				Bonuses.Add(UnitLevelUp.E_StatLevelUpRarity.SmallRarity, result5);
			}
		}
	}
}
