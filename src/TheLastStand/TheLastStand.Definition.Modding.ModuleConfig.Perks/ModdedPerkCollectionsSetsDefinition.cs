using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Sirenix.Utilities;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Modding.ModuleConfig.Perks;

public class ModdedPerkCollectionsSetsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<int, UnitPerkCollectionSetDefinition> PerkCollectionSetDefinitions { get; } = new Dictionary<int, UnitPerkCollectionSetDefinition>();


	public ModdedPerkCollectionsSetsDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = ((container is XDocument) ? container : null).Element(XName.op_Implicit("UnitPerkCollectionSetDefinitions"));
		IEnumerable<XElement> enumerable = ((obj != null) ? ((XContainer)obj).Elements(XName.op_Implicit("UnitPerkCollectionSetDefinition")) : null);
		if (enumerable == null)
		{
			return;
		}
		foreach (XElement item in enumerable)
		{
			UnitPerkCollectionSetDefinition unitPerkCollectionSetDefinition = new UnitPerkCollectionSetDefinition((XContainer)(object)item);
			if (PerkCollectionSetDefinitions.ContainsKey(unitPerkCollectionSetDefinition.Index))
			{
				LinqExtensions.AddRange<Tuple<UnitPerkCollectionDefinition, int, string>>(PerkCollectionSetDefinitions[unitPerkCollectionSetDefinition.Index].CollectionsPerWeight, (IEnumerable<Tuple<UnitPerkCollectionDefinition, int, string>>)unitPerkCollectionSetDefinition.CollectionsPerWeight);
			}
			else
			{
				PerkCollectionSetDefinitions[unitPerkCollectionSetDefinition.Index] = unitPerkCollectionSetDefinition;
			}
		}
	}
}
