using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Apocalypse;

public class ApocalypsesDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<ApocalypseDefinition> ApocalypseDefinitions { get; private set; } = new List<ApocalypseDefinition>();


	public ApocalypsesDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		foreach (XElement item2 in ((container is XElement) ? container : null).Elements(XName.op_Implicit("ApocalypseDefinition")))
		{
			ApocalypseDefinition item = new ApocalypseDefinition((XContainer)(object)item2);
			ApocalypseDefinitions.Add(item);
		}
		ApocalypseDefinitions = ApocalypseDefinitions.OrderBy((ApocalypseDefinition x) => x.Id).ToList();
	}
}
