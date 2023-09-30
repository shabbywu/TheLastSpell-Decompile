using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public abstract class PassiveTriggerDefinition : Definition
{
	public E_EffectTime EffectTime { get; protected set; }

	public PassiveTriggerDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
