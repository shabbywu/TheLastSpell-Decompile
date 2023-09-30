using System.Xml.Linq;

namespace TheLastStand.Definition.Tooltip.Compendium;

public class GameConceptEntryDefinition : ACompendiumEntryDefinition
{
	public static class Constants
	{
		public const string Name = "GameConceptEntry";
	}

	public string GameConceptId { get; private set; }

	public GameConceptEntryDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("GameConceptId"));
		GameConceptId = val.Value;
	}
}
