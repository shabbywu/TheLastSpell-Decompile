using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastStand.Framework.Collections;

public abstract class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[Serializable]
	private struct KeyValueData
	{
		public TKey Key;

		public TValue Value;
	}

	[SerializeField]
	private List<KeyValueData> keyValueData = new List<KeyValueData>();

	public void OnAfterDeserialize()
	{
		Clear();
		foreach (KeyValueData keyValueDatum in keyValueData)
		{
			base[keyValueDatum.Key] = keyValueDatum.Value;
		}
	}

	public void OnBeforeSerialize()
	{
		keyValueData.Clear();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> current = enumerator.Current;
			keyValueData.Add(new KeyValueData
			{
				Key = current.Key,
				Value = current.Value
			});
		}
	}
}
