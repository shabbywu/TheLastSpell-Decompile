using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockPerkCollectionSlotsMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockPerkCollectionSlots";

	public HashSet<int> PerkCollectionSlotsToUnlock = new HashSet<int>();

	public UnlockPerkCollectionSlotsMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in ((container is XElement) ? container : null).Elements(XName.op_Implicit("CollectionSlotToUnlock")))
		{
			if (int.TryParse(item.Value, out var result))
			{
				PerkCollectionSlotsToUnlock.Add(result);
			}
		}
	}
}
