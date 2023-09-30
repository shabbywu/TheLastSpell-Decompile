using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class PlayCommanderFadeOutAnimCutsceneDefinition : Definition, ICutsceneDefinition
{
	public class Constants
	{
		public const string Id = "CommanderPlayFadeOutAnim";
	}

	public PlayCommanderFadeOutAnimCutsceneDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
