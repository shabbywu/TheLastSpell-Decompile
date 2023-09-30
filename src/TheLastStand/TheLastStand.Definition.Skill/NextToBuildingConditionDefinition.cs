using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

public class NextToBuildingConditionDefinition : SkillConditionDefinition
{
	public const string NextToBuildingName = "NextToBuilding";

	public string BuildingDefinitionId { get; private set; }

	public override string Name => "NextToBuilding";

	public NextToBuildingConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		BuildingDefinitionId = val.Value;
	}
}
