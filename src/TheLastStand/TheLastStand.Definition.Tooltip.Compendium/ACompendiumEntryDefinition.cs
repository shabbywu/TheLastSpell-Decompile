using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Tooltip.Compendium;

public abstract class ACompendiumEntryDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public HashSet<string> LinkedEntries { get; private set; }

	protected ACompendiumEntryDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		LinkedEntries = new HashSet<string>();
		XElement val2 = obj.Element(XName.op_Implicit("LinkedEntries"));
		if (val2 == null)
		{
			return;
		}
		foreach (XElement item in ((XContainer)val2).Elements(XName.op_Implicit("LinkedEntry")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("Id"));
			LinkedEntries.Add(val3.Value);
		}
	}
}
