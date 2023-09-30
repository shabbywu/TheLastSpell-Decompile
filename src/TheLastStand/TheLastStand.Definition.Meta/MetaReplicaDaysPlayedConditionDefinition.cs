using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class MetaReplicaDaysPlayedConditionDefinition : MetaReplicaConditionDefinition
{
	public class Constants
	{
		public const string Name = "DaysPlayed";
	}

	public int DaysCount { get; private set; }

	public MetaReplicaDaysPlayedConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (!int.TryParse(val.Value, out var result))
		{
			Debug.LogError((object)("Could not parse " + val.Value + " to a valid int value."));
		}
		else
		{
			DaysCount = result;
		}
	}
}
