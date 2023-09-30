using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockBuildingMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockBuilding";

	public string BuildingId { get; private set; }

	public UnlockBuildingMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		BuildingId = val.Value;
	}

	public override string ToString()
	{
		return "UnlockBuilding (" + BuildingId + ")";
	}
}
