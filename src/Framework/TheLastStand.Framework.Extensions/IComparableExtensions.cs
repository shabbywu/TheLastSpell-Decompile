using System;
using UnityEngine;

namespace TheLastStand.Framework.Extensions;

public static class IComparableExtensions
{
	public static int? SafeCompareTo(this IComparable a, IComparable b)
	{
		return a.SafeNaturalCompareTo(b) ?? b.SafeNaturalCompareTo(a) ?? a.SafeConvertCompareTo(b) ?? b.SafeConvertCompareTo(a);
	}

	public static int? SafeConvertCompareTo(this IComparable a, IComparable b)
	{
		try
		{
			return a.CompareTo(ConvertObject(b, a.GetType()));
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"Caught exception {arg}.");
			return null;
		}
	}

	public static int? SafeNaturalCompareTo(this IComparable a, IComparable b)
	{
		try
		{
			return a.CompareTo(b);
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"Caught exception {arg}.");
			return null;
		}
	}

	public static T CastObject<T>(object input)
	{
		return (T)input;
	}

	public static T ConvertObject<T>(object input)
	{
		return (T)Convert.ChangeType(input, typeof(T));
	}

	public static object ConvertObject(object input, Type type)
	{
		return Convert.ChangeType(input, type);
	}
}
