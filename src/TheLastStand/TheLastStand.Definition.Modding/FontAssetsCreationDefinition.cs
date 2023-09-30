using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using TheLastStand.Framework.Serialization;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace TheLastStand.Definition.Modding;

public class FontAssetsCreationDefinition : Definition
{
	private class Constants
	{
		public const string SamplingPointSizeElementName = "SamplingPointSize";

		public const string AtlasPaddingElementName = "AtlasPadding";

		public const string GlyphRenderModeElementName = "GlyphRenderMode";

		public const string AtlasSizeElementName = "AtlasSize";

		public const string AtlasPopulationModeElementName = "AtlasPopulationMode";

		public const string AtlasSizeXAttributeName = "x";

		public const string AtlasSizeYAttributeName = "y";
	}

	private int atlasPadding = 10;

	private AtlasPopulationMode atlasPopulationMode = (AtlasPopulationMode)1;

	private Vector2Int atlasSize = new Vector2Int(1024, 1024);

	private GlyphRenderMode glyphRenderMode = (GlyphRenderMode)4165;

	private int samplingPointSize = 54;

	public int AtlasPadding => atlasPadding;

	public AtlasPopulationMode AtlasPopulationMode => atlasPopulationMode;

	public Vector2Int AtlasSize => atlasSize;

	public GlyphRenderMode GlyphRenderMode => glyphRenderMode;

	public int SamplingPointSize => samplingPointSize;

	public FontAssetsCreationDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}//IL_000a: Unknown result type (might be due to invalid IL or missing references)
	//IL_001a: Unknown result type (might be due to invalid IL or missing references)
	//IL_001f: Unknown result type (might be due to invalid IL or missing references)
	//IL_002a: Unknown result type (might be due to invalid IL or missing references)


	public override void Deserialize(XContainer container)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (container == null)
		{
			return;
		}
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("GlyphRenderMode"));
		XElement val2 = obj.Element(XName.op_Implicit("AtlasSize"));
		XElement val3 = obj.Element(XName.op_Implicit("AtlasPopulationMode"));
		if (val != null && Enum.TryParse<GlyphRenderMode>(val.Value, out GlyphRenderMode result))
		{
			glyphRenderMode = result;
		}
		if (val2 != null)
		{
			XAttribute val4 = val2.Attribute(XName.op_Implicit("x"));
			XAttribute val5 = val2.Attribute(XName.op_Implicit("y"));
			if (val4 != null && val5 != null && int.TryParse(val4.Value, out var result2) && int.TryParse(val5.Value, out var result3))
			{
				atlasSize = new Vector2Int(result2, result3);
			}
		}
		if (val3 != null && Enum.TryParse<AtlasPopulationMode>(val3.Value, out AtlasPopulationMode result4))
		{
			atlasPopulationMode = result4;
		}
	}
}
