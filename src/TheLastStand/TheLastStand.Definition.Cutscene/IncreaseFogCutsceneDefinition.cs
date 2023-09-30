using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class IncreaseFogCutsceneDefinition : Definition, ICutsceneDefinition
{
	public class Constants
	{
		public const string Id = "IncreaseFog";
	}

	public int Value { get; private set; } = 1;


	public IncreaseFogCutsceneDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute obj = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
		if (int.TryParse((obj != null) ? obj.Value : null, out var result))
		{
			Value = result;
		}
	}
}
