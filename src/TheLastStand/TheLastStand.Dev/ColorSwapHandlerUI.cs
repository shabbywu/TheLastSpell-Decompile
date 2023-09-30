using System;
using System.Text;
using TPLib;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.Dev;

public class ColorSwapHandlerUI : MonoBehaviour
{
	private static class Constants
	{
		public static class Xml
		{
			public const string ColorSwapPaletteTag = "ColorSwapPalette";

			public const string ColorSwapTag = "ColorSwap";

			public const string IndexTag = "Index";

			public const string OutputColorTag = "OutputColor";
		}
	}

	[Serializable]
	private class ColorSwap
	{
		[SerializeField]
		private Color color = Color.white;

		[SerializeField]
		private int index;

		public Color Color => color;

		public int Index => index;
	}

	[SerializeField]
	[FormerlySerializedAs("imageRenderer")]
	private Image imageRenderer;

	private Texture2D colorSwapTex;

	private Color[] spriteColors;

	[SerializeField]
	[FormerlySerializedAs("DBG_colorSwaps")]
	private ColorSwap[] debugColorSwaps;

	[ContextMenu("Init Swap Texture")]
	public void InitColorSwapTex()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(100, 1, (TextureFormat)4, false, false)
		{
			filterMode = (FilterMode)0
		};
		for (int i = 0; i < ((Texture)val).width; i++)
		{
			val.SetPixel(i, 0, new Color(0f, 0f, 0f, 0f));
		}
		val.Apply();
		((Graphic)imageRenderer).material.SetTexture("_SwapTex", (Texture)(object)val);
		colorSwapTex = val;
	}

	public void SwapColor(int index, Color color)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		colorSwapTex.SetPixel(index, 0, color);
	}

	private void Awake()
	{
		if ((Object)(object)imageRenderer == (Object)null)
		{
			imageRenderer = ((Component)this).GetComponent<Image>();
		}
	}

	private void Start()
	{
		InitColorSwapTex();
		SwapColorsInstant();
	}

	[ContextMenu("Swap Colors")]
	public void SwapColorsInstant()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (debugColorSwaps != null)
		{
			for (int i = 0; i < debugColorSwaps.Length; i++)
			{
				SwapColor(debugColorSwaps[i].Index, debugColorSwaps[i].Color);
			}
			colorSwapTex.Apply();
		}
	}

	[ContextMenu("Print XML Swap Palette")]
	public void PrintPalette()
	{
		if (debugColorSwaps != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("<ColorSwapPalette Id=\"ToBeDefined\">");
			for (int i = 0; i < debugColorSwaps.Length; i++)
			{
				AddPrintableXmlColorSwap(debugColorSwaps[i], ref stringBuilder);
			}
			stringBuilder.AppendLine("</ColorSwapPalette>");
			TPDebug.Log((object)("\n" + stringBuilder.ToString()), (Object)(object)this);
		}
	}

	private void AddPrintableXmlColorSwap(ColorSwap colorSwap, ref StringBuilder stringBuilder)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (colorSwap != null)
		{
			stringBuilder.AppendLine("\t<ColorSwap>");
			stringBuilder.AppendLine(string.Format("\t\t<{0}>{1}</{2}>", "Index", colorSwap.Index, "Index"));
			stringBuilder.AppendLine("\t\t<OutputColor>#" + ColorUtility.ToHtmlStringRGBA(colorSwap.Color) + "</OutputColor>");
			stringBuilder.AppendLine("\t</ColorSwap>");
		}
	}
}
