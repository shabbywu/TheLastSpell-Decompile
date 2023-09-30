using System.Globalization;
using System.Xml.Linq;

namespace TheLastStand.Definition.CastFx;

public class PropagationVisualEffectDefinition : VisualEffectDefinition
{
	public float PropagationDelay { get; private set; }

	public PropagationVisualEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = container.Element(XName.op_Implicit("PropagationDelay"));
		PropagationDelay = ((val != null) ? float.Parse(val.Value, CultureInfo.InvariantCulture) : 0f);
	}
}
