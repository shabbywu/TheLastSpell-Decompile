using System.Xml.Linq;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideAppendNextTextDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "AppendNextText";
	}

	public bool DontWaitForSkipInput { get; private set; }

	public bool NoNewLine { get; private set; }

	public AnimatedCutsceneSlideAppendNextTextDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		DontWaitForSkipInput = ((XContainer)val).Element(XName.op_Implicit("DontWaitForSkipInput")) != null;
		NoNewLine = ((XContainer)val).Element(XName.op_Implicit("NoNewLine")) != null;
	}
}
