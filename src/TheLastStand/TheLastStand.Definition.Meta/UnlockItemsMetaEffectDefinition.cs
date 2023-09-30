using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockItemsMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockItems";

	public const string ChildName = "Item";

	public readonly List<string> ItemsToUnlock = new List<string>();

	public UnlockItemsMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in ((container is XElement) ? container : null).Elements(XName.op_Implicit("Item")))
		{
			if (!string.IsNullOrEmpty(item.Value))
			{
				ItemsToUnlock.Add(item.Value);
			}
		}
	}
}
