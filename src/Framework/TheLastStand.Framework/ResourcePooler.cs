using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastStand.Framework;

public static class ResourcePooler<T> where T : Object
{
	private static List<string> failedPaths = new List<string>();

	private static Dictionary<string, T> loadedResources = new Dictionary<string, T>();

	private static Dictionary<string, T[]> loadedResourcesAll = new Dictionary<string, T[]>();

	public static void Cache(string resourcePath)
	{
		if (!loadedResources.ContainsKey(resourcePath))
		{
			loadedResources.Add(resourcePath, Resources.Load<T>(resourcePath));
		}
	}

	public static void CacheAll(string resourcesPath)
	{
		if (!loadedResourcesAll.ContainsKey(resourcesPath))
		{
			loadedResourcesAll.Add(resourcesPath, Resources.LoadAll<T>(resourcesPath));
		}
	}

	public static T LoadOnce(string resourcePath, bool failSilently = false)
	{
		if (loadedResources.TryGetValue(resourcePath, out var value))
		{
			return value;
		}
		T val = Resources.Load<T>(resourcePath);
		if ((Object)(object)val == (Object)null)
		{
			if (!failSilently && !failedPaths.Contains(resourcePath))
			{
				Debug.LogWarning((object)$"Attempted to load {typeof(T)} from path {resourcePath} but found NOTHING!{Environment.NewLine}Please make sure that your assets are where the game expects them.");
			}
			failedPaths.Add(resourcePath);
			return default(T);
		}
		loadedResources.Add(resourcePath, val);
		return val;
	}

	public static T[] LoadAllOnce(string resourcesPath, bool failSilently = false)
	{
		if (loadedResourcesAll.TryGetValue(resourcesPath, out var value))
		{
			return value;
		}
		T[] array = Resources.LoadAll<T>(resourcesPath);
		if (array.Length < 1 && !failSilently && !failedPaths.Contains(resourcesPath))
		{
			Debug.LogWarning((object)("Attempted to load " + typeof(T).ToString() + "s from path " + resourcesPath + " but found NOTHING!" + Environment.NewLine + "Please make sure that your assets are where the game expects them."));
			failedPaths.Add(resourcesPath);
		}
		loadedResourcesAll.Add(resourcesPath, array);
		return LoadAllOnce(resourcesPath);
	}

	public static T Reload(string resourcePath)
	{
		loadedResources.Remove(resourcePath);
		return LoadOnce(resourcePath);
	}

	public static T[] ReloadAll(string resourcesPath)
	{
		loadedResourcesAll.Remove(resourcesPath);
		return LoadAllOnce(resourcesPath);
	}

	public static void Clear()
	{
		loadedResources.Clear();
		loadedResourcesAll.Clear();
	}
}
public static class ResourcePooler
{
	public static T LoadOnce<T>(string resourcePath, bool failSilently = false) where T : Object
	{
		return ResourcePooler<T>.LoadOnce(resourcePath, failSilently);
	}

	public static T Reload<T>(string resourcePath, bool failSilently = false) where T : Object
	{
		return ResourcePooler<T>.Reload(resourcePath);
	}

	public static T[] LoadAllOnce<T>(string resourcePath, bool failSilently = false) where T : Object
	{
		return ResourcePooler<T>.LoadAllOnce(resourcePath, failSilently);
	}

	public static T[] ReloadAll<T>(string resourcePath) where T : Object
	{
		return ResourcePooler<T>.ReloadAll(resourcePath);
	}

	public static void Clear<T>() where T : Object
	{
		ResourcePooler<T>.Clear();
	}

	public static void Cache<T>(string resourcePath) where T : Object
	{
		ResourcePooler<T>.Cache(resourcePath);
	}

	public static void CacheAll<T>(string resourcePath) where T : Object
	{
		ResourcePooler<T>.CacheAll(resourcePath);
	}
}
