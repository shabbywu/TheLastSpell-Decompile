using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit;

public class LinkedHairDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Name { get; private set; }

	public int Weight { get; private set; }

	public LinkedHairDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (int.TryParse(val.Attribute(XName.op_Implicit("Weight")).Value, out var result))
		{
			Weight = result;
		}
		Name = val.Value;
	}
}
