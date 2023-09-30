using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class ColorSwapPaletteDefinition : Definition
{
	public class ColorSwapDefinition : Definition
	{
		public int Index { get; private set; } = -1;


		public Color OutputColor { get; private set; } = Color.cyan;


		public ColorSwapDefinition(XContainer container)
			: base(container, (Dictionary<string, string>)null)
		{
		}//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)


		public override void Deserialize(XContainer container)
		{
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			XElement val = (XElement)(object)((container is XElement) ? container : null);
			XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Index"));
			if (XDocumentExtensions.IsNullOrEmpty(val2))
			{
				Debug.LogError((object)"A ColorSwap hasn't an Index!");
				return;
			}
			if (!int.TryParse(val2.Value, out var result) || result < 0 || result > 99)
			{
				Debug.Log((object)("A ColorSwap " + ((Definition)this).HasAnInvalidInt(val2.Value)));
				return;
			}
			Index = result;
			XElement val3 = ((XContainer)val).Element(XName.op_Implicit("OutputColor"));
			Color outputColor = default(Color);
			if (!ColorUtility.TryParseHtmlString(val3.Value, ref outputColor))
			{
				Debug.Log((object)string.Format("The ColorSwap with Index : {0} {1}", result, ((Definition)this).HasAnInvalid("Color", val3.Value)));
			}
			else
			{
				OutputColor = outputColor;
			}
		}
	}

	public List<ColorSwapDefinition> ColorSwapDefinitions { get; private set; }

	public string Id { get; private set; }

	public int Weight { get; private set; } = 1;


	public ColorSwapPaletteDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			Debug.LogError((object)"A ColorSwapPalette hasn't an Id!");
			return;
		}
		Id = val2.Value;
		IEnumerable<XElement> enumerable = ((XContainer)val).Elements(XName.op_Implicit("ColorSwap"));
		if (!enumerable.Any())
		{
			Debug.LogError((object)("ColorSwapPalette " + Id + " has no ColorSwap at all!"));
			return;
		}
		ColorSwapDefinitions = new List<ColorSwapDefinition>(enumerable.Count());
		foreach (XElement item in enumerable)
		{
			ColorSwapDefinition colorSwapDefinition = new ColorSwapDefinition((XContainer)(object)item);
			if (colorSwapDefinition.Index != -1)
			{
				ColorSwapDefinitions.Add(colorSwapDefinition);
			}
		}
		XAttribute val3 = val.Attribute(XName.op_Implicit("Weight"));
		if (val3 != null)
		{
			if (!int.TryParse(val3.Value, out var result))
			{
				Debug.LogError((object)("The ColorSwapPalette : " + Id + " " + ((Definition)this).HasAnInvalidInt(val3.Value)));
			}
			else
			{
				Weight = result;
			}
		}
	}
}
