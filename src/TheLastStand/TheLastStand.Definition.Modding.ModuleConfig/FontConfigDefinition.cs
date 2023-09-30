using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Modding.ModuleConfig;

public class FontConfigDefinition : ModuleConfigDefinition
{
	private class FontConfigConstants
	{
		public const string FontAssembliesElementName = "FontAssemblies";

		public const string UseFontAssemblyInModListElementName = "UseFontAssemblyInModList";

		public const string FontAssemblyElementName = "FontAssembly";

		public const string IdAttributeName = "id";
	}

	public List<FontAssemblyDefinition> FontPackDefinitions { get; } = new List<FontAssemblyDefinition>();


	public bool UseFontAssemblyInModList { get; private set; }

	public string FontAssemblyToUseInModList { get; private set; }

	public FontConfigDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("FontAssemblies"));
		XElement val2 = obj.Element(XName.op_Implicit("UseFontAssemblyInModList"));
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("FontAssembly")))
		{
			FontAssemblyDefinition fontAssemblyDefinition = new FontAssemblyDefinition((XContainer)(object)item);
			if (fontAssemblyDefinition.IsValidDefinition())
			{
				FontPackDefinitions.Add(fontAssemblyDefinition);
			}
		}
		UseFontAssemblyInModList = val2 != null;
		if (UseFontAssemblyInModList)
		{
			XAttribute val3 = val2.Attribute(XName.op_Implicit("id"));
			if (val3 != null)
			{
				FontAssemblyToUseInModList = val3.Value;
			}
			else
			{
				FontAssemblyToUseInModList = FontPackDefinitions[0].Id;
			}
		}
	}
}
