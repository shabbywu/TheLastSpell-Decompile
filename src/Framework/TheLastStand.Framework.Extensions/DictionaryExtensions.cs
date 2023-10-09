using System;
using System.Collections.Generic;
using System.Linq;

namespace TheLastStand.Framework.Extensions;

public static class DictionaryExtensions
{
	public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			return default(TValue);
		}
		return value;
	}

	public static bool TryRemoveAtKey<TKey, TObject>(this Dictionary<TKey, List<TObject>> dictionary, TKey key, TObject objectToRemove)
	{
		if (dictionary.ContainsKey(key))
		{
			return dictionary[key].Remove(objectToRemove);
		}
		return false;
	}

	public static void AddAtKey<TKey, TObject>(this Dictionary<TKey, List<TObject>> dictionary, TKey key, TObject objectToAdd)
	{
		dictionary.AddAtKey<TKey, TObject, List<TObject>>(key, objectToAdd);
	}

	public static void AddAtKey<TKey, TObject, TListObject>(this Dictionary<TKey, TListObject> dictionary, TKey key, TObject objectToAdd) where TListObject : IList<TObject>, new()
	{
		if (!dictionary.ContainsKey(key) || dictionary[key] == null)
		{
			dictionary[key] = new TListObject { objectToAdd };
		}
		else
		{
			dictionary[key].Add(objectToAdd);
		}
	}

	public static void AddValueOrCreateKey<TKey, TObject>(this Dictionary<TKey, TObject> dictionary, TKey key, TObject objectToAdd, Func<TObject, TObject, TObject> add)
	{
		if (dictionary.ContainsKey(key))
		{
			dictionary[key] = add(dictionary[key], objectToAdd);
		}
		else
		{
			dictionary[key] = objectToAdd;
		}
	}

	public static Dictionary<int, int> Add(this Dictionary<int, int> dictionary, Dictionary<int, int> toAdd)
	{
		Dictionary<int, int> dictionary2 = dictionary.Copy();
		for (int i = 0; i < toAdd.Count; i++)
		{
			if (dictionary2.ContainsKey(toAdd.ElementAt(i).Key))
			{
				dictionary2[toAdd.ElementAt(i).Key] += toAdd.ElementAt(i).Value;
			}
			else
			{
				dictionary2.Add(toAdd.ElementAt(i).Key, toAdd.ElementAt(i).Value);
			}
		}
		return dictionary2;
	}

	public static Dictionary<TKey, TValue> Copy<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
	{
		Dictionary<TKey, TValue> dictionary2 = new Dictionary<TKey, TValue>();
		foreach (TKey key in dictionary.Keys)
		{
			dictionary2.Add(key, dictionary[key]);
		}
		return dictionary2;
	}
}
