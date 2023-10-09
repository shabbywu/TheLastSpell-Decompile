using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class PlayPillarsCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlayPillarsCutscene";
	}

	public bool SkipFadeOut { get; private set; }

	public PlayPillarsCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		SkipFadeOut = ((XContainer)val).Element(XName.op_Implicit("SkipFadeOut")) != null;
	}
}
