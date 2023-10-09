using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit;

public class UnitFaceIdDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int Weight { get; private set; }

	public string FaceId { get; private set; }

	public UnitFaceIdDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Weight"));
		if (val2 != null && int.TryParse(val2.Value, out var result))
		{
			Weight = result;
		}
		FaceId = val.Value;
	}
}
