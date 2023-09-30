using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class InitResourcesBonusMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "InitResourcesBonus";

	public int GoldBonus { get; private set; }

	public int MaterialsBonus { get; private set; }

	public InitResourcesBonusMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("GoldBonus"));
		if (val != null)
		{
			if (int.TryParse(val.Value, out var result))
			{
				GoldBonus = result;
			}
			else
			{
				Debug.LogError((object)"GoldBonus element as an invalid value!");
			}
		}
		XElement val2 = obj.Element(XName.op_Implicit("MaterialsBonus"));
		if (val2 != null)
		{
			if (int.TryParse(val2.Value, out var result2))
			{
				MaterialsBonus = result2;
			}
			else
			{
				Debug.LogError((object)"MaterialsBonus element as an invalid value!");
			}
		}
	}

	public override string ToString()
	{
		return string.Format("{0} ({1} gold / {2} materials)", "InitResourcesBonus", GoldBonus, MaterialsBonus);
	}
}
