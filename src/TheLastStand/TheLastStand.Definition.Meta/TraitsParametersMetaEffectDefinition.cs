using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class TraitsParametersMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "TraitsParameters";

	public int StartTraitTotalPointsModifier;

	public Vector2Int UnitTraitPointsBoundariesModifiers = Vector2Int.zero;

	public TraitsParametersMetaEffectDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
	//IL_0006: Unknown result type (might be due to invalid IL or missing references)


	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("StartTraitTotalPointsModifier"));
		if (val != null && int.TryParse(val.Value, out var result))
		{
			StartTraitTotalPointsModifier = result;
		}
		XElement val2 = obj.Element(XName.op_Implicit("UnitTraitPointsBoundariesModifiers"));
		if (val2 != null)
		{
			XElement val3 = ((XContainer)val2).Element(XName.op_Implicit("Min"));
			XElement val4 = ((XContainer)val2).Element(XName.op_Implicit("Max"));
			if (val3 != null && int.TryParse(val3.Value, out var result2))
			{
				((Vector2Int)(ref UnitTraitPointsBoundariesModifiers)).x = result2;
			}
			if (val4 != null && int.TryParse(val4.Value, out var result3))
			{
				((Vector2Int)(ref UnitTraitPointsBoundariesModifiers)).y = result3;
			}
		}
	}
}
