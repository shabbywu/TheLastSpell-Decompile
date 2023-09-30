using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public abstract class TrophyConditionDefinition : Definition
{
	public virtual object[] DescriptionLocalizationParameters { get; }

	public TrophyConditionDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
