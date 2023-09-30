using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class FillEffectGaugeDefinition : BuildingPassiveEffectDefinition
{
	public Node Value { get; set; }

	public FillEffectGaugeDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		Value = Parser.Parse(((XContainer)val).Element(XName.op_Implicit("Value")).Value, (Dictionary<string, string>)null);
	}
}
