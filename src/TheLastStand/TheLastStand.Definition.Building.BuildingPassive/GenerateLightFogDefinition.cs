using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class GenerateLightFogDefinition : BuildingPassiveEffectDefinition
{
	public bool CanLightFogExistOnSelf { get; private set; }

	public GenerateLightFogDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		CanLightFogExistOnSelf = ((XContainer)val).Element(XName.op_Implicit("CanLightFogExistOnSelf")) != null;
	}
}
