using System;
using UnityEngine;

namespace TheLastStand.Framework.Extensions;

public static class VectorExtensions
{
	public static Vector2 Swap(this Vector2 v)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		v.x += v.y;
		v.y = v.x - v.y;
		v.x -= v.y;
		return v;
	}

	public static string GetSimplifiedRange(this Vector2 v, string separator = "-", Func<float, float> dataFormatter = null)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (dataFormatter == null)
		{
			if (TPHelpers.IsApproxEqual(v.x, v.y, 5E-05f))
			{
				return $"{v.x}";
			}
			return $"{v.x}{separator}{v.y}";
		}
		if (TPHelpers.IsApproxEqual(v.x, v.y, 5E-05f))
		{
			return $"{dataFormatter(v.x)}";
		}
		return $"{dataFormatter(v.x)}{separator}{dataFormatter(v.y)}";
	}

	public static Vector2Int Swap(this Vector2Int v)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		((Vector2Int)(ref v)).x = ((Vector2Int)(ref v)).x + ((Vector2Int)(ref v)).y;
		((Vector2Int)(ref v)).y = ((Vector2Int)(ref v)).x - ((Vector2Int)(ref v)).y;
		((Vector2Int)(ref v)).x = ((Vector2Int)(ref v)).x - ((Vector2Int)(ref v)).y;
		return v;
	}

	public static string GetSimplifiedRange(this Vector2Int v, string separator = "-", Func<int, int> dataFormatter = null)
	{
		if (dataFormatter == null)
		{
			if (((Vector2Int)(ref v)).x == ((Vector2Int)(ref v)).y)
			{
				return $"{((Vector2Int)(ref v)).x}";
			}
			return $"{((Vector2Int)(ref v)).x}{separator}{((Vector2Int)(ref v)).y}";
		}
		if (((Vector2Int)(ref v)).x == ((Vector2Int)(ref v)).y)
		{
			return $"{dataFormatter(((Vector2Int)(ref v)).x)}";
		}
		return $"{dataFormatter(((Vector2Int)(ref v)).x)}{separator}{dataFormatter(((Vector2Int)(ref v)).y)}";
	}

	public static Vector2Int AddPercentage(this Vector2Int range, float percentage)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return Vector2Int.RoundToInt(Vector2Int.op_Implicit(range) * (1f + percentage));
	}

	public static Vector2Int GetRangeFromPercentage(this Vector2Int range, float percentage)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Vector2Int.RoundToInt(Vector2Int.op_Implicit(range) * percentage);
	}

	public static float Lerp(this Vector2 range, float t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Lerp(range.x, range.y, t);
	}

	public static float Lerp(this Vector2 range, float t, AnimationCurve animationCurve)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Lerp(range.x, range.y, animationCurve.Evaluate(t));
	}

	public static float Lerp(this Vector2Int range, float t)
	{
		return Mathf.Lerp((float)((Vector2Int)(ref range)).x, (float)((Vector2Int)(ref range)).y, t);
	}

	public static float Lerp(this Vector2Int range, float t, AnimationCurve animationCurve)
	{
		return Mathf.Lerp((float)((Vector2Int)(ref range)).x, (float)((Vector2Int)(ref range)).y, animationCurve.Evaluate(t));
	}

	public static Vector2 Round(this Vector2 range)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.Round(range.x), Mathf.Round(range.y));
	}

	public static Vector2Int RoundToInt(this Vector2 range)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2Int(Mathf.RoundToInt(range.x), Mathf.RoundToInt(range.y));
	}
}
