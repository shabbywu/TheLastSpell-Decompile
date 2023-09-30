using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Modding.ModuleConfig.Perks;

public class ModdedPerkCollectionsDefinition : Definition
{
	public List<UnitPerkCollectionDefinition> PerkCollectionDefinitions { get; } = new List<UnitPerkCollectionDefinition>();


	public ModdedPerkCollectionsDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = ((container is XDocument) ? container : null).Element(XName.op_Implicit("UnitPerkCollectionDefinitions"));
		IEnumerable<XElement> enumerable = ((obj != null) ? ((XContainer)obj).Elements(XName.op_Implicit("UnitPerkCollectionDefinition")) : null);
		if (enumerable == null)
		{
			return;
		}
		foreach (XElement item in enumerable)
		{
			UnitPerkCollectionDefinition unitPerkCollectionDefinition = new UnitPerkCollectionDefinition((XContainer)(object)item);
			PerkCollectionDefinitions.Add(unitPerkCollectionDefinition);
			PlayableUnitDatabase.UnitPerkCollectionDefinitions[unitPerkCollectionDefinition.Id] = unitPerkCollectionDefinition;
		}
	}
}
