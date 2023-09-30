using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta;

public abstract class MetaReplicaConditionDefinition : Definition
{
	public MetaReplicaConditionDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}
}
