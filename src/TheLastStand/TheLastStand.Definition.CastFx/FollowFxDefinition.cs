using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.CastFx;

public class FollowFxDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Node Delay { get; private set; }

	public Node Speed { get; private set; }

	public FollowFxDefinition(XContainer container)
		: base(container)
	{
	}

	public FollowFxDefinition()
		: this(null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = ((container != null) ? container.Element(XName.op_Implicit("Delay")) : null);
		Delay = ((val != null) ? Parser.Parse(val.Value) : new NodeNumber(0.0));
		XElement val2 = ((container != null) ? container.Element(XName.op_Implicit("Speed")) : null);
		Speed = ((val2 != null) ? Parser.Parse(val2.Value) : new NodeNumber(-1.0));
	}
}
