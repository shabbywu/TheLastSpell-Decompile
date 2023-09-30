using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockBuildingUpgradeMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockBuildingUpgrade";

	public string UpgradeId { get; private set; }

	public UnlockBuildingUpgradeMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		UpgradeId = val.Value;
	}

	public override string ToString()
	{
		return "UnlockBuildingUpgrade (" + UpgradeId + ")";
	}
}
