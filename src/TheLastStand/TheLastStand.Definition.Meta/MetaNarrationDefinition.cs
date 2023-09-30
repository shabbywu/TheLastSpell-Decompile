using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta;

public class MetaNarrationDefinition : Definition
{
	public List<string> DialogueGreetings { get; private set; }

	public List<MetaReplicaDefinition> MandatoryReplicaDefinitions { get; private set; }

	public string NameRevealDialogueId { get; private set; }

	public List<MetaReplicaDefinition> ReplicaDefinitions { get; private set; }

	public List<string> ShopGreetings { get; private set; }

	public List<MetaNarrationConditionsDefinition> VisualEvolutions { get; private set; }

	public MetaNarrationDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		DialogueGreetings = new List<string>();
		ShopGreetings = new List<string>();
		ReplicaDefinitions = new List<MetaReplicaDefinition>();
		MandatoryReplicaDefinitions = new List<MetaReplicaDefinition>();
		VisualEvolutions = new List<MetaNarrationConditionsDefinition>();
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("NameRevealDialogueId"));
		NameRevealDialogueId = val2.Value;
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("DialogueGreetings"))).Elements(XName.op_Implicit("DialogueGreeting")))
		{
			DialogueGreetings.Add(item.Value);
		}
		foreach (XElement item2 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("ShopGreetings"))).Elements(XName.op_Implicit("ShopGreeting")))
		{
			ShopGreetings.Add(item2.Value);
		}
		foreach (XElement item3 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("Replicas"))).Elements(XName.op_Implicit("Replica")))
		{
			MetaReplicaDefinition metaReplicaDefinition = new MetaReplicaDefinition((XContainer)(object)item3);
			if (metaReplicaDefinition.Mandatory)
			{
				MandatoryReplicaDefinitions.Add(metaReplicaDefinition);
			}
			else
			{
				ReplicaDefinitions.Add(metaReplicaDefinition);
			}
		}
		foreach (XElement item4 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("VisualEvolutions"))).Elements(XName.op_Implicit("VisualEvolution")))
		{
			VisualEvolutions.Add(new MetaNarrationConditionsDefinition((XContainer)(object)((XContainer)item4).Element(XName.op_Implicit("Conditions"))));
		}
	}
}
