using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.CastFx;

public class FollowFxDefinition : Definition
{
	public Node Delay { get; private set; }

	public Node Speed { get; private set; }

	public FollowFxDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public FollowFxDefinition()
		: this(null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		XElement val = ((container != null) ? container.Element(XName.op_Implicit("Delay")) : null);
		Delay = (Node)((val != null) ? ((object)Parser.Parse(val.Value, (Dictionary<string, string>)null)) : ((object)new NodeNumber(0.0)));
		XElement val2 = ((container != null) ? container.Element(XName.op_Implicit("Speed")) : null);
		Speed = (Node)((val2 != null) ? ((object)Parser.Parse(val2.Value, (Dictionary<string, string>)null)) : ((object)new NodeNumber(-1.0)));
	}
}
