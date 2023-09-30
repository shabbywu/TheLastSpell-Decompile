using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.BuildingPassive;

public abstract class BuildingPassiveEffectDefinition : Definition
{
	public BuildingPassiveEffectDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}
}
