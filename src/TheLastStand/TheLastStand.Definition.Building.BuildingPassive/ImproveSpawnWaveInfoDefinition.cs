using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class ImproveSpawnWaveInfoDefinition : BuildingPassiveEffectDefinition
{
	public int Level { get; set; } = 1;


	public ImproveSpawnWaveInfoDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		Level = int.Parse(((XContainer)val).Element(XName.op_Implicit("Level")).Value);
	}
}
