using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockTraitsMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockTraits";

	public const string ChildName = "Trait";

	public List<string> TraitsToUnlock = new List<string>();

	public UnlockTraitsMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		foreach (XElement item in ((container is XElement) ? container : null).Elements(XName.op_Implicit("Trait")))
		{
			if (item.Value != string.Empty)
			{
				TraitsToUnlock.Add(item.Value);
			}
		}
	}
}
