using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.AnimatedCutscene;

public abstract class AnimatedCutsceneSlideItemDefinition : Definition
{
	public AnimatedCutsceneSlideItemDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
