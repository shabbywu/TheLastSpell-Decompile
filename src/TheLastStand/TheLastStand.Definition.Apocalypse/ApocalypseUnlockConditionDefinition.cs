using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Apocalypse;

namespace TheLastStand.Definition.Apocalypse;

public abstract class ApocalypseUnlockConditionDefinition : Definition
{
	public virtual string Name { get; }

	public ApocalypseUnlockCondition ApocalypseUnlockCondition { get; set; }

	public ApocalypseUnlockConditionDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}
}
