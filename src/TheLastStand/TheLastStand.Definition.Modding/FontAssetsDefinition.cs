using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Modding;

public class FontAssetsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class Constants
	{
		public class FontType
		{
			public const string Title = "Title";

			public const string Normal = "Normal";
		}

		public const string PathAttributeName = "path";

		public const string TypeAttributeName = "type";

		public const string ImportanceAttributeName = "importance";

		public const string FontAssetElementName = "FontAsset";

		public const string OutlineRatioElementName = "OutlineRatio";

		public const string SpaceBetweenCharactersElementName = "SpaceBetweenCharacters";

		public const string SizeModifierElementName = "SizeModifier";
	}

	private FontAssetsCreationDefinition fontAssetsCreationsDefinition;

	private int importance = -1;

	private string fontPath;

	private float outlineRatio = 1f;

	private int sizeModifier;

	private float spaceBetweenCharacters;

	public FontAssetsCreationDefinition FontAssetsCreationsDefinition => fontAssetsCreationsDefinition;

	public string FontPath => fontPath;

	public float OutlineRatio => outlineRatio;

	public int SizeModifier => sizeModifier;

	public float SpaceBetweenCharacters => spaceBetweenCharacters;

	public int Importance => importance;

	public FontAssetsDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("path"));
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("importance"));
		if (val != null)
		{
			fontPath = val.Value;
		}
		if (val2 != null && int.TryParse(val2.Value, out var result))
		{
			importance = result;
		}
	}

	public bool IsDefinitionValid()
	{
		if (fontPath != null)
		{
			return importance != -1;
		}
		return false;
	}
}
