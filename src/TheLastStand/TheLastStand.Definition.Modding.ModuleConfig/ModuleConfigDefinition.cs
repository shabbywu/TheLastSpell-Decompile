using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Modding.ModuleConfig;

public class ModuleConfigDefinition : Definition
{
	public static class Constants
	{
		public const string ConfigFileName = "config.xml";
	}

	public ModuleConfigDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
