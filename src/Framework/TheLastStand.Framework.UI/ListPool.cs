using System.Collections.Generic;
using UnityEngine.Events;

namespace TheLastStand.Framework.UI;

internal static class ListPool<T>
{
	private static readonly ObjectPool<List<T>> Pool = new ObjectPool<List<T>>(null, (UnityAction<List<T>>)(object)new UnityAction<List<List<T>>>(Clear));

	private static void Clear(List<T> l)
	{
		l.Clear();
	}

	public static List<T> Get()
	{
		return Pool.Get();
	}

	public static void Release(List<T> toRelease)
	{
		Pool.Release(toRelease);
	}
}
