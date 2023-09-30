using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class GetAdditionalExperienceEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "GetAdditionalExperience";
	}

	public Node ValueExpression { get; private set; }

	public GetAdditionalExperienceEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val.Value, ((Definition)this).TokenVariables);
	}
}
