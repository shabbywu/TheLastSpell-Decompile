using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class PlayAnimatedCutsceneDefinition : Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlayAnimatedCutscene";
	}

	public string CutsceneId { get; private set; }

	public PlayAnimatedCutsceneDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		CutsceneId = val.Attribute(XName.op_Implicit("CutsceneId")).Value;
	}
}
