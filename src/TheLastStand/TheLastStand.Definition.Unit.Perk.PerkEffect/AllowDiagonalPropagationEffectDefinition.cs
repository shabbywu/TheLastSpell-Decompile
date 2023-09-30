using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class AllowDiagonalPropagationEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "AllowDiagonalPropagation";
	}

	public PerkDataConditionsDefinition PerkDataConditionsDefinition { get; private set; }

	public AllowDiagonalPropagationEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		PerkDataConditionsDefinition = new PerkDataConditionsDefinition((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("Conditions")), ((Definition)this).TokenVariables);
	}
}
