using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class NotInFogCondition : GoalConditionDefinition
{
	public const string Name = "NotInFog";

	public Node NbTurns { get; private set; }

	public NotInFogCondition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("NbTurns"));
		if (val != null)
		{
			NbTurns = Parser.Parse(val.Value, (Dictionary<string, string>)null);
		}
	}
}
