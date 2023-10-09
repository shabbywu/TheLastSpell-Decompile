using System;

namespace TheLastStand.Framework.Utils;

[Serializable]
public struct SerializablePair<T1, T2>
{
	public T1 Key;

	public T2 Value;

	public SerializablePair(T1 key, T2 value)
	{
		Key = key;
		Value = value;
	}
}
