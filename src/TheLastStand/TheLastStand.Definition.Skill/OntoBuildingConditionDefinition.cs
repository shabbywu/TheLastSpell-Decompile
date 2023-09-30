using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

public class OntoBuildingConditionDefinition : SkillConditionDefinition
{
	public const string OntoBuildingName = "OntoBuilding";

	public string BuildingDefinitionId { get; private set; }

	public override string Name => "OntoBuilding";

	public OntoBuildingConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		BuildingDefinitionId = val.Value;
	}
}
