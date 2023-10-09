using UnityEngine;

namespace TheLastStand.Framework.Extensions;

public static class ColorExtensions
{
	public static Color GenerateRandomColor()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
	}

	public static Color WithR(this Color color, float r)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return new Color(r, color.g, color.b, color.a);
	}

	public static Color WithG(this Color color, float g)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return new Color(color.r, g, color.b, color.a);
	}

	public static Color WithB(this Color color, float b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return new Color(color.r, color.g, b, color.a);
	}

	public static Color WithA(this Color color, float a)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return new Color(color.r, color.g, color.b, a);
	}
}
