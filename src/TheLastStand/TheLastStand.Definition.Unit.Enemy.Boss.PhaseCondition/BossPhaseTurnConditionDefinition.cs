using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;

public class BossPhaseTurnConditionDefinition : Definition, IBossPhaseConditionDefinition
{
	public Node Expression { get; private set; }

	public BossPhaseTurnConditionDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		Expression = Parser.Parse(val.Value, (Dictionary<string, string>)null);
	}
}
