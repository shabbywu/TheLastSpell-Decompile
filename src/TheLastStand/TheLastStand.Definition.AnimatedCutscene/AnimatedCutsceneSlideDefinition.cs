using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideDefinition : Definition
{
	public string Id { get; private set; }

	public List<AnimatedCutsceneSlideItemDefinition> SlideElementsDefinitions { get; private set; }

	public AnimatedCutsceneSlideDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		SlideElementsDefinitions = new List<AnimatedCutsceneSlideItemDefinition>();
		foreach (XElement item in obj.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "Delay":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideDelayDefinition((XContainer)(object)item));
				continue;
			case "NextAnimation":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideNextAnimationDefinition((XContainer)(object)item));
				continue;
			case "AppendNextText":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideAppendNextTextDefinition((XContainer)(object)item));
				continue;
			case "ClearText":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideClearTextDefinition((XContainer)(object)item));
				continue;
			case "PlaySound":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlidePlaySoundDefinition((XContainer)(object)item));
				continue;
			case "ChangeMusic":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideChangeMusicDefinition((XContainer)(object)item));
				continue;
			case "ReplaceCommanderView":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideReplaceCommanderViewDefinition((XContainer)(object)item));
				continue;
			}
			CLoggerManager.Log((object)("Unknown slide item Id " + item.Name.LocalName + " in Slide " + Id + "."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
	}
}
