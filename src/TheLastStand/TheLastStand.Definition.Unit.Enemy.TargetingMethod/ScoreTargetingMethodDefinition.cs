using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public class ScoreTargetingMethodDefinition : TargetingMethodDefinition
{
	public const string Name = "Score";

	public Node Score { get; private set; }

	public ScoreTargetingMethodDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		Score = Parser.Parse(val.Value, (Dictionary<string, string>)null);
	}
}
