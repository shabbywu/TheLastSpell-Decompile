using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Modding;

public class FontAssemblyDefinition : Definition
{
	public class Constants
	{
		public const string IdAttributeName = "id";

		public const string FontAssetElementName = "FontAsset";
	}

	private string id = string.Empty;

	private List<FontAssetsDefinition> fontAssetsDefinitions = new List<FontAssetsDefinition>();

	public string Id => id;

	public List<FontAssetsDefinition> FontAssetsDefinition => fontAssetsDefinitions;

	public FontAssemblyDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("id"));
		if (val != null)
		{
			id = val.Value;
		}
		foreach (XElement item in obj.Elements(XName.op_Implicit("FontAsset")))
		{
			FontAssetsDefinition fontAssetsDefinition = new FontAssetsDefinition((XContainer)(object)item);
			if (fontAssetsDefinition.IsDefinitionValid())
			{
				fontAssetsDefinitions.Add(fontAssetsDefinition);
			}
			else
			{
				((CLogger<FontManager>)(object)TPSingleton<FontManager>.Instance).LogError((object)("A FontAsset definition isn't valid, the FontAssembly id is : " + id), (CLogLevel)1, true, true);
			}
		}
	}

	public bool IsValidDefinition()
	{
		return fontAssetsDefinitions.Count > 0;
	}
}
