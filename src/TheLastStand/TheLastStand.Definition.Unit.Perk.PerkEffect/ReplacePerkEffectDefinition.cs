using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ReplacePerkEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ReplacePerk";
	}

	public string PerkToReplaceId { get; private set; }

	public string PerkReplacementId { get; private set; }

	public ReplacePerkEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("PerkToReplaceId"));
		PerkToReplaceId = val.Value;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("PerkReplacementId"));
		PerkReplacementId = val2.Value;
	}
}
