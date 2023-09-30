using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class PlaySealAnticipationCutsceneDefinition : Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlaySealAnticipation";
	}

	public PlaySealAnticipationCutsceneDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
