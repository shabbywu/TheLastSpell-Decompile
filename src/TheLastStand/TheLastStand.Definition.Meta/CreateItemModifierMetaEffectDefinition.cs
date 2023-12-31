using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta;

public class CreateItemModifierMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "CreateItemModifier";

	public string CreateItemId { get; private set; }

	public Node Count { get; private set; }

	public CreateItemModifierMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("CreateItemId"));
		CreateItemId = val.Value;
		XElement val2 = obj.Element(XName.op_Implicit("Count"));
		Count = (val2.IsNullOrEmpty() ? Parser.Parse("1") : Parser.Parse(val2.Value));
	}
}
