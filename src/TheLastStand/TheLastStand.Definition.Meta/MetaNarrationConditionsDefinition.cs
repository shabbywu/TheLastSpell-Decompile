using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class MetaNarrationConditionsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<MetaReplicaConditionDefinition> Conditions { get; private set; }

	public MetaNarrationConditionsDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		Conditions = new List<MetaReplicaConditionDefinition>();
		foreach (XElement item in obj.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "DaysPlayed":
				Conditions.Add(new MetaReplicaDaysPlayedConditionDefinition((XContainer)(object)item));
				break;
			case "UsedReplica":
				Conditions.Add(new MetaReplicaUsedReplicaConditionDefinition((XContainer)(object)item));
				break;
			case "MetaUpgradeUnlocked":
				Conditions.Add(new MetaReplicaMetaUpgradeUnlockedConditionDefinition((XContainer)(object)item));
				break;
			default:
				Debug.LogError((object)("Invalid condition name " + item.Name.LocalName + "."));
				return;
			}
		}
	}
}
