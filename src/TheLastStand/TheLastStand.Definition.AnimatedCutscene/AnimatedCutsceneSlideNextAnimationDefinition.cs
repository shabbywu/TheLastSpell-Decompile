using System.Xml.Linq;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideNextAnimationDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "NextAnimation";
	}

	public string CustomParameter { get; private set; }

	public bool WaitForClipDuration { get; private set; }

	public AnimatedCutsceneSlideNextAnimationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		WaitForClipDuration = ((XContainer)val).Element(XName.op_Implicit("WaitForClipDuration")) != null;
		XAttribute val2 = val.Attribute(XName.op_Implicit("CustomParameter"));
		CustomParameter = ((val2 != null) ? val2.Value : null) ?? null;
	}
}
