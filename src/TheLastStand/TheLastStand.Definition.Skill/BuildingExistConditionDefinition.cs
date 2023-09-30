using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

public class BuildingExistConditionDefinition : SkillConditionDefinition
{
	public const string BuildingExistName = "BuildingExist";

	public string BuildingDefinitionId { get; private set; }

	public override string Name => "BuildingExist";

	public BuildingExistConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		BuildingDefinitionId = val.Value;
	}
}
