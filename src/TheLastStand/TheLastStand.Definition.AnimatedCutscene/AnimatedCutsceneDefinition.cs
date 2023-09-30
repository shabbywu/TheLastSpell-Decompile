using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneDefinition : Definition
{
	public string Id { get; private set; }

	public List<AnimatedCutsceneSlideDefinition> SlidesDefinitions { get; private set; }

	public AnimatedCutsceneDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		SlidesDefinitions = new List<AnimatedCutsceneSlideDefinition>();
		foreach (XElement item in obj.Elements(XName.op_Implicit("Slide")))
		{
			SlidesDefinitions.Add(new AnimatedCutsceneSlideDefinition((XContainer)(object)item));
		}
	}
}
