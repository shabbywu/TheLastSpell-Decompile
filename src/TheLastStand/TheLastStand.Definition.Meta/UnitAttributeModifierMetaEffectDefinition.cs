using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class UnitAttributeModifierMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "PlayableUnitAttributeModifier";

	public const string AllArchetypesId = "All";

	public string Archetype { get; private set; }

	public bool AllArchetypes => Archetype == "All";

	public Dictionary<UnitStatDefinition.E_Stat, Vector2> StatAndValue { get; } = new Dictionary<UnitStatDefinition.E_Stat, Vector2>();


	public UnitAttributeModifierMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		if (container == null)
		{
			return;
		}
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("Archetype"));
		if (val != null)
		{
			Archetype = val.Value;
		}
		foreach (XElement item in ((XContainer)obj.Element(XName.op_Implicit("StatsBonus"))).Elements(XName.op_Implicit("StatBonus")))
		{
			XElement val2 = ((XContainer)item).Element(XName.op_Implicit("Stat"));
			XElement val3 = ((XContainer)item).Element(XName.op_Implicit("Bonus"));
			if (val2 == null || val3 == null)
			{
				Debug.LogError((object)"xStat or xBonus doesn't exist !");
				continue;
			}
			XAttribute val4 = val2.Attribute(XName.op_Implicit("Id"));
			XAttribute val5 = val3.Attribute(XName.op_Implicit("Min"));
			XAttribute val6 = val3.Attribute(XName.op_Implicit("Max"));
			int result2;
			int result3;
			if (val4 == null || !Enum.TryParse<UnitStatDefinition.E_Stat>(val4.Value, out var result))
			{
				Debug.LogError((object)"xStatId element as an invalid value! Or xStatId doesn't exist !");
			}
			else if (val5 == null || !int.TryParse(val5.Value, out result2))
			{
				Debug.LogError((object)"Min attribute as an invalid value! Or Min doesn't exist !");
			}
			else if (val6 == null || !int.TryParse(val6.Value, out result3))
			{
				Debug.LogError((object)"Max attribute as an invalid value! Or Max doesn't exist !");
			}
			else if (StatAndValue.ContainsKey(result))
			{
				Dictionary<UnitStatDefinition.E_Stat, Vector2> statAndValue = StatAndValue;
				UnitStatDefinition.E_Stat key = result;
				statAndValue[key] += new Vector2((float)result2, (float)result3);
			}
			else
			{
				StatAndValue.Add(result, new Vector2((float)result2, (float)result3));
			}
		}
	}

	public override string ToString()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		string text = "PlayableUnitAttributeModifier Archetype : <b>" + Archetype + "</b>\r\n Stats and Value :\r\n";
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, Vector2> item in StatAndValue)
		{
			text += $"\tStat : <b>{item.Key}</b> Value : <b>Min: {item.Value.x} ; Max: {item.Value.y}</b>\r\n";
		}
		return text;
	}
}
