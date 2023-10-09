using System;

namespace TheLastStand.Framework.Utils;

[Serializable]
public struct SerializableTuple<T1, T2>
{
	public T1 Item1;

	public T2 Item2;

	public SerializableTuple(T1 item1, T2 item2)
	{
		Item1 = item1;
		Item2 = item2;
	}
}
[Serializable]
public struct SerializableTuple<T1, T2, T3>
{
	public T1 Item1;

	public T2 Item2;

	public T3 Item3;

	public SerializableTuple(T1 item1, T2 item2, T3 item3)
	{
		Item1 = item1;
		Item2 = item2;
		Item3 = item3;
	}
}
[Serializable]
public struct SerializableTuple<T1, T2, T3, T4>
{
	public T1 Item1;

	public T2 Item2;

	public T3 Item3;

	public T4 Item4;

	public SerializableTuple(T1 item1, T2 item2, T3 item3, T4 item4)
	{
		Item1 = item1;
		Item2 = item2;
		Item3 = item3;
		Item4 = item4;
	}
}
