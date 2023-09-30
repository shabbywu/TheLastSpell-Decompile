using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.Module;

public abstract class BuildingModuleDefinition : Definition
{
	public readonly BuildingDefinition BuildingDefinition;

	protected BuildingModuleDefinition(BuildingDefinition buildingDefinition, XContainer moduleDefinitionContainer)
		: base((XContainer)null, (Dictionary<string, string>)null)
	{
		BuildingDefinition = buildingDefinition;
		((Definition)this).Deserialize(moduleDefinitionContainer);
	}
}
