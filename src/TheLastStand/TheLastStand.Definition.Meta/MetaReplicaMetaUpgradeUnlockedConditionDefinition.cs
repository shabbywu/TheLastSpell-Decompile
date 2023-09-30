using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class MetaReplicaMetaUpgradeUnlockedConditionDefinition : MetaReplicaConditionDefinition
{
	public class Constants
	{
		public const string Name = "MetaUpgradeUnlocked";
	}

	public string UpgradeId { get; private set; }

	public MetaReplicaMetaUpgradeUnlockedConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		UpgradeId = val.Value;
	}
}
