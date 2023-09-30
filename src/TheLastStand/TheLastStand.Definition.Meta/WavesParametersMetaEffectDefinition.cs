using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class WavesParametersMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "WavesParameters";

	public int DistanceMaxFromCenterModifier;

	public WavesParametersMetaEffectDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container != null)
		{
			XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("DistanceMaxFromCenterModifier"));
			if (val != null && int.TryParse(val.Value, out var result))
			{
				DistanceMaxFromCenterModifier = result;
			}
		}
	}
}
