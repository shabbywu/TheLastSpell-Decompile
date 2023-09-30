using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.EasyMode;

public abstract class EasyModeModifierDefinition : Definition
{
	public abstract string LocalizedModifier { get; }

	public EasyModeModifierDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}
}
