using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheLastStand.Framework.Extensions;

public static class ListExtensions
{
	public static void Shuffle<T>(this IList<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = Random.Range(0, num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static IList<T> ShuffleAndReturn<T>(this IList<T> originalList)
	{
		List<T> list = new List<T>(originalList);
		list.Shuffle();
		return list;
	}

	public static T PickRandom<T>(this IEnumerable<T> enumerable, int length)
	{
		return enumerable.ElementAt(Random.Range(0, length));
	}

	public static T PickRandom<T>(this IEnumerable<T> enumerable)
	{
		return enumerable.PickRandom(enumerable.Count());
	}

	public static bool TryFind<T>(this List<T> list, Predicate<T> predicate, out T value)
	{
		T val = list.Find(predicate);
		if (EqualityComparer<T>.Default.Equals(val, default(T)))
		{
			value = default(T);
			return false;
		}
		value = val;
		return true;
	}

	public static bool TryFind<T>(this IEnumerable<T> enumerable, Func<T, bool> func, out T value)
	{
		T val = enumerable.FirstOrDefault(func);
		if (EqualityComparer<T>.Default.Equals(val, default(T)))
		{
			value = default(T);
			return false;
		}
		value = val;
		return true;
	}

	public static bool TryGetAtIndex<T>(this List<T> list, int index, out T value)
	{
		if (list.Count >= index)
		{
			value = list[index];
			return true;
		}
		value = default(T);
		return false;
	}

	public static bool Contains<T>(this HashSet<T> hashSet, Func<T, bool> func)
	{
		for (int num = hashSet.Count - 1; num >= 0; num--)
		{
			if (func(hashSet.ElementAt(num)))
			{
				return true;
			}
		}
		return false;
	}

	public static int Split<T>(this List<T> list, Func<T, bool> splitCondition, int startIndex = 0, int maxIndex = -1)
	{
		if (maxIndex <= 0 || maxIndex >= list.Count)
		{
			maxIndex = list.Count - 1;
		}
		int num = 0;
		for (int i = startIndex; i <= maxIndex; i++)
		{
			T val = list[i];
			if (splitCondition(val))
			{
				list.RemoveAt(i);
				list.Insert(0, val);
				num++;
			}
		}
		return num;
	}
}
