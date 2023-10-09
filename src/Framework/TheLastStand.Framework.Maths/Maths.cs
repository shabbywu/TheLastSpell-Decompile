using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheLastStand.Framework.Maths;

public static class Maths
{
	public static float Gauss(float x, float height, float center, float deviation)
	{
		float num = (x - center) / (2f * deviation * deviation);
		return height * Mathf.Exp((0f - num) * num / 2f);
	}

	public static int GCD(int a, int b)
	{
		while (b != 0)
		{
			int num = a % b;
			a = b;
			b = num;
		}
		return Mathf.Abs(a);
	}

	public static bool IsPointInTriangle(Vector2 point, Vector2[] triangle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		float num = Sign(point, triangle[0], triangle[1]);
		float num2 = Sign(point, triangle[1], triangle[2]);
		float num3 = Sign(point, triangle[2], triangle[0]);
		bool flag = num < 0f || num2 < 0f || num3 < 0f;
		bool flag2 = num > 0f || num2 > 0f || num3 > 0f;
		return !(flag && flag2);
	}

	public static int IsPointLeftToEdge(Vector2 edgeA, Vector2 edgeB, Vector2 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		float num = (edgeB.y - edgeA.y) * (point.x - edgeA.x) - (point.y - edgeA.y) * (edgeB.x - edgeA.x);
		if (!(num > 0f))
		{
			if (!(num < 0f))
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public static Vector2 GetClosestPointOnSegment(Vector2 point, Vector2 segmentA, Vector2 segmentB)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Dot(point - segmentA, segmentB - segmentA) / Vector2.Dot(segmentB - segmentA, segmentB - segmentA);
		return segmentA + Mathf.Clamp01(num) * (segmentB - segmentA);
	}

	public static Vector2 GetClosestPointOnPolygon(IEnumerable<Vector2> vertices, Vector2 point)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		Vector2[] array = vertices.ToArray();
		Vector2 val = Vector2.positiveInfinity;
		Vector2 val2;
		Vector2 closestPointOnSegment;
		for (int num = array.Length - 2; num >= 0; num--)
		{
			closestPointOnSegment = GetClosestPointOnSegment(point, array[num], array[num + 1]);
			val2 = val - point;
			float sqrMagnitude = ((Vector2)(ref val2)).sqrMagnitude;
			val2 = closestPointOnSegment - point;
			if (sqrMagnitude > ((Vector2)(ref val2)).sqrMagnitude)
			{
				val = closestPointOnSegment;
			}
		}
		closestPointOnSegment = GetClosestPointOnSegment(point, array[^1], array[0]);
		val2 = val - point;
		float sqrMagnitude2 = ((Vector2)(ref val2)).sqrMagnitude;
		val2 = closestPointOnSegment - point;
		if (sqrMagnitude2 > ((Vector2)(ref val2)).sqrMagnitude)
		{
			val = closestPointOnSegment;
		}
		return val;
	}

	public static Vector2 ComputePolygonCentre(IEnumerable<Vector2> vertices)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.zero;
		foreach (Vector2 vertex in vertices)
		{
			val += vertex;
		}
		return val / (float)vertices.Count();
	}

	public static Vector2 ComputePolygonCentreWorldSpace(IEnumerable<Vector2> vertices, Camera worldSpaceConversionCamera)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.zero;
		foreach (Vector2 vertex in vertices)
		{
			val += Vector2.op_Implicit(worldSpaceConversionCamera.ScreenToWorldPoint(Vector2.op_Implicit(vertex)));
		}
		return val / (float)vertices.Count();
	}

	public static Vector2 LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out bool hit)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		Vector2 zero = Vector2.zero;
		hit = false;
		float num = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);
		if (num == 0f)
		{
			return zero;
		}
		float num2 = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / num;
		float num3 = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / num;
		if (num2 < 0f || num2 > 1f || num3 < 0f || num3 > 1f)
		{
			return zero;
		}
		zero.x = p1.x + num2 * (p2.x - p1.x);
		zero.y = p1.y + num2 * (p2.y - p1.y);
		hit = true;
		return zero;
	}

	public static float Normalize(this float x, float r1Min, float r1Max, float r2Min, float r2Max)
	{
		return r2Min + (x - r1Min) * (r2Max - r2Min) / (r1Max - r1Min);
	}

	public static float NormalizeClamped(this float x, float r1Min, float r1Max, float r2Min, float r2Max)
	{
		return Mathf.Clamp(x.Normalize(r1Min, r1Max, r2Min, r2Max), r2Min, r2Max);
	}

	public static float Normalize01(this float x, float rMin, float rMax)
	{
		return (x - rMin) / (rMax - rMin);
	}

	public static float Normalize01Clamped(this float x, float rMin, float rMax)
	{
		return Mathf.Clamp01(x.Normalize01(rMin, rMax));
	}

	private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
	}
}
