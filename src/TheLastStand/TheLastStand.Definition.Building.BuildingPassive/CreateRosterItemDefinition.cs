using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class CreateRosterItemDefinition : Definition
{
	public string BuildingLevelModifiersListId { get; set; }

	public CreateItemDefinition CreateItemDefinition { get; private set; }

	public CreateRosterItemDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("BuildingLevelModifiersList"));
		XAttribute obj = val.Attribute(XName.op_Implicit("Id"));
		BuildingLevelModifiersListId = ((obj != null) ? obj.Value : null);
		CreateItemDefinition = new CreateItemDefinition(container);
	}
}
