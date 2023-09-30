using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class NewEnemyMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "NewEnemy";

	public string EnemyId { get; private set; }

	public NewEnemyMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		EnemyId = val.Value;
	}

	public override string ToString()
	{
		return "NewEnemy (" + EnemyId + ")";
	}
}
