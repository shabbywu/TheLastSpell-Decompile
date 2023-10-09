using System;
using System.Collections.Generic;
using System.Linq;

namespace TheLastStand.Framework.Maths;

public static class TopologicSorter
{
	public class CyclicDependencyException : Exception
	{
		public CyclicDependencyException()
			: this("Cyclic dependency found!")
		{
		}

		public CyclicDependencyException(string message)
			: base(message)
		{
		}

		public CyclicDependencyException(object item)
			: base($"Cyclic dependency found while visiting item {item}!")
		{
		}

		public CyclicDependencyException(object item, object[] visited)
			: base(string.Format("Cyclic dependency found while visiting item {0}! (Retraced cycle: {1}).", item, string.Join(", ", visited)))
		{
		}
	}

	public static IEnumerable<T> Sort<T>(IEnumerable<T> content) where T : ITopologicSortItem<T>
	{
		return Sort(content, (T o) => o.GetDependencies());
	}

	public static IEnumerable<T> Sort<T>(IEnumerable<T> content, Func<T, IEnumerable<T>> getDependencies)
	{
		HashSet<T> hashSet = new HashSet<T>();
		Dictionary<T, bool> visited = new Dictionary<T, bool>();
		foreach (T item in content)
		{
			Visit(item, getDependencies, hashSet, visited);
		}
		return hashSet;
	}

	private static void Visit<T>(T item, Func<T, IEnumerable<T>> getDependencies, HashSet<T> sorted, Dictionary<T, bool> visited)
	{
		if (visited.TryGetValue(item, out var value))
		{
			if (value)
			{
				List<T> list = new List<T>();
				T[] array = visited.Keys.ToArray();
				int num = array.Length - 1;
				while (num >= 0 && !array[num].Equals(item))
				{
					list.Add(array[num]);
					num--;
				}
				throw new CyclicDependencyException(item, list.Cast<object>().Reverse().ToArray());
			}
			return;
		}
		visited[item] = true;
		IEnumerable<T> enumerable = getDependencies(item);
		if (enumerable != null)
		{
			foreach (T item2 in enumerable)
			{
				Visit(item2, getDependencies, sorted, visited);
			}
		}
		visited[item] = false;
		sorted.Add(item);
	}
}
