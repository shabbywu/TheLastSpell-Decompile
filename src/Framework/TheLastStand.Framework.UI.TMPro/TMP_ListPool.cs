using System.Collections.Generic;
using UnityEngine.Events;

namespace TheLastStand.Framework.UI.TMPro;

internal static class TMP_ListPool<T>
{
	private static readonly TMP_ObjectPool<List<T>> Pool = new TMP_ObjectPool<List<T>>(null, (UnityAction<List<T>>)(object)(UnityAction<List<List<T>>>)delegate(List<T> l)
	{
		l.Clear();
	});

	public static List<T> Get()
	{
		return Pool.Get();
	}

	public static void Release(List<T> toRelease)
	{
		Pool.Release(toRelease);
	}
}
