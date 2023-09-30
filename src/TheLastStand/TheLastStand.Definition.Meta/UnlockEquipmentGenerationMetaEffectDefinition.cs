using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockEquipmentGenerationMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockEquipmentGeneration";

	public string Id { get; private set; }

	public UnlockEquipmentGenerationMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
	}
}
